using System;
using Mono.Addins;
using System.IO;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using System.Net;

namespace Terradue.OpenSearch.Model {
    public class DataModel {

        private static string parameterElementTag = "$";
        IOpenSearchClientDataModelExtension modelExtension;

        
        private DataModel(IOpenSearchClientDataModelExtension modelExtension) {
            this.modelExtension = modelExtension;
            
        }


        public static DataModel CreateFromArgs(string queryModelArg, NameValueCollection parameters) {



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

        public void EditItems(string metadata, string replacement){
            foreach (var item in modelExtension.GetCollection().Items) {
                var replaceValue = ProcessReplacementValue(item, replacement);
                var metadataBefore = modelExtension.GetMetadataForItem(new List<string>{metadata}, item);
                modelExtension.SetMetadataForItem(metadata, replaceValue, item);
                var metadataAfter = modelExtension.GetMetadataForItem(new List<string>{metadata}, item);
            }
        }

        private string ProcessReplacementValue(IOpenSearchResultItem item, string replacementValue){
            var rv = replacementValue;
            var final = "";

            while (rv.Contains(parameterElementTag)) {
                //copy what is before parameter to replace
                if (!rv.StartsWith(parameterElementTag)) final += rv.Substring(0, rv.IndexOf(parameterElementTag));

                //get parameter name
                var sub = rv.Substring(rv.IndexOf(parameterElementTag) + 1);
                if (!sub.StartsWith("<")) throw new Exception("Wrong format parameter name");
                rv = sub.Substring(sub.IndexOf(">") + 1);
                sub = sub.Substring(1, sub.IndexOf(">") - 1);
                var metadata = modelExtension.GetMetadataForItem(new List<string>{ sub }, item);
                final += metadata[0];
            }

            final += rv;
            return final;
        }

        public void GetMetadata(string name){
            
        }

        public void LoadResults(IOpenSearchResultCollection osr) {
            modelExtension.LoadOpenSearchResultCollection(osr);
            modelExtension.ApplyParameters();
        }

        public void SetQueryParameters(NameValueCollection nvc){
            modelExtension.SetQueryParameters(nvc);
        }

        public IOpenSearchable CreateOpenSearchable(List<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, NetworkCredential netCreds){
            return modelExtension.CreateOpenSearchable(baseUrls, queryFormatArg, ose, netCreds);
        }
    }
}

