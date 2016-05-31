using System;
using Terradue.OpenSearch.Model.GeoTime;
using Mono.Addins;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using System.Net;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using System.Linq;
using System.Xml;
using log4net;

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable
{
    
    public class FedeoOpenSearchable : GenericOpenSearchable, IOpenSearchable
	{

        private static readonly ILog log = LogManager.GetLogger(typeof(CwicOpenSearchable));

        public FedeoOpenSearchable(OpenSearchDescription osd, OpenSearchEngine ose): base(osd, ose){
        }

        public static FedeoOpenSearchable CreateFrom(GenericOpenSearchable e,  OpenSearchEngine ose) {
            return new FedeoOpenSearchable(e.GetOpenSearchDescription(), ose);
        }

        public new void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
            log.DebugFormat("Applying Fedeo source harvesting");

            base.ApplyResultFilters(request, ref osr, finalContentType);

            QueryEarthObservationResult(ref osr);

        }

        private void QueryEarthObservationResult(ref IOpenSearchResultCollection osr) {


            foreach ( var item in osr.Items){

                log.DebugFormat("Searching for alternate link to om for item {0}", item.Identifier);

                var omlink = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate" && l.Title == "O&M 1.1 format" );

                if (omlink != null) {
                    log.DebugFormat("Link found at {0}", omlink.Uri);
                    var req = HttpWebRequest.Create(omlink.Uri);
                    var response = req.GetResponse();
                    var xr = XmlReader.Create(response.GetResponseStream());
                    item.ElementExtensions.Add(xr);
                }
            }
        }
	}

}

