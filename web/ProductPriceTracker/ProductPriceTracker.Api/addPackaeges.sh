#!/bin/bash
# Add packages to the ProductPriceTracker.Api project
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.2.3
dotnet add package Microsoft.AspNetCore.OpenApi --version 8.0.0
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Selenium.WebDriver
dotnet add package Selenium.Support
dotnet add package Selenium.WebDriver.ChromeDriver
dotnet add package RabbitMQ.Client --version 6.4.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Quartz.Extensions.Hosting