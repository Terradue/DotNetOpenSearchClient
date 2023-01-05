using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {

    public class StartDateMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            string date = null;
            if (date == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "date") {
                        date = ext.GetObject<string>();
                        if (date.Contains("/"))
                            date = DateTime.Parse(date.Split('/')[0]).ToUniversalTime().ToString("O");
                    }
                    if (ext.OuterName == "validTime" && ext.OuterNamespace == "http://www.opengis.net/gml") {
                        using (XmlReader r = ext.GetReader()) {
                            if (r.ReadToFollowing("beginPosition", "http://www.opengis.net/gml")) {
                                date = DateTime.Parse(r.ReadElementContentAsString(), null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime().ToString("O");
                            }
                        }
                        break;
                    }
                }
            }

            return date;
        }


        public virtual string Description {
            get {
                return "Start time of the item (UTC ISO 8601)";
            }
        }
        #endregion
    }

}

