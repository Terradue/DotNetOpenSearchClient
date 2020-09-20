
%define debug_package %{nil}
%define __jar_repack  %{nil}

Name:           opensearch-client
Url:            https://github.com/Terradue/DotNetOpenSearchClient
License:        AGPLv3
Group:          Productivity/Networking/Web/Servers
Version:        %{_version}
Release:        %{_release}
Summary:        Terradue Opensearch Client
BuildArch:      noarch
Source:         /usr/bin/opensearch-client
Requires:       mono
AutoReqProv:    no

%description
Generic OpenSearch Client giving the ability to retrieve element values from generic opensearch queries.

%prep

%build


%install
cp -r %{_sourcedir}/* %{buildroot}
rm -f %{buildroot}/opensearch-client


# temporary commnented 
#cp -r %{_sourcedir}/packages/terradue.metadata.earthobservation/*/content/Resources/ne_110m_land %{buildroot}/usr/local/lib/

%post

%postun


%clean
# rm -rf %{buildroot}


%files
/usr/lib/opensearch-client/*
/usr/bin/opensearch-client


%changelog
