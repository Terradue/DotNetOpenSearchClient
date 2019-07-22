using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {


    /*
    count                         count                    {http://a9.com/-/spec/opensearch/1.1/}count
    startPage                     startPage                {http://a9.com/-/spec/opensearch/1.1/}startPage
    startIndex                    startIndex               {http://a9.com/-/spec/opensearch/1.1/}startIndex
    searchTerms                   q,searchTerms            {http://a9.com/-/spec/opensearch/1.1/}searchTerms
    language                      lang                     {http://a9.com/-/spec/opensearch/1.1/}language
    dct:modified                  update,updated,modified  {http://purl.org/dc/terms/}modified
    t2:downloadOrigin             do                       {http://www.terradue.com/opensearch}downloadOrigin
    time:start                    start                    {http://a9.com/-/opensearch/extensions/time/1.0/}start
    time:end                      stop,end                 {http://a9.com/-/opensearch/extensions/time/1.0/}end
    time:relation                 trel                     {http://a9.com/-/opensearch/extensions/time/1.0/}relation
    geo:box                       box,bbox                 {http://a9.com/-/opensearch/extensions/geo/1.0/}box
    geo:uid                       uid                      {http://a9.com/-/opensearch/extensions/geo/1.0/}uid
    geo:geometry                  geom,geometry            {http://a9.com/-/opensearch/extensions/geo/1.0/}geometry
    geo:relation                  rel                      {http://a9.com/-/opensearch/extensions/geo/1.0/}relation
    dct:subject                   cat                      {http://purl.org/dc/terms/}subject
    t2:extension                                           {http://www.terradue.com/opensearch}extension
    dct:source                                             {http://purl.org/dc/terms/}source
    eop:productType               pt                       {http://a9.com/-/opensearch/extensions/eo/1.0/}productType
    eop:platform                  psn                      {http://a9.com/-/opensearch/extensions/eo/1.0/}platform
    eop:platformSerialIdentifier  psi                      {http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier
    eop:instrument                isn                      {http://a9.com/-/opensearch/extensions/eo/1.0/}instrument
    eop:sensorType                sensor,st                {http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType
    eop:orbitType                 ot                       {http://a9.com/-/opensearch/extensions/eo/1.0/}orbitType
    eop:title                     title                    {http://a9.com/-/opensearch/extensions/eo/1.0/}title
    eop:orbitDirection            od                       {http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection
    eop:track                     track                    {http://a9.com/-/opensearch/extensions/eo/1.0/}track
    eop:frame                     frame                    {http://a9.com/-/opensearch/extensions/eo/1.0/}frame
    eop:swathIdentifier           swath                    {http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier
    eop:cloudCover                cc                       {http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover
    eop:sensorResolution                                   {http://a9.com/-/opensearch/extensions/eo/1.0/}sensorResolution
    t2:landCover                  lc                       {http://www.terradue.com/opensearch}landCover
    t2:doubleCheckGeometry        dcg                      {http://www.terradue.com/opensearch}doubleCheckGeometry
    t2:vendorSpecific                                      {http://www.terradue.com/opensearch}vendorSpecific

     */

    [TestFixture()]
    public class CatalogParameterTests : TestBase {

        XmlNamespaceManager nsm;
        string geometryXPath = "//gml32:posList | //gml:posList | //georss:polygon";

        [Test()]
        public void Test_I01_Identifier_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_I01_Identifier_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000_DOES_NOT_EXIST");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_I02_Identifier_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("geo:uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_I02_Identifier_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("geo:uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000_DOES_NOT_EXIST");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_I03_Identifier_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_I03_Identifier_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000_DOES_NOT_EXIST");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T01_StartStop_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2006-12-04");
            client.Parameters.Add("stop=2006-12-05");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_T01_StartStop_Out_Before() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2004-01-01");
            client.Parameters.Add("stop=2005-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T01_StartStop_Out_After() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2007-01-01");
            client.Parameters.Add("stop=2008-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T02_StartEnd_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2006-12-04");
            client.Parameters.Add("end=2006-12-05");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_T02_StartEnd_Out_Before() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2004-01-01");
            client.Parameters.Add("end=2005-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T02_StartEnd_Out_After() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("start=2007-01-01");
            client.Parameters.Add("end=2008-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T03_StartEnd_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("time:start=2006-12-04");
            client.Parameters.Add("time:end=2006-12-05");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_T03_StartStop_Out_Before_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("time:start=2004-01-01");
            client.Parameters.Add("time:end=2005-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T03_StartStop_Out_After_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("time:start=2007-01-01");
            client.Parameters.Add("time:end=2008-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T04_StartEnd_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2006-12-04");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2006-12-05");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_T04_StartStop_Out_Before_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2004-01-01");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2005-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_T04_StartStop_Out_After_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}start=2007-01-01");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/time/1.0/}end=2008-01-01");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S01_BoundingBox_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("bbox=-77,49,-75,51");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S01_BoundingBox_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("bbox=-87,49,-85,51");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S02_BoundingBox_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geo:box=-77,49,-75,51");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S02_BoundingBox_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geo:box=-87,49,-85,51");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S03_BoundingBox_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}box=-77,49,-75,51");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S03_BoundingBox_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}box=-87,49,-85,51");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S04_Geometry_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geom=POLYGON((-77 49,-75 49,-75 51,-77 51,-77 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S04_Geometry_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geom=POLYGON((-87 49,-85 49,-85 51,-87 51,-87 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S05_Geometry_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geo:geometry=POLYGON((-77 49,-75 49,-75 51,-77 51,-77 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S05_Geometry_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("geo:geometry=POLYGON((-87 49,-85 49,-85 51,-87 51,-87 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_S06_Geometry_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry=-POLYGON((-77 49,-75 49,-75 51,-77 51,-77 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_S06_Geometry_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry=POLYGON((-87 49,-85 49,-85 51,-87 51,-87 49))");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }


        // 

        [Test()]
        public void Test_P01_ProductType_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("pt=ASA_IMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P01_ProductType_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("pt=ASA_WMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P02_ProductType_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:productType=ASA_IMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P02_ProductType_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:productType=ASA_WMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P03_ProductType_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}productType=ASA_IMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P03_ProductType_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}productType=ASA_WMS_1P");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }



        [Test()]
        public void Test_P04_Platform_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("psn=Envisat");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P04_Platform_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("psn=ERS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P05_Platform_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:platform=Envisat");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P05_Platform_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:platform=ERS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P06_Platform_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}platform=Envisat");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P06_Platform_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}platform=ERS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P07_PlatformSerialIdentifier_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("psi=2014-016A");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P07_PlatformSerialIdentifier_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("psi=2014-016B");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P08_PlatformSerialIdentifier_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("eop:platformSerialIdentifier=2014-016A");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P08_PlatformSerialIdentifier_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("eop:platformSerialIdentifier=2014-016B");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P09_PlatformSerialIdentifier_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier=2014-016A");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P09_PlatformSerialIdentifier_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier=2014-016B");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P10_Instrument_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("isn=ASAR");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P10_Instrument_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("isn=MERIS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P11_Instrument_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:instrument=ASAR");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P11_Instrument_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:instrument=MERIS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P12_Instrument_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}instrument=ASAR");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P12_Instrument_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}instrument=MERIS");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P13_OrbitNumber_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("on=24893");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P13_OrbitNumber_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("on=24894");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P14_OrbitNumber_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:orbitNumber=24893");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P14_OrbitNumber_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:orbitNumber=24894");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P15_OrbitNumber_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitNumber=24893");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Ignore("Orbit number queries not supported")]
        public void Test_P15_OrbitNumber_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitNumber=24894");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P16_OrbitDirection_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("od=ASCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P16_OrbitDirection_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("od=DESCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P17_OrbitDirection_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:orbitDirection=ASCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P17_OrbitDirection_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:orbitDirection=DESCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P18_OrbitDirection_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection=ASCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P18_OrbitDirection_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection=DESCENDING");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P19_Track_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("track=290");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P19_Track_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("track=291");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P20_Track_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:track=290");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P20_Track_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:track=291");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P21_Track_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}track=290");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P21_Track_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}track=291");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P22_Frame_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("frame=974");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P22_Frame_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("frame=975");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P23_Frame_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:frame=974");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P23_Frame_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("eop:frame=975");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P24_Frame_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}frame=974");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P24_Frame_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/esar/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}frame=975");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P25_SwathIdentifier_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("swath=EW");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P25_SwathIdentifier_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("swath=EX");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P26_SwathIdentifier_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("eop:frame=EW");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P26_SwathIdentifier_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("eop:swathIdentifier=EX");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P27_SwathIdentifier_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier=EW");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P27_SwathIdentifier_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel1/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S1A_EW_GRDM_1SDH_20171222T201141_20171222T201241_019820_021B6C_98AD");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier=EX");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P28_CloudCoverage_In() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("cc=[60,70]");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P28_CloudCoverage_Out() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("cc=[70,80]");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P29_CloudCoverage_In_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("eop:cloudCover=[60,70]");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P29_CloudCoverage_Out_Prefix() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("eop:cloudCover=[70,80]");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

        [Test()]
        public void Test_P30_CloudCoverage_In_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover=[60,70]");

            int count = client.GetResultCount();
            Assert.AreEqual(1, count);
        }

        [Test()]
        public void Test_P30_CloudCoverage_Out_Full() {
            OpenSearchClient client = CreateTestClient("https://catalog.terradue.com/sentinel2/search", "identifier");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid=S2A_MSIL1C_20181005T071741_N0206_R006_T43WFV_20181005T084018");
            client.Parameters.Add("{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover=[70,80]");

            int count = client.GetResultCount();
            Assert.AreEqual(0, count);
        }

    }

}
