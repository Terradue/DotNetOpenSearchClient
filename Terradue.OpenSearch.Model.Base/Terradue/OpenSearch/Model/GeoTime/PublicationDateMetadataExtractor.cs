using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Text.RegularExpressions;

namespace Terradue.OpenSearch.Model.GeoTime
{
    
    public class PublicationDateMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            return item.PublishDate.ToUniversalTime().ToString("O");
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, List<string> parameters) {
            if (parameters.Count != 2 && !parameters[0].Equals("-r")) throw new Exception("Invalid action for publication date metadata. Allowed actions are: -r.");
            var value = parameters[1];
            item.PublishDate = DateTime.Parse(value);
            return true;
        }
        public virtual string Description {
            get {
                return "Publication date of the item";
            }
        }
        #endregion
	}

}

