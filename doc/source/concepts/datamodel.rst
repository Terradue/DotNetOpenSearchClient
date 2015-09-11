Data model
===========

.. _datamodel:

A data model defines and analyzes :term:`dataset` specifications to support a metadata structure corresponding to the business domain of the dataset. this structure is used to

1. represent the metadata the dataset in different :ref:`media types <mediatype>`.
2. identify the values in the metadata according to the query parameters of a :ref:`opensearch` query and filter the dataset accordingly.

In their base version the opensearch tools includes 3 data models:

Syndication Model : Atom
------------------------

This is the base model for all the others. Based on :term:`Atom`, it is used to describe feeds of information. This is typically used by news web sites, to publish a list a of new articles that are available for reading. Its structure is particulary well designed for :term:`catalogue` service. 

The key issue when cataloging data is to make sure that we don't lose any information in the process. Apart from the document's content itself, we're also interested in preserving the fundamental metadata about the document too, namely:

- What it is called
- Who created it
- When it was created
- Where it is

Atom is specifically designed to never lose any data. To see this, let's take a look at this example of an Atom feed:

.. code-block:: xml

	<?xml version="1.0" encoding="utf-8"?>
	<feed xmlns="http://www.w3.org/2005/Atom">
		<title type="text">Query Result</title>
		<id>https://data.terradue.com/eop/scihub/dataset/search?count=1&format=atom</id>
		<updated>2015-09-10T15:35:50.810201Z</updated>
		<link rel="self" type="application/atom+xml" title="Reference link" href="https://data.terradue.com/eop/scihub/dataset/search?count=1&format=atom" />
		<link rel="search" type="application/opensearchdescription+xml" title="OpenSearch Description link" href="https://data.terradue.com/eop/scihub/dataset/description" />
		<author>
     		<name>Terradue</name>
   		</author>
		<entry>
			<id>https://data.terradue.com/eop/scihub/dataset/search?format=atom&uid=S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406</id>
			<title type="text">S1A GRD IW_DP L1 VV, VH 15 141205T051621-141205T051633</title>
			<summary type="html">
				<table>	<tbody>	<table>	<tr>	<td><strong>Swath</strong></td><td>IW</td>	</tr><tr>	<td><strong>Orbit</strong></td><td>3587 ASCENDING</td>	</tr><tr>	<td><strong>Track</strong></td><td>15</td>	</tr><tr>	<td><strong>Start</strong></td><td>2014-12-05T16:16:21.5300370Z</td>	</tr><tr>	<td><strong>End</strong></td><td>2014-12-05T16:16:33.4265590Z</td>	</tr>	</table>	</tbody></table>
			</summary>
			<published>2014-12-05T20:58:38.024Z</published>
			<updated>2014-12-05T20:58:38.024Z</updated>
			<link rel="enclosure" type="application/octet-stream" length="809403257" href="https://scihub.esa.int/dhus/odata/v1/Products('7206812c-d9cf-485c-89b3-f03a214be924')/$value" />
			<link rel="self" type="application/atom+xml" title="Reference link" href="https://data.terradue.com/eop/scihub/dataset/search?format=atom&uid=S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406" />
			<identifier xmlns="http://purl.org/dc/elements/1.1/">S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406</identifier>
		</entry>
		<queryTime xmlns="http://purl.org/dc/elements/1.1/">1</queryTime>
		<totalResults xmlns="http://a9.com/-/spec/opensearch/1.1/">600</totalResults>
		<startIndex xmlns="http://a9.com/-/spec/opensearch/1.1/">1</startIndex>
		<itemsPerPage xmlns="http://a9.com/-/spec/opensearch/1.1/">1</itemsPerPage>
		<os:Query xmlns:os="http://a9.com/-/spec/opensearch/1.1/" xmlns:param="http://a9.com/-/spec/opensearch/extensions/parameters/1.0/" xmlns:geo="http://a9.com/-/opensearch/extensions/geo/1.0/" xmlns:time="http://a9.com/-/opensearch/extensions/time/1.0/" xmlns:eop="http://www.opengis.net/eop/2.0" os:count="1" />
	</feed>
  
An Atom feed is XML and so must follow all the usual well-formedness rules that that implies. It consists of some metadata about the feed, followed by one or more entries. This metadata chunk contains all of the data mandatory in an :ref:`opensearch` result. The id element provides the "where," giving the feed's URI. The title provides a "what," giving the title of the feed. updated gives the "when," with an obligation to say when the feed was last changed. author says "who" created the feed, and link provides the "how" giving a link to the description of the search and the self link of the resource the feed represents.

