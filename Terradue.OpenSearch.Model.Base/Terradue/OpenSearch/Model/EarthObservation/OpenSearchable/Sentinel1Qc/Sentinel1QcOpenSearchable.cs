using System;
using Terradue.OpenSearch.Model.GeoTime;
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
using System.Xml.Serialization;
using Terradue.Metadata.EarthObservation;
using System.IO;
using System.Text;
using Terradue.ServiceModel.Ogc;
using Terradue.OpenSearch.Engine.Extensions;
using System.Web;
using Terradue.OpenSearch.Model.CustomExceptions;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable
{

    public class Sentinel1QcOpenSearchable : IOpenSearchable
    {


        private static readonly ILog log = LogManager.GetLogger(typeof(Sentinel1QcOpenSearchable));

        public string Identifier
        {
            get
            {
                return "qc-sentinel1";
            }
        }

        public long TotalResults
        {
            get
            {
                return 0;
            }
        }

        public string DefaultMimeType
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public bool CanCache
        {
            get
            {
                return true;
            }
        }

        public int MaxRetries { get; set; } = 5;

        readonly Uri qcBaseUrl;
        readonly OpenSearchEngine ose;
        private ReadNativeFunction originalReadNativeFunctionToExtend;

        public Sentinel1QcOpenSearchable(Uri qcBaseUrl, OpenSearchEngine ose)
        {
            this.ose = ose;
            this.qcBaseUrl = qcBaseUrl;
        }

        public QuerySettings GetQuerySettings(Terradue.OpenSearch.Engine.OpenSearchEngine ose)
        {
            IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
            originalReadNativeFunctionToExtend = osee.ReadNative;
            return new QuerySettings(osee.DiscoveryContentType, Sentinel1QcReadNative);
        }

        
        
        public  IOpenSearchResultCollection Sentinel1QcReadNative(IOpenSearchResponse response) {            
            IOpenSearchResultCollection openSearchResultCollection = originalReadNativeFunctionToExtend(response);
            
            if (response.GetType() == typeof(PartialAtomSearchResponse)) {
                throw new PartialAtomException("Attaching result to exception " , null , openSearchResultCollection);
            }

            return openSearchResultCollection;
        }
        
        
        
        public Terradue.OpenSearch.Request.OpenSearchRequest Create(QuerySettings querySettings, System.Collections.Specialized.NameValueCollection parameters)
        {

            log.DebugFormat("Create OS QC Sentinel1");

            UriBuilder url = new UriBuilder(qcBaseUrl);
            url.Path += string.Format("/search");
            url.Query = string.Format("type={3}&start={0}&stop={1}&startIndex={2}&ills=true", parameters["start"], parameters["stop"], parameters["startIndex"],
                                     string.IsNullOrEmpty(parameters["auxtype"]) ? "aux_resorb" : parameters["auxtype"]);

            //if (parameters["ills"] == "true")
            //{
            return new Sentinel1QcOpenSearchRequest(url.Uri, parameters);
            //}

            NameValueCollection illParams = new NameValueCollection(parameters);
            illParams.Set("ills", "true");

            return new IllimitedOpenSearchRequest<AtomFeed, AtomItem>(new OpenSearchableFactorySettings(ose), this, "application/atom+xml", new OpenSearchUrl(url.Uri), illParams);
        }

        public Terradue.OpenSearch.Schema.OpenSearchDescription GetOpenSearchDescription()
        {
            OpenSearchDescription osd = new OpenSearchDescription();
            osd.ShortName = "SciHub";
            osd.Contact = "info@terradue.com";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Developer = "Terradue OpenSearch Development Team";
            osd.Attribution = "Terradue";

            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            UriBuilder urlb = new UriBuilder(qcBaseUrl);
            urlb.Path += "/description";

            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/opensearchdescription+xml", urlb.ToString(), "self", osd.ExtraNamespace);
            urls.Add(url);

            urlb = new UriBuilder(qcBaseUrl);
            urlb.Path += "/search";
            NameValueCollection query = GetOpenSearchParameters("application/atom+xml");

            string[] queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urlb.Query = string.Join("&", queryString);
            url = new OpenSearchDescriptionUrl("application/atom+xml", urlb.ToString(), "search", osd.ExtraNamespace);
            url.IndexOffset = 0;
            urls.Add(url);

            osd.Url = urls.ToArray();
            osd.DefaultUrl = url;

            return osd;
        }

        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType)
        {
            var osdic = OpenSearchFactory.GetBaseOpenSearchParameter();
            osdic.Add("uid", "{geo:uid?}");
            osdic.Add("start", "{time:start?}");
            osdic.Add("stop", "{time:end?}");
            osdic.Add("ills", "{t2:ills?}");
            osdic.Add("auxtype", "{t2:auxtype?}");
            osdic.Add("orbits", "{t2:orbits?}");
            return osdic;
        }

        public void ApplyResultFilters(Terradue.OpenSearch.Request.OpenSearchRequest request, ref Terradue.OpenSearch.Result.IOpenSearchResultCollection osr, string finalContentType)
        {

        }


    }

}

