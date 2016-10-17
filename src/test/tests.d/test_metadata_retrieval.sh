#!/bin/bash

source ./test_common.sh

reference="https://catalog.terradue.com/sentinel2/search?uid=S2A_OPER_PRD_MSIL1C_PDMC_20161017T015607_R011_V20161015T154222_20161015T154519"

test_wkt()
{
  local output=$( opensearch-client "${reference}" wkt )
  assertEquals "POLYGON((-64.7684720805233 59.5263987530396,-62.8274188167669 59.5382353521927,-62.8322923207965 58.5521716221359,-64.7185624545036 58.5407855059116,-64.7684720805233 59.5263987530396))" "${output}"
}

. ${SHUNIT2_HOME}/shunit2
