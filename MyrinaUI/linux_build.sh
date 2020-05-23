#!/bin/bash

dotnet restore
dotnet build -r linux-x64 --configuration Release
dotnet publish -f netcoreapp3.1 --configuration Release
