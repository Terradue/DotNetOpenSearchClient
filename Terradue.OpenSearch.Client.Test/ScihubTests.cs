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

        [SetUp]
        public void SetUpClient(){
            LoadCredentials();
        }

        [Ignore("")]
        public void S1EOP() {
            Credential credential = GetCredential("SciHubPolygon_01", true);

            OpenSearchClient client = CreateTestClient("http://scihub.terradue.com/apihub/odata/v1", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.Parameters.Add("count=1");
            client.Parameters.Add("profile=eop");
            client.Parameters.Add("uid=S1A_IW_GRDH_1SDV_20160510T000021_20160510T000040_011189_010E6C_BFCE");
            client.QueryModel = "Scihub";
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var xr = XmlReader.Create(ms);
            XDocument doc = XDocument.Load(xr);

            doc.Save("../../out/scihubS1eop.xml");
        }
            
    }

}

