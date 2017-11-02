#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_t2_rdf  ====="

reference="http://catalogue.terradue.com/catalogue/search/ASA_IM__0P/ASA_IM__0CNPDE20030611_204630_000000162017_00129_06696_0866.N1"
format="rdf"

test_startdate()
{
  local output=$( opensearch-client "${reference}/${format}" startdate )
  assertEquals "2003-06-11T20:46:30.6740000Z" "${output}"
}

test_enddate()
{
    local output=$( opensearch-client "${reference}/${format}" enddate )
    assertEquals "2003-06-11T20:46:46.9540000Z" "${output}"
}

test_wkt()
{
    local output=$( opensearch-client "${reference}/${format}" wkt )
    assertEquals "MULTIPOLYGON(((14.615052 36.982241,15.776153 37.177145,15.495613 38.242585,14.347516 38.049914,14.615052 36.982241)))" "${output}"
}

test_enclosure()
{
    local output=$( opensearch-client "${reference}/${format}" enclosure )
    assertTrue "test_enclosure failed" "[ \"http\" = \"${output:0:4}\" -o \"https\" = \"${output:0:5}\" -o \"s3\" = \"${output:0:2}\" -o \"ftp\" = \"${output:0:3}\" ]"
}

test_identifier()
{
    local output=$( opensearch-client "${reference}/${format}" identifier )
    assertEquals "ASA_IM__0CNPDE20030611_204630_000000162017_00129_06696_0866.N1" "${output}"
}


. ${SHUNIT2_HOME}/shunit2
