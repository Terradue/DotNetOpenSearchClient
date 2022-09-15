using System;
using Terradue.OpenSearch.Model.GeoTime;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Model.EarthObservation;
using System.Net;
using System.Linq;
using Terradue.OpenSearch.DataHub;
using Terradue.OpenSearch.Sentinel;
using Terradue.OpenSearch.DataHub.Dias;
using Terradue.OpenSearch.DataHub.DHuS;
using Terradue.OpenSearch.DataHub.Aws;
using Terradue.OpenSearch.DataHub.GoogleCloud;

namespace Terradue.OpenSearch.Model.Scihub {
    
   [OpenSearchClientExtension("Scihub", "SciHub Client extension")]
    public class ScihubDataModelExtension : EarthObservationDataModelExtension, IOpenSearchClientDataModelExtension {
        
        protected override void InitializeExtractors() {

            base.InitializeExtractors();
        }

        public override string Name {
            get {
                return "Scihub";
            }
        }

        public override string Description {
            get {
                return "Data model to handle Science Hub EO dataset according to OGC® OpenSearch Extension for Earth Observation OGC 13 026\n" +
                "Some parameters allows to override or to filter metadata:" +
                "enclosure:scheme filters enclosure with corresponding scheme" +
                "enclosure:host filters enclosure with corresponding hostname";
            }
        }

        public override IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings) {

            IDataHubSourceWrapper wrapper = null;
            IOpenSearchable entity = null;

            if (baseUrls == null || baseUrls.Count() == 0) {
                throw new Exception("Empty list of Base URLs");
            }
            NetworkCredential topNetworkCredential = (netCreds == null || netCreds.Count() == 0) ? null : netCreds.First();

            Uri topBaseUri = baseUrls.First();

            if (topBaseUri.Host == "catalogue.onda-dias.eu") {
                wrapper = new OndaDiasWrapper(new Uri(string.Format("https://catalogue.onda-dias.eu/dias-catalogue")), topNetworkCredential, null);
            }

            if (topBaseUri.Host == "finder.creodias.eu") {
                string url = topBaseUri.AbsoluteUri.EndsWith("describe.xml") ? topBaseUri.AbsoluteUri : null;
                wrapper = new CreoDiasWrapper(topNetworkCredential, url);
            }

            if (topBaseUri.Host.Contains("mundiwebservices.com")) {
                string url = topBaseUri.AbsoluteUri.EndsWith("search") || topBaseUri.AbsoluteUri.EndsWith("opensearch") ? topBaseUri.AbsoluteUri : null;
                var mundiDiasWrapper = new MundiDiasWrapper(topNetworkCredential, url);

                if (topNetworkCredential == null) {
                    throw new Exception("Credentials needed (<s3-key-id>:<s3-secret-key>)");
                }

                mundiDiasWrapper.S3KeyId = topNetworkCredential.UserName;
                mundiDiasWrapper.S3SecretKey = topNetworkCredential.Password;
                wrapper = mundiDiasWrapper;
            }

            if (topBaseUri.Host.Contains("sobloo.eu")) {
                var soblooDiasWrapper = new SoblooDiasWrapper(topNetworkCredential);
                wrapper = soblooDiasWrapper;
            }

            /*if (topBaseUri.Host == "api.daac.asf.alaska.edu") {
                wrapper = new AsfApiWrapper(topBaseUri, topNetworkCredential);
            }*/

            if (topBaseUri.Host.EndsWith("copernicus.eu")) {
                wrapper = new DHuSWrapper(topBaseUri, topNetworkCredential);
            }
                
            if (topBaseUri.Host.EndsWith("amazon.com")) {
                if (topNetworkCredential == null) {
                    throw new Exception("Credentials needed (<s3-key-id>:<s3-secret-key>)");
                }
                var searchWrapper = new DHuSWrapper(new Uri("https://scihub.copernicus.eu/apihub"), topNetworkCredential);
                // Credential is <s3-key-id>:<s3-secret-key>
                var amazonWrapper = new AmazonWrapper(topNetworkCredential.Password, topNetworkCredential.UserName, searchWrapper);
                wrapper = amazonWrapper;
            }

            if (topBaseUri.Host.EndsWith("googleapis.com") || topBaseUri.Host.EndsWith("google.com")) {
                if (topNetworkCredential == null) {
                    throw new Exception("Credentials needed (<project-id>:<google-auth-json-file>)");
                }
                var searchWrapper = new DHuSWrapper(new Uri("https://scihub.copernicus.eu/apihub"), topNetworkCredential);
                // Credential is <project-id>:<google-auth-json-file>
                var googleWrapper = new GoogleWrapper(topNetworkCredential.Password, topNetworkCredential.UserName, searchWrapper);
                wrapper = googleWrapper;
            }

            if (wrapper == null) {
                if (netCreds == null || netCreds.Count() == 0)
                    throw new InvalidOperationException("Missing credentials for access Scihub service");
                settings.Credentials = topNetworkCredential;

                LocalDataWrapper dataWrapper = new LocalDataWrapper(baseUrls.First());
                var dataHubOpenSearchable = new DataHubOpenSearchable(dataWrapper, settings);
                if (queryFormatArg == "eop") {
                    dataHubOpenSearchable.DefaultMimeType = "application/atom+xml; profile=http://earth.esa.int/eop/2.1";
                }
                entity = dataHubOpenSearchable;
            } else {
                OpenSearchableFactorySettings ossettings = new OpenSearchableFactorySettings(ose) {
                    Credentials = wrapper.Settings.Credentials
                };
                entity = wrapper.CreateOpenSearchable(ossettings);
            }
              
            return entity;
        }

    }
}

