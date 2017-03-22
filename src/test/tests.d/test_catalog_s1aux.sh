#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_s1aux  ====="

reference=" https://qc.sentinel1.eo.esa.int/"

test_harvesting()
{
  local output=$( opensearch-client -p count=1 -p startIndex=1 -m EOP -p start=2017-03-20 -p auxtype=aux_resorb -p orbits=true "${reference}" identifier )
  assertEquals "S1B_OPER_AUX_RESORB_OPOD_20170321T033159_V20170320T232016_20170321T023746" "${output}"
}


. ${SHUNIT2_HOME}/shunit2