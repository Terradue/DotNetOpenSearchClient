using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    [TestFixture()]
    public class Sentinel1AuxTests : TestBase {

        XmlNamespaceManager nsm;

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
        }

        [Test()]
        public void ListTest() {
            OpenSearchClient client = CreateTestClient("https://aux.sentinel1.eo.esa.int/", "identifier");
            client.QueryModel = "EOP";
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("startIndex=1");
            client.Parameters.Add("start=2019-05-14");
            client.Parameters.Add("stop=2019-05-15");
            client.Parameters.Add("auxtype=aux_resorb");

            int totalCount;
            string firstExpected = null;

            using (MemoryStream ms = new MemoryStream()) {
                client.ProcessQuery(ms);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                string line;
                int count = 0;
                while ((line = sr.ReadLine()) != null) {
                    Console.WriteLine(line);
                    count++;
                    if (count == 42) firstExpected = line;
                }
                sr.Close();
                totalCount = count;
            }
            Assert.Greater(totalCount, 100);

            client = CreateTestClient("https://aux.sentinel1.eo.esa.int/", "identifier");
            client.QueryModel = "EOP";
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("startIndex=42");
            client.Parameters.Add("start=2019-05-14");
            client.Parameters.Add("stop=2019-05-15");
            client.Parameters.Add("auxtype=aux_resorb");

            string firstActual = null;
            int newCount;
            using (MemoryStream ms = new MemoryStream()) {
                client.ProcessQuery(ms);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                string line;
                int count = 0;
                while ((line = sr.ReadLine()) != null) {
                    count++;
                    if (count == 1) firstActual = line;
                }
                sr.Close();
                newCount = count;
            }
            Assert.AreEqual(totalCount, newCount + 42 - 1);
            Assert.AreEqual(firstExpected, firstActual);

        }

        [Test()]
        public void RedirectTest() {
            OpenSearchClient client = CreateTestClient("https://qc.sentinel1.eo.esa.int", "identifier");
            client.QueryModel = "EOP";
            client.Parameters.Add("count=unlimited");
            client.Parameters.Add("startIndex=1");
            client.Parameters.Add("start=2019-05-14");
            client.Parameters.Add("stop=2019-05-15");
            client.Parameters.Add("auxtype=aux_resorb");

            int count = client.GetResultCount();
            Assert.Greater(count, 100);

        }

        [Test()]
        public void SingleEntryTest() {
            OpenSearchClient client = CreateTestClient("https://qc.sentinel1.eo.esa.int", "{}");
            client.QueryModel = "EOP";
            client.Parameters.Add("uid=S1A_OPER_AUX_RESORB_OPOD_20190515T024532_V20190514T224004_20190515T015734");
            client.Parameters.Add("orbits=true");
            client.Parameters.Add("auxtype=aux_resorb");

            XmlDocument doc = client.GetXmlResult();

            XmlNodeList entryNodes = doc.SelectNodes("atom:feed/atom:entry", nsm);
            Assert.AreEqual(1, entryNodes.Count);

            XmlNodeList orbitNodes = doc.SelectNodes("atom:feed/atom:entry/eop:orbits/eop:orbit/eop:orbitType", nsm);
            Assert.Greater(orbitNodes.Count, 1000);
        }
    }

}

