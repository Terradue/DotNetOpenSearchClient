using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class ProductTypeMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {
            
            return Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindProductTypeFromOpenSearchResultItem(item);
        }
        public bool SetMetadata(IOpenSearchResultItem item, List<string> parameters){
            //TODO
            return false;
        }

        public string Description {
            get {
                return "A string identifying the entry type (e.g. GRD, SLC)";
            }
        }

        #endregion

	}

}

