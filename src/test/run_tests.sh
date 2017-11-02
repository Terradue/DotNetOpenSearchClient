#!/bin/bash

exit_code=0

cp -r ../../packages/Terradue.Metadata.EarthObservation.*/content/Resources/ne_110m_land /usr/local/lib/ne_110m_land

for test in $( ls tests.d/*.sh )
do
  /bin/bash ${test}
  res=$?
  if [ $res -eq 1 ]; then exit_code=1; fi
done

exit ${exit_code}
