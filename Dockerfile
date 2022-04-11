FROM debian:buster-slim

# MAINTAINER Emmanuel Mathot <emmanuel.mathot@terradue.com>

RUN apt-get update \
  && apt-get upgrade -y \
  && apt-get install -y hdf5-tools libssl1.1 libgssapi-krb5-2 libicu63 ca-certificates \
  && rm -rf /var/lib/apt/lists/* /tmp/*

ARG STARS_DEB
COPY $STARS_DEB /tmp/$STARS_DEB
RUN apt install -f /tmp/$STARS_DEB \
    && rm -rf /tmp/$STARS_DEB
RUN chmod +x /usr/bin/opensearch-client
