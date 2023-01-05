using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    public class PublicationDateMetadataExtractor : IMetadataExtractor {
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            return item.PublishDate.ToUniversalTime().ToString("O");
        }
        public virtual string Description {
            get {
                return "Publication date of the item";
            }
        }
        #endregion
	}

}

