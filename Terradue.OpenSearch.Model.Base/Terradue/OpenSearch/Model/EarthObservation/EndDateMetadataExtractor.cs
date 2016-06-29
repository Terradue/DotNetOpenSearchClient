using System;
using Terradue.OpenSearch.Model.GeoTime;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    public class EndDateMetadataExtractor: Terradue.OpenSearch.Model.GeoTime.EndDateMetadataExtractor
    {
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
			string date = base.GetMetadata(item, specifier);
            var start = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindEndDateFromOpenSearchResultItem(item);
            if (start != DateTime.MinValue)
                date = start.ToUniversalTime().ToString("O");

            return date;
        }
        public override string Description {
            get {
                return "Start time of the dataset (UTC ISO 8601)";
            }
        }
        #endregion
    }

}

