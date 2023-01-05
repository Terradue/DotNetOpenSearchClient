using System.Collections.Specialized;
using System.Linq;
using log4net;

namespace Terradue.OpenSearch.Model.GeoTime {

    class ExtensionMetadataExtractor : IMetadataExtractor {

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		NameValueCollection parameters;

		public ExtensionMetadataExtractor(NameValueCollection parameters)
		{
			this.parameters = parameters;
		}

		#region IMetadataExtractor implementation

		public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier)
		{

			if (item.ElementExtensions == null || item.ElementExtensions.Count() == 0)
				return "";

            // retrieve all extensions
			return string.Join(";", item.ElementExtensions.Select(ext => string.Format("{0}:{1}", ext.OuterName, ext)));
		}

		public string Description
		{
			get
			{
				return string.Format(
					"Extensions in a semi-colon seprated format");
			}
		}

		#endregion

		
	}

}