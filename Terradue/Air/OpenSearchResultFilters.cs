using System;
using Terradue.OpenSearch.EngineExtensions;
using Terradue.OpenSearch.GeoJson.Extensions;
using Terradue.Metadata.Model;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Result;
using System.ServiceModel.Syndication;

namespace Terradue.OpenSearchClient {
    public class OpenSearchResultFilters {
        public OpenSearchResultFilters() {
        }

        public static void ProxyOpenSearchResult(IOpenSearchResult osr) {

            if (!(osr.OpenSearchableEntity is ILocalOpenSearchable)) return;

            if ( osr.Result is SyndicationFeed) {

                AtomOpenSearchEngineExtension.ReplaceIdentifier(osr, EOProductFactory.EntrySelfLinkTemplate);
                AtomOpenSearchEngineExtension.ReplaceSelfLinks(osr, EOProductFactory.EntrySelfLinkTemplate);   
                AtomOpenSearchEngineExtension.ReplaceOpenSearchDescriptionLinks(osr);                    

            }

            if (osr.Result is IOpenSearchResultCollection) {

                FeatureCollectionOpenSearchEngineExtension.ReplaceId(osr, EOProductFactory.EntrySelfLinkTemplate);
                FeatureCollectionOpenSearchEngineExtension.ReplaceSelfLinks(osr, EOProductFactory.EntrySelfLinkTemplate);   
                FeatureCollectionOpenSearchEngineExtension.ReplaceOpenSearchDescriptionLinks(osr);                    

            }
        }
    }
}

