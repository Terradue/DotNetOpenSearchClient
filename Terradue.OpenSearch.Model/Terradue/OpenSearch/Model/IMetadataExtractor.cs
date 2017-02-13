using System;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Model
{
	public interface IMetadataExtractor
	{

        string Description { get; }

        string GetMetadata(IOpenSearchResultItem item, string specifier);

	}


}

