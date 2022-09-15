using System;
using Terradue.OpenSearch.Model.GeoTime;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using System.Net;
using Terradue.OpenSearch.Model.EarthObservation.OpenSearchable;
using log4net;
using System.Linq;

namespace Terradue.OpenSearch.Model.EarthObservation {
    
    [OpenSearchClientExtension("EOP", "Earth observation Profile client extension" )]
    public class EarthObservationDataModelExtension : GeoTimeDataModelExtension, IOpenSearchClientDataModelExtension {

        private static readonly ILog log = LogManager.GetLogger(typeof(EarthObservationDataModelExtension));

        protected override void InitializeExtractors() {

            base.InitializeExtractors();

            metadataExtractors.Remove("wkt");
            metadataExtractors.Add("wkt", new GeometryMetadataExtractor());
            metadataExtractors.Remove("identifier");
            metadataExtractors.Add("identifier", new IdentifierMetadataExtractor());
            metadataExtractors.Remove("startdate");
            metadataExtractors.Add("startdate", new StartDateMetadataExtractor());
            metadataExtractors.Remove("enddate");
            metadataExtractors.Add("enddate", new EndDateMetadataExtractor());
            metadataExtractors.Add("productType", new ProductTypeMetadataExtractor());
            metadataExtractors.Add("parentIdentifier", new ParentIdentifierMetadataExtractor());
            metadataExtractors.Add("orbitNumber", new OrbitNumberMetadataExtractor());
            metadataExtractors.Add("orbitDirection", new OrbitDirectionMetadataExtractor());
            metadataExtractors.Add("track", new TrackMetadataExtractor());
            metadataExtractors.Add("frame", new FrameMetadataExtractor());
            metadataExtractors.Add("swathIdentifier", new SwathIdentifierMetadataExtractor());
            metadataExtractors.Add("platform", new PlatformShortNameMetadataExtractor());
            metadataExtractors.Add("operationalMode", new OperationalModeMetadataExtractor());
            metadataExtractors.Add("polarisationChannels", new PolarisationChannelsMetadataExtractor());
            metadataExtractors.Add("wrsLongitudeGrid", new WrsLongitudeGridMetadataExtractor());
			metadataExtractors.Add("wrsLatitudeGrid", new WrsLatitudeGridMetadataExtractor());
            metadataExtractors.Add("processingLevel", new ProcessingLevelMetadataExtractor());
            metadataExtractors.Add("id", new IdMetadataExtractor());
			metadataExtractors.Add("cc", new CloudCoverMetadataExtractor());
            metadataExtractors.Add("sensorResolution", new SensorResolutionMetadataExtractor());
            metadataExtractors.Add("vs", new VendorSpecificMetadataExtractor());
            metadataExtractors.Add("instrument", new InstrumentShortNameMetadataExtractor());
            metadataExtractors.Add("instrumentShortName", new InstrumentShortNameMetadataExtractor());
            metadataExtractors.Add("instrumentType", new InstrumentTypeMetadataExtractor());
            metadataExtractors.Add("instrumentDescription", new InstrumentDescriptionMetadataExtractor());
            metadataExtractors.Add("sensor", new SensorTypeMetadataExtractor());
            metadataExtractors.Add("sensorType", new SensorTypeMetadataExtractor());
            metadataExtractors.Add("cycle", new CycleNumberMetadataExtractor());
            metadataExtractors.Add("cycleNumber", new CycleNumberMetadataExtractor());
        }

        public override string Name {
            get {
                return "EOP";
            }
        }

        public override string Description {
            get {
                return "Data model to handle Earth Observation dataset according to OGC® OpenSearch Extension for Earth Observation OGC 13 026\n" +
                "Some parameters allows to override or to filter metadata:" +
                "enclosure:scheme filters enclosure with corresponding scheme" +
                "enclosure:host filters enclosure with corresponding hostname";
            }
        }

        public override void SetQueryParameters(NameValueCollection nvc) {
            
        }

