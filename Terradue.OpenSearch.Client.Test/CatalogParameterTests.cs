using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    [TestFixture()]
    public class CatalogParameterTests : TestBase {

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

        }

        [Test()]
        public void Test_StartStop_In() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2006-12-04");
            OpenSearchClient.parameterArgs.Add("stop=2006-12-05");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_StartStop_Out_Before() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2004-01-01");
            OpenSearchClient.parameterArgs.Add("stop=2005-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartStop_Out_After() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2007-01-01");
            OpenSearchClient.parameterArgs.Add("stop=2008-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartEnd_In() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2006-12-04");
            OpenSearchClient.parameterArgs.Add("end=2006-12-05");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_StartEnd_Out_Before() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2004-01-01");
            OpenSearchClient.parameterArgs.Add("end=2005-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartEnd_Out_After() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("start=2007-01-01");
            OpenSearchClient.parameterArgs.Add("end=2008-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartEnd_In_Prefix() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("time:start=2006-12-04");
            OpenSearchClient.parameterArgs.Add("time:end=2006-12-05");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_StartStop_Out_Before_Prefix() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("time:start=2004-01-01");
            OpenSearchClient.parameterArgs.Add("time:end=2005-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartStop_Out_After_Prefix() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("time:start=2007-01-01");
            OpenSearchClient.parameterArgs.Add("time:end=2008-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartEnd_In_Full() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2006-12-04");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2006-12-05");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_StartStop_Out_Before_Full() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2004-01-01");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2005-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_StartStop_Out_After_Full() {
            OpenSearchClient.baseUrlArg.Add("https://catalog.terradue.com/esar/search");

            OpenSearchClient.timeout = 10000;
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2007-01-01");
            OpenSearchClient.parameterArgs.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2008-01-01");
            OpenSearchClient.metadataPaths.Add("identifier");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

    }

}
