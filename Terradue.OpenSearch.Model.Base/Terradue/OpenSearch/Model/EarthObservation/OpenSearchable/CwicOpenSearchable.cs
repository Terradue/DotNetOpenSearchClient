using System;
using Terradue.OpenSearch.Model.GeoTime;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using System.Net;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using System.Linq;
using System.Xml;
using log4net;
using System.Xml.Serialization;
using System.Text;
using Terradue.ServiceModel.Ogc;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable
{

    public class CwicOpenSearchable : SoftGenericOpenSearchable, IOpenSearchable
    {

        XmlSerializer eeSer = new XmlSerializer(typeof(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene));

        private static readonly ILog log = LogManager.GetLogger(typeof(CwicOpenSearchable));

        public CwicOpenSearchable(OpenSearchDescription osd, IOpenSearchableFactory factory) : base(osd, factory.Settings)
        {
        }

        public static CwicOpenSearchable CreateFrom(GenericOpenSearchable e, OpenSearchEngine ose)
        {
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(new OpenSearchableFactorySettings(ose));
            return new CwicOpenSearchable(e.GetOpenSearchDescription(), factory);
        }

        public new void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType)
        {
            log.DebugFormat("Applying Cwic source harvesting");

            base.ApplyResultFilters(request, ref osr, finalContentType);

            QueryEarthObservationResult(ref osr);

        }

        private void QueryEarthObservationResult(ref IOpenSearchResultCollection osr)
        {


            foreach (var item in osr.Items)
            {

                log.DebugFormat("Searching for alternate link to metadata URL for item {0}", item.Identifier);

                var altlink = item.Links.FirstOrDefault(l => l.RelationshipType == "via" && l.Title == "Original source metadata");

                string identifier = null;

                if (altlink != null)
                {
                    log.DebugFormat("Link found at {0}", altlink.Uri);
                    var req = HttpWebRequest.Create(altlink.Uri);
                    log.DebugFormat("Query {0}...", altlink.Uri);
                    var response = req.GetResponse();
                    using (var xr = XmlReader.Create(response.GetResponseStream()))
                    {
                        while (xr.Read())
                        {
                            if (xr.LocalName == "scene" && xr.NamespaceURI == "http://earthexplorer.usgs.gov/eemetadata.xsd")
                            {
                                log.DebugFormat("Found scene metadata, harvesting {0} ...", altlink.Uri);
                                Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene eescene = (Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene)eeSer.Deserialize(xr);
                                Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo = EarthExplorerToEo(eescene);
                                AddIMGOffering(eo, item);
                                if (eo != null)
                                {
                                    log.DebugFormat("EOP extension created from {0}", altlink.Uri);
                                    item.ElementExtensions.Add(eo.CreateReader());
                                    identifier = eo.EopMetaDataProperty.EarthObservationMetaData.identifier;
                                    item.Title = new ServiceModel.Syndication.TextSyndicationContent(
                                        string.Format("{0}, {1}, Path: {2}, Row: {3}",
                                                      identifier,
                                                      DateTime.Parse(eo.phenomenonTime.GmlTimePeriod.beginPosition.Value).ToString("yy-MMM-dd hh:mm:ss"),
                                                      eo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLongitudeGrid.Value,
                                                      eo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLatitudeGrid.Value
                                                     ));
                                }
                                continue;
                            }
                        }

                    }

                }

                var identifierext = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "identifier" && e.OuterNamespace == "http://purl.org/dc/elements/1.1/");
                if (identifier == null)
                {
                    UriBuilder url = new UriBuilder(identifierext.GetObject<string>());
                    NameValueCollection nvc = System.Web.HttpUtility.ParseQueryString(url.Query);
                    identifier = nvc["uid"];
                    if (identifier.Contains(":"))
                        identifier = identifier.Split(':')[1];

                }
                item.ElementExtensions.Remove(identifierext);
                item.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", identifier);
                identifierext = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "identifier" && e.OuterNamespace == "http://purl.org/dc/terms/");
                item.ElementExtensions.Remove(identifierext);
            }
        }

        private static void AddIMGOffering(Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo, IOpenSearchResultItem item)
        {
            Terradue.ServiceModel.Ogc.Eop21.BrowseInformationPropertyType[] bi = null;
            if (eo.result != null && eo.result.Eop21EarthObservationResult.browse != null)
            {
                bi = eo.result.Eop21EarthObservationResult.browse;
            }
            if (bi != null)
            {
                foreach (var browse in bi)
                {

                    if (browse.BrowseInformation.type != "img")
                        continue;

                    OwcOffering offering = new OwcOffering();
                    offering.Code = "http://www.opengis.net/spec/owc-atom/1.0/req/img";
                    offering.Contents = new OwcContent[1];
                    offering.Contents[0] = new OwcContent();
                    offering.Contents[0].Url = new Uri(browse.BrowseInformation.fileName.ServiceReference.href);

                    item.ElementExtensions.Add(offering, new XmlSerializer(typeof(OwcOffering)));

                }
            }
        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType EarthExplorerToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene)
        {

            if (scene.metadataFields.FirstOrDefault(m => m.name == "Landsat Scene Identifier") != null)
            {
                return LandsatToEo(scene);
            }

            if (scene.metadataFields.FirstOrDefault(m => m.name == "Entity ID" && m.metadataValue.StartsWith("SRTM")) != null)
            {
                return SRTMToEo(scene);
            }

            return null;

        }

        public static Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationType LandsatToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene)
        {

            Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationType optEo = new Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationType();

            List<Terradue.ServiceModel.Ogc.Eop21.SpecificInformationPropertyType> vs = new List<Terradue.ServiceModel.Ogc.Eop21.SpecificInformationPropertyType>();

            optEo.EopMetaDataProperty = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataPropertyType();
            optEo.EopMetaDataProperty.EarthObservationMetaData = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataType();
            var identifier = scene.metadataFields.FirstOrDefault(m => m.name == "Landsat Scene Identifier");
            optEo.EopMetaDataProperty.EarthObservationMetaData.identifier = identifier.metadataValue;

            optEo.procedure = new Terradue.ServiceModel.Ogc.Om20.OM_ProcessPropertyType();
            optEo.procedure.Eop21EarthObservationEquipment = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationEquipmentType();
            optEo.procedure.Eop21EarthObservationEquipment.platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformPropertyType();
            optEo.procedure.Eop21EarthObservationEquipment.platform.Platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformType();
            optEo.procedure.Eop21EarthObservationEquipment.platform.Platform.shortName = "Landsat-" + identifier.metadataValue.Substring(2, 1);
            optEo.procedure.Eop21EarthObservationEquipment.platform.Platform.orbitType = Terradue.ServiceModel.Ogc.Eop21.OrbitTypeValueType.LEO;
            optEo.procedure.Eop21EarthObservationEquipment.instrument = new Terradue.ServiceModel.Ogc.Eop21.InstrumentPropertyType();
            optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument = new Terradue.ServiceModel.Ogc.Eop21.InstrumentType();
            switch (identifier.metadataValue.Substring(1, 1))
            {
                case "T":
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName = "TIRS";
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.description = "Thermal Infrared Sensor";
                    break;
                case "O":
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName = "OLI";
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.description = "Operational Land Imager";
                    break;
                case "C":
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName = "OLI_TIRS";
                    optEo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.description = "Operational Land Imager & Thermal Infrared Sensor";
                    break;
            }
            optEo.procedure.Eop21EarthObservationEquipment.sensor = new Terradue.ServiceModel.Ogc.Eop21.SensorPropertyType();
            optEo.procedure.Eop21EarthObservationEquipment.sensor.Sensor = new Terradue.ServiceModel.Ogc.Eop21.SensorType();
            optEo.procedure.Eop21EarthObservationEquipment.sensor.Sensor.sensorType = "OPTICAL";

            optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters = new Terradue.ServiceModel.Ogc.Eop21.AcquisitionPropertyType();
            optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition = new Terradue.ServiceModel.Ogc.Eop21.AcquisitionType();
            var swrsRow = scene.metadataFields.FirstOrDefault(m => m.name == "Target WRS Row");
            if (swrsRow != null)
            {
                int wrsRow = int.Parse(swrsRow.metadataValue);
                if (wrsRow > 1 && wrsRow <= 122)
                    optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.ServiceModel.Ogc.Eop21.OrbitDirectionValueType.DESCENDING;
                if (wrsRow >= 123 && wrsRow <= 246)
                    optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.ServiceModel.Ogc.Eop21.OrbitDirectionValueType.ASCENDING;
                if (wrsRow >= 247 && wrsRow <= 248)
                    optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.ServiceModel.Ogc.Eop21.OrbitDirectionValueType.DESCENDING;
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirectionSpecified = true;
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLatitudeGrid = new Terradue.ServiceModel.Ogc.Gml321.CodeWithAuthorityType();
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLatitudeGrid.Value = wrsRow.ToString();
            }
            var swrsPath = scene.metadataFields.FirstOrDefault(m => m.name == "Target WRS Path");
            if (swrsPath != null)
            {
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLongitudeGrid = new Terradue.ServiceModel.Ogc.Gml321.CodeWithAuthorityType();
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLongitudeGrid.Value = swrsPath.metadataValue.Trim();
            }

            var fullscene = scene.metadataFields.FirstOrDefault(m => m.name == "Full or Partial Scene");
            if (fullscene != null)
            {
                Terradue.ServiceModel.Ogc.Eop21.SpecificInformationPropertyType vss = new Terradue.ServiceModel.Ogc.Eop21.SpecificInformationPropertyType();
                vss.SpecificInformation = new Terradue.ServiceModel.Ogc.Eop21.SpecificInformationType();
                vss.SpecificInformation.localAttribute = "full_partial_scene";
                vss.SpecificInformation.localValue = fullscene.metadataValue;
            }

            var dataCat = scene.metadataFields.FirstOrDefault(m => m.name == "Data Category");
            if (dataCat != null)
            {
                switch (dataCat.metadataValue)
                {
                    case "NOMINAL":
                        optEo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.NOMINAL;
                        break;
                    case "EXCHANGE":
                        optEo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.OTHER;
                        break;
                    case "TEST":
                        optEo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.OTHER;
                        break;
                    case "ENGINEERING":
                        optEo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.OTHER;
                        break;
                    case "VALIDATION":
                        optEo.EopMetaDataProperty.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.CALIBRATION;
                        break;
                }
            }

            optEo.EopMetaDataProperty.EarthObservationMetaData.processing = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType[1];
            optEo.EopMetaDataProperty.EarthObservationMetaData.processing[0] = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType();
            optEo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationType();

            var level1 = scene.metadataFields.FirstOrDefault(m => m.name == "Data Type Level 1");
            if (level1 != null)
            {
                optEo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation.processingLevel = level1.metadataValue;
            }



            List<string> aux = new List<string>();

            var bpfoli = scene.metadataFields.FirstOrDefault(m => m.name == "Bias Parameter File Name OLI");
            if (bpfoli != null)
            {
                aux.Add(bpfoli.metadataValue);
            }
            var bpftirs = scene.metadataFields.FirstOrDefault(m => m.name == "Bias Parameter File Name TIRS");
            if (bpftirs != null)
            {
                aux.Add(bpftirs.metadataValue);
            }
            var bpfcal = scene.metadataFields.FirstOrDefault(m => m.name == "Calibration Parameter File");
            if (bpfcal != null)
            {
                aux.Add(bpfcal.metadataValue);
            }
            var bpfrlut = scene.metadataFields.FirstOrDefault(m => m.name == "RLUT File Name");
            if (bpfrlut != null)
            {
                aux.Add(bpfrlut.metadataValue);
            }

            optEo.EopMetaDataProperty.EarthObservationMetaData.processing[0].ProcessingInformation.auxiliaryDataSetFileName = aux.ToArray();

            optEo.phenomenonTime = new Terradue.ServiceModel.Ogc.Om20.TimeObjectPropertyType();
            optEo.phenomenonTime.GmlTimePeriod = new Terradue.ServiceModel.Ogc.Gml321.TimePeriodType();
            var start = scene.metadataFields.FirstOrDefault(m => m.name == "Start Time");
            var stop = scene.metadataFields.FirstOrDefault(m => m.name == "Stop Time");
            if (start != null)
            {
                DateTime startdate = ParseDateTime(start.metadataValue);
                optEo.phenomenonTime.GmlTimePeriod.beginPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
                optEo.phenomenonTime.GmlTimePeriod.beginPosition.Value = startdate.ToString("O");
            }
            if (stop != null)
            {
                DateTime stopdate = ParseDateTime(stop.metadataValue);
                optEo.phenomenonTime.GmlTimePeriod.endPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
                optEo.phenomenonTime.GmlTimePeriod.endPosition.Value = stopdate.ToString("O");
            }

            optEo.EopMetaDataProperty.EarthObservationMetaData.vendorSpecific = vs.ToArray();

            var qua = scene.metadataFields.FirstOrDefault(m => m.name == "Image Quality");
            if (qua != null)
            {
                optEo.EopMetaDataProperty.EarthObservationMetaData.imageQualityDegradation = new Terradue.ServiceModel.Ogc.Gml321.MeasureType();
                optEo.EopMetaDataProperty.EarthObservationMetaData.imageQualityDegradation.Value = double.Parse(qua.metadataValue);
            }

            optEo.result = new Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationResultPropertyType();
            optEo.result.Opt21EarthObservationResult = new Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationResultType();


            var cc = scene.metadataFields.FirstOrDefault(m => m.name == "Scene Cloud Cover");
            double ccd;
            if (cc != null && double.TryParse(cc.metadataValue, out ccd))
            {
                optEo.result.Opt21EarthObservationResult.cloudCoverPercentage = new Terradue.ServiceModel.Ogc.Gml321.MeasureType();
                optEo.result.Opt21EarthObservationResult.cloudCoverPercentage.Value = ccd;
            }

            var sunelev = scene.metadataFields.FirstOrDefault(m => m.name == "Sun Elevation");
            if (sunelev != null)
            {
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationElevationAngle = new Terradue.ServiceModel.Ogc.Gml321.AngleType();
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationElevationAngle.Value = double.Parse(sunelev.metadataValue);
            }

            var sunazimuth = scene.metadataFields.FirstOrDefault(m => m.name == "Sun Azimuth");
            if (sunazimuth != null)
            {
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationAzimuthAngle = new Terradue.ServiceModel.Ogc.Gml321.AngleType();
                optEo.procedure.Eop21EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationAzimuthAngle.Value = double.Parse(sunazimuth.metadataValue);
            }

            var brwexist = scene.metadataFields.FirstOrDefault(m => m.name == "Browse Exists");
            if (brwexist != null && brwexist.metadataValue == "Y")
            {

                if (scene.overlayLinks.Count() > 0)
                {

                    var overlay = scene.overlayLinks.FirstOrDefault(l => l.caption.Contains("Natural"));
                    if (overlay != null)
                    {
                        optEo.result.Opt21EarthObservationResult.browse = new Terradue.ServiceModel.Ogc.Eop21.BrowseInformationPropertyType[1];
                        optEo.result.Opt21EarthObservationResult.browse[0] = new Terradue.ServiceModel.Ogc.Eop21.BrowseInformationPropertyType();
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation = new Terradue.ServiceModel.Ogc.Eop21.BrowseInformationType();
                        var wmsparams = overlay.overlayLink.Split('&');
                        if (wmsparams.Count() > 0 && wmsparams.FirstOrDefault(p => p.StartsWith("srs=")) != null)
                        {
                            optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.referenceSystemIdentifier = new Terradue.ServiceModel.Ogc.Gml321.CodeWithAuthorityType();
                            optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.referenceSystemIdentifier.Value = wmsparams.FirstOrDefault(p => p.StartsWith("srs=")).Split('=')[1];
                        }
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.fileName = new Terradue.ServiceModel.Ogc.Eop21.BrowseInformationTypeFileName();
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference = new Terradue.ServiceModel.Ogc.Ows20.ServiceReferenceType();
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.href = overlay.overlayLink;
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.type = "img";
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.title = overlay.caption;
                        optEo.result.Opt21EarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.type = "image/png";
                    }
                }
            }

            return optEo;

        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType SRTMToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene)
        {

            throw new NotImplementedException();
        }

        private static DateTime ParseDateTime(string date)
        {

            var dateelem = date.Split(':');

            DateTime doy = DateTime.Parse(dateelem[0] + "-01-01T00:00:00");
            doy = doy.AddDays(double.Parse(dateelem[1]) - 1);

            var sec = dateelem[4].Split('.');

            double div = 100.0 / double.Parse(new StringBuilder("1").Append('0', sec[1].Length - 1).ToString());
            double milli = double.Parse(sec[1]) * div;
            return new DateTime(int.Parse(dateelem[0]), doy.Month, doy.Day, int.Parse(dateelem[2]), int.Parse(dateelem[3]), int.Parse(sec[0]), (int)milli, DateTimeKind.Utc);

        }
    }

}

