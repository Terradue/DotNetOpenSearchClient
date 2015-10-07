using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    class RelatedMetadataExtractor : IMetadataExtractor {
        
        NameValueCollection parameters;

        public RelatedMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
            
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {

            var link = item.Links.FirstOrDefault(l => {
                if (l.RelationshipType == "related") {
                    bool ret = true;

                    return ret;
                } 
                return false;
            });

            if (link != null)
                return link.Uri.ToString();

            return "";
        }

        public string Description {
            get {
                return string.Format("Link to resource related to the resource described by the containing element. For example, slaves feed in an correlated search. " +
                    "This metadata takes into account the following data model parameters: related:title, related:type");
            }
        }

        #endregion
    }

}

