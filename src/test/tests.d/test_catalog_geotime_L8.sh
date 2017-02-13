#!/bin/bash

source ./test_common.sh

echo "===== test_catalog_geotime_L8  ====="

reference="https://catalog.terradue.com/landsat8/search?uid=LC81422132016290LGN00"
formats="json atom rdf"

test_startdate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" startdate )
    assertEquals "2016-10-16T06:03:14.1510000Z" "${output}"
  done
}

test_enddate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" enddate )
    assertEquals "2016-10-16T06:03:45.9210000Z" "${output}"
  done
}

test_wkt()
{
  # There's a bug in the catalogue (2016-10-18) for the json response. Skipping this test for the moment
  #for format in ${formats}
  for format in atom rdf
  do
    local output=$( opensearch-client "${reference}&format=${format}" wkt )
    assertEquals "POLYGON((-124.23042 42.82261,-123.694 41.10299,-125.89652 40.69215,-126.49337 42.40881,-124.23042 42.82261))" "${output}"
  done
}

test_enclosure()
{
  for format in ${formats}
  do
    local output=$( opensearch-client -p do=terradue "${reference}&format=${format}" enclosure )
    # Here the do parameter shall be set automatically, so we expect always store.terradue.com
    assertEquals "https://store.terradue.com/download/landsat8/files/v1/LC81422132016290LGN00" "${output}"
  done
}

test_enclosure_do()
{
  for format in ${formats}
  do
    local output=$( opensearch-client -p do=sandbox.terradue.int "${reference}&format=${format}" enclosure )
    assertEquals "https://store.terradue.com/download/landsat8/files/v1/LC81422132016290LGN00" "${output}"
  done
}

test_identifier()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" identifier )
    assertEquals "LC81422132016290LGN00" "${output}"
  done
}



. ${SHUNIT2_HOME}/shunit2
