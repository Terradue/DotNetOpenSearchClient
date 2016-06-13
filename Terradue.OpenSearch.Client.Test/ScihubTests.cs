using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class ScihubTests {

        OpenSearchClient client;
        XNamespace atom = "http://www.w3.org/2005/Atom";
        XNamespace georss10 = "http://www.georss.org/georss/10";

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

        }

        [Test()]
        public void S1EOP() {

            OpenSearchClient.baseUrlArg.Add("https://scihub.copernicus.eu/apihub/odata/v1");
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

