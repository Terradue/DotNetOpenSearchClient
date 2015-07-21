using System;
using Terradue.OpenSearch.Client.Model.GeoTime;

namespace Terradue.OpenSearch.Client.Model.EarthObservation {
    public class EarthObservationDataModelExtension : GeoTimeDataModelExtension {
        
        protected virtual void InitializeExtractors() {

            base.InitializeExtractors();

            metadataExtractors.Add("productType", new ProductTypeMetadataExtractor());
            metadataExtractors.Add("parentIdentifier", new ParentIdentifierMetadataExtractor(parameters));
            metadataExtractors.Add("orbitNumber", new OrbitNumberMetadataExtractor());
            metadataExtractors.Add("orbitDirection", new OrbitDirectionMetadataExtractor());
            metadataExtractors.Add("track", new TrackMetadataExtractor());
            metadataExtractors.Add("frame", new FrameMetadataExtractor());
            metadataExtractors.Add("swathIdentifier", new SwathIdentifierMetadataExtractor());

        }
    }
}

