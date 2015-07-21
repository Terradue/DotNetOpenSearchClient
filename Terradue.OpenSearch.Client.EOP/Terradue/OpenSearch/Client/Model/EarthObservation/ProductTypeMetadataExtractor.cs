using System;
using Terradue.OpenSearch.Client.Model.GeoTime;

namespace Terradue.OpenSearch.Client.Model.EarthObservation
{
    class ProductTypeMetadataExtractor : IMetadataExtractor
	{
        #region IMetadataExtractor implementation

        public string GetMetadata(IOpenSearchResultItem item) {
            throw new NotImplementedException();
        }

        public string Description {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion



	}

}

