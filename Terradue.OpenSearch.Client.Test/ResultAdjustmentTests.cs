using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test{
    
    [TestFixture()]
    public class ResultAdjustmentTests : TestBase {

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

            LoadCredentials();
        }

        [Test()]
        public void Test_SciHubPolyon_01() {

            Credential credential = GetCredential("SciHubPolygon_01", true);

            OpenSearchClient client = CreateTestClient("https://scihub.copernicus.eu/apihub", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.QueryModel = "Scihub";
            client.Parameters.Add("uid=S3A_SL_1_RBT____20190414T044653_20190414T044953_20190415T091626_0179_043_290_1440_LN2_O_NT_003");

            XmlDocument doc = client.GetXmlResult();

            int total = 0;
            int corrected = 0;

            XmlNodeList posLists = doc.SelectNodes(geometryXPath, nsm);
            foreach (XmlNode posList in posLists) {
                string text = posList.InnerText;
                if (text.Contains("85.05115 180 85.05115 72.8804")) {
                    total++;
                }
                if (text.Contains("85.05115 180 85.0512 126.4402 85.05115 72.8804")) {
                    total++;
                    corrected++;
                }
            }

            Console.WriteLine("Total polygons to correct: {0}; polygons corrected: {1}", total, corrected);

            Assert.Greater(total, 0);
            Assert.AreEqual(total, corrected);
        }

        [Test()]
        public void Test_SciHubPolyon_02() {

            Credential credential = GetCredential("SciHubPolygon_02", true);

            OpenSearchClient client = CreateTestClient("https://scihub.copernicus.eu/apihub", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.QueryModel = "Scihub";
            client.Parameters.Add("uid=S3A_SY_2_VGP____20190415T074341_20190415T082648_20190416T152039_2587_043_306______LN2_O_NT_002");
            client.Parameters.Add("psn=Sentinel-3");

            XmlDocument doc = client.GetXmlResult();

            int total = 0;
            int corrected = 0;

            XmlNodeList posLists = doc.SelectNodes(geometryXPath, nsm);
            foreach (XmlNode posList in posLists) {
                string text = posList.InnerText;
                if (text.Contains("85.05115 132.658809581381 85.05115 34.7498902383989")) {
                    total++;
                }
                if (text.Contains("85.05115 132.658809581381 85.0512 83.7043 85.05115 34.7498902383989")) {
                    total++;
                    corrected++;
                }
            }

            Console.WriteLine("Total polygons to correct: {0}; polygons corrected: {1}", total, corrected);

            Assert.Greater(total, 0);
            Assert.AreEqual(total, corrected);
        }


        [Test]
        public void Test_SciHubPolyon_03() {

            // DATAAUTHOR-168
            // footprints with scientific notation for coordinates
            // opensearch-client -m Scihub -p uid=S3A_SY_2_VGP____20190415T074341_20190415T082648_20190416T152039_2587_043_306______LN2_O_NT_002 -p psn=Sentinel-3 https://scihub.copernicus.eu/apihub {}

            Credential credential = GetCredential("SciHubPolygon_02", true); // same as other test case

            OpenSearchClient client = CreateTestClient("https://scihub.copernicus.eu/apihub", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.QueryModel = "Scihub";
            client.Parameters.Add("uid=S3A_SY_2_VGP____20190415T074341_20190415T082648_20190416T152039_2587_043_306______LN2_O_NT_002");
            client.Parameters.Add("psn=Sentinel-3");

            XmlDocument doc = client.GetXmlResult();

            int count = 0;

            XmlNodeList posLists = doc.SelectNodes(geometryXPath, nsm);
            foreach (XmlNode posList in posLists) {
                string text = posList.InnerText;
                if (text.Contains(" 1.30829628547697E-18 ")) {
                    count++;
                }
            }

            Console.WriteLine("Occurences of 1.30829628547697E-18: {0}", count);

            Assert.Greater(count, 0);
        }

        [Test()]
        public void Test_VirtualArchiveIdentifier() {

            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/COSMOSKYMED/rdf", "{}");
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("modified_start=2019-05-01");
            client.Parameters.Add("modified_stop=2019-05-26");
            client.Parameters.Add("uid=CSKS1_RAW_B_HI_08_HH_RA_SF_20190524161228_20190524161235");
            client.AdjustIdentifiers = true;

            XmlDocument doc = client.GetXmlResult();

            XmlNode idNode = doc.SelectSingleNode("atom:feed/atom:entry/atom:id", nsm);
            Assert.NotNull(idNode);
            Assert.AreEqual("CSKS1_RAW_B_HI_08_HH_RA_SF_20190524161228_20190524161235", idNode.InnerText);

            XmlNode identifierNode = doc.SelectSingleNode("atom:feed/atom:entry/dc:identifier", nsm);
            Assert.NotNull(identifierNode);
            Assert.AreEqual("CSKS1_RAW_B_HI_08_HH_RA_SF_20190524161228_20190524161235", identifierNode.InnerText);

        }

    }

}

