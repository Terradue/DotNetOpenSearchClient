using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {

    public class EndDateMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            string date = null;
            if (date == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "date") {
                        date = ext.GetObject<string>();
                        if (date.Contains("/"))
                            date = DateTime.Parse(date.Split('/')[1]).ToUniversalTime().ToString("O");
                    }
                    if (ext.OuterName == "validTime" && ext.OuterNamespace == "http://www.opengis.net/gml") {
                        using (XmlReader r = ext.GetReader()) {
                            if (r.ReadToFollowing("endPosition", "http://www.opengis.net/gml")) {
                                date = DateTime.Parse(r.ReadElementContentAsString(), null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime().ToString("O");
                            }
                        }
                        break;
                    }
                    if (ext.OuterName == "dtend" && ext.OuterNamespace == "http://www.w3.org/2002/12/cal/ical#") {
                        date = DateTime.Parse(ext.GetObject<string>(), null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime().ToString("O");
                        break;
                    }
                }
            }

            return date;
        }
        public virtual string Description {
            get {
                return "End time of the item (UTC ISO 8601)";
            }
        }
        #endregion
    }

}

