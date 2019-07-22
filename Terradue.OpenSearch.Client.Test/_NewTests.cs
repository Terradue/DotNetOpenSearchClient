using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    [TestFixture()]
    public class NewTests : TestBase {

        XmlNamespaceManager nsm;
        string geometryXPath = "//gml32:posList | //gml:posList | //georss:polygon";

        [SetUp]
        public void SetUpClient() {

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
        public void Test_Landsat8_01() {
            // opensearch-client --max-retries 0 --time-out 600000 -m EOP -p profile=eop -p count=unlimited --pagination 100 -p start=2018-09-26 -p stop=2018-09-26 -p pi=LANDSAT_8_C1 -o earthexplorer_usgs_gov.list http://earthexplorer.usgs.gov identifier
            Credential credential = GetCredential("Landsat8_01", true);

            OpenSearchClient client = CreateTestClient("http://earthexplorer.usgs.gov", "identifier");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };

            client.Timeout = 600000;
            client.QueryModel = "EOP";
            client.Pagination = 100;
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("profile=eop");
            client.Parameters.Add("pi=LANDSAT_8_C1");
            client.Parameters.Add("start=2018-09-26");
            client.Parameters.Add("stop=2018-09-26");

            int count = client.GetResultCount();
            Assert.Greater(count, 700);

        }

        [Test()]
        public void Test_Landsat8_02() {
            // opensearch-client -v --max-retries 0 --time-out 600000 -m EOP -p profile=eop -p count=unlimited --pagination 100 -p start= -p stop= -p modified=2019-06-21 -p pi=LANDSAT_8_C1 -o landsat_datasets.atom http://earthexplorer.usgs.gov {}
            Credential credential = GetCredential("Landsat8_02", true);

            OpenSearchClient client = CreateTestClient("http://earthexplorer.usgs.gov", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };

            client.Timeout = 600000;
            client.QueryModel = "EOP";
            client.Pagination = 100;
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("profile=eop");
            client.Parameters.Add("pi=LANDSAT_8_C1");
            client.Parameters.Add("start=");
            client.Parameters.Add("stop=");
            client.Parameters.Add("modified=2019-06-21");
            client.OutputFilePath = "Landsat_Dataset.atom";

            string[] feeds = Directory.GetFiles(".", "Landsat_Dataset.atom*");

            client.ProcessQuery();
            int count = 0;
            foreach (string feed in feeds) {
                XmlDocument doc = new XmlDocument();
                doc.Load(feed);
                XmlNodeList entryNodes = doc.SelectNodes("atom:feed/atom:entry", nsm);
                count += entryNodes.Count;
            }
            Assert.Greater(700, count);

        }

        [Test()]
        public void Test_CosmoSkyMed() {
            // opensearch-client --adjust-identifiers -f rdf -p count=unlimited --pagination 100 -p modified_start=2019-06-20T00:00:00Z -p modified_stop=2019-06-21T00:00:00Z http://eo-virtual-archive4.esa.int/search/COSMOSKYMED/rdf {} > query_results.rdf
            // DONE

            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/COSMOSKYMED/rdf", "identifier");

            client.AdjustIdentifiers = true;
            client.QueryFormat = "rdf";
            client.Pagination = 100;
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("modified_start=2019-06-20T00:00:00");
            client.Parameters.Add("modified_stop=2019-06-21T00:00:00Z");

            int count = client.GetResultCount();

        }

        [Test()]
        public void Test_Fedeo() {
            // opensearch-client -p count=unlimited -p recordSchema=om -p startDate=2019-06-20T00:00:00Z -p endDate=2019-06-21T00:00:00Z "http://fedeo.esa.int/opensearch/series/urn:eop:DLR:EOWEB:TSX-1.SAR.L1b-ScanSAR/datasets/?httpAccept=application/atom%2Bxml" {} | xmllint --format - >query_results.xml

            OpenSearchClient client = CreateTestClient("http://fedeo.esa.int/opensearch/series/urn:eop:DLR:EOWEB:TSX-1.SAR.L1b-ScanSAR/datasets/?httpAccept=application/atom%2Bxml", "identifier");

            client.Parameters.Add("count=20");
            client.Parameters.Add("recordSchema=om");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2019-06-05T00:00:00Z");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2019-06-06T00:00:00Z");

            int count = client.GetResultCount();
            Assert.AreEqual(7, count);

            client = CreateTestClient("http://fedeo.esa.int/opensearch/series/urn:eop:DLR:EOWEB:TSX-1.SAR.L1b-ScanSAR/datasets/?httpAccept=application/atom%2Bxml", "identifier");

            client.Parameters.Clear();
            client.Parameters.Add("count=20");
            client.Parameters.Add("recordSchema=om");
            client.Parameters.Add("startDate=2019-06-20T00:00:00Z");
            client.Parameters.Add("endDate=2019-06-21T00:00:00Z");

            count = client.GetResultCount();
            Assert.AreEqual(7, count);
        }

    }

}

