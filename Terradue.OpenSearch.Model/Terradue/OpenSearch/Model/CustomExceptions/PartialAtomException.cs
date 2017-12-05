using System;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model.CustomExceptions {

    public class PartialAtomException : Exception {

        public List<AtomItem> Items { get; set; }
        public IOpenSearchResultCollection PartialOpenSearchResultCollection { get; set; }
  
        
        public PartialAtomException(){}


        public PartialAtomException(string message = null, List<AtomItem> items = null, IOpenSearchResultCollection openSearchResultCollection = null) : base (message) {

            if (items != null) {
                Items = items;
            }

            if (openSearchResultCollection != null) {
                PartialOpenSearchResultCollection = openSearchResultCollection;
            }
        }
        

    }

}