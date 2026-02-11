#!/bin/sh

# script to generate OpenAPI sources from JSON schema
# this requires that the SharedGenerator project be built first and the latest JSON placed in the rpc-schema directory

if [ "$#" -lt 1 ]; then
        echo "Usage: codegen.sh Debug|Release"
        exit 1
fi

SCRIPTPATH="$( cd "$(dirname "$0")" ; pwd -P )"
CONFIGURATION=$1

# first clean any existing sources
echo "Removing ${SCRIPTPATH}/DeephavenOpenAPI/Generated/*"
rm -rf ${SCRIPTPATH}/DeephavenOpenAPI/Generated/*

# generate sources from rpc-schema JSON files
dotnet ${SCRIPTPATH}/SharedGenerator/bin/$CONFIGURATION/net6.0/DeephavenOpenAPI.SharedGenerator.dll  ${SCRIPTPATH}/rpc-schema WebApi,Worker ${SCRIPTPATH}/DeephavenOpenAPI/Generated
