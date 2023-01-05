using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class SwathIdentifierMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindSwathIdentifier(item);
        }

        public string Description {
            get {
                return "A string representing the acquisition swath identifier";
            }
        }

        #endregion
	}

}

