#!/bin/bash

# function to execute before each test (e.g., to setup the environment)
function setUp() {

  export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:${OPENSEARCH_CLIENT_PATH}


} 

function opensearch-client() {

  exec mono ${OPENSEARCH_CLIENT_PATH}/OpenSearchClient.exe "$@"
}

# function to execute after each test (e.g., to clean up the environment)
#function tearDown() {}
