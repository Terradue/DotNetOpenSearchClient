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

namespace Terradue.Shell.OpenSearch {
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
        private static List<string> baseUrlArg = null;
        private static int timeout = 10000;
        private static int pagination = 20;
        private static int totalResults;
        private static List<string> metadataPaths = null;
        private static List<string> parameterArgs = new List<string>();
        private List<IOpenSearchEngineExtension> openSearchEngineExtensions;
        private static OpenSearchEngine ose;


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

            } catch (Exception e) {
                log.Error(e.Message);
                log.Error(e.StackTrace);
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
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s,ce,ca,p) => true;

            log.Debug("Load OpenSearch Engine.");
            ose = new OpenSearchEngine();

            LoadOpenSearchEngineExtensions(ose);


        }

        void LoadOpenSearchEngineExtensions(OpenSearchEngine ose) {
            if (queryFormatArg == null) {
                ose.LoadPlugins();
            } else {
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                    IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                    if (string.Compare(osee.Identifier, queryFormatArg, true) == 0)
                        ose.RegisterExtension(osee);
                }
            }
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

            NameValueCollection parametersNvc;

            // Find OpenSearch Entity
            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            foreach (var url in baseUrls)
                entities.Add(OpenSearchFactory.FindOpenSearchable(ose, url));

            IOpenSearchable entity;

            if (entities.Count > 1) {
                entity = new MultiGenericOpenSearchable(entities, ose);
            } else {
                entity = entities[0];
            }



            NameValueCollection parameters = PrepareQueryParameters();
            string startIndex = parameters.Get("startIndex");
            int index = 1;
            if (startIndex != null) {
                index = int.Parse(startIndex);
            }

            IOpenSearchResult osr = null;

            while (totalResults > 0) {

                parametersNvc = ResolveParameters(parameters, entity);

                // Perform the query
                osr = QueryOpenSearch(ose, entity, parametersNvc);

                // Transform the result
                OutputResult(osr, outputStream);

                int count = CountResults(osr);
                if (count == 0)
                    break;

                totalResults -= count;
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
                            timeout = int.Parse(args[++argpos]);
                        } else
                            return false;
                        break;
                    case "--pagination": 
                        if (argpos < args.Length - 1) {
                            pagination = int.Parse(args[++argpos]);
                        } else
                            return false;
                        break;
                    case "--list-osee": 
                        listOsee = true;
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
            Console.Error.WriteLine("                         By default, the client is automatic and uses the best format.");
            Console.Error.WriteLine(" -to/--time-out <file>   Specify query timeout (millisecond)");
            Console.Error.WriteLine(" --pagination            Specify the pagination number for search loops. Default: 20");
            Console.Error.WriteLine(" --list-osee             List the OpenSearch Engine Extensions");
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

            return nvc;
        }

        private IOpenSearchResult QueryOpenSearch(OpenSearchEngine ose, IOpenSearchable entity, NameValueCollection parameters) {

            IOpenSearchResult osr = ose.Query(entity, parameters);

            return osr;

        }

        void ListFormat(Stream outputStream) {



        }

        void OutputResult(IOpenSearchResult osr, Stream outputStream) {

            StreamWriter sw = new StreamWriter(outputStream);

            if (metadataPaths == null) {
                if (osr.Result is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                    foreach ( var item in rc.Items ){
                        var link = item.Links.FirstOrDefault(l => l.RelationshipType == "self");
                        if ( link != null )
                            sw.WriteLine(link.Uri.ToString());
                        else
                            sw.WriteLine(item.Id);
                    }
                    sw.Flush();
                }

                return;
            }

            if (metadataPaths[0] == "enclosure") {
                if (osr.Result is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                    rc.Items.FirstOrDefault(i => {
                        i.Links.FirstOrDefault(l => {
                            if (l.RelationshipType == "enclosure") {
                                sw.WriteLine(l.Uri.ToString());
                                return true;
                            } else
                                return false;
                        });
                        return false;
                    });
                    sw.Flush();
                }

                return;
            }

            if (metadataPaths[0] == "{}") {
                if (osr.Result is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                    rc.SerializeToStream(outputStream);
                }

                if (osr.Result is SyndicationFeed) {
                    SyndicationFeed feed = (SyndicationFeed)osr.Result;
                    Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
                    XmlWriter xw = XmlWriter.Create(outputStream);
                    atomFormatter.WriteTo(xw);
                    xw.Flush();
                }
                return;
            }

            if (osr.Result is IOpenSearchResultCollection) {
                IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                foreach (IOpenSearchResultItem item in rc.Items) {
                    string sep = "";
                    XmlDocument doc = new XmlDocument();
                    doc.Load(item.ElementExtensions.GetReaderAtExtensionWrapper());
                    foreach (var path in metadataPaths) {
                        XmlNamespaceManager xnsm = new XmlNamespaceManager(doc.NameTable);
                        xnsm.AddNamespace("dclite4g", "http://xmlns.com/2008/dclite4g#");
                        xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
                        xnsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                        sw.Write(sep);
                        XmlNode noder = doc.SelectSingleNode(path, xnsm);
                        if (noder != null) {
                            if (noder.NodeType == XmlNodeType.Attribute)
                                sw.Write(noder.Value);
                            else
                                sw.Write(noder.InnerText);
                        }
                        sep = "\t";
                    }
                    sw.WriteLine();
                }
                sw.Flush();
            }

            return;

        }

        void SerializeXmlDocument(XmlDocument xmlDocument, Stream outputStream) {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlDocument));
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

        int CountResults(IOpenSearchResult osr) {
            if (osr.Result is IOpenSearchResultCollection) {
                IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                return rc.Items.Count();
            }

            if (osr.Result is SyndicationFeed) {
                SyndicationFeed feed = (SyndicationFeed)osr.Result;
                return feed.Items.Count();
            }

            return 0;
        }

    }
}

