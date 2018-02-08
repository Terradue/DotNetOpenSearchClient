using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NuGet;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {

    class EnclosureMetadataExtractor : IMetadataExtractor {

        NameValueCollection parameters;

        public EnclosureMetadataExtractor(NameValueCollection parameters) {
            this.parameters = parameters;
        }

        #region IMetadataExtractor implementation

        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {

            if (item.Links == null)
                return "";

            // retrieve all enclosure links
            var links = item.Links.Where(l => l.RelationshipType == "enclosure").Select(l => l.Uri.ToString()).ToList();

            string enclosure;
            if (links.IsEmpty()) {
                enclosure = "";
            }
            else if (parameters.AllKeys.Contains("allEnclosures")) {
                enclosure = string.Join("|", links);
            }
            else {
                enclosure = links.First();
            }
            return enclosure;
        }

        public string Description {
            get {
                return string.Format(
                    "Link to related resource that is potentially large in size and might require special handling (RFC 4287). This metadata takes into account the following data model parameters: enclosure:scheme, enclosure:host");
            }
        }

        #endregion

    }

}