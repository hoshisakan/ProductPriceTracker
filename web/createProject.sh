#!/bin/sh

mkdir ProductPriceTracker
cd ProductPriceTracker
dotnet new sln --name ProductPriceTracker

dotnet new webapi -n ProductPriceTracker.Api
dotnet new classlib -n ProductPriceTracker.Core
dotnet new classlib -n ProductPriceTracker.Infrastructure