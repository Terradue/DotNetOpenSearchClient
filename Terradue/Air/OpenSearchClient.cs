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
using System.ServiceModel.Syndication;
using Mono.Addins;
using Terradue.OpenSearch.Result;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Core;
using log4net.Appender;
using Terradue.GeoJson.Feed;

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
        private static string outputFormatArg = "Atom";
        private static List<string> baseUrlArg = null;
        private static int timeout = 10000;
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
            AddinManager.Initialize();
            AddinManager.Registry.Update(null);
            ose = new OpenSearchEngine();
            ose.LoadPlugins();
        }

        private void ListOpenSearchEngineExtensions() {
            // Config log
            ConfigureLog();

            // Initialize the output stream
            Stream outputStream = InitializeOutputStream();

            StreamWriter sw = new StreamWriter(outputStream);

            sw.WriteLine(string.Format("{0,-30}{1,-25}{2,-25}", "Extension Name", "Input Mime-Type", "Output Format"));
            sw.WriteLine(string.Format("{0,-30}{1,-25}{2,-25}", "==============", "===============", "============="));

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                foreach (string input in osee.GetInputFormatTransformPath()) {
                    sw.WriteLine(string.Format("{0,-30}{1,-25}{2,-25}", osee.Name, input, osee.GetTransformName()));
                }
            }

            sw.Close();

        }

        private void ListOutputFormat() {
            // Config log
            ConfigureLog();

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

            parametersNvc = ResolveParameters(PrepareQueryParameters(), entity, outputFormatArg);
             
            // Perform the query
            IOpenSearchResult osr = QueryOpenSearch(ose, entity, parametersNvc, outputFormatArg);



            // Transform the result
            OutputResult(osr, outputStream);


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
                            outputFormatArg = args[++argpos];
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
            Console.Error.WriteLine(" -f/--format <format>    Specify the output format of the query. Format available can be listed with --list-osee.");
            Console.Error.WriteLine("                         Default: Atom");
            Console.Error.WriteLine(" -to/--time-out <file>   Specify query timeout (millisecond)");
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

            foreach (var parameter in parameterArgs) {
                Match matchParamDef = Regex.Match(parameter, @"^(.*)=(.*)$");
                // if martch is successful
                if (matchParamDef.Success) {
                    // TODO filter and convert query param
                    nvc.Add(matchParamDef.Groups[1].Value, matchParamDef.Groups[2].Value);
                }
               
            }

            return nvc;
        }

        private IOpenSearchResult QueryOpenSearch(OpenSearchEngine ose, IOpenSearchable entity, NameValueCollection parameters, string type) {

            IOpenSearchResult osr = ose.Query(entity, parameters, type);

            return osr;

        }

        void ListFormat(Stream outputStream) {



        }

        void OutputResult(IOpenSearchResult osr, Stream outputStream) {



            if (metadataPaths == null) {
                StreamWriter sw = new StreamWriter(outputStream);
                if (osr.Result is IOpenSearchResultCollection) {
                    IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                    rc.Items.FirstOrDefault(i => {
                        i.Links.FirstOrDefault(l => {
                            if (l.RelationshipType == "self") {
                                sw.WriteLine(l.Uri.ToString());
                                return true;
                            } else
                                return false;
                        });
                        return false;
                    });
                    sw.Close();
                }

                if (osr.Result is SyndicationFeed) {
                    SyndicationFeed feed = (SyndicationFeed)osr.Result;
                    foreach (SyndicationItem item in feed.Items) {
                        sw.WriteLine(item.Id);
                    }
                    sw.Close();
                }

            } else {

                if (metadataPaths[0] == "{}") {
                    if (osr.Result is IOpenSearchResultCollection) {
                        IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                        rc.Serialize(outputStream);
                    }

                    if (osr.Result is SyndicationFeed) {
                        SyndicationFeed feed = (SyndicationFeed)osr.Result;
                        Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
                        XmlWriter xw = XmlWriter.Create(outputStream);
                        atomFormatter.WriteTo(xw);
                        xw.Close();
                    }
                } else {
                    StreamWriter sw = new StreamWriter(outputStream);

                    if (osr.Result is IOpenSearchResultCollection) {
                        IOpenSearchResultCollection rc = (IOpenSearchResultCollection)osr.Result;
                        foreach (IOpenSearchResultItem item in rc.Items) {
                            string sep = "";
                            foreach (var path in metadataPaths) {
                                foreach (XmlNode node in item.ElementExtensions) {
                                    XmlNamespaceManager xnsm = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                                    xnsm.AddNamespace("dclite4g", "http://xmlns.com/2008/dclite4g#");
                                    xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
                                    xnsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                                    sw.Write(sep);
                                    XmlNode noder = node.ParentNode.SelectSingleNode(path, xnsm);
                                    if (noder != null) {
                                        if (noder.NodeType == XmlNodeType.Attribute)
                                            sw.Write(noder.Value);
                                        else
                                            sw.Write(noder.InnerText);
                                    }
                                    sep = "\t";
                                }
                            }
                            sw.WriteLine();
                        }
                        sw.Close();
                    }

                    if (osr.Result is SyndicationFeed) {
                        SyndicationFeed feed = (SyndicationFeed)osr.Result;
                        foreach (SyndicationItem item in feed.Items) {
                            foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                                var reader = ext.GetReader();
                                XmlDocument doc = new XmlDocument();
                                doc.Load(reader);
                                XmlNamespaceManager xnsm = new XmlNamespaceManager(doc.NameTable);
                                string sep = "";
                                foreach (var path in metadataPaths) {
                                    sw.Write(sep);
                                    XmlNode node = doc.SelectSingleNode(path, xnsm);
                                    if (node.NodeType == XmlNodeType.Attribute)
                                        sw.Write(node.Value);
                                    else
                                        sw.Write(node.InnerText);
                                    sep = "\t";
                                }
                            }
                        }
                        sw.Close();
                    }
                }

            }

        }

        void SerializeXmlDocument(XmlDocument xmlDocument, Stream outputStream) {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlDocument));
            serializer.Serialize(outputStream, xmlDocument);
        }

        NameValueCollection ResolveParameters(NameValueCollection nameValueCollection, IOpenSearchable entity, string resultName) {

            Type resultType = ose.GetTypeByExtensionName(resultName);
            string contentType = entity.GetTransformFunction(ose, resultType).Item1;

            NameValueCollection osdParam = entity.GetOpenSearchParameters(contentType);
            NameValueCollection osdRevParams = OpenSearchFactory.ReverseTemplateOpenSearchParameters(osdParam);
            NameValueCollection parameters = new NameValueCollection();

            foreach (string key in nameValueCollection.AllKeys) {

                if (osdRevParams[key] != null) {
                    parameters.Add(osdRevParams[key], nameValueCollection[key]);
                } else {
                    parameters.Add(key, nameValueCollection[key]);
                }
            }
            return parameters;

        }
    }
}

