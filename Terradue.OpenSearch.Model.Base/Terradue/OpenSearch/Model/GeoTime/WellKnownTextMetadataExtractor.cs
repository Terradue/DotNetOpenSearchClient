using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.Feature;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Linq;

namespace Terradue.OpenSearch.Model.GeoTime {
    
    public class WellKnownTextMetadataExtractor : IMetadataExtractor {
        #region IMetadataExtractor implementation

        public virtual string Description {
            get {
                return "Well Know Text geometry";
            }
        }


        public virtual string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item) {
            string geom = null;
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
                foreach (SyndicationElementExtension ext in item.ElementExtensions.ToArray()) {

                    if (ext.OuterNamespace == "http://www.georss.org/georss/10" || ext.OuterNamespace == "http://www.georss.org/georss")
                        geom = GeometryFactory.GeoRSSToGeometry(ext.GetObject<XmlElement>()).ToWkt();

                }
                    
            }
            return geom;
        }
        public bool SetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string value) {
            //TODO
            return false;
        }

        #endregion




    }

}

