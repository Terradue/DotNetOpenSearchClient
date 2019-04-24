using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Filters;
using Terradue.OpenSearch.Model;
using Terradue.OpenSearch.Model.CustomExceptions;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Benchmarking;

namespace Terradue.OpenSearch.Client {



    public class OpenSearchClient {

        public const string XMLNS_OPENSEARCH_1_1 = "http://a9.com/-/spec/opensearch/1.1/";

        private static ILog log = LogManager.GetLogger(typeof(OpenSearchClient));
        private static Version version = typeof(OpenSearchClient).Assembly.GetName().Version;
        internal static bool verbose;
        internal static bool lax = false;
        internal static bool adjustIdentifiers = false;
        internal static bool quotingOutput = false;
        internal static bool alternative = false;
        internal static bool listEncoding;
        internal static string outputFilePathArg = null;
        internal static string descriptionArg = null;
        internal static string queryFormatArg = null;
        internal static string metricsType = null;
        internal static string queryModelArg = "GeoTime";
        internal static List<string> baseUrlArg = null;
        internal static int retryAttempts = 5;
        internal static uint timeout = 300000;
        internal static int pagination = 20;
        private static OpenSearchMemoryCache searchCache;

        internal static List<NetworkCredential> netCreds;
        internal static List<string> metadataPaths = null;
        internal static List<string> parameterArgs = new List<string>();
        internal static List<string> dataModelParameterArgs = new List<string>();

        private OpenSearchEngine ose;
        OpenSearchableFactorySettings settings;
        private int totalResults;
        private NameValueCollection dataModelParameters;
        private DataModel dataModel;
        public bool outputStarted = false;


        private static Regex argRegex = new Regex(@"^([^\.]+)(\.(.+))?$");
        private static Regex paramRegex = new Regex(@"([^&=]+?)=\{((([^:\}]+):)?([^\}]+?))(\?)?\}");

        private static Dictionary<string, string> urlTypes;
        private static Dictionary<string, string> urlParameterLabels;

