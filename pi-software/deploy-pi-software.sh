#!/bin/bash
set -e
pushd `dirname $0`
echo Deploying Pi software. Running in `pwd`

dotnet build Crg.PsuManager.sh

popd
