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



namespace Terradue.OpenSearch.Data.Publisher {
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    public class OpenSearchDataPublisher {
        private static readonly ILog log = LogManager.GetLogger(typeof(OpenSearchDataPublisher));
        private static Version version = Assembly.GetEntryAssembly().GetName().Version;
        private static bool verbose;
        private static bool listOsee;
        private static string action = null;
        private static string outputFilePathArg = null;
        private static string outputDirPathArg = null;
        private static string queryFormatArg = null;
        private static string queryModelArg = "GeoTime";
        private static string baseUrlArg = null;
        private static uint timeout = 20000;
        private static int pagination = 20;
        private static int totalResults;
        private static List<List<string>> metadataPaths = null;
        private static List<string> parameterArgs = new List<string>();
        private static List<string> dataModelParameterArgs = new List<string>();
        private static OpenSearchEngine ose;
        private static OpenSearchMemoryCache searchCache;
        private static NameValueCollection dataModelParameters;
        private static DataModel dataModel;
        private static NetworkCredential netCreds;


        public static void Main(string[] args) {

            if (!GetArgs(args)) {
                if (action == null) PrintUsage();
                else {
                    switch (action) {
                        case "add":
                            PrintUsage_Add();
                            break;
                        case "edit":
                            PrintUsage_Edit();
                            break;
                        case "delete":
                            PrintUsage_Delete();
                            break;
                        default:
                            PrintUsage();
                            break;
                    }
                }
                Environment.ExitCode = 1;
                return;
            }

            OpenSearchDataPublisher client = null;
            try {
                client = new OpenSearchDataPublisher();

                client.Initialize();

                if (baseUrlArg != null)
                    client.ProcessQuery();

                if (listOsee == true)
                    client.ListOpenSearchEngineExtensions();

                if (!string.IsNullOrEmpty(queryModelArg) && baseUrlArg == null) {
                    client.PrintDataModelHelp(DataModel.CreateFromArgs(queryModelArg, new NameValueCollection()));
                }

            } catch (Exception e) {
                Console.Error.WriteLine(string.Format("{0} : {1} {2}", e.Source, e.Message, e.HelpLink));
                if (verbose) {
                    Console.Error.WriteLine(e.StackTrace);
                    if (e.InnerException != null) {
                        Console.Error.WriteLine(string.Format("{0} : {1} {2}", e.InnerException.Source, e.InnerException.Message, e.InnerException.HelpLink));
                        Console.Error.WriteLine(e.InnerException.StackTrace);
                    }
                }
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
            Stream outputStream = CreateOutputStream();

            StreamWriter sw = new StreamWriter(outputStream);

            sw.WriteLine(string.Format("{0,-30}{1,-40}", "Extension Id", "Mime-Type capability"));
            sw.WriteLine(string.Format("{0,-30}{1,-40}", "============", "===================="));

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                sw.WriteLine(string.Format("{0,-30}{1,-40}", osee.Identifier, osee.DiscoveryContentType));
            }

            sw.WriteLine();

            sw.Close();

        }

        private void PrintDataModelHelp(DataModel dataModel) {


            // Initialize the output stream
            Stream outputStream = CreateOutputStream();

            dataModel.PrintHelp(outputStream);
        }

        private void ListOutputFormat() {

            // Initialize the output stream
            Stream outputStream = CreateOutputStream();

            ListFormat(outputStream);


        }

         private void ProcessQuery() {

            if (verbose) LogArgs();

            // Base OpenSearch URL
            List<Uri> baseUrls = InitializeUrl();

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
                    if(entity == null) entity = dataModel.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds);
                    index = entity.GetOpenSearchDescription().DefaultUrl.IndexOffset;
                    break;
                } catch (Exception e) {
                    log.Debug("GetOpenSearchDescription retry - " + retry);
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
                        log.Debug("QueryOpenSearch retry - " + retry);
                        if (retry == 0)
                            throw e;
                        retry--;
                        InitCache();
                    }
                }

                //Do the actual editing
                // Initialize the output stream
                ProcessEdit(osr);

                int count = CountResults(osr);
                if (count == 0)
                    break;
                
                //output
                Stream outputStream = CreateOutputStream(index, index + count);
                StreamWriter sw = new StreamWriter(outputStream);
                sw.Write(osr.SerializeToString());
                sw.WriteLine();
                sw.Flush();
                outputStream.Close();

                //TODO: check this
                if (osr.TotalResults < totalResults)
                    break;
                
                totalResults -= count;
                int paramCount;
                if(Int32.TryParse(parameters.Get("count"), out paramCount) && totalResults < paramCount){
                    parameters.Set("count", "" + totalResults);
                }
                index += count;

                parameters.Set("startIndex", "" + index);

            }

        }

        public void LogArgs(){
            if (outputFilePathArg != null) log.Debug("output file name: " + outputFilePathArg);
            if (outputDirPathArg != null) log.Debug("output directory: " + outputDirPathArg);
            if (queryFormatArg != null) log.Debug("format: " + queryFormatArg);
            if (netCreds != null) log.Debug("auth: " + netCreds.UserName + ":" + netCreds.Password);
            if (parameterArgs != null) foreach(var p in parameterArgs) log.Debug("query parameter: " + p);
            log.Debug("timeout: " + timeout);
            log.Debug("pagination: " + pagination);
            if (dataModelParameterArgs != null) foreach(var p in dataModelParameterArgs) log.Debug("datamodel parameter: " + p);
            if (queryModelArg != null) log.Debug("model: " + queryModelArg);
            if (baseUrlArg != null) log.Debug("query url: " + baseUrlArg);
            if (metadataPaths != null) foreach(var m in metadataPaths) log.Debug("metadata: " + string.Join(" ", m));
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
                    case "-d": 
                    case "--dir": 
                        if (argpos < args.Length - 1) {
                            outputDirPathArg = args[++argpos];
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
                        if (action == null) {
                            action = args[argpos];
                            switch (action) {
                                case "add":
                                case "edit":
                                case "delete":
                                    break;
                                default:
                                    return false;
                            }
                            break;
                        }
                        if (baseUrlArg == null) {
                            baseUrlArg = args[argpos];
                            break;
                        }
                        if (metadataPaths == null) {
                            metadataPaths = new List<List<string>>();
                            List<string> metadata = new List<string>();
                            while (argpos < args.Length) {
                                if (args[argpos] == ",") {
                                    metadataPaths.Add(metadata);
                                    metadata = new List<string>();
                                } else {
                                    metadata.Add(args[argpos]);
                                }
                                argpos++;
                            }
                            metadataPaths.Add(metadata);
                            break;
                        }
                        break;
                }
                argpos++;
            }

            if (action == null) {
                Console.Error.WriteLine("ERROR: no action set");
                Console.Error.WriteLine();
                return false;
            }

            return true;
        }
        //---------------------------------------------------------------------------------------------------------------------
        public static void PrintUsage() {
            Console.Error.WriteLine(String.Format("{0} (v{1}) - Data Publisher - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));

            Console.Error.WriteLine("Usage: " + Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " [action] [options]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Action:");
            Console.Error.WriteLine(" add      Add items into the catalogue");
            Console.Error.WriteLine(" edit     Edit the items resulting from the query on the catalogue");
            Console.Error.WriteLine(" delete   Delete items resulting from the query from the catalogue");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine(" -h/--help  Prints the usage.");
            Console.Error.WriteLine();
        }

        public static void PrintUsage_Add(){
            Console.Error.WriteLine(String.Format("{0} (v{1}) - Data Publisher - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));
            Console.Error.WriteLine("Sorry but this function is not yet implemented.");
            Console.Error.WriteLine();
        }

        public static void PrintUsage_Edit(){
            Console.Error.WriteLine(String.Format("{0} (v{1}) - Data Publisher - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));

            Console.Error.WriteLine("Usage: " + Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " edit [options...] [url] [metadataPath1 metadataParameters1,metadatapath2 parameters2,...]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            Console.Error.WriteLine(" -a/--auth <auth>        Set Credentials to be used (format must be username:password).");
            Console.Error.WriteLine(" -d/--dir <directory>    Write outputs to the directory <directory> instead of current directory (if <file> is set).");
            Console.Error.WriteLine("                         Default directory is the current directory.");
            Console.Error.WriteLine(" -f/--format <format>    Specify the format of the query. Format available can be listed with --list-osee.");
            Console.Error.WriteLine("                         By default, the client is automatic and uses the default or the first format.");
            Console.Error.WriteLine(" -h/--help               Prints the usage.");
            Console.Error.WriteLine(" --list-osee             List the OpenSearch Engine Extensions including the list of available metadata paths.");
            Console.Error.WriteLine(" -m/--model <format>     Specify the data model of the results for the query. Data model give access to specific " +
                                    "metadata extractors or transformers. By default the \"GeoTime\" model is used. Used without urls, it lists the metadata options.");
            Console.Error.WriteLine(" -o/--output <file>      Write output to <file> instead of stdout");
            Console.Error.WriteLine("                         Files are split by pagination and named with the indexes.");
            Console.Error.WriteLine("                         Default filename (if <directory> is set) is feed_startindex_endindex.xml.");
            Console.Error.WriteLine(" -p/--parameter <param>  Specify a parameter for the query");
            Console.Error.WriteLine(" --pagination            Specify the pagination number for search loops. Default: 20");
            Console.Error.WriteLine(" -to/--time-out <file>   Specify query timeout (millisecond)");
            Console.Error.WriteLine(" -v/--verbose            Make the operation more talkative");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Metadatapath:");
            Console.Error.WriteLine(" possible values can be found using --list-osee");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Parameters:");
            Console.Error.WriteLine(" -a <value>              Add a metadata with the value <value>");
            Console.Error.WriteLine(" -d                      Remove all metadata");
            Console.Error.WriteLine(" -d <value>              Remove metadata having <value> as value");
            Console.Error.WriteLine(" -r <value> <template>   Replace metadata having <value> as value with the new <template> value");
            Console.Error.WriteLine(" -r <template>           Replace metadata with the new <template> value");
            Console.Error.WriteLine(" note: <template> can use $<metadata> to get value from other elements, e.g $<identifier> to use the value of the identifier in the new metadata value");
            Console.Error.WriteLine();
            Console.Error.WriteLine("example:");
            Console.Error.WriteLine(Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " edit \"https://data2.terradue.com/eop/...\" enclosure -r \"https://google.com\" \"https://yahoo.com\", \"identifier\" -r \"new_id\" ");
            Console.Error.WriteLine(Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " edit \"https://data2.terradue.com/eop/...\" enclosure -d -a \"https://google.com\" \"https://yahoo.com\", \"identifier\" -r \"new_id\" ");
            Console.Error.WriteLine();
        }

        public static void PrintUsage_Delete(){
            Console.Error.WriteLine(String.Format("{0} (v{1}) - OpenSearch Data Publisher - (c) Terradue S.r.l.", Path.GetFileName(Environment.GetCommandLineArgs()[0]), version));
            Console.Error.WriteLine("Sorry but this function is not yet implemented.");
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
                baseUrlArg = Environment.GetEnvironmentVariable("_CIOP_CQI_LOCATION");
            }

            try {
                baseUrl.Add(new Uri(baseUrlArg));
            } catch (UriFormatException) {
                baseUrlArg = string.Format("{0}/{1}", Environment.GetEnvironmentVariable("_CIOP_CQI_LOCATION"), baseUrlArg);
                try {
                    baseUrl.Add(new Uri(baseUrlArg));
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
        private Stream CreateOutputStream() {
            if (outputDirPathArg == null && outputFilePathArg == null) {
                return Console.OpenStandardOutput();
            } else {
                if (outputFilePathArg == null) outputFilePathArg = "stream.xml";
                var filename = (outputDirPathArg == null ? "" : outputDirPathArg + "/") + outputFilePathArg;
                return new FileStream(filename, FileMode.Create);
            }
        }

        private Stream CreateOutputStream(int indexstart, int indexstop) {
            if (outputDirPathArg == null && outputFilePathArg == null) {
                return Console.OpenStandardOutput();
            } else {
                if (outputFilePathArg == null) outputFilePathArg = "feed.xml";
                var filenamebase = outputFilePathArg.Substring(0, outputFilePathArg.LastIndexOf("."));
                var extension = outputFilePathArg.Substring(outputFilePathArg.LastIndexOf("."));
                var filename = (outputDirPathArg == null ? "" : outputDirPathArg + "/") + filenamebase + "_" + indexstart + "-" + indexstop + extension;
                return new FileStream(filename, FileMode.Create);
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
                // if match is successful
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

        void ProcessEdit(IOpenSearchResultCollection osr) {

            dataModel.LoadResults(osr);
            dataModel.EditItems(metadataPaths);
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

