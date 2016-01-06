using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class PlatformShortNameMetadataExtractor : IMetadataExtractor
    {
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {

            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindPlatformShortNameFromOpenSearchResultItem(item);
        }
        public bool SetMetadata(IOpenSearchResultItem item, List<string> parameters){
            //TODO
            return false;
        }

        public string Description {
            get {
                return "A string identifying the acquisition platform shortname.";
            }
        }

        #endregion
    }

}

