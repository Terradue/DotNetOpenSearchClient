using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class TrackMetadataExtractor : IMetadataExtractor {
        
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindTrackFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A number representing the acquisition orbit track";
            }
        }

        #endregion
	}

}

