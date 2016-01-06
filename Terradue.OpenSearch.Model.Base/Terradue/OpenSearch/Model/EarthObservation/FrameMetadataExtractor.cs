using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class FrameMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {
            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindFrameFromOpenSearchResultItem(item);
        }
        public bool SetMetadata(IOpenSearchResultItem item, List<string> parameters){
            //TODO
            return false;
        }

        public string Description {
            get {
                return "A number representing the acquisition orbit track";
            }
        }

        #endregion
	}

}

