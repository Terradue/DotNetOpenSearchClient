using System;
using Terradue.OpenSearch.Client.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Client.Model.EarthObservation {
    class OrbitNumberMetadataExtractor: IMetadataExtractor {
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindOrbitNumberFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A number reprensenting the acquisition orbit";
            }
        }

        #endregion
    }

}

