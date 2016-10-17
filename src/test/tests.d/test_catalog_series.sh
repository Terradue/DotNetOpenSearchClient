#!/bin/bash

source ./test_common.sh

echo "===== test_catalog_series (TO UPDATE) ====="

# This test shall be updated with a proper series reference pointing to catalog.terradue.com
reference="https://data2.terradue.com/eop/sentinel1/series/med/search&uid=S1A_IW_GRDH_1SDV_20150324T051221_20150324T051246_005169_00684F_DC61"
formats="json atom rdf"

test_startdate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" startdate )
    assertEquals "2015-03-24T05:12:21.2441820Z" "${output}"
  done
}

test_enddate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" enddate )
    assertEquals "2015-03-24T05:12:46.2438890Z" "${output}"
  done
}

test_wkt()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" wkt )
    assertEquals "POLYGON((13.997004 37.217716,11.114591 37.62225,11.426163 39.123463,14.368804 38.720371,13.997004 37.217716))" "${output}"
  done
}

test_enclosure()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" enclosure )
    assertTrue "test_enclosure failed" "[ \"http\" = \"${output:0:4}\" -o \"https\" = \"${output:0:5}\" -o \"s3\" = \"${output:0:2}\" -o \"ftp\" = \"${output:0:3}\" ]"
  done
}

test_identifier()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" identifier )
    assertEquals "S1A_IW_GRDH_1SDV_20150324T051221_20150324T051246_005169_00684F_DC61" "${output}"
  done
}


. ${SHUNIT2_HOME}/shunit2
