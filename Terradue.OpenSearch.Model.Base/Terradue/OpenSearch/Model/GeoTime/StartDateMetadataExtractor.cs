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
            string date = null;
            if (date == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "date") {
                        date = ext.GetObject<string>();
                        if (date.Contains("/"))
                            date = DateTime.Parse(date.Split('/')[0]).ToUniversalTime().ToString("O");
                        break;
                    }
                    if (ext.OuterName == "dtstart" && ext.OuterNamespace == "http://www.w3.org/2002/12/cal/ical#") {
                        date = DateTime.Parse(ext.GetObject<string>()).ToUniversalTime().ToString("O");
                        break;
                    }
                }
            }
            return date;
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string value){
            bool isSet = false;
            foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                if (ext.OuterName == "date") {
                    var date = ext.GetObject<string>();
                    if (date.Contains("/"))
                        value = value + "/" + DateTime.Parse(date.Split('/')[1]).ToUniversalTime().ToString("O");
                    item.ElementExtensions.Remove(ext);
                    item.ElementExtensions.Add(new SyndicationElementExtension("date", "http://purl.org/dc/elements/1.1/", value));
                    isSet = true;
                    break;
                }
                if (ext.OuterName == "dtstart" && ext.OuterNamespace == "http://www.w3.org/2002/12/cal/ical#") {
                    item.ElementExtensions.Remove(ext);
                    item.ElementExtensions.Add(new SyndicationElementExtension("dtstart", "http://www.w3.org/2002/12/cal/ical#", value));
                    isSet = true;
                    break;
                }
            }
            return isSet;
        }
        public virtual string Description {
            get {
                return "Start time of the item (UTC ISO 8601)";
            }
        }
        #endregion
	}

}

