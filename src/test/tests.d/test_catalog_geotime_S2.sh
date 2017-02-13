#!/bin/bash

source ./test_common.sh

echo "===== test_catalog_geotime_S2  ====="

reference="https://catalog.terradue.com/sentinel2/search?uid=S2A_OPER_PRD_MSIL1C_PDMC_20161017T015607_R011_V20161015T154222_20161015T154519"
formats="json atom rdf"

test_startdate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" startdate )
    assertEquals "2016-10-15T15:42:22.0260000Z" "${output}"
  done
}

test_enddate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" enddate )
    assertEquals "2016-10-15T15:45:19.8460000Z" "${output}"
  done
}

test_wkt()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" wkt )
    assertEquals "POLYGON((-64.7684720805233 59.5263987530396,-62.8274188167669 59.5382353521927,-62.8322923207965 58.5521716221359,-64.7185624545036 58.5407855059116,-64.7684720805233 59.5263987530396))" "${output}"
  done
}

test_enclosure()
{
  for format in ${formats}
  do
    local output=$( opensearch-client -p do=terradue "${reference}&format=${format}" enclosure )
    # Here the do parameter shall be set automatically, so we expect always store.terradue.com
    assertEquals "https://store.terradue.com/download/sentinel2/files/v1/S2A_OPER_PRD_MSIL1C_PDMC_20161017T015607_R011_V20161015T154222_20161015T154519" "${output}"
  done
}

test_enclosure_do()
{
  for format in ${formats}
  do
    local output=$( opensearch-client -p do=sandbox.terradue.int "${reference}&format=${format}" enclosure )
    assertEquals "https://store.terradue.com/download/sentinel2/files/v1/S2A_OPER_PRD_MSIL1C_PDMC_20161017T015607_R011_V20161015T154222_20161015T154519" "${output}"
  done
}

test_identifier()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" identifier )
    assertEquals "S2A_OPER_PRD_MSIL1C_PDMC_20161017T015607_R011_V20161015T154222_20161015T154519" "${output}"
  done
}



. ${SHUNIT2_HOME}/shunit2
