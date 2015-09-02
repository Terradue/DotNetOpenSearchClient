using System;
using Terradue.OpenSearch.Client.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Client.Model.EarthObservation
{
    class SwathIdentifierMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindSwathIdentifierFromOpenSearchResultItem(item);
        }

        public string Description {
            get {
                return "A string representing the acquisition swath identifier";
            }
        }

        #endregion
	}

}

