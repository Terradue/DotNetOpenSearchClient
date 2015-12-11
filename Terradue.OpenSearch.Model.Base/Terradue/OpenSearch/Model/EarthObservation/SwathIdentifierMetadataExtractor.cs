using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class SwathIdentifierMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindSwathIdentifierFromOpenSearchResultItem(item);
        }
        public bool SetMetadata(IOpenSearchResultItem item, string value){
            //TODO
            return false;
        }

        public string Description {
            get {
                return "A string representing the acquisition swath identifier";
            }
        }

        #endregion
	}

}

