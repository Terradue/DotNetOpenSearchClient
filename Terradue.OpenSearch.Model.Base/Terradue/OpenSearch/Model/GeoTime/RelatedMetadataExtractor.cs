using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    class RelatedMetadataExtractor : IMetadataExtractor {
        
        NameValueCollection parameters;

        public RelatedMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
            
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {

            var link = item.Links.FirstOrDefault(l => {
                if (l.RelationshipType == "related") {
                    bool ret = true;

                    return ret;
                } 
                return false;
            });

            if (link != null)
                return link.Uri.ToString();

            return "";
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, List<string> parameters){
            SyndicationLink newlink = null;
            SyndicationLink link = null;
            string action = parameters[0];

            int argpos = 0;
            while (argpos < parameters.Count) {
                switch (parameters[argpos]) {
                    //Add a new related
                    case "-a":
                        newlink = new SyndicationLink(new Uri(parameters[++argpos]), "related", null, null, 0);
                        item.Links.Add(newlink);
                        return true;
                        //Replace an existing related
                    case "-r":
                        link = item.Links.FirstOrDefault(l => {
                            if (l.RelationshipType == "related" && l.Uri.AbsoluteUri.Equals(parameters[++argpos])) {
                                bool ret = true;
                                return ret;
                            } 
                            return false;
                        });
                        if (link == null) return false;
                        newlink = link;
                        newlink.Uri = new Uri(parameters[++argpos]);
                        item.Links.Remove(link);
                        item.Links.Add(newlink);
                        return true;
                        //Delete an existing related
                    case "-d":
                        if (parameters.Count == argpos + 1 || parameters[argpos + 1].StartsWith("-")) {
                            item.Links = new System.Collections.ObjectModel.Collection<SyndicationLink>();
                        } else {
                            link = item.Links.FirstOrDefault(l => {
                                if (l.RelationshipType == "related" && l.Uri.AbsoluteUri.Equals(parameters[++argpos])) {
                                    bool ret = true;

                                    return ret;
                                } 
                                return false;
                            });
                            if (link == null) return false;
                            item.Links.Remove(link);
                        }
                        return true;
                    default:
                        throw new Exception("Invalid action for related. Allowed actions are: -a|-r|-d.");
                        return false;
                }
                argpos++;
            }

            return false;
        }
        public string Description {
            get {
                return string.Format("Link to resource related to the resource described by the containing element. For example, slaves feed in an correlated search. " +
                    "This metadata takes into account the following data model parameters: related:title, related:type");
            }
        }

        #endregion
    }

}

