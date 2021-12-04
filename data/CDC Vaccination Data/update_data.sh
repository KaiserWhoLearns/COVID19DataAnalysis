#!/usr/bin/env bash

URL="https://data.cdc.gov/api/views/8xkx-amqh/rows.csv?accessType=DOWNLOAD"
FILENAME="vaccinations.csv"

now=$(date)

echo "Downloading Update for ${FILENAME} from CDC: ${URL} on ${now}"
curl -o $FILENAME $URL
# shellcheck disable=SC2034
read -r -n 1 -p "Update Complete. Press any key to continue: " input
