using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    [TestFixture()]
    public class ParameterInteroperabilityTests : TestBase {

        OpenSearchClient client;
        XmlNamespaceManager nsm;
        string geometryXPath = "//gml32:posList | //gml:posList | //georss:polygon";

        [SetUp]
        public void SetUpClient() {

            client = new OpenSearchClient();
            client.Initialize();

            OpenSearchClient.baseUrlArg = new List<string>();
            OpenSearchClient.metadataPaths = new List<string>();
            OpenSearchClient.parameterArgs = new List<string>();
            OpenSearchClient.dataModelParameterArgs = new List<string>();
            OpenSearchClient.queryModelArg = "GeoTime";

            XmlDocument doc = new XmlDocument();
            nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("gml32", "http://www.opengis.net/gml/3.2");
            nsm.AddNamespace("gml", "http://www.opengis.net/gml");
            nsm.AddNamespace("georss", "http://www.georss.org/georss");
            nsm.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            nsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsm.AddNamespace("eop", "http://www.terradue.com/model/eop");

            LoadCredentials();
        }

        [Test()]
        public void Test_Parameters_01() {
            // opensearch-client "https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC" | wc -l
            OpenSearchClient.baseUrlArg.Add("https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC");

            int count = client.GetResultCount();
            Assert.AreEqual(count, 20);

        }



        [Test()]
        public void Test_Parameters_02() {
            // opensearch-client -p count=1 "https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC"
            OpenSearchClient.baseUrlArg.Add("https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC");
            OpenSearchClient.parameterArgs.Add("count=1");

            int count = client.GetResultCount();
            Assert.AreEqual(count, 1);

        }


        [Test()]
        public void Test_Parameters_03() {
            // opensearch-client -p {http://a9.com/-/spec/opensearch/1.1/}count=1 "https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC" | wc -l
            OpenSearchClient.baseUrlArg.Add("https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/spec/opensearch/1.1/}count=1");

            int count = client.GetResultCount();
            Assert.AreEqual(count, 1);

        }


        [Test()]
        public void Test_Parameters_04() {
            // opensearch-client -p rows=1 "https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC"
            OpenSearchClient.baseUrlArg.Add("https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=SENTINEL1_L1_SLC");
            OpenSearchClient.parameterArgs.Add("rows=1");

            int count = client.GetResultCount();
            Assert.AreEqual(count, 1);

        }

    }

}

