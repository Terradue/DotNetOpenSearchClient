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

        public FedeoOpenSearchable(OpenSearchUrl url, OpenSearchEngine ose): base(url, ose){
        }

        public static FedeoOpenSearchable CreateFrom(Uri url,  OpenSearchEngine ose) {
            return new FedeoOpenSearchable(new OpenSearchUrl(url), ose);
        }

        public new void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
            log.DebugFormat("Applying Fedeo source harvesting");

            base.ApplyResultFilters(request, ref osr, finalContentType);

            QueryEarthObservationResult(ref osr);

        }

        private void QueryEarthObservationResult(ref IOpenSearchResultCollection osr) {


            foreach ( var item in osr.Items){

                log.DebugFormat("Searching for alternate link to om for item {0}", item.Identifier);

                var eo = Terradue.Metadata.EarthObservation.MetadataHelpers.GetEarthObservationFromIOpenSearchResultItem(item);

                if (eo != null && eo is ServiceModel.Ogc.Eop20.EarthObservationType) {
                    AddOrReplaceEarthObservation((ServiceModel.Ogc.Eop20.EarthObservationType)eo, item);
                }
            }
        }

        public static void AddOrReplaceEarthObservation(ServiceModel.Ogc.Eop20.EarthObservationType eo, IOpenSearchResultItem item)
        {
            if (eo != null)
            {
                foreach (var ext in item.ElementExtensions.ToArray())
                {
                    if (ext.OuterName == "EarthObservation")
                    {
                        item.ElementExtensions.Remove(ext);
                    }
                }

                item.ElementExtensions.Add(Terradue.ServiceModel.Ogc.OgcHelpers.CreaterReader(eo));
            }
        }
	}

}

