#!/bin/sh

dotnet add package RabbitMQ.Client --version 6.4.0
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.AspNetCore