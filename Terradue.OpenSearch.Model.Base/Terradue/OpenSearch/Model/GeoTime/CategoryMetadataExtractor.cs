using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Model.GeoTime {

	class CategoryMetadataExtractor : IMetadataExtractor {

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		NameValueCollection parameters;

		public CategoryMetadataExtractor(NameValueCollection parameters)
		{
			this.parameters = parameters;
		}

		#region IMetadataExtractor implementation

		public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier)
		{

			if (item.Categories == null || item.Categories.Count() == 0)
				return "";

            // retrieve all categories

			return string.Join(";", item.Categories.Select(cat => cat.Name));
		}

		public string Description
		{
			get
			{
				return string.Format(
					"Categories in a semi-colon seprated format");
			}
		}

		#endregion

		
	}

}