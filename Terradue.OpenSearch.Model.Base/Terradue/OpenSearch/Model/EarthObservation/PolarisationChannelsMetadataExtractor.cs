using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class PolarisationChannelsMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindPolarisationChannelsFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A string representing the acquisition polarisation channels";
            }
        }

        #endregion
	}

}

