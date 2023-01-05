using System;
using System.Text.RegularExpressions;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class OperationalModeMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            var mode = Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindOperationalMode(item);
            if (mode == null) return null;

			if ( mode.EndsWith("_SP") || mode.EndsWith("_DP") ){
				mode = mode.Replace("_SP", "").Replace("_DP", "");
			}
			return mode;
        }

        public string Description {
            get {
                return "A string representing the acquisition sensor operational mode";
            }
        }

        #endregion
	}

}

