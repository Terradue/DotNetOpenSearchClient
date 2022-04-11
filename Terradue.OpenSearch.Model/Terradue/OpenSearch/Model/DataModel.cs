using System;
using System.IO;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using System.Net;
using log4net;
using System.Reflection;
using System.Linq;

namespace Terradue.OpenSearch.Model {
    public class DataModel {

        IOpenSearchClientDataModelExtension modelExtension;

        private static readonly ILog log = LogManager.GetLogger(typeof(DataModel));


        private DataModel(IOpenSearchClientDataModelExtension modelExtension) {
            this.modelExtension = modelExtension;

        }


        public static DataModel CreateFromArgs(string queryModelArg, NameValueCollection parameters) {

            log.DebugFormat("loading model extension for {0}", queryModelArg);

            IOpenSearchClientDataModelExtension modelExtension = DataModel.FindPluginByName(queryModelArg);
            if (modelExtension == null)
                throw new NotSupportedException(string.Format("No data model with name \"{0}\" found", queryModelArg));

            modelExtension.InitModelExtension(parameters);

            return new DataModel(modelExtension);

        }

        public static IOpenSearchClientDataModelExtension FindPluginByName(string name) {

            List<Assembly> allAssemblies = new List<Assembly>();
            string dirpath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);

            log.Debug(string.Format("Scan {0} for OpenSearch Client plugins", dirpath));
            foreach (string dll in Directory.GetFiles(dirpath, "Terradue.*.dll"))
                allAssemblies.Add(Assembly.LoadFile(dll));

            foreach (var assembly in allAssemblies) {
                foreach (var cl in assembly.GetTypes()) {
                    var dnAttributes = cl.GetCustomAttributes(typeof(OpenSearchClientExtensionAttribute), true);
                    foreach (OpenSearchClientExtensionAttribute dnAttribute in dnAttributes) {
                        log.Debug(String.Format("Found {0} [{1}] in class {2}", dnAttribute.NodeName, dnAttribute.Description, cl.Name));
                        try {
                            IOpenSearchClientDataModelExtension modelExtension = (IOpenSearchClientDataModelExtension)Activator.CreateInstance(cl);
                            if (modelExtension.Name == name) {
                                return modelExtension;
                            }
                        } catch (Exception e) {
                            log.Warn(string.Format("Impossible to load {0} : {1}. Skipping extension", cl.FullName, e.Message));
                        }
                    }
                }
            }

            return null;

        }

        public void PrintHelp(System.IO.Stream outputStream, bool emptyLine = false) {

            StreamWriter sw = new StreamWriter(outputStream);

            if (emptyLine) sw.WriteLine();

            sw.WriteLine(string.Format("Data model: {0}", modelExtension.Name));
            sw.WriteLine(string.Format("============" + new string('=', modelExtension.Name.Length)));

            sw.WriteLine(modelExtension.Description);
            sw.WriteLine();

            sw.WriteLine(string.Format("{0,-30}{1,-40}", "Metadata Id", "Metadata capability"));
            sw.WriteLine(string.Format("{0,-30}{1,-40}", "===========", "==================="));
            foreach (string metadataKey in modelExtension.MetadataExtractors.Keys) {
                sw.WriteLine(string.Format("{0,-30}{1,-40}", metadataKey, modelExtension.MetadataExtractors[metadataKey].Description));
            }

            sw.Flush();


        }

        public void PrintCollection(System.IO.Stream outputStream) {

            modelExtension.GetCollection().SerializeToStream(outputStream);

            StreamWriter sw = new StreamWriter(outputStream);

            sw.WriteLine();
            sw.Flush();

        }

        public void PrintByItem(List<string> metadataPaths, System.IO.Stream outputStream, bool quoting=false) {

            StreamWriter sw = new StreamWriter(outputStream);

            List<List<String>> metadatas = new List<List<string>>();
            bool[] toQuote = new bool[metadataPaths.Count()];

            foreach (var item in modelExtension.GetCollection().Items) {

                var metadata = modelExtension.GetMetadataForItem(metadataPaths, item);

                for (int i = 0; i < metadata.Count; i++) {
                    if (!string.IsNullOrEmpty(metadata[i]) && metadata[i].Contains(",")) {
                        toQuote[i] |= true;
                    }
                }

                metadatas.Add(metadata);

            }

            foreach (var metadata in metadatas) {

                for (int i = 0; i < metadata.Count; i++) {
                    if(toQuote[i] && quoting)
                        metadata[i] = '"' + metadata[i].Replace('"', '\"') + '"';
                }

                sw.Write(string.Join(",", metadata));

                sw.WriteLine();

                sw.Flush();

            }

        }

        public void LoadResults(IOpenSearchResultCollection osr) {
            modelExtension.LoadOpenSearchResultCollection(osr);
            modelExtension.ApplyParameters();
        }

        public void SetQueryParameters(NameValueCollection nvc) {
            modelExtension.SetQueryParameters(nvc);
        }

        public IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings) {
            return modelExtension.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds, settings);
        }
    }
}

