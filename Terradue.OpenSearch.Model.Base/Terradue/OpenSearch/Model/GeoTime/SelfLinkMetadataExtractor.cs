using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    class SelfLinkMetadataExtractor : IMetadataExtractor {
        
        NameValueCollection parameters;

        public SelfLinkMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
            
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {

            var link = item.Links.FirstOrDefault(l => {
                if (l.RelationshipType == "self") {
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
                return string.Format("[OBSOLETE use link:self instead] Link to resource that identifies a resource equivalent to the containing element (RFC 4287).");
            }
        }

        #endregion
    }

}

