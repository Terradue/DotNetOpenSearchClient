using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class WrsLatitudeGridMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindWrsLatitudeGridFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A string representing the acquisition Wrs Latitude Grid";
            }
        }

        #endregion
	}

}
