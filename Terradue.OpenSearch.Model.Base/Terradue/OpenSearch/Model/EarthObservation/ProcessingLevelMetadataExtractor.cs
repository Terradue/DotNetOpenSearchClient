using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class ProcessingLevelMetadataExtractor : IMetadataExtractor {
        
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindProcessingLevel(item);
        }
       
        public string Description {
            get {
                return "A number representing the metadata processing level";
            }
        }

        #endregion
	}

}

