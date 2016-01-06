using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime
{
    
    public class StartDateMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            DateTime date = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindStartDateFromOpenSearchResultItem(item);

            return date.ToUniversalTime().ToString("O");
        }
        public virtual string Description {
            get {
                return "Start time of the item (UTC ISO 8601)";
            }
        }
        #endregion
	}

}

