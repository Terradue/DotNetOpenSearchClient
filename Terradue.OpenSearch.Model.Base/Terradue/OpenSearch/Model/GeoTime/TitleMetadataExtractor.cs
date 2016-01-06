using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime
{
    
    public class TitleMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string title = "";
            if (item.Title != null)
                title = item.Title.Text;

            return title;
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, List<string> parameters){
            if (parameters.Count != 2 && !parameters[0].Equals("-r")) throw new Exception("Invalid action for title metadata. Allowed actions are: -r.");
            var value = parameters[1];
            item.Title = new TextSyndicationContent(value);
            return true;
        }
        public virtual string Description {
            get {
                return "Title of the item (string)";
            }
        }
        #endregion
	}

}

