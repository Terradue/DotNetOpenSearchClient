using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using HtmlAgilityPack;
using log4net;
using Terradue.OpenSearch.Model.CustomExceptions;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Ogc;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable {




    class Sentinel1AuxOpenSearchRequest : OpenSearchRequest {

        static Regex identifierRegex = new Regex(@"^(?'mission'\w{3})_OPER_AUX_(?'type'\w{6})_(?'system'\w{4})_(?'proddate'\d{8}T\d{6})_V(?'startdate'\w{15})_(?'stopdate'\w{15})$");
        NameValueCollection parameters;
        readonly Uri auxSearchUrl;

        XmlSerializer eeser = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File));

        private static readonly ILog log = LogManager.GetLogger(typeof(Sentinel1AuxOpenSearchRequest));

        Uri auxBaseUrl;



        public Sentinel1AuxOpenSearchRequest(Uri auxSearchUrl, NameValueCollection parameters) : base(new OpenSearchUrl(auxSearchUrl), "application/atom+xml") {
            this.auxSearchUrl = auxSearchUrl;
            var tmpurl = new UriBuilder(auxSearchUrl);
            tmpurl.Path = "";
            tmpurl.Query = "";
            this.auxBaseUrl = tmpurl.Uri;
            this.parameters = parameters;
        }

        public override NameValueCollection OriginalParameters {
            get {
                return parameters;
            }

            set {
                parameters = value;
            }
        }

        public override IOpenSearchResponse GetResponse() {

            AtomFeed feed = new AtomFeed();

            int count, startIndex;
            if (parameters["count"] == "unlimited") {
                count = 10000;
            } else if (String.IsNullOrEmpty(parameters["count"]) || !Int32.TryParse(parameters["count"], out count)) {
                count = 20;
            }
            if (String.IsNullOrEmpty(parameters["startIndex"]) || !Int32.TryParse(parameters["startIndex"], out startIndex)) startIndex = 1;

            string type = string.IsNullOrEmpty(parameters["auxtype"]) ? "aux_resorb" : parameters["auxtype"];
            bool withOrbits = parameters["orbits"] == "true";

            bool partialAtom = false;

            List<AtomItem> items = new List<AtomItem>();

            string uid = null;
            int day = 0, dayCount = 1;
            DateTime startDate = DateTime.UtcNow, stopDate = DateTime.UtcNow;
            if (!String.IsNullOrEmpty(parameters["uid"])) {
                uid = parameters["uid"];
                Match match = identifierRegex.Match(uid);
                if (match.Success && DateTime.TryParseExact(match.Groups["proddate"].Value, "yyyyMMddTHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out startDate)) {
                    startDate = startDate.ToUniversalTime();
                    count = 1;
                    startIndex = 1;
                }
            }
            if (!String.IsNullOrEmpty(parameters["start"]) && DateTime.TryParse(parameters["start"], DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out startDate)) {
                startDate = startDate.ToUniversalTime();
            }
            if (!String.IsNullOrEmpty(parameters["stop"]) && DateTime.TryParse(parameters["stop"], DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out stopDate)) {
                stopDate = stopDate.ToUniversalTime();
                dayCount = (stopDate - startDate).Days + 1;
            }



            Dictionary<string, Uri> list = new Dictionary<string, Uri>();
            int index = 0;
            while (day < dayCount) {
                log.DebugFormat("Querying day {0}/{1}: {2:yyyy-MM-dd}", day + 1, dayCount, startDate.AddDays(day));

                if (auxBaseUrl.Host == "qc.sentinel1.eo.esa.int") auxBaseUrl = new Uri(auxBaseUrl.AbsoluteUri.Replace("qc.sentinel1.eo.esa.int", "aux.sentinel1.eo.esa.int"));
                Uri url = BuildUrl(auxBaseUrl, type, startDate.AddDays(day));

                var request = WebRequest.Create(url);

                HtmlDocument doc = new HtmlDocument();

                using (var response = request.GetResponse()) {
                    doc.Load(response.GetResponseStream());
                }

                var links = doc.DocumentNode.SelectNodes("/html/body/div/pre/a");

                if (links == null) break;

                int addedCount = 0;
                int skippedCount = 0;

                foreach (HtmlNode link in links) {
                    HtmlAttribute href = link.Attributes["href"];
                    string text = link.InnerText;
                    if (!text.EndsWith(".EOF", StringComparison.InvariantCulture)) continue;

                    index++;

                    if (uid != null && String.Format("{0}.EOF", uid) != text) continue;

                    if (index < startIndex) {
                        skippedCount++;
                        continue;
                    }

                    Uri hrefUrl = new Uri(href.Value, UriKind.RelativeOrAbsolute);
                    Uri productUrl;
                    if (hrefUrl.IsAbsoluteUri) {
                        productUrl = hrefUrl;
                    } else {
                        Uri.TryCreate(url, hrefUrl, out productUrl);
                    }
                    addedCount++;
                    list.Add(link.InnerText, productUrl);
                    if (uid != null || list.Count >= count) break;
                }
                if (uid != null) log.DebugFormat("Items skipped: {0}, added: {1}", skippedCount, addedCount);

                if (list.Count >= count) break; // break also from day loop

                day++;
            }

            try {
                items.AddRange(BuildAtomItem(list, withOrbits));
            } catch (PartialAtomException e) {
                items.AddRange(e.Items);
                partialAtom = true;

            }

            feed.Items = items;

           if (partialAtom) {
                return new PartialAtomSearchResponse(feed);
           }
            
           return new Terradue.OpenSearch.Response.AtomOpenSearchResponse(feed);
        }



        IEnumerable<AtomItem> BuildAtomItem(IEnumerable<KeyValuePair<string, Uri>> products, bool withOrbits) {
            
            List<AtomItem> items = new List<AtomItem>();

            bool partial = false;
            foreach (var product in products)
            {
                try {
                    var item = CreateItemFromLink(product.Key, product.Value, withOrbits);
                    if (item != null)
                        items.Add(item);
                }
                catch (Exception) {
                        partial = true;
                        log.Warn("Ommitting corrupted xml: " + product.Value);
                }
            }

            if (partial) {
                throw new PartialAtomException("Atom is partial " , items);
            }
            
            return items;
        }



        AtomItem CreateItemFromLink(string key, Uri url, bool withOrbits) {
            string identifier = key.Replace(".EOF", "");
            Match match = identifierRegex.Match(identifier);

            if (!match.Success)
                return null;

            AtomItem item = new AtomItem(identifier, string.Format("{0} {1} {2} {3}", match.Groups["mission"].Value,
                                                                   match.Groups["type"].Value,
                                                                   match.Groups["startdate"].Value,
                                                                   match.Groups["stopdate"].Value),
                                        url,
                                         identifier,
                                        DateTimeOffset.ParseExact(match.Groups["proddate"].Value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime());

            DateTime start = DateTime.ParseExact(match.Groups["startdate"].Value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            DateTime stop = DateTime.ParseExact(match.Groups["stopdate"].Value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

            item.Identifier = identifier;
            item.PublishDate = DateTimeOffset.ParseExact(match.Groups["proddate"].Value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            item.Links.Add(SyndicationLink.CreateMediaEnclosureLink(url, "application/xml", 0));
            item.ElementExtensions.Add("polygon", "http://www.georss.org/georss", "-90 -180 -90 180 90 180 90 -180 -90 -180");
            item.ElementExtensions.Add("date", "http://purl.org/dc/elements/1.1/", string.Format("{0}/{1}", start.ToString("O"), stop.ToString("O")));

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo = OrbToEo(identifier, match.Groups["mission"].Value, match.Groups["type"].Value, start, stop, item.PublishDate);
            if (eo != null)
            {
                log.DebugFormat("EOP extension created from {0}", url);
                item.ElementExtensions.Add(eo.CreateReader());
            }

            if (withOrbits)
            {

                var request = HttpWebRequest.Create(url);

                using (var response = request.GetResponse())
                {
                    Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File eefile = (Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File)eeser.Deserialize(response.GetResponseStream());
                    item.ElementExtensions.Add(GenerateOrbitsExtension(eefile));
                }
            }

            return item;

        }



        static Uri BuildUrl(Uri auxSearchUrl, string type, DateTime date) {
            UriBuilder url = new UriBuilder(auxSearchUrl);
            url.Path += String.Format("/{0}/{1:yyyy/MM/dd}/", String.IsNullOrEmpty(type) ? "RESORB" : Regex.Replace(type, "^aux_", String.Empty).ToUpper(), date);
            var qs = HttpUtility.ParseQueryString(String.Empty);
            return url.Uri;

        }



        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType OrbToEo(string identifier, string mission, string type, DateTime start, DateTime stop, DateTimeOffset published) {

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationType();

            eo.EopMetaDataProperty = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataPropertyType();
            eo.EopMetaDataProperty.EarthObservationMetaData = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataType();
            eo.EopMetaDataProperty.EarthObservationMetaData.identifier = identifier;

            eo.procedure = new Terradue.ServiceModel.Ogc.Om20.OM_ProcessPropertyType();
            eo.procedure.Eop21EarthObservationEquipment = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationEquipmentType();
            eo.procedure.Eop21EarthObservationEquipment.platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformPropertyType();
            eo.procedure.Eop21EarthObservationEquipment.platform.Platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformType();
            eo.procedure.Eop21EarthObservationEquipment.platform.Platform.shortName = mission;
            eo.procedure.Eop21EarthObservationEquipment.platform.Platform.orbitType = Terradue.ServiceModel.Ogc.Eop21.OrbitTypeValueType.LEO;
            eo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.NOMINAL;
            eo.EopMetaDataProperty.EarthObservationMetaData.productType = type;
            eo.EopMetaDataProperty.EarthObservationMetaData.processing = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType[1];
            eo.EopMetaDataProperty.EarthObservationMetaData.processing[0] = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType();
            eo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationType();
            eo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation.processingCenter = new ServiceModel.Ogc.Gml321.CodeListType();
            eo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation.processingCenter.Text = "POD";
            eo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation.processingDate = published.DateTime;

            eo.phenomenonTime = new Terradue.ServiceModel.Ogc.Om20.TimeObjectPropertyType();
            eo.phenomenonTime.GmlTimePeriod = new Terradue.ServiceModel.Ogc.Gml321.TimePeriodType();
            if (start != null)
            {
                eo.phenomenonTime.GmlTimePeriod.beginPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
                eo.phenomenonTime.GmlTimePeriod.beginPosition.Value = start.ToString("O");
            }
            if (stop != null)
            {
                eo.phenomenonTime.GmlTimePeriod.endPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
                eo.phenomenonTime.GmlTimePeriod.endPosition.Value = stop.ToString("O");
            }



            return eo;

        }

        public static SyndicationElementExtension GenerateOrbitsExtension(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File file) {

            SyndicationElementExtension extension = new SyndicationElementExtension(GetS1OrbitsFromEE(file), Terradue.Metadata.EarthObservation.Model.orbitListType.OrbitsSerializer);

            return extension;
        }


        public static Terradue.Metadata.EarthObservation.Model.orbitListType GetS1OrbitsFromEE(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File file) {

            Terradue.Metadata.EarthObservation.Model.orbitListType orbits = new Terradue.Metadata.EarthObservation.Model.orbitListType();

            orbits.orbit = file.Data_Block.List_of_OSVs.OSV.
                Select<Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_FileData_BlockList_of_OSVsOSV, Terradue.Metadata.EarthObservation.Model.orbitType>(o =>
                {
                    Terradue.Metadata.EarthObservation.Model.orbitType orbit = new Terradue.Metadata.EarthObservation.Model.orbitType();
                    orbit.time = DateTime.SpecifyKind(DateTime.Parse(o.UTC.Replace("UTC=", "")), DateTimeKind.Utc);
                    orbit.frame = Terradue.Metadata.EarthObservation.Model.referenceFrameType.EarthFixed;
                    orbit.absoluteOrbit = int.Parse(o.Absolute_Orbit.Replace("+", ""));
                    orbit.position = new double[] { o.X.Value, o.Y.Value, o.Z.Value };
                    orbit.velocity = new double[] { o.VX.Value, o.VY.Value, o.VZ.Value };
                    return orbit;
                }).ToArray();

            return orbits;

        }
    }
}