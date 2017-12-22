#!/bin/bash

source ./test_common.sh

echo "=====  test_catalog_s3a  ====="

reference=" https://catalog.terradue.com/sentinel3/series/SR_2_LAN/search"

test_harvesting()
{
  local output=$(opensearch-client -m EOP -p uid=S3A_SR_2_LAN____20171112T214318_20171112T215007_20171112T224902_0408_024_229______SVL_O_NR_002 "${reference}" 'track,identifier' )
  assertEquals "229,S3A_SR_2_LAN____20171112T214318_20171112T215007_20171112T224902_0408_024_229______SVL_O_NR_002" "${output}"
}


. ${SHUNIT2_HOME}/shunit2