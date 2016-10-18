#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_va4_rdf  ====="

reference="http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/ASA_IM__0CNPDE20120407_182038_000000173113_00257_52857_6356.N1"
format="rdf"

test_startdate()
{
  local output=$( opensearch-client "${reference}/${format}" startdate )
  assertEquals "2012-04-07T18:20:38.8100000Z" "${output}"
}

test_enddate()
{
    local output=$( opensearch-client "${reference}/${format}" enddate )
    assertEquals "2012-04-07T18:20:55.0400000Z" "${output}"
}

test_wkt()
{
    local output=$( opensearch-client "${reference}/${format}" wkt )
    assertEquals "MULTIPOLYGON(((-120.992484 49.133766,-121.987525 49.24517,-122.270375 48.105365,-121.312357 47.998146,-120.992484 49.133766)))" "${output}"
}

test_enclosure()
{
    local output=$( opensearch-client "${reference}/${format}" enclosure )
    assertTrue "test_enclosure failed" "[ \"http\" = \"${output:0:4}\" -o \"https\" = \"${output:0:5}\" -o \"s3\" = \"${output:0:2}\" -o \"ftp\" = \"${output:0:3}\" ]"
}

test_identifier()
{
    local output=$( opensearch-client "${reference}/${format}" identifier )
    assertEquals "ASA_IM__0CNPDE20120407_182038_000000173113_00257_52857_6356.N1" "${output}"
}


. ${SHUNIT2_HOME}/shunit2