using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    
    class IdentifierMetadataExtractor : Terradue.OpenSearch.Model.GeoTime.IdentifierMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string ident = base.GetMetadata(item);
            if (ident == null) {
                var identifier = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindIdentifierFromOpenSearchResultItem(item);
                if (identifier != null)
                    ident = identifier;
            }

            return ident;
        }
        public override string Description {
            get {
                return "Identifier time of the dataset within the collection";
            }
        }
        #endregion
	}

}

