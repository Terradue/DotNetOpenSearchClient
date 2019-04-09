using System;
using System.Collections.Generic;
using Terradue.OpenSearch.Benchmarking;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model {

    /// <summary>
    /// This class is to be used when the result of the opensearch query is incomplete
    /// due, for instance, to server errors.
    /// </summary>
    public class PartialAtomSearchResponse : AtomOpenSearchResponse {

        public IOpenSearchResultCollection PartialOpenSearchResultCollection { get; set; }

        public PartialAtomSearchResponse(AtomFeed result, IOpenSearchResultCollection partialOpenSearchResultCollection = null) : base(result) {
            if (partialOpenSearchResultCollection != null) {
                PartialOpenSearchResultCollection = partialOpenSearchResultCollection;
            }
        }

    }

}