using System;
using System.Linq;
using Terradue.OpenSearch.Model.GeoTime;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Ogc;
using Terradue.ServiceModel.Ogc.Eop21;

namespace Terradue.OpenSearch.Model.EarthObservation {

    class VendorSpecificMetadataExtractor : IMetadataExtractor {

        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item, string specifier)
        {
            var eo = (EarthObservationType)Terradue.Metadata.EarthObservation.OpenSearch.Extensions.EarthObservationOpenSearchResultExtensions.GetEarthObservationProfile(item);
            try
            {
                return eo.EopMetaDataProperty.EarthObservationMetaData.vendorSpecific.FirstOrDefault(
                    vs => vs.SpecificInformation.localAttribute == specifier)
                    .SpecificInformation.localValue;
            }
            catch
            {
                return null;
            }
        }

        public string Description
        {
            get
            {
                return "A vendor specific local value from the local name";
            }
        }

        #endregion

    }

}

