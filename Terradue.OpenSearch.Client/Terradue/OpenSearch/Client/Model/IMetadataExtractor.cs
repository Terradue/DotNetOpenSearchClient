using System;
using Mono.Addins;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Client.Model
{
	public interface IMetadataExtractor
	{

        string Description { get; }

        string GetMetadata(IOpenSearchResultItem item);

	}


}

