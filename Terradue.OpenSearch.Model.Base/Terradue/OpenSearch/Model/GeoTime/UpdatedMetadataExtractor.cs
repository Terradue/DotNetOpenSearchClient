using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    public class UpdatedMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            return item.LastUpdatedTime.ToUniversalTime().ToString("O");
        }
        public virtual string Description {
            get {
                return "Updated date of the item";
            }
        }
        #endregion
	}

}

