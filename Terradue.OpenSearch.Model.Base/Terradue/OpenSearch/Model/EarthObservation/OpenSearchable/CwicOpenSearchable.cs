using System;
using Terradue.OpenSearch.Model.GeoTime;
using Mono.Addins;
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
using Terradue.Metadata.EarthObservation;
using System.IO;
using System.Text;
using Terradue.ServiceModel.Ogc.OwsContext;

namespace Terradue.OpenSearch.Model.EarthObservation.OpenSearchable {
    
    public class CwicOpenSearchable : GenericOpenSearchable, IOpenSearchable {

        XmlSerializer eeSer = new XmlSerializer(typeof(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene));

        private static readonly ILog log = LogManager.GetLogger(typeof(CwicOpenSearchable));

        public CwicOpenSearchable(OpenSearchDescription osd, OpenSearchEngine ose) : base(osd, ose) {
        }

        public static CwicOpenSearchable CreateFrom(GenericOpenSearchable e, OpenSearchEngine ose) {
            return new CwicOpenSearchable(e.GetOpenSearchDescription(), ose);
        }

        public new void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
            log.DebugFormat("Applying Cwic source harvesting");

            base.ApplyResultFilters(request, ref osr, finalContentType);

            QueryEarthObservationResult(ref osr);

        }

