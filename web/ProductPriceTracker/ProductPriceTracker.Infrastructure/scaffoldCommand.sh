#!/bin/sh
##--project 指定要 Scaffold 的專案（Model 要生成到哪個專案）
##--startup-project 指定執行專案（通常是 Api 專案）
## 需要手動將 Scaffold 的 Model 更改命名空間由  ProductPriceTracker.Infrastructure 改為 ProductPriceTracker.Core.Entities
dotnet ef dbcontext scaffold "Server=localhost,1434;Database=ScrapeDb;User Id=sa;Password=!QAZ@WSX123;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir ../ProductPriceTracker.Core/Entities --context ScrapeDbContext --context-dir Data --startup-project ../ProductPriceTracker.Api --no-onconfiguring --force
##  使用此指令會將 DbContext 生成到 ProductPriceTracker.Core/Entities/Data/ScrapeDbContext.cs
# dotnet ef dbcontext scaffold "Server=localhost;Database=ScrapeDb;User Id=sa;Password=!QAZ@WSX123;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir ../ProductPriceTracker.Core/Entities --context ScrapeDbContext --context-dir Data --project ../ProductPriceTracker.Core --startup-project ../ProductPriceTracker.Api --no-onconfiguring --force