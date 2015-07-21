using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using System.Linq;
using System.Collections.ObjectModel;
using Terradue.ServiceModel.Syndication;


namespace Terradue.OpenSearch.Client.Model.GeoTime {

    [Extension(typeof(IOpenSearchClientDataModelExtension))]
    [ExtensionNode("GeoTime", "Geo & Time Data Model")]
    public class GeoTimeDataModelExtension : IOpenSearchClientDataModelExtension {

        protected Dictionary<string, IMetadataExtractor> metadataExtractors;
        protected IOpenSearchResultCollection osr;
        protected NameValueCollection parameters;

        protected virtual void InitializeExtractors() {

            metadataExtractors = new Dictionary<string, IMetadataExtractor>();

            metadataExtractors.Add("wkt", new WellKnownTextMetadataExtractor());
            metadataExtractors.Add("enclosure", new EnclosureMetadataExtractor(parameters));
            metadataExtractors.Add("identifier", new IdentifierMetadataExtractor());
            metadataExtractors.Add("startdate", new StartDateMetadataExtractor());
            metadataExtractors.Add("enddate", new EndDateMetadataExtractor());

        }

        #region IOpenSearchClientDataModelExtension implementation

        public void InitModelExtension(NameValueCollection parameters){

            this.parameters = parameters;
            this.InitializeExtractors();

           
        }

        public System.Collections.Generic.Dictionary<string, IMetadataExtractor> MetadataExtractors {
            get {
                return metadataExtractors;
            }
        }


        public List<string> GetMetadataForItem(List<string> metadataPaths, IOpenSearchResultItem item) {

            List<string> metadataItems = new List<string>();

            if (osr != null) {

                foreach (var metadata in metadataPaths) {

                    if (!metadataExtractors.ContainsKey(metadata))
                        throw new NotSupportedException("metadata extractor \"{0}\" not found in the data model");
                        
                    metadataItems.Add(metadataExtractors[metadata].GetMetadata(item));
                   
                }
            }

            return metadataItems;
        }

        public void LoadOpenSearchResultCollection(Terradue.OpenSearch.Result.IOpenSearchResultCollection osr) {
            this.osr = osr;
        }

        public string Name {
            get {
                return "GeoTime";
            }
        }

        public string Description {
            get {
                return "Data model to handle simple Geo & Time dataset according to OGC® OpenSearch Geo and Time Extensions 10-032r8\n" +
                    "Some parameters allows to ovveride or to filter metadata:" +
                    "enclosure:scheme filters enclosure with corresponding scheme" +
                    "enclosure:host filters enclosure with corresponding hostname";
            }
        }

        public IOpenSearchResultCollection GetCollection() {
            return osr;

        }

        public void ApplyParameters() {

            foreach (var item in osr.Items.ToArray()) {

                if (!string.IsNullOrEmpty(parameters["enclosure:scheme"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:scheme"]).ToList());
                }
                if (!string.IsNullOrEmpty(parameters["enclosure:host"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:host"]).ToList());
                }

            }

        }

        #endregion
    }
}