        private void QueryEarthObservationResult(ref IOpenSearchResultCollection osr) {


            foreach (var item in osr.Items) {

                log.DebugFormat("Searching for alternate link to metadata URL for item {0}", item.Identifier);

                var altlink = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate" && l.Title == "Alternate metadata URL");

                if (altlink != null) {
                    log.DebugFormat("Link found at {0}", altlink.Uri);
                    var req = HttpWebRequest.Create(altlink.Uri);
                    log.DebugFormat("Query {0}...", altlink.Uri);
                    var response = req.GetResponse();
                    var xr = XmlReader.Create(response.GetResponseStream());
                    while (xr.Read()) {
                        if (xr.LocalName == "scene" && xr.NamespaceURI == "http://earthexplorer.usgs.gov/eemetadata.xsd") {
                            log.DebugFormat("Found scene metadata, harvesting {0} ...", altlink.Uri);
                            Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene eescene = (Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene)eeSer.Deserialize(xr);
                            Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationType eo = EarthExplorerToEo(eescene);
                            AddIMGOffering(eo, item);
                            if (eo != null) {
                                log.DebugFormat("EOP extension created from {0}", altlink.Uri);
                                MemoryStream stream = new MemoryStream();
                                XmlWriter writer = XmlWriter.Create(stream);
                                var ser = MetadataHelpers.GetXmlSerializerFromType(eo.GetType());
                                ser.Serialize(stream, eo);
                                writer.Flush();
                                stream.Seek(0, SeekOrigin.Begin);

                                item.ElementExtensions.Add(XmlReader.Create(stream));
                            }
                            continue;
                        }
                            
                    }

                }

                var identifierext = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "identifier" && e.OuterNamespace ==  "http://purl.org/dc/elements/1.1/");
                string identifier = identifierext.GetObject<string>().Replace("http://cwic.wgiss.ceos.org/opensearch/granules.atom?uid=", "");
                item.ElementExtensions.Remove(identifierext);
                item.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", identifier);
            }
        }

        private static void AddIMGOffering(Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationType eo, IOpenSearchResultItem item) {
            Terradue.Metadata.EarthObservation.Ogc.Eop.BrowseInformationPropertyType[] bi = null;
            if (eo.EopResult != null && eo.EopResult.EarthObservationResult.browse != null) {
                bi = eo.EopResult.EarthObservationResult.browse;
            }
            if (eo is Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType) {
                Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType optEO = (Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType)eo;
                if (optEO.Optresult.OptEarthObservationResult.browse != null)
                    bi = optEO.Optresult.OptEarthObservationResult.browse;
            }
            if (bi != null) {
                foreach (var browse in bi) {

                    if (browse.BrowseInformation.type != "img")
                        continue;

                    OwcOffering offering = new OwcOffering();
                    offering.Code = "http://www.opengis.net/spec/owc-atom/1.0/req/img";
                    offering.Contents = new OwcContent[1];
                    offering.Contents[0] = new OwcContent();
                    offering.Contents[0].Href = new Uri(browse.BrowseInformation.fileName.ServiceReference.href);

                    item.ElementExtensions.Add(offering, new XmlSerializer(typeof(OwcOffering)));

                }
            }
        }

        public static Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationType EarthExplorerToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene) {

            if (scene.metadataFields.FirstOrDefault(m => m.name == "Landsat Scene Identifier") != null) {
                return LandsatToEo(scene);
            }

            if (scene.metadataFields.FirstOrDefault(m => m.name == "Entity ID" && m.metadataValue.StartsWith("SRTM")) != null) {
                return SRTMToEo(scene);
            }

            return null;

        }

        public static Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType LandsatToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene) {

            Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType optEo = new Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationType();

            List<Terradue.Metadata.EarthObservation.Ogc.Eop.SpecificInformationPropertyType> vs = new List<Terradue.Metadata.EarthObservation.Ogc.Eop.SpecificInformationPropertyType>();

            optEo.metaDataProperty1 = new Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationMetaDataPropertyType();
            optEo.metaDataProperty1.EarthObservationMetaData = new Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationMetaDataType();
            var identifier = scene.metadataFields.FirstOrDefault(m => m.name == "Landsat Scene Identifier");
            optEo.metaDataProperty1.EarthObservationMetaData.identifier = identifier.metadataValue;

            optEo.EopProcedure = new Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationEquipmentPropertyType();
            optEo.EopProcedure.EarthObservationEquipment = new Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationEquipmentType();
            optEo.EopProcedure.EarthObservationEquipment.platform = new Terradue.Metadata.EarthObservation.Ogc.Eop.PlatformPropertyType();
            optEo.EopProcedure.EarthObservationEquipment.platform.Platform = new Terradue.Metadata.EarthObservation.Ogc.Eop.PlatformType();
            optEo.EopProcedure.EarthObservationEquipment.platform.Platform.shortName = "Landsat-" + identifier.metadataValue.Substring(2, 1);
            optEo.EopProcedure.EarthObservationEquipment.platform.Platform.orbitType = Terradue.Metadata.EarthObservation.Ogc.Eop.OrbitTypeValueType.LEO;
            optEo.EopProcedure.EarthObservationEquipment.instrument = new Terradue.Metadata.EarthObservation.Ogc.Eop.InstrumentPropertyType();
            optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument = new Terradue.Metadata.EarthObservation.Ogc.Eop.InstrumentType();
            switch (identifier.metadataValue.Substring(1, 1)) {
                case "T": 
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.shortName = "TIRS";
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.description = "Thermal Infrared Sensor";
                    break;
                case "O": 
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.shortName = "OLI";
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.description = "Operational Land Imager";
                    break;
                case "C": 
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.shortName = "OLI_TIRS";
                    optEo.EopProcedure.EarthObservationEquipment.instrument.Instrument.description = "Operational Land Imager & Thermal Infrared Sensor";
                    break;
            }
            optEo.EopProcedure.EarthObservationEquipment.sensor = new Terradue.Metadata.EarthObservation.Ogc.Eop.SensorPropertyType();
            optEo.EopProcedure.EarthObservationEquipment.sensor.Sensor = new Terradue.Metadata.EarthObservation.Ogc.Eop.SensorType();
            optEo.EopProcedure.EarthObservationEquipment.sensor.Sensor.sensorType = "OPTICAL";

            optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters = new Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionPropertyType();
            optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition = new Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionType();
            var swrsRow = scene.metadataFields.FirstOrDefault(m => m.name == "Target WRS Row");
            if (swrsRow != null) {
                int wrsRow = int.Parse(swrsRow.metadataValue);
                if (wrsRow > 1 && wrsRow <= 122)
                    optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.Metadata.EarthObservation.Ogc.Eop.OrbitDirectionValueType.DESCENDING;
                if (wrsRow >= 123 && wrsRow <= 246)
                    optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.Metadata.EarthObservation.Ogc.Eop.OrbitDirectionValueType.ASCENDING;
                if (wrsRow >= 247 && wrsRow <= 248)
                    optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.orbitDirection = Terradue.Metadata.EarthObservation.Ogc.Eop.OrbitDirectionValueType.DESCENDING;
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLatitudeGrid = new Terradue.GeoJson.Gml.CodeWithAuthorityType();
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLatitudeGrid.Value = wrsRow.ToString();
            }
            var swrsPath = scene.metadataFields.FirstOrDefault(m => m.name == "Target WRS Path");
            if (swrsPath != null) {
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLongitudeGrid = new Terradue.GeoJson.Gml.CodeWithAuthorityType();
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.wrsLongitudeGrid.Value = swrsPath.metadataValue.Trim();
            }

            var fullscene = scene.metadataFields.FirstOrDefault(m => m.name == "Full or Partial Scene");
            if (fullscene != null) {
                Terradue.Metadata.EarthObservation.Ogc.Eop.SpecificInformationPropertyType vss = new Terradue.Metadata.EarthObservation.Ogc.Eop.SpecificInformationPropertyType();
                vss.SpecificInformation = new Terradue.Metadata.EarthObservation.Ogc.Eop.SpecificInformationType();
                vss.SpecificInformation.localAttribute = "full_partial_scene";
                vss.SpecificInformation.localValue = fullscene.metadataValue;
            }

            var dataCat = scene.metadataFields.FirstOrDefault(m => m.name == "Data Category");
            if (dataCat != null) {
                switch (dataCat.metadataValue) {
                    case "NOMINAL":
                        optEo.metaDataProperty1.EarthObservationMetaData.acquisitionType = Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionTypeValueType.NOMINAL;
                        break;
                    case "EXCHANGE":
                        optEo.metaDataProperty1.EarthObservationMetaData.acquisitionType = Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionTypeValueType.OTHER;
                        break;
                    case "TEST":
                        optEo.metaDataProperty1.EarthObservationMetaData.acquisitionType = Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionTypeValueType.OTHER;
                        break;
                    case "ENGINEERING":
                        optEo.metaDataProperty1.EarthObservationMetaData.acquisitionType = Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionTypeValueType.OTHER;
                        break;
                    case "VALIDATION":
                        optEo.metaDataProperty1.EarthObservationMetaData.acquisitionType = Terradue.Metadata.EarthObservation.Ogc.Eop.AcquisitionTypeValueType.CALIBRATION;
                        break;
                }
            }

            optEo.metaDataProperty1.EarthObservationMetaData.processing = new Terradue.Metadata.EarthObservation.Ogc.Eop.ProcessingInformationPropertyType[1];
            optEo.metaDataProperty1.EarthObservationMetaData.processing[0] = new Terradue.Metadata.EarthObservation.Ogc.Eop.ProcessingInformationPropertyType();
            optEo.metaDataProperty1.EarthObservationMetaData.processing[0].ProcessingInformation = new Terradue.Metadata.EarthObservation.Ogc.Eop.ProcessingInformationType();

            var level1 = scene.metadataFields.FirstOrDefault(m => m.name == "Data Type Level 1");
            if (level1 != null) {
                optEo.metaDataProperty1.EarthObservationMetaData.processing[0].ProcessingInformation.processingLevel = level1.metadataValue;
            }



            List<string> aux = new List<string>();

            var bpfoli = scene.metadataFields.FirstOrDefault(m => m.name == "Bias Parameter File Name OLI");
            if (bpfoli != null) {
                aux.Add(bpfoli.metadataValue);
            }
            var bpftirs = scene.metadataFields.FirstOrDefault(m => m.name == "Bias Parameter File Name TIRS");
            if (bpftirs != null) {
                aux.Add(bpftirs.metadataValue);
            }
            var bpfcal = scene.metadataFields.FirstOrDefault(m => m.name == "Calibration Parameter File");
            if (bpfcal != null) {
                aux.Add(bpfcal.metadataValue);
            }
            var bpfrlut = scene.metadataFields.FirstOrDefault(m => m.name == "RLUT File Name");
            if (bpfrlut != null) {
                aux.Add(bpfrlut.metadataValue);
            }

            optEo.metaDataProperty1.EarthObservationMetaData.processing[0].ProcessingInformation.auxiliaryDataSetFileName = aux.ToArray();

            optEo.phenomenonTime = new Terradue.Metadata.EarthObservation.Ogc.Om.TimeObjectPropertyType();
            optEo.phenomenonTime.GmlTimePeriod = new Terradue.GeoJson.Gml.TimePeriodType();
            var start = scene.metadataFields.FirstOrDefault(m => m.name == "Start Time");
            var stop = scene.metadataFields.FirstOrDefault(m => m.name == "Stop Time");
            if (start != null) {
                DateTime startdate = ParseDateTime(start.metadataValue);
                optEo.phenomenonTime.GmlTimePeriod.beginPosition = new Terradue.GeoJson.Gml.TimePositionType();
                optEo.phenomenonTime.GmlTimePeriod.beginPosition.Value = startdate.ToString("O");
            }
            if (stop != null) {
                DateTime stopdate = ParseDateTime(stop.metadataValue);
                optEo.phenomenonTime.GmlTimePeriod.endPosition = new Terradue.GeoJson.Gml.TimePositionType();
                optEo.phenomenonTime.GmlTimePeriod.endPosition.Value = stopdate.ToString("O");
            }

            optEo.metaDataProperty1.EarthObservationMetaData.vendorSpecific = vs.ToArray();

            var qua = scene.metadataFields.FirstOrDefault(m => m.name == "Image Quality");
            if (qua != null) {
                optEo.metaDataProperty1.EarthObservationMetaData.imageQualityDegradation = new Terradue.GeoJson.Gml.MeasureType();
                optEo.metaDataProperty1.EarthObservationMetaData.imageQualityDegradation.Value = double.Parse(qua.metadataValue);
            }

            optEo.Optresult = new Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationResultPropertyType();
            optEo.Optresult.OptEarthObservationResult = new Terradue.Metadata.EarthObservation.Ogc.Opt.OptEarthObservationResultType();


            var cc = scene.metadataFields.FirstOrDefault(m => m.name == "Scene Cloud Cover");
            double ccd;
            if (cc != null && double.TryParse(cc.metadataValue, out ccd)) {
                optEo.Optresult.OptEarthObservationResult.cloudCoverPercentage = new Terradue.GeoJson.Gml.MeasureType();
                optEo.Optresult.OptEarthObservationResult.cloudCoverPercentage.Value = ccd;
            }

            var sunelev = scene.metadataFields.FirstOrDefault(m => m.name == "Sun Elevation");
            if (sunelev != null) {
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationElevationAngle = new Terradue.GeoJson.Gml.AngleType();
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationElevationAngle.Value = double.Parse(sunelev.metadataValue);
            }

            var sunazimuth = scene.metadataFields.FirstOrDefault(m => m.name == "Sun Azimuth");
            if (sunazimuth != null) {
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationAzimuthAngle = new Terradue.GeoJson.Gml.AngleType();
                optEo.EopProcedure.EarthObservationEquipment.acquisitionParameters.Acquisition.illuminationAzimuthAngle.Value = double.Parse(sunazimuth.metadataValue);
            }

            var brwexist = scene.metadataFields.FirstOrDefault(m => m.name == "Browse Exists");
            if (brwexist != null && brwexist.metadataValue == "Y") {

                if (scene.overlayLinks.Count() > 0) {

                    var overlay = scene.overlayLinks.FirstOrDefault(l => l.caption.Contains("Natural"));
                    if (overlay != null) {
                        optEo.Optresult.OptEarthObservationResult.browse = new Terradue.Metadata.EarthObservation.Ogc.Eop.BrowseInformationPropertyType[1];
                        optEo.Optresult.OptEarthObservationResult.browse[0] = new Terradue.Metadata.EarthObservation.Ogc.Eop.BrowseInformationPropertyType();
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation = new Terradue.Metadata.EarthObservation.Ogc.Eop.BrowseInformationType();
                        var wmsparams = overlay.overlayLink.Split('&');
                        if (wmsparams.Count() > 0 && wmsparams.FirstOrDefault(p => p.StartsWith("srs=")) != null) {
                            optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.referenceSystemIdentifier = new Terradue.GeoJson.Gml.CodeWithAuthorityType();
                            optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.referenceSystemIdentifier.Value = wmsparams.FirstOrDefault(p => p.StartsWith("srs=")).Split('=')[1];
                        }
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.fileName = new Terradue.Metadata.EarthObservation.Ogc.Eop.BrowseInformationTypeFileName();
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference = new Terradue.Metadata.EarthObservation.Ogc.Ows.ServiceReferenceType();
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.href = overlay.overlayLink;
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.type = "img";
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.title = overlay.caption;
                        optEo.Optresult.OptEarthObservationResult.browse[0].BrowseInformation.fileName.ServiceReference.type = "image/png";
                    }
                }
            }

            return optEo;
            
        }

        public static Terradue.Metadata.EarthObservation.Ogc.Eop.EarthObservationType SRTMToEo(Terradue.OpenSearch.Model.Schemas.EarthExplorer.scene scene) {

            throw new NotImplementedException();
        }

        private static DateTime ParseDateTime(string date) {

            var dateelem = date.Split(':');

            DateTime doy = DateTime.Parse("2000-01-01T00:00:00");
            doy = doy.AddDays(double.Parse(dateelem[1])-1);

            var sec = dateelem[4].Split('.');

            double div = 100.0 / double.Parse(new StringBuilder("1").Append('0', sec[1].Length - 1).ToString());
            double milli = double.Parse(sec[1]) * div;
            return new DateTime(int.Parse(dateelem[0]), doy.Month, doy.Day, int.Parse(dateelem[2]), int.Parse(dateelem[3]), int.Parse(sec[0]), (int)milli, DateTimeKind.Utc); 

        }
    }

}

