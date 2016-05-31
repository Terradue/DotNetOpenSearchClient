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

            OpenSearchClient.parameterArgs = new List<string>();

            OpenSearchClient.dataModelParameterArgs = new List<string>();
        }

        [Test()]
        public void Issue5() {

            OpenSearchClient.baseUrlArg.Add("https://data2.terradue.com/eop/sentinel1/dataset/search?format=atom&uid=S1A_IW_GRDH_1SDV_20150522T154256_20150522T154321_006036_007CB1_593F");

            OpenSearchClient.metadataPaths.Add("enclosure");

            OpenSearchClient.dataModelParameterArgs.Add("enclosure:scheme=s3");

            MemoryStream ms = new MemoryStream();

            client.ProcessQuery(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var enclosure = Encoding.UTF8.GetString(ms.ToArray());

            Assert.AreEqual("s3://sentinel-1/2015/05/S1A_IW_GRDH_1SDV_20150522T154256_20150522T154321_006036_007CB1_593F.zip\n", enclosure);

        }
            
    }
}

