using System;
using Terradue.OpenSearch.Client.Model.GeoTime;

namespace Terradue.OpenSearch.Client.Model.EarthObservation
{
    public class EndDateMetadataExtractor: Terradue.OpenSearch.Client.Model.GeoTime.EndDateMetadataExtractor
    {
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string date = base.GetMetadata(item);
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

