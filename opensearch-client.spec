Name:           opensearch-client
Url:            https://github.com/Terradue/DotNetOpenSearchClient
License:        AGPLv3
Group:          Productivity/Networking/Web/Servers
Version:        1.8.32
Release:        %{_release}
Summary:        Terradue Opensearch Client
BuildArch:      noarch
Source:         /usr/bin/opensearch-client
Requires:       mono
AutoReqProv:    no
BuildRequires:  libtool


%description
Generic OpenSearch Client giving the ability to retrieve element values from generic opensearch queries.

%define debug_package %{nil}

%prep

%build


%install
mkdir -p %{buildroot}/usr/lib/opensearch-client
cp -r %{_sourcedir}/bin/* %{buildroot}/usr/lib/opensearch-client
mkdir -p %{buildroot}/usr/bin/
cp %{_sourcedir}/opensearch-client %{buildroot}/usr/bin/
mkdir -p %{buildroot}/usr/local/lib/
cp -r %{_sourcedir}/packages/Terradue.Metadata.EarthObservation.*/content/Resources/ne_110m_land %{buildroot}/usr/local/lib/


%post

%postun


%clean
rm -rf %{buildroot}


%files
/usr/lib/opensearch-client/*
/usr/bin/opensearch-client
/usr/local/lib/*

%changelog
