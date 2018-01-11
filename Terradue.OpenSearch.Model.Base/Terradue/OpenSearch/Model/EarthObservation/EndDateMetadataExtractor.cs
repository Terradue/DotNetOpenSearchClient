using System;
using Terradue.OpenSearch.Model.GeoTime;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    public class EndDateMetadataExtractor: Terradue.OpenSearch.Model.GeoTime.EndDateMetadataExtractor
    {
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
			string date = base.GetMetadata(item, specifier);
            var end = Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindEndDate(item);
            if (end != DateTime.MinValue)
                date = end.ToUniversalTime().ToString("O");

            return date;
        }
        public override string Description {
            get {
                return "End time of the dataset (UTC ISO 8601)";
            }
        }
        #endregion
    }

}

