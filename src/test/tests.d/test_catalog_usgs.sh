#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_usgs  ====="

reference="http://earthexplorer.usgs.gov"



 test_id()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP  -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LC81530102016303LGN00  id )
  assertEquals "LC81920462017162LGN00" "${output}"
 }


test_indentifier()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LC81530102016303LGN00  identifier )
  assertEquals "LC81920462017162LGN00" "${output}"
}

test_enclosure()
{
  local output=$( opensearch-client "${reference}" -a "${USGS_CREDENTIALS}" -m EOP -p count=1 -p pi=LANDSAT_8_C1 -p profile=eop -p uid=LC81530102016303LGN00  enclosure )
  assertEquals "https://earthexplorer.usgs.gov/download/external/options/LANDSAT_8_C1/LC81530102016303LGN00/INVSVC/" "${output}"
 }
 



. ${SHUNIT2_HOME}/shunit2