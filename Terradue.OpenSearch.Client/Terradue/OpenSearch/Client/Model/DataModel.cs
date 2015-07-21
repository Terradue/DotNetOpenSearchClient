using System;
using Mono.Addins;
using System.IO;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;

namespace Terradue.OpenSearch.Client.Model {
    public class DataModel {
        
        IOpenSearchClientDataModelExtension modelExtension;

        
        private DataModel(IOpenSearchClientDataModelExtension modelExtension) {
            this.modelExtension = modelExtension;
            
        }


        public static DataModel CreateFromArgs(string queryModelArg, IOpenSearchResultCollection osr, NameValueCollection parameters) {

            IOpenSearchClientDataModelExtension modelExtension = DataModel.FindPluginByName(queryModelArg);
            if (modelExtension == null)
                throw new NotSupportedException(string.Format("No data model with name \"{0}\" found", queryModelArg));

            modelExtension.InitModelExtension(parameters);
            modelExtension.LoadOpenSearchResultCollection(osr);
            modelExtension.ApplyParameters();

            return new DataModel(modelExtension);

        }

        public static IOpenSearchClientDataModelExtension FindPluginByName(string name) {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

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
    }
}

