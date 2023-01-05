using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.Feature;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Linq;
using Terradue.Metadata.EarthObservation.OpenSearch.Extensions;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    public class WellKnownTextMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public virtual string Description {
            get {
                return "Well Know Text geometry";
            }
        }


        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier) {
            string wkt = null;
            if (item is Feature)
				wkt = WktExtensions.ToWkt((Feature)item);
			if (!string.IsNullOrEmpty(wkt))
				return wkt;
			var geom = item.FindGeometry();
            if (geom == null) {
                foreach (SyndicationElementExtension ext in item.ElementExtensions.ToArray()) {

                    if (ext.OuterNamespace == "http://www.georss.org/georss/10" )
                        geom = Terradue.ServiceModel.Ogc.GeoRss.GeoRss10.GeoRss10Extensions.ToGeometry(Terradue.ServiceModel.Ogc.GeoRss.GeoRss10.GeoRss10Helper.Deserialize(ext.GetReader()));

                    if (ext.OuterNamespace == "http://www.georss.org/georss" )
                        geom = Terradue.ServiceModel.Ogc.GeoRss.GeoRss.GeoRssExtensions.ToGeometry(Terradue.ServiceModel.Ogc.GeoRss.GeoRss.GeoRssHelper.Deserialize(ext.GetReader()));

                }
                    
            }
			if ( geom != null )
				return geom.ToWkt();

			return null;
        }

        #endregion




    }

}

