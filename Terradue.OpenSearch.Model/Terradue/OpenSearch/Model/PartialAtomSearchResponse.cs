using System;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model {

    /// <summary>
    /// This class is to be used when the result of the opensearch query is incomplete
    /// due, for instance, to server errors.
    /// </summary>
    public class PartialAtomSearchResponse : AtomOpenSearchResponse {

        public IOpenSearchResultCollection PartialOpenSearchResultCollection { get; set; }

        public PartialAtomSearchResponse(AtomFeed result, TimeSpan timeSpan, IOpenSearchResultCollection partialOpenSearchResultCollection = null) : base(result, timeSpan) {
            if (partialOpenSearchResultCollection != null) {
                PartialOpenSearchResultCollection = partialOpenSearchResultCollection;
            }
        }

    }

}