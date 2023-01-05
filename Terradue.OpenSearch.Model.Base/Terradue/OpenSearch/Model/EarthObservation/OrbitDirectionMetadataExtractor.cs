using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class OrbitDirectionMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindOrbitDirection(item);
        }

        public string Description {
            get {
                return "A string identifying the acquisition orbit direction. Possible values are: ASCENDING, DESCENDING";
            }
        }

        #endregion
    }

}

