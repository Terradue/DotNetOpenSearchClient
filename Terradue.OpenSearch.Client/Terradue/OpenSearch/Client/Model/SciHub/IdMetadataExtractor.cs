using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Client.Model.Scihub
{
    
    class IdMetadataExtractor : Terradue.OpenSearch.Client.Model.GeoTime.IdentifierMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {


            return item.Id;
        }
        public override string Description {
            get {
                return "Id of the dataset within the scihub collection";
            }
        }
        #endregion
	}

}

