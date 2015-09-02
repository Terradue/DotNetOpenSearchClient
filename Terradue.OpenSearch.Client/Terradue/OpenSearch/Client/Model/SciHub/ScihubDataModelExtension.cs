using System;
using Terradue.OpenSearch.Client.Model.GeoTime;
using Mono.Addins;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Client.Model.EarthObservation;
using Terradue.OpenSearch.SciHub;
using System.Net;

namespace Terradue.OpenSearch.Client.Model.Scihub {
    
    [Extension(typeof(IOpenSearchClientDataModelExtension))]
    [ExtensionNode("Scihub", "Scihub Data Model")]
    public class ScihubDataModelExtension : EarthObservationDataModelExtension, IOpenSearchClientDataModelExtension {
        
        protected override void InitializeExtractors() {

            base.InitializeExtractors();
            metadataExtractors.Add("id", new IdMetadataExtractor());
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

        public override IOpenSearchable CreateOpenSearchable(List<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, NetworkCredential netCreds) {
            
            SciHubOpenSearchable entity = new SciHubOpenSearchable(baseUrls[0], netCreds);
            if (queryFormatArg == "eop") {
                entity.DefaultMimeType = "application/atom+xml; profile=http://earth.esa.int/eop/2.1";
            }
              
            return entity;
        }

    }
}

