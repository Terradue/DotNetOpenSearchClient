using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using Mono.Addins;
using System.Linq;
using System.Collections.ObjectModel;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Engine;
using System.Net;

[assembly:Addin]
[assembly:AddinDependency("OpenSearchDataModel", "1.0")]
namespace Terradue.OpenSearch.Model.GeoTime {

    [Extension(typeof(IOpenSearchClientDataModelExtension))]
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
            metadataExtractors.Add("published", new PublicationDateMetadataExtractor());
            metadataExtractors.Add("updated", new UpdatedMetadataExtractor());

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
                        throw new NotSupportedException(string.Format("metadata extractor \"{0}\" not found in the data model", metadata));
                        
                    metadataItems.Add(metadataExtractors[metadata].GetMetadata(item));
                   

                }
            }

            return metadataItems;
        }

        public void LoadOpenSearchResultCollection(Terradue.OpenSearch.Result.IOpenSearchResultCollection osr) {
            this.osr = osr;
        }

        public virtual string Name {
            get {
                return "GeoTime";
            }
        }

        public virtual string Description {
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

        public virtual void ApplyParameters() {

            foreach (var item in osr.Items.ToArray()) {

                if (!string.IsNullOrEmpty(parameters["enclosure:scheme"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:scheme"]).ToList());
                }
                if (!string.IsNullOrEmpty(parameters["enclosure:host"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:host"]).ToList());
                }

            }

        }


        public virtual void SetQueryParameters(NameValueCollection nvc) {
            
        }

        public virtual IOpenSearchable CreateOpenSearchable(List<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, NetworkCredential netCreds){
            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            foreach (var url in baseUrls) {
                if (string.IsNullOrEmpty(queryFormatArg))
                    entities.Add(OpenSearchFactory.FindOpenSearchable(ose, url));
                else {
                    var e = OpenSearchFactory.FindOpenSearchable(ose, url, ose.GetExtensionByExtensionName(queryFormatArg).DiscoveryContentType);
                    entities.Add(e);
                }
            }

            IOpenSearchable entity;

            if (entities.Count > 1) {
                entity = new MultiGenericOpenSearchable(entities, ose);
            } else {
                entity = entities[0];
            }

            return entity;
        }
        #endregion
    }
}

