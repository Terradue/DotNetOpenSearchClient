using System;
using System.Xml;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel;
using Terradue.ServiceModel.Ogc;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class InstrumentTypeMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            ServiceModel.Ogc.Om20.OM_ObservationType eo = null;
            var extensions = item.ElementExtensions;

            foreach (var ext in extensions) {
                if (ext.OuterName == "EarthObservation") {
                    XmlReader reader = ext.GetReader();

                    eo = (ServiceModel.Ogc.Om20.OM_ObservationType)OgcHelpers.DeserializeEarthObservation(reader);
                }
            }

            if (eo != null) {
                if (eo is ServiceModel.Ogc.Eop21.EarthObservationType) {
                    try {
                        return eo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.instrumentType;
                    } catch (Exception) {
                        return null;
                    }
                }

                if (eo is ServiceModel.Ogc.Eop20.EarthObservationType) {
                    try {
                        return eo.procedure.Eop20EarthObservationEquipment.instrument[0].Instrument.instrumentType;
                    } catch (Exception) {
                        return null;
                    }
                }
            }

            return null;
        }


        public string Description {
            get {
                return "A number representing the instrument type";
            }
        }

        #endregion
    }


}
