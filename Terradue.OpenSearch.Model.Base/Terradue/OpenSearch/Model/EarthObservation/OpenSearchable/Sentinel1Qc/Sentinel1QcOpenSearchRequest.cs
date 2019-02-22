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

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable
{
    class Sentinel1QcOpenSearchRequest : OpenSearchRequest
    {
        NameValueCollection parameters;
        readonly Uri qcSearchUrl;

        XmlSerializer eeser = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File));

        private static readonly ILog log = LogManager.GetLogger(typeof(Sentinel1QcOpenSearchRequest));

        Uri qcBaseUrl;



        public Sentinel1QcOpenSearchRequest(Uri qcSearchUrl, NameValueCollection parameters) : base(new OpenSearchUrl(qcSearchUrl), "application/atom+xml")
        {
            this.qcSearchUrl = qcSearchUrl;
            var tmpurl = new UriBuilder(qcSearchUrl);
            tmpurl.Path = "";
            tmpurl.Query = "";
            this.qcBaseUrl = tmpurl.Uri;
            this.parameters = parameters;
        }

        public override NameValueCollection OriginalParameters
        {
            get
            {
                return parameters;
            }

            set
            {
                parameters = value;
            }
        }

        public override IOpenSearchResponse GetResponse()
        {

            AtomFeed feed = new AtomFeed();

            int count = string.IsNullOrEmpty(parameters["count"]) ? 20 : int.Parse(parameters["count"]);
            int page = string.IsNullOrEmpty(parameters["startPage"]) ? 1 : int.Parse(parameters["startPage"]);
            int index = string.IsNullOrEmpty(parameters["startIndex"]) ? 1 : int.Parse(parameters["startIndex"]);
            string type = string.IsNullOrEmpty(parameters["auxtype"]) ? "aux_resorb" : parameters["auxtype"];
            bool withOrbits = parameters["orbits"] == "true";

            int absindex = index + ((page - 1) * count);
            int queryindex = absindex % 20;
            int querypage = (absindex / 20) + 1;

            bool partialAtom = false;

            List<AtomItem> items = new List<AtomItem>();

            while (items.Count() < count)
            {

                Uri url = BuildUrl(qcBaseUrl, count, index, page, type, parameters["start"], parameters["stop"]);

                var request = HttpWebRequest.Create(url);

                log.DebugFormat("Query: {0}", url);

                HtmlDocument doc = new HtmlDocument();

                using (var response = request.GetResponse())
                {
                    doc.Load(response.GetResponseStream());
                }

                Dictionary<string, Uri> list = new Dictionary<string, Uri>();

                var pagesli = doc.DocumentNode.SelectNodes("/html/body/div/div/div/div[1]/div/ul/li");
                int lastpage = 1;

                if (pagesli != null)
                {
                    lastpage = pagesli.Max(li =>
                    {
                        int pageo = 0;
                        if (int.TryParse(li.FirstChild.InnerText, out pageo))
                            return pageo;
                        return 0;
                    });
                }

                if (lastpage < querypage)
                    break;

                var trs = doc.DocumentNode.SelectNodes("/html/body/div/div/div/div[1]/table/tbody/tr");

                if (trs == null)
                    break;

                foreach (HtmlNode tr in trs)
                {
                    var a = tr.SelectSingleNode("td/a");
                    log.Debug(a.InnerText);
                    HtmlAttribute href = a.Attributes["href"];
                    var producturl = new Uri(href.Value);
                    log.Debug(producturl);
                    list.Add(a.InnerText, producturl);
                }

                if (list.Count() == 0)
                    break;

                if (items.FirstOrDefault(i => i.Identifier == list.Last().Key.Replace(".EOF", "")) != null)
                    break;

                try {
                    items.AddRange(BuildAtomItem(list.Skip(queryindex - 1).Take(count - items.Count()), withOrbits));
                }
                catch (PartialAtomException e) {
                    items.AddRange(e.Items);
                    partialAtom = true;

                }

                queryindex = 1;
                page++;

            }

            feed.Items = items;

           if (partialAtom) {
                return new PartialAtomSearchResponse(feed);
           }
            
           return new Terradue.OpenSearch.Response.AtomOpenSearchResponse(feed);
        }

        IEnumerable<AtomItem> BuildAtomItem(IEnumerable<KeyValuePair<string, Uri>> products, bool withOrbits)
        {
            
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

        AtomItem CreateItemFromLink(string key, Uri url, bool withOrbits)
        {
            string identifier = key.Replace(".EOF", "");
            Match match = Regex.Match(identifier,
                                      @"^(?'mission'\w{3})_OPER_AUX_(?'type'\w{6})_(?'system'\w{4})_(?'proddate'\w{15})_V(?'startdate'\w{15})_(?'stopdate'\w{15})$");

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

        static Uri BuildUrl(Uri qcSearchUrl, int count, int index, int page, string type, string start, string stop)
        {
            UriBuilder url = new UriBuilder(qcSearchUrl);
            url.Path += string.Format("/{0}/", string.IsNullOrEmpty(type) ? "aux_resorb" : type);
            var qs = HttpUtility.ParseQueryString("");

            int absindex = index + ((page - 1) * count);
            int querypage = (absindex / 20) + 1;

            qs.Set("page", querypage.ToString());

            if (!string.IsNullOrEmpty(start))
                qs.Set("validity_start", string.Format("{0}{1}", start, string.IsNullOrEmpty(stop) ? "" : ".." + stop));
            
            url.Query = qs.ToString();

            return url.Uri;

        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType OrbToEo(string identifier, string mission, string type, DateTime start, DateTime stop, DateTimeOffset published)
        {

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

        public static SyndicationElementExtension GenerateOrbitsExtension(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File file)
        {

            SyndicationElementExtension extension = new SyndicationElementExtension(GetS1OrbitsFromEE(file), Terradue.Metadata.EarthObservation.Model.orbitListType.OrbitsSerializer);

            return extension;
        }


        public static Terradue.Metadata.EarthObservation.Model.orbitListType GetS1OrbitsFromEE(Terradue.OpenSearch.Sentinel.Data.Earth_Explorer_File file)
        {

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