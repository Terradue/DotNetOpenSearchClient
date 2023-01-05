using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    public class IdentifierMetadataExtractor : IMetadataExtractor {
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            string ident = null;
            if (ident == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "identifier") {
                        ident = ext.GetObject<string>();
                        break;
                    }
                }
            }

            return ident;
        }
        public virtual string Description {
            get {
                return "Identifier time of the item within the collection";
            }
        }
        #endregion
	}

}

