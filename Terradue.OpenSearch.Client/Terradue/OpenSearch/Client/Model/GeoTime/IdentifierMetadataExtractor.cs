using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Client.Model.GeoTime
{
    
    class IdentifierMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string ident = null;
            if (ident == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "identifier") {
                        ident = ext.GetObject<string>();
                        break;
                    }
                }
            }
            if (ident == null) {
                var identifier = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindIdentifierFromOpenSearchResultItem(item);
                if (identifier != null)
                    ident = identifier;
            }

            return ident;
        }
        public string Description {
            get {
                return "Identifier time of the item within the collection";
            }
        }
        #endregion
	}

}

