using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class ProcessingLevelMetadataExtractor : IMetadataExtractor {
        
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindProcessingLevelFromOpenSearchResultItem(item);
        }
       
        public string Description {
            get {
                return "A number representing the metadata processing level";
            }
        }

        #endregion
	}

}

