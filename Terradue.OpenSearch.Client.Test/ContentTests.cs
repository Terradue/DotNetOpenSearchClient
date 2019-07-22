using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    [TestFixture()]
    public class ContetTests : TestBase {

        XmlNamespaceManager nsm;
        string eoNodePath = "//eop:EarthObservation | //opt:EarthObservation | //sar:EarthObservation";

        [SetUp]
        public void SetUpClient() {

            XmlDocument doc = new XmlDocument();
            nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("eop", "http://www.opengis.net/eop/2.1");
            nsm.AddNamespace("opt", "http://www.opengis.net/opt/2.1");
            nsm.AddNamespace("sar", "http://www.opengis.net/sar/2.1");
        }

        [Test()]
        public void Test_Sentinel1() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "{}");
            client.Parameters.Add("uid=S1A_WV_OCN__2SSV_20190505T235215_20190506T001205_027099_030DCE_A66C");

            XmlDocument doc = client.GetXmlResult();

            XmlNode eoNode = doc.SelectSingleNode(eoNodePath, nsm);
            Assert.NotNull(eoNode, "EarthObservation not found");

            Console.WriteLine("HASH {0}", GetHashString(eoNode.OuterXml));

            Assert.AreEqual("C42504BAA568FDD3FB6579B2D681F53E05C01F2F073E718D20223423434CEFBE", GetHashString(eoNode.OuterXml));

        }


        [Test()]
        public void Test_Sentinel2() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "{}");
            client.Parameters.Add("uid=S2B_MSIL2A_20190505T235629_N0211_R116_T60VVR_20190506T020948");

            XmlDocument doc = client.GetXmlResult();

            XmlNode eoNode = doc.SelectSingleNode(eoNodePath, nsm);
            Assert.NotNull(eoNode, "EarthObservation not found");

            Console.WriteLine("HASH {0}", GetHashString(eoNode.OuterXml));

            Assert.AreEqual("CB2491462AAF53EDB3E2F9CE4BBD96888EC6FDDB89A4CFFC597D47CA4747FC6D", GetHashString(eoNode.OuterXml));

        }



        [Test()]
        public void Test_Sentinel3() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel3/search", "{}");
            client.Parameters.Add("uid=S3A_SL_2_LST____20190305T235619_20190306T013718_20190307T104215_6059_042_116______LN2_O_NT_003");

            XmlDocument doc = client.GetXmlResult();
            doc.Save("s3.doc");

            XmlNode eoNode = doc.SelectSingleNode(eoNodePath, nsm);
            Assert.NotNull(eoNode, "EarthObservation not found");

            Console.WriteLine("HASH {0}", GetHashString(eoNode.OuterXml));

            Assert.AreEqual("E5CFC2CDFC3C3D11861E3469E7DB82017C18B9E03022450AEAC9EE9ACC755834", GetHashString(eoNode.OuterXml));

        }


        [Test()]
        public void Test_Landsat7() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/landsat7/search", "{}");
            client.Parameters.Add("uid=LE07_L1TP_204033_20171130_20171226_01_T1");

            XmlDocument doc = client.GetXmlResult();
            doc.Save("s3.doc");

            XmlNode eoNode = doc.SelectSingleNode(eoNodePath, nsm);
            Assert.NotNull(eoNode, "EarthObservation not found");

            Console.WriteLine("HASH {0}", GetHashString(eoNode.OuterXml));

            Assert.AreEqual("D9312324E632FB5BAE75F6B905CCE14B6E59FE1A51CFFE886BB6DE422E06F7B8", GetHashString(eoNode.OuterXml));

        }


        [Test()]
        public void Test_Landsat8() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/landsat8/search", "{}");
            client.Parameters.Add("uid=LC08_L1GT_095022_20190305_20190309_01_T2");

            XmlDocument doc = client.GetXmlResult();
            doc.Save("s3.doc");

            XmlNode eoNode = doc.SelectSingleNode(eoNodePath, nsm);
            Assert.NotNull(eoNode, "EarthObservation not found");

            Console.WriteLine("HASH {0}", GetHashString(eoNode.OuterXml));

            Assert.AreEqual("5BD92B7D38EEC7786FA9ECDA94372C82F4CAB065E55E701D61FFDEBE5AC3C10C", GetHashString(eoNode.OuterXml));

        }


        public static string GetHashString(string inputString) {
            HashAlgorithm algorithm = SHA256.Create();
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

    }

}

