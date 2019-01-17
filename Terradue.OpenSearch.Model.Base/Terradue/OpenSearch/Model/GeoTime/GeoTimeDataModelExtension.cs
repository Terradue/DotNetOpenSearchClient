using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Linq;
using System.Collections.ObjectModel;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Engine;
using System.Net;

namespace Terradue.OpenSearch.Model.GeoTime
{

    [OpenSearchClientExtension("GeoTime", "Geo And Time Client extension")]
    public class GeoTimeDataModelExtension : IOpenSearchClientDataModelExtension
    {

        protected Dictionary<string, IMetadataExtractor> metadataExtractors;
        protected IOpenSearchResultCollection osr;
        protected NameValueCollection parameters;

        protected virtual void InitializeExtractors()
        {

            metadataExtractors = new Dictionary<string, IMetadataExtractor>();

            metadataExtractors.Add("wkt", new WellKnownTextMetadataExtractor());
            metadataExtractors.Add("title", new TitleMetadataExtractor());
            metadataExtractors.Add("enclosure", new EnclosureMetadataExtractor(parameters));
            metadataExtractors.Add("identifier", new IdentifierMetadataExtractor());
            metadataExtractors.Add("startdate", new StartDateMetadataExtractor());
            metadataExtractors.Add("enddate", new EndDateMetadataExtractor());
            metadataExtractors.Add("published", new PublicationDateMetadataExtractor());
            metadataExtractors.Add("updated", new UpdatedMetadataExtractor());
            metadataExtractors.Add("related", new RelatedMetadataExtractor(parameters));
            metadataExtractors.Add("self", new SelfLinkMetadataExtractor(parameters));
            metadataExtractors.Add("link", new LinkMetadataExtractor(parameters));
            metadataExtractors.Add("cat", new CategoryMetadataExtractor(parameters));
            metadataExtractors.Add("ext", new ExtensionMetadataExtractor(parameters));
        }

        #region IOpenSearchClientDataModelExtension implementation

        public void InitModelExtension(NameValueCollection parameters)
        {
            this.parameters = parameters;
            this.InitializeExtractors();


        }

        public System.Collections.Generic.Dictionary<string, IMetadataExtractor> MetadataExtractors
        {
            get
            {
                return metadataExtractors;
            }
        }


        public List<string> GetMetadataForItem(List<string> metadataPaths, IOpenSearchResultItem item)
        {

            List<string> metadataItems = new List<string>();

            if (osr != null)
            {

                foreach (var metadata in metadataPaths)
                {

                    string path = metadata;
                    string specifier = null;

                    if (metadata.Contains(":"))
                    {
                        path = metadata.Split(':')[0];
                        specifier = metadata.Split(':')[1];
                    }

                    if (!metadataExtractors.ContainsKey(path))
                        throw new NotSupportedException(string.Format("metadata extractor \"{0}\" not found in the data model", path));



                    metadataItems.Add(metadataExtractors[path].GetMetadata(item, specifier));


                }
            }

            return metadataItems;
        }

        public void LoadOpenSearchResultCollection(Terradue.OpenSearch.Result.IOpenSearchResultCollection osr)
        {
            this.osr = osr;
        }

        public virtual string Name
        {
            get
            {
                return "GeoTime";
            }
        }

        public virtual string Description
        {
            get
            {
                return "Data model to handle simple Geo & Time dataset according to OGC® OpenSearch Geo and Time Extensions 10-032r8\n" +
                "Some parameters allows to ovveride or to filter metadata:" +
                "enclosure:scheme filters enclosure with corresponding scheme" +
                "enclosure:host filters enclosure with corresponding hostname";
            }
        }

        public IOpenSearchResultCollection GetCollection()
        {
            return osr;

        }

        public virtual void ApplyParameters()
        {

            foreach (var item in osr.Items.ToArray())
            {

                if (!string.IsNullOrEmpty(parameters["enclosure:scheme"]))
                {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:scheme"]).ToList());
                }
                if (!string.IsNullOrEmpty(parameters["enclosure:host"]))
                {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "enclosure" || l.Uri.Scheme == parameters["enclosure:host"]).ToList());
                }

                if (!string.IsNullOrEmpty(parameters["related:title"]))
                {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "related" || l.Title == parameters["related:title"]).ToList());
                }
                if (!string.IsNullOrEmpty(parameters["related:type"]))
                {
                    item.Links = new Collection<SyndicationLink>(item.Links.Where(l => l.RelationshipType != "related" || l.RelationshipType == parameters["related:type"]).ToList());
                }

            }

        }


        public virtual void SetQueryParameters(NameValueCollection nvc)
        {

        }


        public virtual IOpenSearchable CreateOpenSearchable(IEnumerable<Uri> baseUrls, string queryFormatArg, OpenSearchEngine ose, IEnumerable<NetworkCredential> netCreds, OpenSearchableFactorySettings settings)
        {
            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            for (int i = 0; i < baseUrls.Count(); i++)
            {
                var url = baseUrls.ElementAt(i);

                settings.Credentials = netCreds == null ? null : netCreds.ElementAt(i);
           
                if (string.IsNullOrEmpty(queryFormatArg))
                    try
                    {
                        entities.Add(OpenSearchFactory.FindOpenSearchable(settings, url, null));
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                else
                {
                    var e = OpenSearchFactory.FindOpenSearchable(settings, url, ose.GetExtensionByExtensionName(queryFormatArg).DiscoveryContentType);
                    entities.Add(e);
                }
            }

            IOpenSearchable entity;
            OpenSearchableFactorySettings settings2 = new OpenSearchableFactorySettings(ose);

            if (entities.Count > 1)
            {
                entity = new MultiGenericOpenSearchable(entities, settings2);
            }
            else
            {
                entity = entities[0];
            }

            return entity;
        }

        #endregion
    }
}

