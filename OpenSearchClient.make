

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=.

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG_X86)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;"
ASSEMBLY = bin/Debug/OpenSearchClient.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll \
	externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
BUILD_DIR = bin/Debug

OPENSEARCHCLIENT_EXE_MDB_SOURCE=bin/Debug/OpenSearchClient.exe.mdb
OPENSEARCHCLIENT_EXE_MDB=$(BUILD_DIR)/OpenSearchClient.exe.mdb
LOG4NET_DLL_SOURCE=packages/log4net.2.0.0/lib/net40-full/log4net.dll
TERRADUE_METADATA_DLL_SOURCE=packages/Terradue.Metadata.1.3.1.0/lib/net40/Terradue.Metadata.dll
TERRADUE_UTIL_DLL_SOURCE=packages/Terradue.Util.1.3.1.0/lib/net40/Terradue.Util.dll
SERVICESTACK_TEXT_DLL_SOURCE=packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_DLL_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll.mdb

endif

ifeq ($(CONFIG),RELEASE_X86)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/OpenSearchClient.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll \
	externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
BUILD_DIR = bin/Release

OPENSEARCHCLIENT_EXE_MDB=
LOG4NET_DLL_SOURCE=packages/log4net.2.0.0/lib/net40-full/log4net.dll
TERRADUE_METADATA_DLL_SOURCE=packages/Terradue.Metadata.1.3.1.0/lib/net40/Terradue.Metadata.dll
TERRADUE_UTIL_DLL_SOURCE=packages/Terradue.Util.1.3.1.0/lib/net40/Terradue.Util.dll
SERVICESTACK_TEXT_DLL_SOURCE=packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_DLL_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll.mdb

endif

ifeq ($(CONFIG),LIST_OSEE_X86)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;"
ASSEMBLY = bin/Debug/OpenSearchClient.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll \
	externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
BUILD_DIR = bin/Debug

OPENSEARCHCLIENT_EXE_MDB_SOURCE=bin/Debug/OpenSearchClient.exe.mdb
OPENSEARCHCLIENT_EXE_MDB=$(BUILD_DIR)/OpenSearchClient.exe.mdb
LOG4NET_DLL_SOURCE=packages/log4net.2.0.0/lib/net40-full/log4net.dll
TERRADUE_METADATA_DLL_SOURCE=packages/Terradue.Metadata.1.3.1.0/lib/net40/Terradue.Metadata.dll
TERRADUE_UTIL_DLL_SOURCE=packages/Terradue.Util.1.3.1.0/lib/net40/Terradue.Util.dll
SERVICESTACK_TEXT_DLL_SOURCE=packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB_SOURCE=externals/DotNetGeoJson/Terradue.GeoJson/bin/Terradue.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_DLL_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB_SOURCE=externals/DotNetOpenSearch/Terradue.OpenSearch/bin/Terradue.OpenSearch.dll.mdb

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(OPENSEARCHCLIENT_EXE_MDB) \
	$(LOG4NET_DLL) \
	$(TERRADUE_METADATA_DLL) \
	$(TERRADUE_UTIL_DLL) \
	$(SERVICESTACK_TEXT_DLL) \
	$(TERRADUE_GEOJSON_DLL) \
	$(TERRADUE_GEOJSON_DLL_MDB) \
	$(TERRADUE_OPENSEARCH_DLL) \
	$(TERRADUE_OPENSEARCH_DLL_MDB)  

BINARIES = \
	$(OPENSEARCHCLIENT)  


RESGEN=resgen2

LOG4NET_DLL = $(BUILD_DIR)/log4net.dll
TERRADUE_METADATA_DLL = $(BUILD_DIR)/Terradue.Metadata.dll
TERRADUE_UTIL_DLL = $(BUILD_DIR)/Terradue.Util.dll
SERVICESTACK_TEXT_DLL = $(BUILD_DIR)/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL = $(BUILD_DIR)/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB = $(BUILD_DIR)/Terradue.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_DLL = $(BUILD_DIR)/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB = $(BUILD_DIR)/Terradue.OpenSearch.dll.mdb
OPENSEARCHCLIENT = $(BUILD_DIR)/opensearchclient

FILES = \
	Terradue/Air/OpenSearchClient.cs \
	Properties/AssemblyInfo.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	packages.config \
	OpenSearchClient.nuspec \
	opensearchclient.in 

REFERENCES =  \
	System \
	System.Xml \
	System.Data \
	System.ServiceModel \
	-pkg:mono-addins

DLL_REFERENCES =  \
	packages/log4net.2.0.0/lib/net40-full/log4net.dll \
	packages/Terradue.Metadata.1.3.1.0/lib/net40/Terradue.Metadata.dll \
	packages/Terradue.Util.1.3.1.0/lib/net40/Terradue.Util.dll \
	packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

#Targets
all-local: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make



$(eval $(call emit-deploy-target,LOG4NET_DLL))
$(eval $(call emit-deploy-target,TERRADUE_METADATA_DLL))
$(eval $(call emit-deploy-target,TERRADUE_UTIL_DLL))
$(eval $(call emit-deploy-target,SERVICESTACK_TEXT_DLL))
$(eval $(call emit-deploy-target,TERRADUE_GEOJSON_DLL))
$(eval $(call emit-deploy-target,TERRADUE_GEOJSON_DLL_MDB))
$(eval $(call emit-deploy-target,TERRADUE_OPENSEARCH_DLL))
$(eval $(call emit-deploy-target,TERRADUE_OPENSEARCH_DLL_MDB))
$(eval $(call emit-deploy-wrapper,OPENSEARCHCLIENT,opensearchclient,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(OPENSEARCHCLIENT_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LOG4NET_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_METADATA_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_UTIL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SERVICESTACK_TEXT_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_GEOJSON_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(OPENSEARCHCLIENT),$(DESTDIR)$(bindir))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(OPENSEARCHCLIENT_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LOG4NET_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_METADATA_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_UTIL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SERVICESTACK_TEXT_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_GEOJSON_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(OPENSEARCHCLIENT),$(DESTDIR)$(bindir))
	make post-uninstall-local-hook prefix=$(prefix)
