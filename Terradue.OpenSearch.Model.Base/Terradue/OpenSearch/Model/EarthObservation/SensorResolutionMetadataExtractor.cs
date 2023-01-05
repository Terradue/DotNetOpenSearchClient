using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class SensorResolutionMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindSensorResolution(item).ToString();
        }

        public string Description {
            get {
                return "A double value representing the sensor acquisition resolution";
            }
        }

        #endregion
	}

}

