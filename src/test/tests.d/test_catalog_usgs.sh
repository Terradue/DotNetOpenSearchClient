#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_usgs  ====="

reference="http://earthexplorer.usgs.gov"



 test_id()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP  -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LT82270302013068LGN02  id )
  assertEquals "LT82270302013068LGN02" "${output}"
 }


test_indentifier()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LT82270302013068LGN02  identifier )
  assertEquals "LT82270302013068LGN02" "${output}"
}

test_enclosure()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LT82270302013068LGN02  enclosure )
  assertEquals "https://earthexplorer.usgs.gov/download/external/options/LANDSAT_8_C1/LT82270302013068LGN02/INVSVC/" "${output}"
 }
 



. ${SHUNIT2_HOME}/shunit2