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
            metadataExtractors.Add("title", new TitleMetadataExtractor());
            metadataExtractors.Add("enclosure", new EnclosureMetadataExtractor(parameters));
            metadataExtractors.Add("identifier", new IdentifierMetadataExtractor());
            metadataExtractors.Add("startdate", new StartDateMetadataExtractor());
            metadataExtractors.Add("enddate", new EndDateMetadataExtractor());
            metadataExtractors.Add("published", new PublicationDateMetadataExtractor());
            metadataExtractors.Add("updated", new UpdatedMetadataExtractor());
            metadataExtractors.Add("related", new RelatedMetadataExtractor(parameters));
            metadataExtractors.Add("self", new SelfLinkMetadataExtractor(parameters));
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
                    if (!metadataExtractors.ContainsKey(metadata)) throw new NotSupportedException(string.Format("metadata extractor \"{0}\" not found in the data model", metadata));
                    metadataItems.Add(metadataExtractors[metadata].GetMetadata(item));
                }
            }

            return metadataItems;
        }

        public void SetMetadataForItem(string metadata, List<string> parameters, IOpenSearchResultItem item){
            if (osr != null) {
                if (!metadataExtractors.ContainsKey(metadata)) throw new NotSupportedException(string.Format("metadata extractor \"{0}\" not found in the data model", metadata));
                //we process the template and then replace the template with the new value in the parameters
                //we assume the template is the last parameter of the list
                var newVal = ProcessTemplate(item, parameters[parameters.Count - 1]);
                parameters[parameters.Count - 1] = newVal;
                metadataExtractors[metadata].SetMetadata(item, parameters);
            }
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

                if (!string.IsNullOrEmpty(parameters["related:title"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "related" || l.Title == parameters["related:title"]).ToList());
                }
                if (!string.IsNullOrEmpty(parameters["related:type"])) {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "related" || l.RelationshipType == parameters["related:type"]).ToList());
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

        /// <summary>
        /// Processes the template by replacing $<element> with their real value within the item
        /// </summary>
        /// <returns>The template.</returns>
        /// <param name="item">Item.</param>
        /// <param name="template">Template.</param>
        public virtual string ProcessTemplate(IOpenSearchResultItem item, string template){
            string parameterElementTag = "$";
            var rv = template;
            var final = "";

            while (rv.Contains(parameterElementTag)) {
                //copy what is before parameter to replace
                if (!rv.StartsWith(parameterElementTag)) final += rv.Substring(0, rv.IndexOf(parameterElementTag));

                //get parameter name
                var sub = rv.Substring(rv.IndexOf(parameterElementTag) + 1);
                if (!sub.StartsWith("<")) throw new Exception("Cannot process template, wrong format parameter name : " + sub);
                rv = sub.Substring(sub.IndexOf(">") + 1);
                sub = sub.Substring(1, sub.IndexOf(">") - 1);
                var metadata = GetMetadataForItem(new List<string>{ sub }, item);
                final += metadata[0];
            }

            final += rv;
            return final;
        }
        #endregion
    }
}

