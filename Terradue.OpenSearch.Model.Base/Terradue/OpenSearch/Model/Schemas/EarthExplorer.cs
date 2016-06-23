using System;

namespace Terradue.OpenSearch.Model.Schemas.EarthExplorer {


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd", IsNullable = false)]
    public partial class scene {

        private sceneMetadataFieldsMetadataField[] metadataFieldsField;

        private sceneBrowseLinksBrowse[] browseLinksField;

        private sceneOverlayLinksOverlay[] overlayLinksField;

        private string linkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("metadataField", typeof(sceneMetadataFieldsMetadataField), IsNullable = false)]
        public sceneMetadataFieldsMetadataField[] metadataFields {
            get {
                return this.metadataFieldsField;
            }
            set {
                this.metadataFieldsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("browse", typeof(sceneBrowseLinksBrowse), IsNullable = false)]
        public sceneBrowseLinksBrowse[] browseLinks {
            get {
                return this.browseLinksField;
            }
            set {
                this.browseLinksField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("overlay", typeof(sceneOverlayLinksOverlay), IsNullable = false)]
        public sceneOverlayLinksOverlay[] overlayLinks {
            get {
                return this.overlayLinksField;
            }
            set {
                this.overlayLinksField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string link {
            get {
                return this.linkField;
            }
            set {
                this.linkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd")]
    public partial class sceneMetadataFieldsMetadataField {

        private string metadataValueField;

        private string nameField;

        private string linkField;

        /// <remarks/>
        public string metadataValue {
            get {
                return this.metadataValueField;
            }
            set {
                this.metadataValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string link {
            get {
                return this.linkField;
            }
            set {
                this.linkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd")]
    public partial class sceneBrowseLinksBrowse {

        private string browseLinkField;

        private string idField;

        private string captionField;

        private string thumbLinkField;

        /// <remarks/>
        public string browseLink {
            get {
                return this.browseLinkField;
            }
            set {
                this.browseLinkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string caption {
            get {
                return this.captionField;
            }
            set {
                this.captionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string thumbLink {
            get {
                return this.thumbLinkField;
            }
            set {
                this.thumbLinkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd")]
    public partial class sceneOverlayLinksOverlay {

        private string overlayLinkField;

        private string idField;

        private string captionField;

        private string typeField;

        private string thumbLinkField;

        /// <remarks/>
        public string overlayLink {
            get {
                return this.overlayLinkField;
            }
            set {
                this.overlayLinkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string caption {
            get {
                return this.captionField;
            }
            set {
                this.captionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string thumbLink {
            get {
                return this.thumbLinkField;
            }
            set {
                this.thumbLinkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://earthexplorer.usgs.gov/eemetadata.xsd", IsNullable = false)]
    public partial class NewDataSet {

        private scene[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("scene")]
        public scene[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
    }

}

