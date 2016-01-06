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
using Mono.Addins;
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



namespace Terradue.OpenSearch.Client {
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    public class OpenSearchClient {
        private static readonly ILog log = LogManager.GetLogger(typeof(OpenSearchClient));
        private static Version version = Assembly.GetEntryAssembly().GetName().Version;
        private static bool verbose;
        private static bool listOsee;
        private static string outputFilePathArg = null;
        private static string queryFormatArg = null;
        private static string queryModelArg = "GeoTime";
        private static List<string> baseUrlArg = null;
        private static uint timeout = 20000;
        private static int pagination = 20;
        private static int totalResults;
        private static List<string> metadataPaths = null;
        private static List<string> parameterArgs = new List<string>();
        private static List<string> dataModelParameterArgs = new List<string>();
        private static OpenSearchEngine ose;
        private static OpenSearchMemoryCache searchCache;
        private static NameValueCollection dataModelParameters;
        private static DataModel dataModel;
        private static NetworkCredential netCreds;


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
                foreach (var e in ae.InnerExceptions) {
                    Console.Error.WriteLine(string.Format("{0} : {1} {2}", e.ToString(), e.Message, e.HelpLink));
                    if (verbose)
                        Console.Error.WriteLine(e.StackTrace);
                }
                Environment.ExitCode = 1;
                return;
            
            } catch (Exception e) {
                Console.Error.WriteLine(string.Format("{0} : {1} {2}", e.Source, e.Message, e.HelpLink));
                if (verbose)
                    Console.Error.WriteLine(e.StackTrace);
                Environment.ExitCode = 1;
                return;
            }

        }

        private void Initialize() {

            // Config log
            ConfigureLog();

            log.Debug("Initialize Addins registry.");

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            log.Debug("Initialize SSL verification.");
            // Override automatic validation of SSL server certificates.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, ce, ca, p) => true;

            log.Debug("Load OpenSearch Engine.");
            ose = new OpenSearchEngine();

            LoadOpenSearchEngineExtensions(ose);

            InitCache();

        }

        private void InitCache(){
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

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                sw.WriteLine(string.Format("{0,-30}{1,-40}", osee.Identifier, osee.DiscoveryContentType));
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

        private void ProcessQuery() {

            // Config log
            ConfigureLog();

            // Base OpenSearch URL
            List<Uri> baseUrls = InitializeUrl();

            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            // Init data Model
            dataModelParameters = PrepareDataModelParameters();
            dataModel = DataModel.CreateFromArgs(queryModelArg, dataModelParameters);

            NameValueCollection parametersNvc;

            // Find OpenSearch Entity
            IOpenSearchable entity = null;
            int retry = 5;
            int index = 1;
            while (retry >= 0) {
                // Perform the query
                try {
                    entity = dataModel.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds);
                    index = entity.GetOpenSearchDescription().DefaultUrl.IndexOffset;
                    break;
                } catch (Exception e) {
                    if (retry == 0)
                        throw e;
                    retry--;
                    InitCache();
                }
            }

            NameValueCollection parameters = PrepareQueryParameters();
            string startIndex = parameters.Get("startIndex");
            if (startIndex != null) {
                index = int.Parse(startIndex);
            }

            IOpenSearchResultCollection osr = null;

            while (totalResults > 0) {

                parametersNvc = ResolveParameters(parameters, entity);

                retry = 5;
                while (retry >= 0) {
                    // Perform the query
                    try {
                        osr = QueryOpenSearch(ose, entity, parametersNvc);
                        break;
                    } catch (Exception e) {
                        if (retry == 0)
                            throw e;
                        retry--;
                        InitCache();
                    }
                }

                // Transform the result
                OutputResult(osr, outputStream);

                int count = CountResults(osr);
                if (count == 0)
                    break;

                totalResults -= count;
                int paramCount;
                if(Int32.TryParse(parameters.Get("count"), out paramCount) && totalResults < paramCount){
                    parameters.Set("count", "" + totalResults);
                }
                index += count;
                parameters.Set("startIndex", "" + index);

            }

            outputStream.Close();


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
                            string[] creds = args[++argpos].Split(':');
                            netCreds = new NetworkCredential(creds[0], creds[1]);
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
                    default: 
                        if (baseUrlArg == null) {
                            baseUrlArg = args[argpos].Split(',').ToList();
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
            Console.Error.WriteLine("Usage: " + Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " [options...] [url1,url2,url3,...] [metadatapath1,metadatapath2,...]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");

            Console.Error.WriteLine(" -p/--parameter <param>  Specify a parameter for the query");
            Console.Error.WriteLine(" -o/--output <file>      Write output to <file> instead of stdout");
            Console.Error.WriteLine(" -f/--format <format>    Specify the format of the query. Format available can be listed with --list-osee.");
            Console.Error.WriteLine("                         By default, the client is automatic and uses the default or the first format.");
            Console.Error.WriteLine(" -to/--time-out <file>   Specify query timeout (millisecond)");
            Console.Error.WriteLine(" --pagination            Specify the pagination number for search loops. Default: 20");
            Console.Error.WriteLine(" --list-osee             List the OpenSearch Engine Extensions");
            Console.Error.WriteLine(" -m/--model <format>     Specify the data model of the results for the query. Data model give access to specific" +
            "metadata extractors or transformers. By default the \"GeoTime\" model is used. Used without urls, it lists the metadata options");
            Console.Error.WriteLine(" -v/--verbose            Make the operation more talkative");
            Console.Error.WriteLine();
        }

        void ConfigureLog() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "[%-5level] %message%newline";
            patternLayout.ActivateOptions();

            ConsoleAppender roller = new ConsoleAppender();
            roller.Layout = patternLayout;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            hierarchy.Root.Level = Level.Info;
            if (verbose == true) {
                hierarchy.Root.Level = Level.Debug;
            }
            hierarchy.Configured = true;
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

                    throw new UriFormatException("The format of the URI could not be determined. Please check ${_CIOP_CQI_LOCATION}");
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

        NameValueCollection PrepareQueryParameters() {

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

        void ListFormat(Stream outputStream) {



        }

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

