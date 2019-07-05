using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Net;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class ScihubTests : TestBase {

        OpenSearchClient client;

        [SetUp]
        public void SetUpClient(){
            
            client = new OpenSearchClient();

            client.Initialize();

            OpenSearchClient.baseUrlArg = new List<string>();

            OpenSearchClient.metadataPaths = new List<string>();

            OpenSearchClient.parameterArgs = new List<string>();

            OpenSearchClient.dataModelParameterArgs = new List<string>();

            OpenSearchClient.queryModelArg = "GeoTime";

            OpenSearchClient.verbose = true;

            LoadCredentials();

        }

        [Ignore]
        public void S1EOP() {

            Credential credential = GetCredential("SciHubPolygon_01", true);

            OpenSearchClient.baseUrlArg.Add("http://scihub.terradue.com/apihub/odata/v1");
            OpenSearchClient.netCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };

            OpenSearchClient.metadataPaths.Add("{}");
            OpenSearchClient.parameterArgs.Add("count=1");
            OpenSearchClient.parameterArgs.Add("profile=eop");
            OpenSearchClient.parameterArgs.Add("uid=S1A_IW_GRDH_1SDV_20160510T000021_20160510T000040_011189_010E6C_BFCE");
            OpenSearchClient.queryModelArg = "Scihub";
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var xr = XmlReader.Create(ms);
            XDocument doc = XDocument.Load(xr);


            doc.Save("../../out/scihubS1eop.xml");


        }

            
    }
}

