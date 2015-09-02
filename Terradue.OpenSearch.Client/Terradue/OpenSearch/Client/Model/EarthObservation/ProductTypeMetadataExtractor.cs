using System;
using Terradue.OpenSearch.Client.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Client.Model.EarthObservation
{
    class ProductTypeMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {
            
            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindProductTypeFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A string identifying the entry type (e.g. GRD, SLC)";
            }
        }

        #endregion

	}

}

