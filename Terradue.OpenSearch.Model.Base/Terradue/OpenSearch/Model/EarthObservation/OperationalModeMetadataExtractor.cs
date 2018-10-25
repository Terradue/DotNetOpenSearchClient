using System;
using System.Text.RegularExpressions;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class OperationalModeMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            var mode = Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindOperationalMode(item);
			if ( mode == "EW" || mode == "IW"){
				var match = Regex.Match(item.Identifier, @"^(?'psn'S1[AB])_(?'mode'\w{2})_(?'pt'\w{3})(?'res'\w{1})_(?'level'\d{1})(?'class'\w{1})(?'polarisation'\w{2})_(?'begin'\w{15})_(?'end'\w{15})_.*$");
                if (!match.Success)
					mode = string.Format("{0}_{1}P", match.Groups["mode"].Value, match.Groups["polarisation"].Value[0]);
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