        public override IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings) {
            List<IOpenSearchable> entities = new List<IOpenSearchable>();

            IOpenSearchEngineExtension ext;

            if (string.IsNullOrEmpty(queryFormatArg)) {
                ext = ose.GetExtensionByExtensionName("atom");
            } else {
                ext = ose.GetExtensionByExtensionName(queryFormatArg);
            }

			for (int i = 0; i < baseUrls.Count(); i++)
			{
				var url = baseUrls.ElementAt(i);

			    settings.Credentials = netCreds == null ? null : netCreds.ElementAt(i);

				IOpenSearchable e = null;
                // QC Sentinel1 case
                if (url.Host == "qc.sentinel1.eo.esa.int") {
                    log.DebugFormat("QC Sentinel1 source. No longer supported; using aux.sentinel1.eo.esa.int instead");
                    log.DebugFormat("AUX Sentinel1 source. Trying to get the earthobservation profile");
                    //e = new Sentinel1QcOpenSearchable(url, settings.OpenSearchEngine);
                    e = new Sentinel1AuxOpenSearchable(url, settings.OpenSearchEngine);
                    entities.Add(e);
                    continue;
                }
                if (url.Host == "aux.sentinel1.eo.esa.int") {
                    log.DebugFormat("AUX Sentinel1 source. Trying to get the earthobservation profile");
                    e = new Sentinel1AuxOpenSearchable(url, settings.OpenSearchEngine);
                    entities.Add(e);
                    continue;
                }
                // USGS case
                if (url.Host == "earthexplorer.usgs.gov") {
					log.DebugFormat("Usgs source. Trying to get the earthobservation profile");

                    // usgsOpenSearchable
                    Terradue.OpenSearch.Usgs.UsgsDataWrapper usgsDataWrapper = new Usgs.UsgsDataWrapper(new Uri("https://m2m.cr.usgs.gov"), settings.Credentials);
                    e = new Terradue.OpenSearch.Usgs.UsgsOpenSearchable(usgsDataWrapper, settings);
                    entities.Add(e);
                    continue;
				}
                e = OpenSearchFactory.FindOpenSearchable(settings, url, ext.DiscoveryContentType);
                if (!e.DefaultMimeType.Contains("profile=http://earth.esa.int/eop")) {
                    try {
                        var ef = OpenSearchFactory.FindOpenSearchable(settings, url, "application/atom+xml; profile=http://earth.esa.int/eop/2.1");
                        e = ef;
                    } catch (EntryPointNotFoundException){
                        log.WarnFormat("No URL template found for EOP profile. Looking later in entries' links");
                    }
                    if (!e.DefaultMimeType.Contains("xml"))
                        throw new InvalidOperationException("No Url in the OpenSearch Description Document that could fit the EOP data model");
                }
                // Fedeo case
                if (url.Host == "fedeo.esa.int" && e.DefaultMimeType == "application/atom+xml" && e is Terradue.OpenSearch.GenericOpenSearchable) {
                    log.DebugFormat("Fedeo source. Trying to get the earthobservation profile");
                    e = FedeoOpenSearchable.CreateFrom(url, settings.OpenSearchEngine);
                }
                // Cwic case
                if (url.Host == "cwic.wgiss.ceos.org" && e.DefaultMimeType == "application/atom+xml" && e is Terradue.OpenSearch.GenericOpenSearchable) {
                    log.DebugFormat("Cwic source. Trying to get the earthobservation profile");
                    e = CwicOpenSearchable.CreateFrom((Terradue.OpenSearch.GenericOpenSearchable)e, settings.OpenSearchEngine);
                }
			    
			    
			    
			    

                entities.Add(e);
            }

            IOpenSearchable entity;

            OpenSearchableFactorySettings settings2 = new OpenSearchableFactorySettings(ose);

            if (entities.Count > 1) {
                entity = new MultiGenericOpenSearchable(entities, settings2);
            } else {
                entity = entities[0];
            }

            return entity;
        }

    }
}

