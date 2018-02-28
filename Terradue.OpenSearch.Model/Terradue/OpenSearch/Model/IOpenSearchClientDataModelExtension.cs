using System;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using System.Net;


namespace Terradue.OpenSearch.Model {



    public interface IOpenSearchClientDataModelExtension {

        void InitModelExtension(NameValueCollection parameters);

        string Name { get; }

        string Description { get; }

        Dictionary<string, IMetadataExtractor> MetadataExtractors { get; }

        void LoadOpenSearchResultCollection(IOpenSearchResultCollection osr);

        List<string> GetMetadataForItem(List<string> metadataPaths, IOpenSearchResultItem item);

        void ApplyParameters();

        IOpenSearchResultCollection GetCollection();

        void SetQueryParameters(NameValueCollection nvc);

        IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine factory, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings);

    }

}

