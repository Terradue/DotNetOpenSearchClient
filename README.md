[![Build Status](https://build.terradue.com/buildStatus/icon?job=opensearch-client)](https://build.terradue.com/job/opensearch-client/)

# DotNetOpenSearchClient

Package including the following tools:

- **opensearch-client**: Generic OpenSearch Client giving the ability to retrieve element values from generic opensearch queries

- **data-publisher**: Generic Data Publisher client giving the ability to add/edit/delete data of a catalogue

## Getting Started

DotNetOpenSearchClient is available as yum package on Terradue Developer Cloud Sandboxes.

Log on your Developer Cloud Sandbox and install the package:

```
sudo yum install opensearch-client
```

## Build

DotNetOpenSearchClient is a single assembly designed to be easily deployed anywhere. 

To clone it locally click the "Clone in Desktop" button above or run the 
following git commands.

```
git clone git@github.com:Terradue/DotNetOpenSearchClient.git DotNetOpenSearchClient
```

To compile it yourself, you need to have mono 3.x.x installed (recommanded is mono 3.12.1).
Then you can use:

* Visual Studio 2012 or later, or Xamarin Studio
* xbuild command
 
Once build, simply do:

```
cp -r Terradue.OpenSearch.DataPublisher/Resources/* /usr/local/lib

mkdir /usr/lib/opensearch-client
cp Terradue.OpenSearch.Client/bin/*.exe /usr/lib/opensearch-client
cp Terradue.OpenSearch.Client/bin/*.dll /usr/lib/opensearch-client
./src/main/scripts/opensearch-client


mkdir /usr/lib/data-publisher
cp Terradue.OpenSearch.DataPublisher/bin/*.exe /usr/lib/data-publisher
cp Terradue.OpenSearch.DataPublisher/bin/*.dll /usr/lib/data-publisher
./src/main/scripts/data-publisher
```
*note: you can choose a different path than /usr/lib, but the scripts (under src/main/scripts) need to be updated accordingly*

## Usage

### opensearch-client

```
Usage: opensearch-client [options...] [url1,url2,url3,...] [metadatapath1,metadatapath2,...]

Options:
 -p/--parameter <param>  Specify a parameter for the query
 -o/--output <file>      Write output to <file> instead of stdout
 -f/--format <format>    Specify the format of the query. Format available can be listed with --list-osee.
                         By default, the client is automatic and uses the default or the first format.
 -to/--time-out <file>   Specify query timeout (millisecond)
 --pagination            Specify the pagination number for search loops. Default: 20
 --list-osee             List the OpenSearch Engine Extensions
 -m/--model <format>     Specify the data model of the results for the query. Data model give access to specificmetadata extractors or transformers. By default the "GeoTime" model is used. Used without urls, it lists the metadata options
 -v/--verbose            Make the operation more talkative
```

### data-publisher

```
Usage: data-publisher [action] [options]

Action:
 add      Add items into the catalogue
 edit     Edit the items resulting from the query on the catalogue
 delete   Delete items resulting from the query from the catalogue

Options:
 -h/--help  Prints the usage.
```

#### data-publisher add

```
Function not yet implemented
```

#### data-publisher edit

```
Usage: data-publisher edit [options...] [url] [metadataPath1 metadataParameters1 , metadatapath2 parameters2 , ...]

Options:
 -a/--auth <auth>        Set Credentials to be used (format must be username:password).
 -d/--dir <directory>    Write outputs to the directory <directory> instead of current directory (if <file> is set).
                         Default directory is the current directory.
 -f/--format <format>    Specify the format of the query. Format available can be listed with --list-osee.
                         By default, the client is automatic and uses the default or the first format.
 -h/--help               Prints the usage.
 --list-osee             List the OpenSearch Engine Extensions including the list of available metadata paths.
 -m/--model <format>     Specify the data model of the results for the query. Data model give access to specific metadata extractors or 
                         transformers. By default the "GeoTime" model is used. Used without urls, it lists the metadata options.
 -o/--output <file>      Write output to <file> instead of stdout
                         Files are split by pagination and named with the indexes.
                         Default filename (if <directory> is set) is feed_startindex_endindex.xml.
 -p/--parameter <param>  Specify a parameter for the query
 --pagination            Specify the pagination number for search loops. Default: 20
 -to/--time-out <file>   Specify query timeout (millisecond)
 -v/--verbose            Make the operation more talkative

Metadatapath:
 possible values can be found using --list-osee

Parameters:
 -a <value>              Add a metadata with the value <value>
 -d                      Remove all metadata
 -d <value>              Remove metadata having <value> as value
 -r <value> <template>   Replace metadata having <value> as value with the new <template> value
 -r <template>           Replace metadata with the new <template> value
 note: <template> can use @@metadata@@ to get value from other elements, e.g @@identifier@@ to use the value of the identifier in the new metadata value
```

#### data-publisher delete

```
Function not yet implemented
```

## Usage examples

### opensearch-client

```shell
opensearch-client -p format=json -p uid=S1A_IW_SLC__1SDV_20151117T163127_20151117T163155_008647_00C499_5DC1 "https://data2.terradue.com/eop/scihub/dataset/search" identifier
```

### data-publisher

```shell
data-publisher edit -d mydirectory -o myfile.xml -m EOP -p format=atomeop -p uid=S1A_IW_SLC__1SDV_20151117T163127_20151117T163155_008647_00C499_5DC1 https://data2.terradue.com/eop/scihub/dataset/search title -r "@@platform@@ @@productType@@ @@operationalMode@@ @@processingLevel@@ @@polarisationChannels@@ @@wrsLongitudeGrid@@ @@startdate@@-@@enddate@@" , enclosure -d
```

## TODO

* Following commands are still to be implemented
  * data-publisher add
  * data-publisher delete

## Copyright and License

Copyright (c) 2016 Terradue

Licensed under the [GPL v3 License](https://github.com/Terradue/DotNetOpenSearchClient/blob/master/LICENSE)

## Questions, bugs, and suggestions

Please file any bugs or questions as [issues](https://github.com/Terradue/DotNetOpenSearchClient/issues/new) 

## Want to contribute?

Fork the repository [here](https://github.com/Terradue/DotNetOpenSearchClient/fork) and send us pull requests.
