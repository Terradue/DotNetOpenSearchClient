using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Amazon.S3;
using IniParser;
using IniParser.Model;
using Terradue.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace Terradue.OpenSearch.Model.GeoTime {

	class EnclosureMetadataExtractor : IMetadataExtractor {

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		NameValueCollection parameters;

		public EnclosureMetadataExtractor(NameValueCollection parameters)
		{
			this.parameters = parameters;
		}

		#region IMetadataExtractor implementation

		public string GetMetadata(Terradue.OpenSearch.Result.IOpenSearchResultItem item, string specifier)
		{

			if (item.Links == null)
				return "";

			// retrieve all enclosure links
			var links = item.Links.Where(l => l.RelationshipType == "enclosure");

			// Enclosure availability control function
			if (!parameters.AllKeys.Contains("disableEnclosureControl") && !parameters.AllKeys.Contains("allEnclosures"))
			{
				links = RemoveNonAvailableLinks(links);
			}

			string enclosure;
			if (links == null || links.Count() == 0)
			{
				enclosure = "";
			}
			else if (parameters.AllKeys.Contains("allEnclosures"))
			{
				enclosure = string.Join("|", links.Select(l => l.Uri.ToString()));
			}
			else
			{
				enclosure = links.First().Uri.ToString();
			}
			return enclosure;
		}

		public string Description
		{
			get
			{
				return string.Format(
					"Link to related resource that is potentially large in size and might require special handling (RFC 4287). This metadata takes into account the following data model parameters: enclosure:scheme, enclosure:host");
			}
		}

		#endregion

		private IEnumerable<SyndicationLink> RemoveNonAvailableLinks(IEnumerable<SyndicationLink> links)
		{
			List<SyndicationLink> finalLinks = new List<SyndicationLink>();

			foreach (var link in links)
			{
				switch (link.Uri.Scheme)
				{
					case "s3":
						if (TestS3LinkAsync(link).GetAwaiter().GetResult()) finalLinks.Add(link);
						break;
					case "file":
                        if (TestFileLink(link)) finalLinks.Add(link);
                        break;
					default:
						finalLinks.Add(link);
						break;
				}
			}

			return finalLinks;

		}

		private bool TestFileLink(SyndicationLink link)
		{
			return File.Exists(link.Uri.AbsolutePath);
		}

		private async Task<bool> TestS3LinkAsync(SyndicationLink link)
		{
			string accessKey = "";
			string secretKey = "";
			string hostBase = "";
			bool useHttps = false;
			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				   Environment.OSVersion.Platform == PlatformID.MacOSX)
                	? Environment.GetEnvironmentVariable("HOME")
                	: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

			try
			{
				var parser = new FileIniDataParser();
				IniData data = parser.ReadFile(Path.Combine(homePath, ".s3cfg"));

				accessKey = data["default"]["access_key"];
				secretKey = data["default"]["host_base"];
				hostBase = data["default"]["host_base"];
				bool.TryParse(data["default"]["use_https"], out useHttps);
			}
			catch (Exception)
			{
				log.Warn("No S3 config to check the S3 links!");
				return false;
			}


			AmazonS3Config config = new AmazonS3Config();
			config.ServiceURL = string.Format("http{0}://{1}", useHttps ? "s" : "", hostBase);

			AmazonS3Client client = new AmazonS3Client(accessKey, secretKey, config);
			
			Uri uriWithManifestFile = new Uri(link.Uri, "manifest.safe");
			Match match = Regex.Match(uriWithManifestFile.AbsolutePath , @"^\/(?'bucket'[^\/]*)\/(?'key'.*)$");

			if (!match.Success) return false;

			try
			{
				var list = await client.ListObjectsAsync(match.Groups["bucket"].Value, match.Groups["key"].Value);
				log.DebugFormat("S3 reponse: {0} objects: {1}", list.HttpStatusCode, string.Join(",", list.S3Objects.Select(s3 => s3.Key)));

				if (list.HttpStatusCode == System.Net.HttpStatusCode.OK && list.S3Objects.Count() > 0) return true;
			}
			catch (Exception e)
			{
				log.WarnFormat("Error listing {0} using {1} : {2}", link.Uri, config.ServiceURL, e.Message);
				return false;
			}

			return false;
		}
	}

}