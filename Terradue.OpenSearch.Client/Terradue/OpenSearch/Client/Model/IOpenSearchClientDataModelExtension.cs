using System;
using Mono.Addins;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;

[assembly:AddinRoot("OpenSearchDataModel", "1.0")]
[assembly:AddinDescription("OpenSearch Data Model")]
namespace Terradue.OpenSearch.Client.Model {

    [TypeExtensionPoint()]
    public interface IOpenSearchClientDataModelExtension {

        void InitModelExtension(NameValueCollection parameters);

        string Name { get; }

        string Description { get; }

        Dictionary<string, IMetadataExtractor> MetadataExtractors { get; }

        void LoadOpenSearchResultCollection(IOpenSearchResultCollection osr);

        List<string> GetMetadataForItem(List<string> metadataPaths, IOpenSearchResultItem item);

        void ApplyParameters();

        IOpenSearchResultCollection GetCollection();

    }

}

