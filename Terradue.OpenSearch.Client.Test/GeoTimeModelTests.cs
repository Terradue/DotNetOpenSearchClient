using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class GeoTimeModelTests {

        OpenSearchClient client;

        [SetUp]
        public void SetUpClient(){
            
            client = new OpenSearchClient();

            client.Initialize();

            OpenSearchClient.baseUrlArg = new List<string>();

            OpenSearchClient.metadataPaths = new List<string>();

            OpenSearchClient.parameterArgs = new List<string>();

            OpenSearchClient.dataModelParameterArgs = new List<string>();

        }

        [Test()]
        public void WktFromRdf() {

            OpenSearchClient.baseUrlArg.Add("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf");
            OpenSearchClient.metadataPaths.Add("wkt");
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var wkt = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("MULTIPOLYGON(((-121.758523 37.489391,-120.940872 37.601472,-121.187833 38.74263,-122.004685 38.630714,-121.758523 37.489391)))\n", wkt);

        }

        [Test()]
        public void EnclosureFromRdf() {

            OpenSearchClient.baseUrlArg.Add("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf");
            OpenSearchClient.metadataPaths.Add("enclosure");
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var enclosure = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("https://eo-virtual-archive4.esa.int/supersites/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1\n", enclosure);

        }

        [Test()]
        public void StartdateFromRdf() {

            OpenSearchClient.baseUrlArg.Add("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf");
            OpenSearchClient.metadataPaths.Add("startdate");
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var wkt = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("2012-04-07T06:12:42.9110000Z\n", wkt);

        }

        [Test()]
        public void EnddateFromRdf() {

            OpenSearchClient.baseUrlArg.Add("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf");
            OpenSearchClient.metadataPaths.Add("enddate");
            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var wkt = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("2012-04-07T06:12:59.1460000Z\n", wkt);

        }
            
    }
}

