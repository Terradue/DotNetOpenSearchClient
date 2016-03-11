using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class WrsLongitudeGridMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindWrsLongitudeGridFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A string representing the acquisition Wrs Longitude Grid";
            }
        }

        #endregion
	}

}

