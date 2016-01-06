using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime
{
    
    public class IdentifierMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string ident = null;
            if (ident == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "identifier") {
                        ident = ext.GetObject<string>();
                        break;
                    }
                }
            }

            return ident;
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, List<string> parameters){
            bool isSet = false;
            if (parameters.Count != 2 && !parameters[0].Equals("-r")) throw new Exception("Invalid action for identifier metadata. Allowed actions are: -r.");
            var value = parameters[1];
            foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                if (ext.OuterName == "identifier") {
                    item.ElementExtensions.Remove(ext);
                    item.ElementExtensions.Add(new SyndicationElementExtension("identifier", "http://purl.org/dc/elements/1.1/", value));
                    isSet = true;
                    break;
                }
            }
            return isSet;
        }
        public virtual string Description {
            get {
                return "Identifier time of the item within the collection";
            }
        }
        #endregion
	}

}

