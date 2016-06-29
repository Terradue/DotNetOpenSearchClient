using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class BugTests {

        OpenSearchClient client;

        [SetUp]
        public void SetUpClient() {
            
            client = new OpenSearchClient();

            client.Initialize();

            OpenSearchClient.baseUrlArg = new List<string>();

            OpenSearchClient.metadataPaths = new List<string>();

            OpenSearchClient.parameterArgs = new List<string>();

            OpenSearchClient.dataModelParameterArgs = new List<string>();

            OpenSearchClient.queryModelArg = "GeoTime";
        }

        [Test()]
        public void Issue5() {

            OpenSearchClient.baseUrlArg.Add("https://data2.terradue.com/eop/s1-cache/dataset/search?format=atom&uid=S1A_IW_SLC__1SDV_20160524T160839_20160524T160906_011403_011568_F048");

            OpenSearchClient.metadataPaths.Add("enclosure");

            OpenSearchClient.dataModelParameterArgs.Add("enclosure:scheme=http");

            MemoryStream ms = new MemoryStream();

            client.ProcessQuery(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("http://download.terradue.com/sentinel-1/2016/05/24/S1A_IW_SLC__1SDV_20160524T160839_20160524T160906_011403_011568_F048.zip\n", enclosure);

        }

		[Test()]
		public void Issue_19006()
		{

			OpenSearchClient.baseUrlArg.Add("https://data2.terradue.com:443/eop/landsat8/series/ecop-gran-paradiso/search?format=atom&uid=LC81950292016117LGN00");

			OpenSearchClient.queryModelArg = "EOP";

			OpenSearchClient.metadataPaths.Add("wrsLatitudeGrid");

			MemoryStream ms = new MemoryStream();

			client.ProcessQuery(ms);

			ms.Seek(0, SeekOrigin.Begin);

			var enclosure = Encoding.UTF8.GetString(ms.ToArray());

			Assert.AreEqual("29\n", enclosure);

		}

        //[Test()]
        public void ValueTooLarge() {

            OpenSearchClient.baseUrlArg.Add("https://scihub.copernicus.eu/apihub/odata/v1");

            OpenSearchClient.metadataPaths.Add("{}");

            OpenSearchClient.parameterArgs.Add("profile=eop");

            OpenSearchClient.parameterArgs.Add("count=1");

            OpenSearchClient.parameterArgs.Add("uid=S1A_S6_SLC__1SSV_20160601T055814_20160601T055843_011513_011908_173A");

            OpenSearchClient.queryModelArg = "Scihub";

            string[] creds = "t2da:t2da".Split(':');
            OpenSearchClient.netCreds = new NetworkCredential(creds[0], creds[1]);

            MemoryStream ms = new MemoryStream();

            client.ProcessQuery(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("s3://sentinel-1/2015/05/S1A_IW_GRDH_1SDV_20150522T154256_20150522T154321_006036_007CB1_593F.zip\n", enclosure);

        }
            
    }
}

