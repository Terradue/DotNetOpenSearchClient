using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    class LinkMetadataExtractor : IMetadataExtractor {
        
        NameValueCollection parameters;

        public LinkMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
            
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {

            var link = item.Links.FirstOrDefault(l => {
                if (l.RelationshipType == specifier) {
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
				return string.Format("Link with a specific relationship (set link:search for link rel=search) (RFC 4287).");
            }
        }

        #endregion
    }

}

