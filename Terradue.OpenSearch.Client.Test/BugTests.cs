using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class BugTests {

        OpenSearchClient client;

        [SetUp]
        public void SetUpClient(){
            
            client = new OpenSearchClient();

            client.Initialize();

            OpenSearchClient.baseUrlArg = new List<string>();

            OpenSearchClient.metadataPaths = new List<string>();


        }

        [Test()]
        public void Issue15021_1() {

            OpenSearchClient.baseUrlArg.Add("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf");

            OpenSearchClient.metadataPaths.Add("wkt");

            MemoryStream ms = new MemoryStream();

            client.ProcessQuery(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var wkt = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("MULTIPOLYGON(((-121.758523 37.489391,-120.940872 37.601472,-121.187833 38.74263,-122.004685 38.630714,-121.758523 37.489391)))\n", wkt);

        }
            
    }
}

