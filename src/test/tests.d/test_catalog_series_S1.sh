#!/bin/bash

source ./test_common.sh

echo "===== test_catalog_series_S1  ====="

reference="https://catalog.terradue.com/sentinel1/series/italy/search?uid=S1B_IW_RAW__0SDV_20161017T053650_20161017T053722_002542_0044B5_E4F8"
formats="json atom rdf"

test_startdate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" startdate )
    assertEquals "2016-10-17T05:36:50.0280230Z" "${output}"
  done
}

test_enddate()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" enddate )
    assertEquals "2016-10-17T05:37:22.4275890Z" "${output}"
  done
}

test_wkt()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" wkt )
    assertEquals "POLYGON((5.264 37.9473,4.87 35.992,7.5762 35.7226,8.0415 37.6769,5.264 37.9473))" "${output}"
  done
}

test_enclosure()
{
  for format in ${formats}
  do
    local output=$( opensearch-client -p do=terradue "${reference}&format=${format}" enclosure )
    assertEquals "https://store.terradue.com/download/sentinel1/files/v1/S1B_IW_RAW__0SDV_20161017T053650_20161017T053722_002542_0044B5_E4F8" "${output}"
  done
}

test_identifier()
{
  for format in ${formats}
  do
    local output=$( opensearch-client "${reference}&format=${format}" identifier )
    assertEquals "S1B_IW_RAW__0SDV_20161017T053650_20161017T053722_002542_0044B5_E4F8" "${output}"
  done
}

test_track()
{
    local output=$( opensearch-client "${reference}" -m EOP  track )
    assertEquals "66" "${output}"
}


. ${SHUNIT2_HOME}/shunit2
