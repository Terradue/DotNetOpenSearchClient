using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class BugTests : TestBase {

        [OneTimeSetUp]
        public void SetUpClient() {
            LoadCredentials();
        }

        [Ignore("different download location (catalog URL previously data2.terradue.com)")]
        public void Test_Issue5() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search?format=atom", "enclosure");
            client.Parameters.Add("uid=S1A_IW_SLC__1SDV_20160524T160839_20160524T160906_011403_011568_F048");

            //OpenSearchClient.dataModelParameterArgs.Add("enclosure:scheme=http");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("http://download.terradue.com/sentinel-1/2016/05/24/S1A_IW_SLC__1SDV_20160524T160839_20160524T160906_011403_011568_F048.zip\n", enclosure);

        }

		[Ignore("product not available")]
		public void Test_Issue_19006() {
            OpenSearchClient client = CreateTestClient("https://data2.terradue.com:443/eop/landsat8/series/ecop-gran-paradiso/search?format=atom&uid=LC81950292016117LGN00", "wrsLatitudeGrid");
			client.QueryModel = "EOP";

            MemoryStream ms = new MemoryStream();
			client.ProcessQuery(ms);
			ms.Seek(0, SeekOrigin.Begin);
			var enclosure = Encoding.UTF8.GetString(ms.ToArray());

			Assert.AreEqual("29\n", enclosure);

		}

        //[Test()]
        public void Test_ValueTooLarge() {
            Credential credential = GetCredential("ValueTooLarge", true);

            OpenSearchClient client = CreateTestClient("https://scihub.copernicus.eu/apihub/odata/v1", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.QueryModel = "Scihub";
            client.Parameters.Add("profile=eop");
            client.Parameters.Add("count=1");
            client.Parameters.Add("uid=S1A_S6_SLC__1SSV_20160601T055814_20160601T055843_011513_011908_173A");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("s3://sentinel-1/2015/05/S1A_IW_GRDH_1SDV_20150522T154256_20150522T154321_006036_007CB1_593F.zip\n", enclosure);

        }
	    
	    
	    
	    [Test()]
	    public void Test_DataAuthor_84() {
            //opensearch-client -p count=20 -p startIndex=1 -m EOP -p start=2017-11-30 -p auxtype=aux_resorb -p orbits=true https://aux.sentinel1.eo.esa.int/ identifier

            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
		    client.Parameters.Add("profile=eop");
            client.Parameters.Add("count=20");
		    client.Parameters.Add("start=2017-11-30");
		    client.Parameters.Add("startIndex=1");
		    client.Parameters.Add("auxtype=aux_resorb");
		    client.Parameters.Add("orbits=true");
		    client.QueryModel = "EOP";

		    MemoryStream ms = new MemoryStream();

		    client.ProcessQuery(ms);

		    ms.Seek(0, SeekOrigin.Begin);

		    var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Console.WriteLine(enclosure);

	    }
	    
        [Test()]
        public void Test_DataAuthor_123() {
            Credential credential = GetCredential("DataAuthor_123", true);

            OpenSearchClient client = new OpenSearchClient();
            client.Initialize();

            string[] args = {
                "-m", "Scihub",
                "-p", "modified=2018-08-28T13:00:00Z/2018-08-28T14:00:00Z/",
                "-p", "count=unlimited",
                "--pagination", "100",
                "-a", "t2user:1psed1xiT",
                "http://scihub.terradue.com/apihub",
                "id,identifier,published,updated"
            };

            client.GetArgs(args);

            int count = client.GetResultCount();

            Console.WriteLine("Products found: {0}", count);
            Assert.IsTrue(count > 100, "The number of products seems too small");

        }


        [Test()]
        public void Test_DataAuthor_164() {
            Credential credential = GetCredential("DataAuthor_164", true);

            OpenSearchClient client = CreateTestClient("http://earthexplorer.usgs.gov", "{}");
            client.NetCreds = new List<NetworkCredential> { new NetworkCredential(credential.Username, credential.Password) };
            client.QueryModel = "EOP";
            client.Parameters.Add("profile=eop");
            client.Parameters.Add("startPage=1");
            client.Parameters.Add("count=20");
            client.Parameters.Add("start=2019-05-01");
            client.Parameters.Add("stop=2019-05-02");
            client.Parameters.Add("pi=LANDSAT_8_C1");

            long size = 0;
            using (MemoryStream ms = new MemoryStream()) {
                client.ProcessQuery(ms);
                ms.Seek(0, SeekOrigin.Begin);
                size = ms.Length;
            }

            Assert.Greater(size, 300000);

        }

        [Test()]
        public void Test_DataAuthor_165() {
            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/COSMOSKYMED/rdf", "{}");
            client.QueryModel = "EOP";
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("modified_start=2019-05-01");
            client.Parameters.Add("modified_stop=2019-05-26");
            client.Pagination = 100;
            client.AdjustIdentifiers = true;

            long size = client.GetResultSize();

            Assert.Greater(size, 150000);

        }

    }

}

