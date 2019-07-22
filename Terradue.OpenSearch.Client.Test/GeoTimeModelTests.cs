using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Terradue.OpenSearch.Client.Test {
    
    [TestFixture()]
    public class GeoTimeModelTests : TestBase {

        [Test()]
        public void WktFromRdf() {
            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf", "wkt");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            string wkt = Encoding.UTF8.GetString(ms.ToArray());
            Regex wktRegex = new Regex(@"^((MULTIPOLYGON|POLYGON)\(+)(.*?)(\)+)\n?$");
            Match wktMatch = wktRegex.Match(wkt);
            if (!wktMatch.Success) Assert.Fail("Invalid WKT: {0}", wkt);

            string newWkt = null;

            Regex numberRegex = new Regex("[^ ,]+");
            MatchCollection numberMatches = numberRegex.Matches(wktMatch.Groups[3].Value);
            int count = 0;
            foreach (Match numberMatch in numberMatches) {
                double coordinate;
                if (!Double.TryParse(numberMatch.Value, out coordinate)) Assert.Fail("Not a valid coordinate: {0}", numberMatch.Value);
                if (newWkt == null) newWkt = String.Empty;
                else if (count % 2 == 0) newWkt += ",";
                else newWkt += " ";
                newWkt += String.Format("{0:#.####}", coordinate);
                count++;
            }

            newWkt = String.Format("{0}{1}{2}", wktMatch.Groups[1].Value, newWkt, wktMatch.Groups[4].Value);

            if (newWkt == "MULTIPOLYGON(((-121.7585 37.4894,-120.9409 37.6015,-121.1878 38.7426,-122.0047 38.6307,-121.7585 37.4894)))"
                    || newWkt == "POLYGON((-121.7585 37.4894,-120.9409 37.6015,-121.1878 38.7426,-122.0047 38.6307,-121.7585 37.4894))") {
                Assert.Pass();
            } else {
                Assert.Fail("Incorrect footprint: {0}\n(expected: an equivalent of 'POLYGON((-121.7585 37.4894,-120.9409 37.6015,-121.1878 38.7426,-122.0047 38.6307,-121.7585 37.4894))'", wkt);
            }

        }

        [Test()]
        public void EnclosureFromRdf() {
            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf", "enclosure");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            string enclosure = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("https://eo-virtual-archive4.esa.int/supersites/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1\n", enclosure);

        }

        [Test()]
        public void StartdateFromRdf() {
            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/rdf", "startdate");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            string date = Encoding.UTF8.GetString(ms.ToArray());
            Assert.GreaterOrEqual(date.Length, 23);
            string date2 = date.Substring(0, 23);
            Assert.AreEqual("2012-04-07T06:12:42.911", date2);

        }

        [Test()]
        public void EnddateFromRdf() {
            OpenSearchClient client = CreateTestClient("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_061242_000000173113_00250_52850_6352.N1/atom", "enddate");

            MemoryStream ms = new MemoryStream();
            client.ProcessQuery(ms);
            ms.Seek(0, SeekOrigin.Begin);
            string date = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("2012-04-07T06:12:59.1460000Z\n", date);

        }
            
    }
}

