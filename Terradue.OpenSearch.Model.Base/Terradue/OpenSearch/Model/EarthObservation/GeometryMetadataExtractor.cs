using System;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.GeoJson.Geometry;

namespace Terradue.OpenSearch.Model.EarthObservation
{
    class GeometryMetadataExtractor : WellKnownTextMetadataExtractor
	{
        #region IMetadataExtractor implementation
        public override string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {

            string geom = base.GetMetadata(item, specifier);

            if (geom == null) {
                var geometry = Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.FindGeometry(item);
                if (geometry != null)
                    geom = geometry.ToWkt();
            }

            return geom;
        }


        /*public string Description {
            get {
                throw new NotImplementedException();
            }
        }*/
        #endregion
	}

}

