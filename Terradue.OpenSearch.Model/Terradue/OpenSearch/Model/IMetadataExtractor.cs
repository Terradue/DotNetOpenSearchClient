using System;
using Mono.Addins;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model
{
	public interface IMetadataExtractor
	{

        string Description { get; }

        string GetMetadata(IOpenSearchResultItem item);

        bool SetMetadata(IOpenSearchResultItem item, List<string> parameters);

	}


}

