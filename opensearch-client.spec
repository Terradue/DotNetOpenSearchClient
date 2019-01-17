Name:           opensearch-client
Url:            https://github.com/Terradue/DotNetOpenSearchClient
License:        AGPLv3
Group:          Productivity/Networking/Web/Servers
Version:        1.8.21
Release:        %{_release}
Summary:        Terradue Opensearch Client
BuildArch:      noarch
Source:         Global.asax
Requires:       mono-complete
AutoReqProv:    no
BuildRequires:  libtool


%description
Generic OpenSearch Client giving the ability to retrieve element values from generic opensearch queries.

%define debug_package %{nil}

%prep

%build


%install
mkdir -p %{buildroot}/usr/lib/opensearch-client
cp -r %{_sourcedir}/bin %{buildroot}/usr/lib/opensearch-client
mkdir -p %{buildroot}/usr/bin/
cp %{_sourcedir}/opensearch-client %{_sourcedir}/log4net.config %{buildroot}/usr/bin/
mkdir -p %{buildroot}/usr/local/lib/ne_110m_land
cp -r %{_sourcedir}/packages/Terradue.Metadata.EarthObservation.*/content/Resources/ne_110m_land %{buildroot}/usr/local/lib/ne_110m_land


%post

%postun


%clean
rm -rf %{buildroot}


%files
/usr/lib/opensearch-client/*
/usr/bin/opensearch-client  

%changelog
