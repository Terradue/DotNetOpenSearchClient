
EXTRA_DIST = OpenSearchClient.make rules.make configure Makefile.include packages/log4net.2.0.0/lib/net40-full/log4net.dll packages/Terradue.Metadata.1.3.1.0/lib/net40/Terradue.Metadata.dll packages/Terradue.Util.1.3.1.0/lib/net40/Terradue.Util.dll packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll

all: all-recursive

top_srcdir=.
include $(top_srcdir)/config.make
include $(top_srcdir)/Makefile.include
include $(top_srcdir)/rules.make

#include $(top_srcdir)/custom-hooks.make

#Warning: This is an automatically generated file, do not edit!
ifeq ($(CONFIG),DEBUG_X86)
 SUBDIRS =  externals/DotNetGeoJson/Terradue.GeoJson externals/DotNetOpenSearch/Terradue.OpenSearch . 
endif
ifeq ($(CONFIG),RELEASE_X86)
 SUBDIRS =  externals/DotNetGeoJson/Terradue.GeoJson externals/DotNetOpenSearch/Terradue.OpenSearch . 
endif
ifeq ($(CONFIG),LIST_OSEE_X86)
 SUBDIRS =  externals/DotNetGeoJson/Terradue.GeoJson externals/DotNetOpenSearch/Terradue.OpenSearch . 
endif

# Include project specific makefile
include OpenSearchClient.make

CONFIG_MAKE=$(top_srcdir)/config.make

%-recursive: $(CONFIG_MAKE)
	@set . $$MAKEFLAGS; final_exit=:; \
	case $$2 in --unix) shift ;; esac; \
	case $$2 in *=*) dk="exit 1" ;; *k*) dk=: ;; *) dk="exit 1" ;; esac; \
	make pre-$*-hook prefix=$(prefix) ; \
	for dir in $(call quote_each,$(SUBDIRS)); do \
		case "$$dir" in \
		.) make $*-local || { final_exit="exit 1"; $$dk; };;\
		*) (cd "$$dir" && make $*) || { final_exit="exit 1"; $$dk; };;\
		esac \
	done; \
	make post-$*-hook prefix=$(prefix) ; \
	$$final_exit

$(CONFIG_MAKE):
	echo "You must run configure first"
	exit 1

clean: clean-recursive
install: install-recursive
uninstall: uninstall-recursive

dist: $(CONFIG_MAKE)
	rm -rf $(PACKAGE)-$(VERSION)
	mkdir $(PACKAGE)-$(VERSION)
	make pre-dist-hook distdir=$$distdir
	for dir in $(call quote_each,$(SUBDIRS)); do \
		pkgdir=`pwd`/$(PACKAGE)-$(VERSION); \
		mkdir "$$pkgdir/$$dir" || true; \
		case $$dir in \
		.) make dist-local "distdir=$$pkgdir" || exit 1;; \
		*) (cd "$$dir"; make dist-local "distdir=$$pkgdir/$$dir") || exit 1;; \
		esac \
	done
	(make dist-local distdir=$(PACKAGE)-$(VERSION))
	make post-dist-hook "distsir=$$distdir"
	tar czvf $(PACKAGE)-$(VERSION).tar.gz $(PACKAGE)-$(VERSION)
	rm -rf $(PACKAGE)-$(VERSION)
	@echo "=========================================="
	@echo "$(PACKAGE)-$(VERSION) has been packaged > $(PACKAGE)-$(VERSION).tar.gz"
	@echo "=========================================="

distcheck: dist
	(mkdir test; cd test;  \
	tar xzvf ../$(PACKAGE)-$(VERSION).tar.gz; cd $(PACKAGE)-$(VERSION); \
	./configure --prefix=$$(cd `pwd`/..; pwd); \
	make && make install && make dist);
	rm -rf test
