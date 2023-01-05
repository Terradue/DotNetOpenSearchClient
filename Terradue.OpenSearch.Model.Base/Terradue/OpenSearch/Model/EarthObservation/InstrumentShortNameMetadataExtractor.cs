﻿using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class InstrumentShortNameMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier) {

            return Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindInstrumentShortName(item);
        }

        public string Description {
            get {
                return "A string identifying the instrument.";
            }
        }

        #endregion
    }


}
