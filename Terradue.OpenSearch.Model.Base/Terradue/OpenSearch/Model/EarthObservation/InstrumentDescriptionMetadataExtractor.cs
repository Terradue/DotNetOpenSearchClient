﻿using System;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Ogc;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class InstrumentDescriptionMetadataExtractor : IMetadataExtractor {
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
                        return eo.procedure.Eop21EarthObservationEquipment.instrument.Instrument.description;
                    } catch (Exception) {
                        return null;
                    }
                }

                if (eo is ServiceModel.Ogc.Eop20.EarthObservationType) {
                    try {
                        return eo.procedure.Eop20EarthObservationEquipment.instrument[0].Instrument.description;
                    } catch (Exception) {
                        return null;
                    }
                }
            }

            return null;
        }


        public string Description {
            get {
                return "A number representing the instrument description, if available";
            }
        }

        #endregion
    }


}
