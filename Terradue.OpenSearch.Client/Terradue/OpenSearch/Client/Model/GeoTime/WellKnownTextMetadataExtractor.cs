using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.Feature;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Client.Model.GeoTime
{
    
    class WellKnownTextMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation

        public string Description {
            get {
                return "Well Know Text geometry";
            }
        }


        public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string geom = "";
            if (item is Feature)
                geom = WktFeatureExtensions.ToWkt((Feature)item);
            if (geom == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                    if (ext.OuterName == "spatial") {
                        geom = ext.GetObject<string>();
                        break;
                    }
                }
            }
            if (geom == null) {
                var geometry = Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindGeometryFromEarthObservation(item);
                if (geometry != null)
                    geom = geometry.ToWkt();
            }

            return geom;
        }
        #endregion




	}

}

