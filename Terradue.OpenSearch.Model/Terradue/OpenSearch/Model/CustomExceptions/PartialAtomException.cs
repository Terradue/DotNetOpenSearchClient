using System;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;


namespace Terradue.OpenSearch.Model.CustomExceptions {
    
    /// <summary>
    /// This exception should be thrown when the server delivers corrupted data but you still want to return
    /// a partial list of AtomItem or a partial OpenSearchResultCollection including the valid data.
    /// </summary>
    public class PartialAtomException : Exception {

        public List<AtomItem> Items { get; set; }
        public IOpenSearchResultCollection PartialOpenSearchResultCollection { get; set; }
  
        
        public PartialAtomException(){}


        public PartialAtomException(string message = null, List<AtomItem> items = null) : base (message) {

            if (items != null) {
                Items = items;
            }
        }
        
        public PartialAtomException(string message = null, IOpenSearchResultCollection openSearchResultCollection = null) : base (message) {

            if (openSearchResultCollection != null) {
                PartialOpenSearchResultCollection = openSearchResultCollection;
            }
        }
        
        
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