        public static void Main(string[] args) {
            if (!GetArgs(args)) {
                PrintUsage();
                Environment.ExitCode = 1;
                return;
            }

            try {
                OpenSearchClient client = new OpenSearchClient();

                client.Initialize();

                if (listEncoding == true) {
                    client.ListOpenSearchEngineExtensions();
                } else if (baseUrlArg != null) {
                    if (descriptionArg == null) client.ProcessQuery();
                    else client.PrintOpenSearchDescription(descriptionArg);
                }

                if (!string.IsNullOrEmpty(queryModelArg) && baseUrlArg == null) {
                    client.PrintDataModelHelp(DataModel.CreateFromArgs(queryModelArg, new NameValueCollection()));
                }
            } catch (AggregateException ae) {
                foreach (Exception e in ae.InnerExceptions)
                    LogError(e, true);

                Environment.ExitCode = 1;
                return;
            } catch (PartialAtomException) {
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

        internal static bool GetArgs(string[] args) {
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
                    case "-d":
                    case "--description":
                        if (argpos < args.Length - 1) {
                            descriptionArg = args[++argpos];
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
                    case "--list-encoding":
                        listEncoding = true;
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
                    case "--adjust-identifiers":
                        adjustIdentifiers = true;
                        break;
                    case "-qo":
                    case "--quoting-output":
                        quotingOutput = true;
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
                    case "--metrics":
                    case "-me":
                        if (argpos < args.Length - 1) {
                            metricsType = args[++argpos];
                            switch (metricsType) {
                                case "basic":
                                    break;
                                default:
                                    log.ErrorFormat("{0} is not a valid type for metrics", metricsType);
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



        private static void PrintUsage() {
            Console.Error.WriteLine(String.Format("{0} (v{1}) - OpenSearch client - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));
            Console.Error.WriteLine("Usage: " + Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " [options...] [url1[,url2[,url3[,...]]]] [metadatapath1[,metadatapath2[,...]]]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");

            Console.Error.WriteLine(@"
-d/--description full|types|<type>[.<param>
                                   Shows a human-readable version of the OpenSearch description document
                                   ('full': entirely, 'types': URL types)
                                   <type> specifies a query URL type, <param> specifies a parameter name
-p/--parameter <param>=<value>     <param> specifies a parameter for the query with value <value>
-o/--output <file>                 Writes output to <file> instead of stdout
--list-encoding                    Lists the available OpenSearch Engine Extensions handling various metadata
                                   encodings or exchange formats (e.g. ATOM, JSON or RDF)
-f/--format <format>               Specify the output format of the query result. If the catalogue supports
                                   that format as output, the client requests it for the query.
                                   Available formats can be listed with --list-encoding. By default
                                   the client uses the default or the first format ('atom' in many cases).
-m/--model <format>                <format> specifies the data model of the results for the query. The data model
                                   gives access to specific metadata extractors or transformers. By default
                                   the ""GeoTime"" model is used. Used without urls, it lists the metadata options
-to/--time-out <n>                 <n> specifies query timeout (millisecond)
--pagination <n>                   <n> specifies the pagination number for search loops. Default: 20
-dp/--datamodel-parameter <param>  <param> specifies a data model parameter
-a/--auth <creds>[,...[,...]]      <creds> is a string representing the credentials with format username:password.
--lax                              Lax query: assigns parameters even if not described by the opensearch server.
--alternative                      Altenative query: Instead of making a parallel multi search in case of multiple URL,
                                   the client tries the URL until 1 returns results
--adjust-identifiers               Adjusts entry identifiers to match the title to avoid identifiers that are URLs
--all-enclosures                   Returns all available enclosures. Implies disable enclosure control
                                   (disable-enclosure-control)
-dec/--disable-enclosure-control   Disables the check for enclosure avaialability when enclosure metadata is queried
--max-retries <n>                  <n> specifies the number of retries if the action fails
--metrics/-me <metrics_type>       <metrics_type> specifies the type  metrics to extract and write (e.g. basic)
-v/--verbose                       Makes the operation more talkative
--exit-on-error                    Exit on any ERROR.

");
        }



        public void Initialize() {
            // Config log
            log = ConfigureLog();

            log.Debug("Initialize Addins registry.");

            log.Debug("Initialize SSL verification.");
            // Override automatic validation of SSL server certificates.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, ce, ca, p) => true;

            log.Debug("Load OpenSearch Engine.");
            ose = new OpenSearchEngine();
            settings = new OpenSearchableFactorySettings(ose);
            settings.MaxRetries = retryAttempts;
            if (!string.IsNullOrEmpty(metricsType))
                settings.ReportMetrics = true;
            settings.ParametersKeywordsTable = InitializeParametersKeywordsTable();

            LoadOpenSearchEngineExtensions(ose);

            InitCache();
        }

        private Dictionary<string, string> InitializeParametersKeywordsTable() {
            Dictionary<string, string> table = OpenSearchFactory.GetBaseOpenSearchParametersKeywordsTable();
            table["update"] = "{http://purl.org/dc/terms/}modified";
            table["updated"] = "{http://purl.org/dc/terms/}modified";
            table["modified"] = "{http://purl.org/dc/terms/}modified";
            table["do"] = "{http://www.terradue.com/opensearch}downloadOrigin";
            table["from"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}accessedFrom";
            table["start"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}start";
            table["stop"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}end";
            table["end"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}end";
            table["trel"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}relation";
            table["box"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}box";
            table["bbox"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}box";
            table["geom"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry";
            table["geometry"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry";
            table["uid"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid";
            table["id"] = "{http://purl.org/dc/terms/}identifier";
            table["rel"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}relation";
            table["cat"] = "{http://purl.org/dc/terms/}subject";
            table["pt"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}productType";
            table["psn"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}platform";
            table["psi"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier";
            table["isn"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}instrument";
            table["sensor"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType";
            table["st"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType";
            table["od"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection";
            table["ot"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitType";
            table["title"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}title";
            table["track"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}track";
            table["frame"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}frame";
            table["swath"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier";
            table["cc"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover";
            table["lc"] = "{http://www.terradue.com/opensearch}landCover";
            table["dcg"] = "{http://www.terradue.com/opensearch}doubleCheckGeometry";
            return table;
        }

        private ILog ConfigureLog() {
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



        private void LoadOpenSearchEngineExtensions(OpenSearchEngine ose) {
            ose.LoadPlugins();
        }



        private void InitCache() {
            NameValueCollection cacheSettings = new NameValueCollection();
            cacheSettings.Add("SlidingExpiration", "600");

            searchCache = new OpenSearchMemoryCache("cache", cacheSettings);
            ose.RegisterPreSearchFilter(searchCache.TryReplaceWithCacheRequest);
            ose.RegisterPostSearchFilter(searchCache.CacheResponse);
        }



        public void ProcessQuery(Stream outputStream = null) {
            
            bool isAtomFeedPartial = false;

            // Base OpenSearch URL
            log.Debug("Initialize urls");
            List<Uri> baseUrls = InitializeUrl();

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
                    Task task = Task.Run(() => alternativeSuccess = ProcessAlternative(altBaseUrlLists[i], altNetCredsLists[i], ref isAtomFeedPartial, ref canceled[i]));
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

            

            if (isAtomFeedPartial) {
                throw new PartialAtomException();
            }

            if (alternativeCount != 1 && errorCount == alternativeCount) throw new Exception("All alternative queries failed");
        }



        private bool ProcessAlternative(List<Uri> uri, List<NetworkCredential> credential, ref bool isAtomFeedPartial, ref bool canceled) {
            // Find OpenSearch Entity
            IOpenSearchable entity = null;
            int retry = retryAttempts;
            int index = 1;
            while (retry >= 0) {
                // Perform the query
                try {

                    entity = dataModel.CreateOpenSearchable(uri, queryFormatArg, ose, credential, settings);
                    index = entity.GetOpenSearchDescription().DefaultUrl.IndexOffset;
                    log.Debug("IndexOffset : " + index);
                    break;
                } catch (Exception e) {
                    log.Warn(e.Message);
                    if (retry == 0)
                        throw;
                    retry--;
                    searchCache.ClearCache(".*", DateTime.Now);
                }
            }

            if (!string.IsNullOrEmpty(metricsType))
                EnableBenchmarking();

            NameValueCollection parameters = PrepareQueryParameters(entity);
            string startIndex = parameters.Get("startIndex");
            if (startIndex != null) {
                index = int.Parse(startIndex);
            }

            NameValueCollection parametersNvc;

            IOpenSearchResultCollection osr = null;
            long totalCount = 0;
            log.DebugFormat("{0} entries requested", totalResults);

            if (outputStarted) return false;

            while (totalResults > 0) {
                bool closeOutputStream = true;
                Stream outputStream = null;
                log.DebugFormat("startIndex: {0}", index);
                parametersNvc = ResolveParameters(parameters, entity);

                // Initialize the output stream
                if (outputStream == null)
                    outputStream = InitializeOutputStream(index);
                else
                    closeOutputStream = false;

                retry = retryAttempts;
                while (retry >= 0) {
                    if (canceled) return false;
                    // Perform the query
                    log.Debug("Launching query...");
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
                    } catch (ThreadAbortException) {

                    } catch (Exception e) {
                        if (retry == 0) {
                            throw;
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
                    DeleteFileStream(outputStream);
                    return false;
                }

                if (canceled) return false;

                if (adjustIdentifiers) AdjustIdentifiers(osr, outputStream);


                if (osr.Count > 0) {
                    // Transform the result
                    OutputResult(osr, outputStream);
                } else {
                    closeOutputStream = false;
                    DeleteFileStream(outputStream);
                }

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

                if (!string.IsNullOrEmpty(metricsType))
                    WriteMetrics(osr);

                if (closeOutputStream) outputStream.Close();
            }

            return (totalCount > 0); // success
        }

        private void EnableBenchmarking() {
            ose.RegisterPostSearchFilter(MetricFactory.GenerateBasicMetrics);
        }

        private void WriteMetrics(IOpenSearchResultCollection osr) {
            log.Debug("Writing benchmark");

            var metricsArray = osr.ElementExtensions.ReadElementExtensions<Metrics>("Metrics", "http://www.terradue.com/metrics", MetricFactory.Serializer);
            if (metricsArray == null || metricsArray.Count == 0) {
                log.Warn("No metrics found");
                return;
            }

            var metrics = metricsArray.First();

            using (var fs = new FileStream(string.Format("opensearch-client_metrics_{0}_{1}.txt", metricsType, DateTime.UtcNow.ToString("O")), FileMode.Create, FileAccess.Write)) {
                using (var fw = new StreamWriter(fs)) {
                    foreach (var metric in metrics.Metric) {
                        fw.WriteLine("{0};{1};{2};{3}", metric.Identifier, metric.Value, metric.Uom, metric.Description);
                    }
                    fw.Close();
                }
            }


        }

        private void DeleteFileStream(Stream outputStream) {
            if ( outputStream is FileStream ){
                outputStream.Close();
                log.DebugFormat("Delete {0}", (outputStream as FileStream).Name);
                File.Delete((outputStream as FileStream).Name);
            }
        }

        private void PrintOpenSearchDescription(string arg) {

            Match argMatch = argRegex.Match(arg);
            string type = argMatch.Groups[1].Value;
            string paramName = (argMatch.Groups[2].Success ? argMatch.Groups[3].Value : null);

            bool typeFound = false;
            bool paramFound = false;

            dataModelParameters = PrepareDataModelParameters();
            dataModel = DataModel.CreateFromArgs(queryModelArg, dataModelParameters);
            settings.MaxRetries = retryAttempts;
            List<Uri> baseUrls = InitializeUrl();
            IOpenSearchable entity = dataModel.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds, settings);
            OpenSearchDescription osd = entity.GetOpenSearchDescription();

            using (Stream outputStream = InitializeOutputStream()) {
                using (StreamWriter sw = new StreamWriter(outputStream)) {

                    if (type == "types") {
                        sw.WriteLine("Formats");
                        sw.WriteLine("=======");
                        sw.WriteLine("{0,-10}{1,-10}{2,-60}", "Format", "Supported", "MIME type");
                        sw.WriteLine("{0,-10}{1,-10}{2,-60}", "------", "---------", "---------");

                        foreach (OpenSearchDescriptionUrl url in osd.Url) {
                            if (url.Relation == "self") continue;

                            bool isFormat = false;
                            foreach (IOpenSearchEngineExtension osee in ose.Extensions.Values) {
                                if (osee.DiscoveryContentType == url.Type) {
                                    isFormat = true;
                                    break;
                                }
                            }
                            string urlShortType = GetUrlShortType(url.Type);

                            sw.WriteLine("{0,-14}{1,-6}{2,-40}", urlShortType == url.Type ? "-" : urlShortType, isFormat ? "*" : " ", url.Type);
                        }
                    } else {
                        foreach (OpenSearchDescriptionUrl url in osd.Url) {
                            if (url.Relation == "self") continue;

                            string urlShortType = GetUrlShortType(url.Type);
                            if (type != "full" && type != urlShortType) continue;

                            // ---- only for full view or if dealing with the correct type: ----

                            typeFound = true;

                            bool isFormat = false;
                            foreach (IOpenSearchEngineExtension osee in ose.Extensions.Values) {
                                if (osee.DiscoveryContentType == url.Type) {
                                    isFormat = true;
                                    break;
                                }
                            }
                            sw.Write("Search URL type: {0}", url.Type);
                            if (isFormat) sw.Write(" (format: {0})", urlShortType);
                            sw.WriteLine();

                            int qmPos = url.Template.IndexOf('?');
                            if (qmPos == -1) continue;

                            string queryString = url.Template.Substring(qmPos + 1);

                            if (paramName == null) {
                                sw.WriteLine("Parameters");
                                sw.WriteLine("==========");
                                sw.Write("{0,-30}{1,-40}", "Parameter name", "Description/title");
                                if (type != "full") sw.Write(" M O R P S (mandatory, options, range, pattern, step)");
                                sw.WriteLine();
                                sw.Write("{0,-30}{1,-40}", "--------------", "-----------------");
                                if (type != "full") sw.Write(" ---------");
                                sw.WriteLine();
                            }

                            MatchCollection paramMatches = paramRegex.Matches(queryString);
                            foreach (Match paramMatch in paramMatches) {
                                string name = string.Format("{0}{1}", paramMatch.Groups[3].Value, paramMatch.Groups[5].Value);
                                string identifier = paramMatch.Groups[2].Value;
                                bool mandatory = !paramMatch.Groups[6].Success;
                                string qualifiedName = GetParameterQualifiedName(url, paramMatch.Groups[4].Value, paramMatch.Groups[5].Value);
                                string title =  GetParameterDescription(url, paramMatch.Groups[1].Value, qualifiedName);

                                bool options = false;
                                bool range = false;
                                bool pattern = false;
                                bool step = false;

                                OpenSearchDescriptionUrlParameter param = null;
                                if (url.Parameters != null) {
                                    foreach (OpenSearchDescriptionUrlParameter p in url.Parameters) {
                                        if (p.Name == paramMatch.Groups[1].Value) param = p;
                                    }
                                }

                                if (param != null) {
                                    title = param.Title;
                                    options = param.Options != null && param.Options.Count != 0;
                                    range = param.Maximum != null || param.MinInclusive != null || param.MaxInclusive != null || param.MinExclusive != null || param.MaxExclusive != null;
                                    pattern = param.Pattern != null;
                                    step = param.Step != null;
                                }

                                // ---- only for table view (no parameter specified): ----

                                if (paramName == null) {
                                    if (type != "full" && title != null && title.Length > 40) title = String.Format("{0}...", title.Substring(0, 37));
                                    sw.Write("{0,-30}{1,-40}",
                                        name,
                                        title
                                    );
                                    if (type != "full") {
                                        sw.Write(" {0,-2}{1,-2}{2,-2}{3,-2}{4,-2}",
                                            mandatory ? "M" : "-",
                                            options ? "O" : "-",
                                            range ? "R" : "-",
                                            pattern ? "P" : "-",
                                            step ? "S" : "-"
                                        );
                                        
                                    }
                                    sw.WriteLine();
                                }

                                if (type != "full" && paramName != name) continue;

                                // ---- only for "full" display of a single parameter: ----

                                paramFound = true;
                                if (paramName != null) {
                                    sw.WriteLine("- Parameter: {0}", name);
                                    sw.WriteLine("    Description/title: {0}", title);
                                    sw.WriteLine("    Qualified name:    {0}", qualifiedName);
                                }
                                if (options) {
                                    sw.WriteLine("    Options:");
                                    sw.WriteLine("    {0,-22} {1,-40}", "Value", "Label/description");
                                    sw.WriteLine("    {0,-22} {1,-40}", "-----", "-----------------");
                                    foreach (OpenSearchDescriptionUrlParameterOption o in param.Options) {
                                        sw.WriteLine("    - {0,-20} {1,-40}", o.Value, o.Label);
                                    }
                                }
                                if (range) {
                                    string min = (param.MinExclusive != null ? param.MinExclusive : param.MinInclusive != null ? param.MinInclusive : param.Minimum);
                                    string max = (param.MaxExclusive != null ? param.MaxExclusive : param.MaxInclusive != null ? param.MaxInclusive : param.Maximum);
                                    sw.WriteLine("    Range: {0} {2} value {3} {1}", min, max, param.MinExclusive == null ? "<=" : "<", param.MaxExclusive == null ? "<=" : "<");
                                }
                                if (pattern) {
                                    sw.WriteLine("    Pattern: {0}", param.Pattern);
                                }
                                if (step) {
                                    sw.WriteLine("    Step: {0}", param.Step);
                                }
                            }
                            sw.WriteLine();


                            //sw.WriteLine("URL {0} {1} {2}", url.Type, url.Relation, url.Template);
                        }


                        // Display fully qualified names

                        if (type != null && paramName == null) {
                            foreach (OpenSearchDescriptionUrl url in osd.Url) {
                                if (url.Relation == "self") continue;

                                int qmPos = url.Template.IndexOf('?');
                                if (qmPos == -1) continue;

                                string queryString = url.Template.Substring(qmPos + 1);

                                string urlShortType = GetUrlShortType(url.Type);
                                if (type != urlShortType) continue;

                                sw.WriteLine("Valid names for parameters");
                                sw.WriteLine("==========================");
                                sw.WriteLine("{0,-30}{1,-25}{2}", "Prefixed name", "Aliases", "Qualified name");
                                sw.WriteLine("{0,-30}{1,-25}{2}", "-------------", "-------", "--------------");

                                MatchCollection paramMatches = paramRegex.Matches(queryString);
                                foreach (Match paramMatch in paramMatches) {
                                    string name = string.Format("{0}{1}", paramMatch.Groups[3].Value, paramMatch.Groups[5].Value);
                                    string qualifiedName = GetParameterQualifiedName(url, paramMatch.Groups[4].Value, paramMatch.Groups[5].Value);
                                    string alias = string.Join(",", settings.ParametersKeywordsTable.Where(kvp => kvp.Value == qualifiedName).Select(kvp => kvp.Key));
                                    sw.WriteLine("{0,-30}{1,-25}{2}", name, alias, qualifiedName);

                                }

                            }
                            sw.WriteLine();

                        }


                    }
                    sw.Close();
                }
            }

            if (!typeFound && type != "types" && type != "full") log.Error("URL Type not found");
            else if (!paramFound && paramName != null) log.Error("Parameter not found");

        }



        private void ListOpenSearchEngineExtensions() {
            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            StreamWriter sw = new StreamWriter(outputStream);

            sw.WriteLine(string.Format("{0,-30}{1,-40}", "Extension Id", "Mime-Type capability"));
            sw.WriteLine(string.Format("{0,-30}{1,-40}", "============", "===================="));

            foreach (IOpenSearchEngineExtension osee in ose.Extensions.Values) {
                sw.WriteLine(string.Format("{0,-30}{1,-40}", osee.Identifier, osee.DiscoveryContentType));
            }

            sw.Close();
        }



        private void PrintDataModelHelp(DataModel dataModel) {
            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            dataModel.PrintHelp(outputStream, listEncoding);
        }



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
                foreach (string url in baseUrlArg)
                    baseUrl.Add(new Uri(url));
            } catch (UriFormatException) {
                baseUrlArg.Add(string.Format("{0}/{1}", Environment.GetEnvironmentVariable("_CIOP_CQI_LOCATION"), baseUrlArg));
                try {
                    foreach (string url in baseUrlArg)
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
        private Stream InitializeOutputStream(int index = 0) {
            if (outputFilePathArg == null) {
                return Console.OpenStandardOutput();
            } else {
                string path = outputFilePathArg;
                if ( path.Contains("%index%") ){
                    path = path.Replace("%index%", index.ToString());
                } else if (index > 0){
                    path += "-" + index;
                }
                log.DebugFormat("output to {0}", path);
                return new FileStream(path, FileMode.Create);
            }
        }



        private NameValueCollection PrepareQueryParameters(IOpenSearchable entity) {
            NameValueCollection nvc = new NameValueCollection();
            totalResults = 0;

            foreach (string parameter in parameterArgs) {
                Match matchParamDef = Regex.Match(parameter, @"^(.*)=(.*)$");
                // if martch is successful
                if (matchParamDef.Success) {
                    // TODO filter and convert query param
                    if (matchParamDef.Groups[1].Value == "count" || matchParamDef.Groups[1].Value == "{http://a9.com/-/spec/opensearch/1.1/}count") {
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

            NameValueCollection remoteParams = entity.GetOpenSearchParameters(entity.DefaultMimeType);

            if (remoteParams["do"] != null && nvc["do"] == null) {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOWNLOAD_ORIGIN")))
                    nvc["do"] = Environment.GetEnvironmentVariable("DOWNLOAD_ORIGIN");
                else
                    nvc["do"] = Dns.GetHostName();
            }

            dataModel.SetQueryParameters(nvc);

            return nvc;
        }



        private NameValueCollection PrepareDataModelParameters() {
            NameValueCollection nvc = new NameValueCollection();

            foreach (string parameter in dataModelParameterArgs) {
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



        private NameValueCollection ResolveParameters(NameValueCollection nameValueCollection, IOpenSearchable entity) {
            string contentType = entity.DefaultMimeType;

            NameValueCollection osdParam = entity.GetOpenSearchParameters(contentType);
            NameValueCollection osdRevParams = OpenSearchFactory.ReverseTemplateOpenSearchParameters(osdParam);
            NameValueCollection parameters = new NameValueCollection();

            foreach (string key in nameValueCollection.AllKeys) {
                if (osdRevParams[key] != null) {
                    foreach (string id in osdRevParams.GetValues(key))
                        parameters.Set(id, nameValueCollection[key]);
                } else {
                    parameters.Set(key, nameValueCollection[key]);
                }
            }
            return parameters;
        }



        private int CountResults(IOpenSearchResultCollection osr) {
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



        private void OutputResult(IOpenSearchResultCollection osr, Stream outputStream) {
            if (metadataPaths == null) {
                StreamWriter sw = new StreamWriter(outputStream);
                if (osr is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr;
                    foreach (IOpenSearchResultItem item in rc.Items) {
                        SyndicationLink link = item.Links.FirstOrDefault(l => l.RelationshipType == "self");
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

            dataModel.PrintByItem(metadataPaths, outputStream, quotingOutput);

            return;
        }



        public void AdjustIdentifiers(IOpenSearchResultCollection osr, Stream outputStream) {
            Regex badIdentifierRegex = new Regex(@"^(http|ftp)(s)?://");
            Regex goodIdentifierRegex = new Regex(@".*[A-Za-z]+.*\d{8}.*");
            StreamWriter sw = new StreamWriter(outputStream);
            foreach (IOpenSearchResultItem item in osr.Items) {
                sw.WriteLine("ITEM: '{0}' '{1}' '{2}'", item.Id, item.Identifier, item.Title.Text);
                if (badIdentifierRegex.Match(item.Identifier).Success && goodIdentifierRegex.Match(item.Title.Text).Success) {
                    item.Identifier = item.Title.Text;
                }
            }
        }



        public static string GetUrlShortType(string type) {
            if (urlTypes == null) {
                urlTypes = new Dictionary<string, string>();
                urlTypes["application/atom+xml"] = "atom";
                urlTypes["application/atom+xml; profile=http://earth.esa.int/eop/2.1"] = "eop";
                urlTypes["application/json"] = "json";
                urlTypes["application/rdf+xml"] = "rdf";
                urlTypes["application/elasticsearch+json; profile=multi"] = "json-multi";
            }

            if (urlTypes.ContainsKey(type)) return urlTypes[type];

            return type;
        }



        public static string GetParameterDescription(OpenSearchDescriptionUrl url, string key, string fqdn) {

            if (url.Parameters != null) {
                var param = url.Parameters.FirstOrDefault(p => p.Name == key);

                if (param != null && !string.IsNullOrEmpty(param.Title))
                    return param.Title;
            }

            return GetParameterDescriptionByFQDN(fqdn);

        }

        public static string GetParameterDescriptionByFQDN(string fqdn) {

            if (urlParameterLabels == null) {
                urlParameterLabels = new Dictionary<string, string>();
                urlParameterLabels["{http://a9.com/-/spec/opensearch/1.1/}count"] = "Number of search results per page desired";
                urlParameterLabels["{http://a9.com/-/spec/opensearch/1.1/}startPage"] = "Page number of the set of search results desired";
                urlParameterLabels["{http://a9.com/-/spec/opensearch/1.1/}startIndex"] = "Index of the first search result desired";
                urlParameterLabels["{http://a9.com/-/spec/opensearch/1.1/}searchTerms"] = "EO Free Text Search";
                urlParameterLabels["{http://a9.com/-/spec/opensearch/1.1/}language"] = "Desired language of the results";
                urlParameterLabels["{http://purl.org/dc/terms/}modified"] = "Date after which dataset are updated (RFC-3339)";
                urlParameterLabels["{http://www.terradue.com/opensearch}downloadOrigin"] = "A string identifying the download origin (keyword, hostname...) to adapt the enclosure. If the parameter is enclosed between [] (e.g. [terradue]), enclosure will be returned only if there is a enclosure found for this source.";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}accessedFrom"] = "A string identifying the location from which the resource will be accessed. The catalogue shall return the download location in the enclosure atom link according to the parameter value.";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/time/1.0/}start"] = "Start of the temporal interval (RFC-3339)";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/time/1.0/}end"] = "Stop of the temporal interval (RFC-3339)";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/time/1.0/}relation"] = "Temporal relation (possible values are “intersects”, “contains”, “during”, “disjoint”, “equals”)";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/geo/1.0/}box"] = "Rectangular bounding box (minlon,minlat,maxlon,maxlat)";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry"] = "Geometry in WKT";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/geo/1.0/}uid"] = "The identifier of the resource within the search engine context (local reference)";
                urlParameterLabels["{http://purl.org/dc/terms/}identifier"] = "The identifier of the resource within the search engine context (local reference)";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/geo/1.0/}relation"] = "Spatial relation (possible values are “intersects”, “contains”, “disjoint”). The default is intersects.";
                urlParameterLabels["{http://purl.org/dc/terms/}subject"] = "The identifier of a category. Recommended best practice is to use a controlled vocabulary.";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}productType"] = "A string identifying the product type";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}platform"] = "A string with the platform short name";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier"] = "A string with the Platform serial identifier";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}instrument"] = "A string identifying the instrument";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType"] = "A string identifying the sensor type";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection"] = "A string identifying the orbit direction";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitType"] = "A string identifying the orbit type";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}title"] = "A name given to the resource";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}track"] = "A number, set or interval requesting the range of orbit tracks";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}frame"] = "A number, set or interval requesting the range of orbit frames";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier"] = "Swath identifier that corresponds to precise incidence angles for the sensor";
                urlParameterLabels["{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover"] = "A number, set or interval requesting the cloud coverage";
                urlParameterLabels["{http://www.terradue.com/opensearch}landCover"] = "A number, set or interval requesting the land coverage";
                urlParameterLabels["{http://www.terradue.com/opensearch}doubleCheckGeometry"] = "Set to apply a finer geometry filtering";
            }

            if (urlParameterLabels.ContainsKey(fqdn)) return urlParameterLabels[fqdn];

            return fqdn;
        }

        public static string GetParameterQualifiedName(OpenSearchDescriptionUrl url, string namespacePrefix, string localName) {

            XmlQualifiedName ns = url.ExtraNamespace.ToArray().FirstOrDefault(n => n.Name == namespacePrefix);

            if ( ns == null )
                return String.Format("{0}:{1} (namespae URI not retrievable)", namespacePrefix, localName);
            return String.Format("{{{0}}}{1}", ns.Namespace, localName);
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

    }

}
