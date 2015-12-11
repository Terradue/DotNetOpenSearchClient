using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.Scihub
{
    
    class IdMetadataExtractor : Terradue.OpenSearch.Model.GeoTime.IdentifierMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            return item.Id;
        }
        public override bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string value) {
            item.Id = value;
            return true;
        }
        public override string Description {
            get {
                return "Id of the dataset within the scihub collection";
            }
        }
        #endregion
	}

}

