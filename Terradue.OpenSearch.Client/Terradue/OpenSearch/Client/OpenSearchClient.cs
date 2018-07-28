using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Reflection;
using Terradue.OpenSearch;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Core;
using log4net.Appender;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.Feature;
using Terradue.OpenSearch.Filters;
using Terradue.OpenSearch.Model;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Terradue.OpenSearch.Model.CustomExceptions;
using System.Runtime.ExceptionServices;

namespace Terradue.OpenSearch.Client {

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    public class OpenSearchClient {

        private static ILog log = LogManager.GetLogger(typeof(OpenSearchClient));
        private static Version version = typeof(OpenSearchClient).Assembly.GetName().Version;
        internal static bool verbose;
        internal static bool lax = false;
        internal static bool alternative = false;
        private static bool listOsee;
        private static string outputFilePathArg = null;
        private static string queryFormatArg = null;
        internal static string queryModelArg = "GeoTime";
        internal static List<string> baseUrlArg = null;
        private static int retryAttempts = 5;
        private static uint timeout = 300000;
        private static int pagination = 20;
        private static int totalResults;
        internal static List<string> metadataPaths = null;
        internal static List<string> parameterArgs = new List<string>();
        internal static List<string> dataModelParameterArgs = new List<string>();
        private static OpenSearchEngine ose;
        private static OpenSearchMemoryCache searchCache;
        private static NameValueCollection dataModelParameters;
        private static DataModel dataModel;
        internal static List<NetworkCredential> netCreds;

        public static bool outputStarted = false;


        public static void Main(string[] args) {
            if (!GetArgs(args)) {
                PrintUsage();
                Environment.ExitCode = 1;
                return;
            }

            OpenSearchClient client = null;
            try {
                client = new OpenSearchClient();

                client.Initialize();

                if (baseUrlArg != null)
                    client.ProcessQuery();

                if (listOsee == true)
                    client.ListOpenSearchEngineExtensions();

                if (!string.IsNullOrEmpty(queryModelArg) && baseUrlArg == null) {
                    client.PrintDataModelHelp(DataModel.CreateFromArgs(queryModelArg, new NameValueCollection()));
                }
            } catch (AggregateException ae) {
                foreach (var e in ae.InnerExceptions)
                    LogError(e, true);
                
                Environment.ExitCode = 1;
                return;
            } catch (PartialAtomException e) {
                Environment.ExitCode = 18;
                searchCache.ClearCache(".*", DateTime.Now);
                return;
            } catch (TimeoutException e) {
                LogError(e);
                Environment.ExitCode = 124;
                return;

            } catch (Exception e) {
                LogError(e, verbose);
                Environment.ExitCode = 1;
                return;
            }
        }

        internal void Initialize() {
            // Config log
            log = ConfigureLog();

            log.Debug("Initialize Addins registry.");

            log.Debug("Initialize SSL verification.");
            // Override automatic validation of SSL server certificates.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, ce, ca, p) => true;

            log.Debug("Load OpenSearch Engine.");
            ose = new OpenSearchEngine();

            LoadOpenSearchEngineExtensions(ose);

            InitCache();
        }

        private void InitCache() {
            NameValueCollection cacheSettings = new NameValueCollection();
            cacheSettings.Add("SlidingExpiration", "600");

            searchCache = new OpenSearchMemoryCache("cache", cacheSettings);
            ose.RegisterPreSearchFilter(searchCache.TryReplaceWithCacheRequest);
            ose.RegisterPostSearchFilter(searchCache.CacheResponse);
        }

        void LoadOpenSearchEngineExtensions(OpenSearchEngine ose) {
            ose.LoadPlugins();
            return;
        }

        private void ListOpenSearchEngineExtensions() {
            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            StreamWriter sw = new StreamWriter(outputStream);

            sw.WriteLine(string.Format("{0,-30}{1,-40}", "Extension Id", "Mime-Type capability"));
            sw.WriteLine(string.Format("{0,-30}{1,-40}", "============", "===================="));

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            foreach (var osee in ose.Extensions) {
                sw.WriteLine(string.Format("{0,-30}{1,-40}", osee.Value.Identifier, osee.Value.DiscoveryContentType));
            }

            sw.Close();
        }

        private void PrintDataModelHelp(DataModel dataModel) {
            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            dataModel.PrintHelp(outputStream);
        }

        private void ListOutputFormat() {
            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            ListFormat(outputStream);
        }

        internal void ProcessQuery(Stream outputStream = null) {
            bool closeOutputStream = true;
            bool isAtomFeedPartial = false;

            // Base OpenSearch URL
            log.Debug("Initialize urls");
            List<Uri> baseUrls = InitializeUrl();

            // Initialize the output stream
            log.Debug("Initialize output");
            if (outputStream == null)
                outputStream = InitializeOutputStream();
            else
                closeOutputStream = false;

            // Init data Model
            log.Debug("Init data models");
            dataModelParameters = PrepareDataModelParameters();

            dataModel = DataModel.CreateFromArgs(queryModelArg, dataModelParameters);

            List<List<Uri>> altBaseUrlLists = alternative ? new List<List<Uri>>(baseUrls.Select(u => new List<Uri>() { u })) : new List<List<Uri>>() { baseUrls };

            List<List<NetworkCredential>> altNetCredsLists = alternative ? new List<List<NetworkCredential>>(netCreds.Select(u => new List<NetworkCredential>() { u })) : new List<List<NetworkCredential>>() { netCreds };

            int alternativeCount = altBaseUrlLists.Count;
            int errorCount = 0;
            bool[] canceled = new bool[alternativeCount]; // is used to avoid multiple output
            for (int i = 0; i < alternativeCount; i++) {
                log.DebugFormat("Alternative #{0} : {1} (timeout = {2} ms)", i, string.Join(",", altBaseUrlLists[i]), timeout);

                bool alternativeSuccess = false;
                try {
                    Task task = Task.Run(() => alternativeSuccess = ProcessAlternative(altBaseUrlLists[i], altNetCredsLists[i], outputStream, ref isAtomFeedPartial, ref canceled[i]));
                    if (!task.Wait(TimeSpan.FromMilliseconds(timeout))) {
                        // NOTE: At this point the timeout has been reached, but the task continues in the background.
                        // Using the reference to the canceled[] item the execution of the alternative query can be interrupted
                        // at certain points in the ProcessAlternative method
                        // so that no output is written when the query eventually completes while the next alternative is tried
                        canceled[i] = true;
                        throw new TimeoutException("Timed out");
                    }
                    if (alternativeSuccess) break;
                } catch (Exception e) {
                    if (alternativeCount == 1) {
                        throw e;
                    } else {
                        errorCount++;
                        LogError(e, verbose);
                    }
                    searchCache.ClearCache(".*", DateTime.Now);
                }

            }

            if (closeOutputStream) outputStream.Close();

            if (isAtomFeedPartial) {
                throw new PartialAtomException();
            }

            if (alternativeCount != 1 && errorCount == alternativeCount) throw new Exception("All alternative queries failed");
        }



        internal bool ProcessAlternative(List<Uri> uri, List<NetworkCredential> credential, Stream outputStream, ref bool isAtomFeedPartial, ref bool canceled) {
            // Find OpenSearch Entity
            IOpenSearchable entity = null;
            int retry = retryAttempts;
            int index = 1;
            while (retry >= 0) {
                // Perform the query
                try {
                    OpenSearchableFactorySettings settings = new OpenSearchableFactorySettings(ose);
                    settings.MaxRetries = retryAttempts;
                    entity = dataModel.CreateOpenSearchable(uri, queryFormatArg, ose, credential, settings);
                    index = entity.GetOpenSearchDescription().DefaultUrl.IndexOffset;
                    log.Debug("IndexOffset : " + index);
                    break;
                } catch (Exception e) {
                    log.Warn(e.Message);
                    if (retry == 0)
                        throw e;
                    retry--;
                    searchCache.ClearCache(".*", DateTime.Now);
                }
            }

            NameValueCollection parameters = PrepareQueryParameters(entity);
            string startIndex = parameters.Get("startIndex");
            if (startIndex != null) {
                index = int.Parse(startIndex);
            }

            NameValueCollection parametersNvc;

            IOpenSearchResultCollection osr = null;
            long totalCount = 0;
            log.DebugFormat(DateTime.Now + " - " + "{0} entries requested", totalResults);
            while (totalResults > 0) {
                log.DebugFormat(DateTime.Now + "startIndex : {0}", index);
                parametersNvc = ResolveParameters(parameters, entity);

                retry = retryAttempts;
                while (retry >= 0) {
                    if (canceled) return false;
                    // Perform the query
                    log.Debug(DateTime.Now + " - " + "Launching query...");
                    try {
                        osr = QueryOpenSearch(ose, entity, parametersNvc);
                        isAtomFeedPartial = false;
                        break;
                    } catch (AggregateException ae) {
                        if (retry == 0) {
                            throw ae;
                        }
                        foreach (Exception e in ae.InnerExceptions) {
                            log.Warn("Exception " + e.Message);
                        }
                        retry--;
                        searchCache.ClearCache(".*", DateTime.Now);
                    } catch (KeyNotFoundException e) {
                        log.Error("Query not found : " + e.Message);
                        throw e;
                    } catch (PartialAtomException e) {
                        if (retry == 0) {
                            osr = e.PartialOpenSearchResultCollection;
                            isAtomFeedPartial = true;
                        }
                        retry--;
                        searchCache.ClearCache(".*", DateTime.Now);
                    } catch (ThreadAbortException e) {

                    } catch (Exception e) {
                        if (retry == 0) {
                            throw e;
                        }

                        log.Warn("Exception " + e.Message);
                        retry--;
                        searchCache.ClearCache(".*", DateTime.Now);
                    }
                }

                if (canceled) return false;

                int count = CountResults(osr);
                if (totalCount == 0 && count == 0) {
                    LogInfo("No entries found");
                    return false;
                }

                if (canceled) return false;

                // Transform the result
                OutputResult(osr, outputStream);

                if (outputStarted) return false;
                outputStarted = true;


                log.Debug(count + " entries found");
                if (count == 0)
                    break;
                int expectedCount = count;
                if (!string.IsNullOrEmpty(parameters["count"]) && int.TryParse(parameters["count"], out expectedCount) && count < expectedCount)
                    break;

                totalResults -= count;
                totalCount += count;
                log.Debug(count + " entries found on " + totalResults + " requested");
                int paramCount;
                if (Int32.TryParse(parameters.Get("count"), out paramCount) && totalResults < paramCount) {
                    parameters.Set("count", "" + totalResults);
                }
                index += count;

                parameters.Set("startIndex", "" + index);
            }

            return (totalCount > 0); // success
        }


        //---------------------------------------------------------------------------------------------------------------------
        public static bool GetArgs(string[] args) {
            if (args.Length == 0)
                return false;

            int argpos = 0;
            while (argpos < args.Length) {
                switch (args[argpos]) {
                    case "-v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "-o":
                    case "--output":
                        if (argpos < args.Length - 1) {
                            outputFilePathArg = args[++argpos];
                        } else
                            return false;
                        break;
                    case "-f":
                    case "--format":
                        if (argpos < args.Length - 1) {
                            queryFormatArg = args[++argpos];
                        } else
                            return false;
                        break;
                    case "-a":
                    case "--auth":
                        if (argpos < args.Length - 1) {
                            netCreds = Array.ConvertAll<string, NetworkCredential>(args[++argpos].Split(','), c => {
                                string[] creds = c.Split(':');
                                return new NetworkCredential(creds[0], creds[1]);
                            }).ToList();
                        } else
                            return false;
                        break;
                    case "-p":
                    case "--parameter":
                        if (argpos < args.Length - 1) {
                            parameterArgs.Add(args[++argpos]);
                        } else
                            return false;
                        break;
                    case "-to":
                    case "--time-out":
                        if (argpos < args.Length - 1) {
                            try {
                                timeout = uint.Parse(args[++argpos]);
                            } catch (OverflowException) {
                                Console.Error.WriteLine("Range timeout value allowed: 0 - 2147483647");
                                return false;
                            }
                        } else
                            return false;
                        break;
                    case "-h":
                    case "--help":
                        return false;
                    case "--pagination":
                        if (argpos < args.Length - 1) {
                            pagination = int.Parse(args[++argpos]);
                        } else
                            return false;
                        break;
                    case "--list-osee":
                        listOsee = true;
                        break;
                    case "-m":
                    case "--model":
                        if (argpos < args.Length - 1) {
                            queryModelArg = args[++argpos];
                        } else
                            return false;
                        break;
                    case "-dp":
                    case "--datamodel-parameter":
                        if (argpos < args.Length - 1) {
                            dataModelParameterArgs.Add(args[++argpos]);
                        } else
                            return false;
                        break;
                    case "--lax":
                        lax = true;
                        break;
                    case "--alternative":
                        alternative = true;
                        break;
                    case "--all-enclosures":
                        dataModelParameterArgs.Add("allEnclosures=true");
                        break;
                    case "-dec":
                    case "--disable-enclosure-control":
                        dataModelParameterArgs.Add("disableEnclosureControl=true");
                        break;
                    case "--max-retries":
                        if (argpos < args.Length - 1) {
                            retryAttempts = int.Parse(args[++argpos]);
                            if (retryAttempts < 0) {
                                Console.Error.WriteLine("Number of maximum retries must be a non negative integer");
                                return false;
                            }
                        } else
                            return false;
                        break;
                    default:
                        if (Regex.Match(args[argpos], "^-").Success) {
                            throw new ArgumentException(String.Format("Invalid URL or option: {0}", args[argpos]));
                        }
                        if (baseUrlArg == null) {
                            baseUrlArg = new List<string>();
                            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
                            foreach (Match match in csvSplit.Matches(args[argpos])) {
                                baseUrlArg.Add(match.Value.TrimStart(','));
                            }
                            break;
                        }
                        if (metadataPaths == null) {
                            metadataPaths = args[argpos].Split(',').ToList();
                            break;
                        }
                        break;
                }
                argpos++;
            }

            return true;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public static void PrintUsage() {
            Console.Error.WriteLine(String.Format("{0} (v{1}) - OpenSearch client - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));
            Console.Error.WriteLine("Usage: " + Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " [options...] [url1[,url2[,url3[,...]]]] [metadatapath1[,metadatapath2[,...]]]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");

            Console.Error.WriteLine(" -p/--parameter <param>=<value>      <param> specifies a parameter for the query with value <value>");
            Console.Error.WriteLine(" -o/--output <file>                  Writes output to <file> instead of stdout");
            Console.Error.WriteLine(" -f/--format <format>                Specify the format of the query. Format available can be listed with --list-osee.");
            Console.Error.WriteLine("                                     By default, the client is automatic and uses the default or the first format.");
            Console.Error.WriteLine(" -to/--time-out <n>                  <n> specifies query timeout (millisecond)");
            Console.Error.WriteLine(" --pagination <n>                    <n> specifies the pagination number for search loops. Default: 20");
            Console.Error.WriteLine(" --list-osee                         Lists the OpenSearch Engine Extensions");
            Console.Error.WriteLine(" -m/--model <format>                 <format> specifies the data model of the results for the query. Data model gives access to specific metadata extractors or transformers.");
            Console.Error.WriteLine("                                     By default the \"GeoTime\" model is used. Used without urls, it lists the metadata options");
            Console.Error.WriteLine(" -dp/--datamodel-parameter <param>   <param> Specify a data model parameter");
            Console.Error.WriteLine(" -a/--auth <creds>[,...[,...]]       <creds> is a string representing the credentials with format username:password.");
            Console.Error.WriteLine(" --lax                               Lax query: assign parameters even if not described by the opensearch server.");
            Console.Error.WriteLine(" --alternative                       Altenative query: Instead of making a parallel multi search in case of multiple URL, it tries the URL until 1 returns results");
            Console.Error.WriteLine(" --all-enclosures                    Returns all available enclosures. Implies disable enclosure control (disable-enclosure-control)");
            Console.Error.WriteLine(" -dec/--disable-enclosure-control    Do not check enclosure avaialability when enclosure metadata is queried");
            Console.Error.WriteLine(" --max-retries <n>                   <n> specifies the number of retries if the action fails");
            Console.Error.WriteLine(" -v/--verbose                        Makes the operation more talkative");
            Console.Error.WriteLine();
        }

        ILog ConfigureLog() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            ConsoleAppender consoleErrAppender = new ConsoleAppender();
            consoleErrAppender.Layout = patternLayout;
            consoleErrAppender.ActivateOptions();
            consoleErrAppender.Target = "Console.Error";
            log4net.Filter.LevelRangeFilter errfilter = new log4net.Filter.LevelRangeFilter();
            errfilter.LevelMin = Level.Verbose;
            errfilter.LevelMax = Level.Emergency;
            consoleErrAppender.AddFilter(errfilter);
            hierarchy.Root.AddAppender(consoleErrAppender);

            hierarchy.Root.Level = Level.Info;
            if (verbose == true) {
                hierarchy.Root.Level = Level.Debug;
            }
            hierarchy.Configured = true;

            BasicConfigurator.Configure(new ConsoleAppender[] { consoleErrAppender });

            return LogManager.GetLogger(typeof(OpenSearchClient));
        }



        internal static void LogError(Exception e, bool withStackTrace = false) {
            log.Error(String.Format("{0} : {1} {2}", e.Source, e.Message, e.HelpLink));
            if (verbose && withStackTrace)
                Console.Error.WriteLine(e.StackTrace);
        }



        internal static void LogWarning(string message) {
            log.Warn(message);
        }



        internal static void LogInfo(string message) {
            log.Info(message);
        }



        //---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the OpenSearch URL to query
        /// </summary>
        private List<Uri> InitializeUrl() {
            List<Uri> baseUrl = new List<Uri>();
            ;

            if (baseUrlArg == null) {
                baseUrlArg = new List<string>();
                baseUrlArg.Add(Environment.GetEnvironmentVariable("_CIOP_CQI_LOCATION"));
            }

            try {
                foreach (var url in baseUrlArg)
                    baseUrl.Add(new Uri(url));
            } catch (UriFormatException) {
                baseUrlArg.Add(string.Format("{0}/{1}", Environment.GetEnvironmentVariable("_CIOP_CQI_LOCATION"), baseUrlArg));
                try {
                    foreach (var url in baseUrlArg)
                        baseUrl.Add(new Uri(url));
                } catch (UriFormatException) {
                    throw new UriFormatException(
                        "The format of the URI could not be determined. Please check that the urls passed as argument do not contain any unencoded field separator (,). Replace them with %2C encoding character.");
                }
            }


            return baseUrl;
        }

        /// <summary>
        /// Initializes the output stream.
        /// </summary>
        /// <returns>The output stream.</returns>
        private Stream InitializeOutputStream() {
            if (outputFilePathArg == null) {
                return Console.OpenStandardOutput();
            } else {
                return new FileStream(outputFilePathArg, FileMode.Create);
            }
        }

        /// <summary>
        /// Prepares the query URL.
        /// </summary>
        /// <returns>The query URL.</returns>
        /// <param name="baseUrl">Base URL.</param>
        /// <param name="parametersNvc">Parameters nvc.</param>
        Uri PrepareQueryUrl(Uri baseUrl, NameValueCollection parametersNvc) {
            throw new NotImplementedException();
        }

        NameValueCollection PrepareQueryParameters(IOpenSearchable entity) {
            NameValueCollection nvc = new NameValueCollection();
            totalResults = 0;

            foreach (var parameter in parameterArgs) {
                Match matchParamDef = Regex.Match(parameter, @"^(.*)=(.*)$");
                // if martch is successful
                if (matchParamDef.Success) {
                    // TODO filter and convert query param
                    if (matchParamDef.Groups[1].Value == "count") {
                        if (matchParamDef.Groups[2].Value == "unlimited") {
                            nvc.Add(matchParamDef.Groups[1].Value, pagination.ToString());
                            totalResults = int.MaxValue;
                            continue;
                        }
                        totalResults = int.Parse(matchParamDef.Groups[2].Value);
                        if (totalResults > pagination) {
                            nvc.Add(matchParamDef.Groups[1].Value, pagination.ToString());
                            continue;
                        }
                    }
                    nvc.Add(matchParamDef.Groups[1].Value, matchParamDef.Groups[2].Value);
                }
            }

            if (totalResults == 0) {
                totalResults = pagination;
                nvc.Add("count", pagination.ToString());
            }

            var remoteParams = entity.GetOpenSearchParameters(entity.DefaultMimeType);

            if (remoteParams["do"] != null && nvc["do"] == null) {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOWNLOAD_ORIGIN")))
                    nvc["do"] = Environment.GetEnvironmentVariable("DOWNLOAD_ORIGIN");
                else
                    nvc["do"] = Dns.GetHostName();
            }

            dataModel.SetQueryParameters(nvc);

            return nvc;
        }

        NameValueCollection PrepareDataModelParameters() {
            NameValueCollection nvc = new NameValueCollection();

            foreach (var parameter in dataModelParameterArgs) {
                Match matchParamDef = Regex.Match(parameter, @"^(.*)=(.*)$");
                // if martch is successful
                if (matchParamDef.Success) {
                    nvc.Add(matchParamDef.Groups[1].Value, matchParamDef.Groups[2].Value);
                }
            }

            return nvc;
        }

        private IOpenSearchResultCollection QueryOpenSearch(OpenSearchEngine ose, IOpenSearchable entity, NameValueCollection parameters) {
            IOpenSearchResultCollection osr;

            if (string.IsNullOrEmpty(queryFormatArg))
                osr = ose.Query(entity, parameters);
            else
                osr = ose.Query(entity, parameters, queryFormatArg);

            return osr;
        }

        void ListFormat(Stream outputStream) { }

        void OutputResult(IOpenSearchResultCollection osr, Stream outputStream) {
            if (metadataPaths == null) {
                StreamWriter sw = new StreamWriter(outputStream);
                if (osr is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr;
                    foreach (var item in rc.Items) {
                        var link = item.Links.FirstOrDefault(l => l.RelationshipType == "self");
                        if (link != null)
                            sw.WriteLine(link.Uri.ToString());
                        else
                            sw.WriteLine(item.Id);
                    }
                    sw.Flush();
                }

                return;
            }

            dataModel.LoadResults(osr);

            if (metadataPaths.Contains("{}")) {
                dataModel.PrintCollection(outputStream);
                return;
            }

            dataModel.PrintByItem(metadataPaths, outputStream);

            return;
        }

        void SerializeXmlDocument(XmlDocument xmlDocument, Stream outputStream) {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(XmlDocument));
            serializer.Serialize(outputStream, xmlDocument);
        }

        NameValueCollection ResolveParameters(NameValueCollection nameValueCollection, IOpenSearchable entity) {
            string contentType = entity.DefaultMimeType;

            NameValueCollection osdParam = entity.GetOpenSearchParameters(contentType);
            NameValueCollection osdRevParams = OpenSearchFactory.ReverseTemplateOpenSearchParameters(osdParam);
            NameValueCollection parameters = new NameValueCollection();

            foreach (string key in nameValueCollection.AllKeys) {
                if (osdRevParams[key] != null) {
                    foreach (var id in osdRevParams.GetValues(key))
                        parameters.Set(id, nameValueCollection[key]);
                } else {
                    parameters.Set(key, nameValueCollection[key]);
                }
            }
            return parameters;
        }

        int CountResults(IOpenSearchResultCollection osr) {
            if (osr is IOpenSearchResultCollection) {
                IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr;
                return rc.Items.Count();
            }

            if (osr is SyndicationFeed) {
                SyndicationFeed feed = (SyndicationFeed)osr;
                return feed.Items.Count();
            }

            return 0;
        }

    }

}