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
    public class CwicTests {

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

        }

        [Test()]
        public void Landsat8EOP() {

            OpenSearchClient.baseUrlArg.Add("http://cwic.wgiss.ceos.org/opensearch/datasets/Landsat_8/osdd.xml?clientId=foo");
            OpenSearchClient.metadataPaths.Add("{}");
            OpenSearchClient.parameterArgs.Add("count=1");
            OpenSearchClient.parameterArgs.Add("timeStart=2016-05-03T00:27:10Z");
            OpenSearchClient.parameterArgs.Add("timeEnd=2016-05-03T00:27:40Z");
            OpenSearchClient.queryModelArg = "EOP";

            XmlDocument doc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream()) {
                client.ProcessQuery(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc.Load(ms);
                //doc.Save("../../out/landsattest.xml");

            }

        }

            
    }
}

