using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class CloudCoverMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

			return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindCloudCoverPercentage(item).ToString();
        }

        public string Description {
            get {
                return "A float number indicating the cloud coverage value in percentage of the image surface.";
            }
        }

        #endregion
    }

}

