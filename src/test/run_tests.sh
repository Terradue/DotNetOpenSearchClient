#!/bin/bash

exit_code=0

for test in $( ls tests.d/* )
do
  /bin/bash ${test}
  res=$?
  if [ $res -eq 1 ]; then exit_code=1; fi
done

exit ${exit_code}
