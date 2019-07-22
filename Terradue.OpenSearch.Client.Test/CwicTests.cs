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
    public class CwicTests : TestBase {

        [Test()]
        public void Landsat8EOP() {
            OpenSearchClient client = CreateTestClient("http://cwic.wgiss.ceos.org/opensearch/datasets/Landsat_8/osdd.xml?clientId=foo", "{}");
            client.Parameters.Add("count=1");
            client.Parameters.Add("timeStart=2016-05-03T00:27:10Z");
            client.Parameters.Add("timeEnd=2016-05-03T00:27:40Z");
            client.QueryModel = "EOP";

            XmlDocument doc = client.GetXmlResult();

        }


    }
}

