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
            DateTime date = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindStartDateFromOpenSearchResultItem(item);

            return date.ToUniversalTime().ToString("O");
        }
        public virtual bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, List<string> parameters){
            bool isSet = false;
            if (parameters.Count != 2 && !parameters[0].Equals("-r")) throw new Exception("Invalid action for start date metadata. Allowed actions are: -r.");
            var value = parameters[1];
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

