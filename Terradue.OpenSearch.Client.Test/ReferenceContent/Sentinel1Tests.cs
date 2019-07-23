using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test.ReferenceContent {

    [TestFixture()]
    public class NewTests : TestBase {

        XmlNamespaceManager nsm;

        [SetUp]
        public void SetUpClient() {
            LoadCredentials();
        }

        [Test()]
        public void Test_Sentinel1_Scihub() {
        }

        [Test()]
        public void Test_Sentinel1_OpenSearch() {
        }


        [Test()]
        public void Test_Sentinel1_GeoSquare() {
        }


    }

}