The entry section, just like the feed's main metadata section, has the obligatory id, title, updated, and :ref:`opensearch` links. It wouldn't be much use without the content, and it's recommended to have the summary, a "a short summary, abstract, or excerpt of the entry. In addition to those core features, it can also contain extended elements such as the "identifier" element with another namespace that has a meaning in the domain it was defined.


Geo & Time model
----------------

The Geo & Time model is directly derived from the specification for the :ref:`opensearch` Geo and Time extensions. it is intended to provide a very simple way to structure spatial and temporal metadata to a series of geospatial dataset that contains geographic and temporal properties and to allow simple syndication of series.

In short, a dataset is specified by:
- a temporal reference
- a geographic reference

In atom representation, here is the previous Atom entry example supplemented with the Geo & Time model information

.. code-block:: xml

    <entry>
		<id>
			https://data.terradue.com/eop/scihub/dataset/search?format=atom&uid=S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406
		</id>
		<title type="text">S1A GRD IW_DP L1 VV, VH 15 141205T051621-141205T051633</title>
		<summary type="html">
			<table>	<tbody>	<table>	<tr>	<td><strong>Swath</strong></td><td>IW</td>	</tr><tr>	<td><strong>Orbit</strong></td><td>3587 ASCENDING</td>	</tr><tr>	<td><strong>Track</strong></td><td>15</td>	</tr><tr>	<td><strong>Start</strong></td><td>2014-12-05T16:16:21.5300370Z</td>	</tr><tr>	<td><strong>End</strong></td><td>2014-12-05T16:16:33.4265590Z</td>	</tr>	</table>	</tbody></table>
		</summary>
		<published>2014-12-05T20:58:38.024Z</published>
		<updated>2014-12-05T20:58:38.024Z</updated>
		<link rel="enclosure" type="application/octet-stream" length="809403257" href="https://scihub.esa.int/dhus/odata/v1/Products('7206812c-d9cf-485c-89b3-f03a214be924')/$value" />
		<link rel="self" type="application/atom+xml" title="Reference link" href="https://data.terradue.com/eop/scihub/dataset/search?format=atom&uid=S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406" />
		<link rel="alternate" type="application/xml" title="EOP profile" href="https://data2.terradue.com/eop/scihub/profile/xml?uid=S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406" />
		<link rel="search" type="application/opensearchdescription+xml" title="OpenSearch Description link" href="https://data.terradue.com/eop/scihub/dataset/description" />
		<where xmlns="http://www.georss.org/georss">
			<MultiSurface xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.opengis.net/gml/3.2">
				<surfaceMembers>
					<Polygon>
						<exterior>
							<LinearRing>
								<posList srsDimension="2" count="5">51.944363 5.898916 52.340702 9.548466 51.629356 9.728382 51.234165 6.135372 51.944363 5.898916</posList>
							</LinearRing>
						</exterior>
					</Polygon>
				</surfaceMembers>
			</MultiSurface>
		</where>
		<date xmlns="http://purl.org/dc/elements/1.1/">2014-12-05T16:16:21.5300370Z/2014-12-05T16:16:33.4265590Z</date>
		<spatial xmlns="http://purl.org/dc/terms/">
			MULTIPOLYGON(((5.898916 51.944363,9.548466 52.340702,9.728382 51.629356,6.135372 51.234165,5.898916 51.944363)))
		</spatial>
		<identifier xmlns="http://purl.org/dc/elements/1.1/">S1A_IW_GRDH_1SDV_20141205T171621_20141205T171633_003587_0043C4_3406</identifier>
	</entry>

Several elements have been added representing a temporal and geographic reference in different format representation. Some elements are mandatory to fulfill the model such as "date" and the georrss "where".

A :term:`catalogue` implementing the Geo & Time data model shall be able to provide with :ref:`opensearch` GeoTemporal service.
The :ref:`opensearch-client` is able to analyse and extract geotemporal specification of the entries of an :ref:`opensearch` results (with the limitation of the capability to read the :ref:`mediatype` in which the result is represented).

Full specification : `[10-032r8] OGC® OpenSearch Geo and Time Extensions <https://portal.opengeospatial.org/files/?artifact_id=55239>`_


Earth Observation Profile Model
-------------------------------

Earth Observation :term:`dataset` are generally managed within logical collections that are usually structured to contain data items derived from sensors onboard a satellite or series of satellites. The key characteristics differentiating products within the collections are date of acquisition, location as well as characteristics depending on the type of sensor, For example, key characteristics for optical imagery are the possible presence of cloud, haze, smokes or other atmospheric or on ground phenomena obscuring the image. 
The common metadata used to distinguish EO products types are defined and analysed for generic and thematic EO products (i.e optical, radar, atmospheric, altimetry, limb-looking and synthesis and systematic products).


 



