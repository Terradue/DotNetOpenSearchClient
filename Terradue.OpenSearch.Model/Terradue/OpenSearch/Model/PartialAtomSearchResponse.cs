using System;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model {

    public class PartialAtomSearchResponse : AtomOpenSearchResponse {

        public IOpenSearchResultCollection PartialOpenSearchResultCollection { get; set; }

        public PartialAtomSearchResponse(AtomFeed result, TimeSpan timeSpan, IOpenSearchResultCollection partialOpenSearchResultCollection = null) : base(result, timeSpan) {
            if (partialOpenSearchResultCollection != null) {
                PartialOpenSearchResultCollection = partialOpenSearchResultCollection;
            }
        }

    }

}