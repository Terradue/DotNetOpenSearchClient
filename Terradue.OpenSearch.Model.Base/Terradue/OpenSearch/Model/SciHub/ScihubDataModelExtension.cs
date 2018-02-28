using System;
using Terradue.OpenSearch.Model.GeoTime;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Model.EarthObservation;
using Terradue.OpenSearch.SciHub;
using System.Net;
using System.Linq;

namespace Terradue.OpenSearch.Model.Scihub {
    
   [OpenSearchClientExtension("Scihub", "SciHub Client extension")]
    public class ScihubDataModelExtension : EarthObservationDataModelExtension, IOpenSearchClientDataModelExtension {
        
        protected override void InitializeExtractors() {

            base.InitializeExtractors();
        }

        public override string Name {
            get {
                return "Scihub";
            }
        }

        public override string Description {
            get {
                return "Data model to handle Science Hub EO dataset according to OGC® OpenSearch Extension for Earth Observation OGC 13 026\n" +
                "Some parameters allows to override or to filter metadata:" +
                "enclosure:scheme filters enclosure with corresponding scheme" +
                "enclosure:host filters enclosure with corresponding hostname";
            }
        }

        public override IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings) {
            
            SciHubOpenSearchable entity = new SciHubOpenSearchable(baseUrls.First(), netCreds.First(), settings.MaxRetries);
            if (queryFormatArg == "eop") {
                entity.DefaultMimeType = "application/atom+xml; profile=http://earth.esa.int/eop/2.1";
            }
              
            return entity;
        }

    }
}

