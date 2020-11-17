#!/bin/bash

dotnet restore
dotnet build -r osx.10.14-x64 --configuration Release
dotnet publish -f netcoreapp3.1 --configuration Release
