using System;
using Mono.Addins;
using System.IO;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using System.Net;
using log4net;

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

            AddinManager.Initialize();
            AddinManager.Registry.Update();

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchClientDataModelExtension))) {
                IOpenSearchClientDataModelExtension modelExtension = (IOpenSearchClientDataModelExtension)node.CreateInstance();
                if (modelExtension.Name == name) {
                    return modelExtension;
                }
            }

            return null;

        }

        public void PrintHelp(System.IO.Stream outputStream) {

            StreamWriter sw = new StreamWriter(outputStream);

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

        public void PrintByItem(List<string> metadataPaths, System.IO.Stream outputStream) {

            StreamWriter sw = new StreamWriter(outputStream);

            foreach ( var item in modelExtension.GetCollection().Items ){
                
                var metadata = modelExtension.GetMetadataForItem(metadataPaths, item);

                sw.Write(string.Join(",", metadata));

                sw.WriteLine();

                sw.Flush();

            }

        }

        public void LoadResults(IOpenSearchResultCollection osr) {
            modelExtension.LoadOpenSearchResultCollection(osr);
            modelExtension.ApplyParameters();
        }

        public void SetQueryParameters(NameValueCollection nvc){
            modelExtension.SetQueryParameters(nvc);
        }

        public IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, bool lax){
            return modelExtension.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds, lax);
        }
    }
}

