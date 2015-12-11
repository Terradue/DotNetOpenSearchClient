using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Text.RegularExpressions;

namespace Terradue.OpenSearch.Model.GeoTime
{
    
    public class UpdatedMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            return item.LastUpdatedTime.ToUniversalTime().ToString("O");
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string value) {
            var dateRegEx1 = new Regex("/^\\d{4}-\\d{2}-\\d{2}$/");
            var dateRegEx2 = new Regex("/^\\d{2}-\\d{2}-\\d{4}$/");
            if (dateRegEx1.Match(value).Success) {
                char[] splitter = "-".ToCharArray();
                string[] split = value.Split(splitter);
                item.LastUpdatedTime = new DateTime(Int32.Parse(split[0]),Int32.Parse(split[1]),Int32.Parse(split[2]));
                return true;
            }
            if (dateRegEx2.Match(value).Success) {
                char[] splitter = "-".ToCharArray();
                string[] split = value.Split(splitter);
                item.LastUpdatedTime = new DateTime(Int32.Parse(split[2]),Int32.Parse(split[1]),Int32.Parse(split[0]));
                return true;
            }
            return false;
        }
        public virtual string Description {
            get {
                return "Updated date of the item";
            }
        }
        #endregion
	}

}

