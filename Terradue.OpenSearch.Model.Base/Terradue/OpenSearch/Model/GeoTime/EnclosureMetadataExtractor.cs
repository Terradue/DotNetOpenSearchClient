using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    class EnclosureMetadataExtractor : IMetadataExtractor {
        
        NameValueCollection parameters;

        public EnclosureMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
            
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {

            var link = item.Links.FirstOrDefault(l => {
                if (l.RelationshipType == "enclosure") {
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
                return string.Format("Link to related resource that is potentially large in size and might require special handling (RFC 4287). This metadata takes into account the following data model parameters: enclosure:scheme, enclosure:host");
            }
        }

        #endregion
    }

}

