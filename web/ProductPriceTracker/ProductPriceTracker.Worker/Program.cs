using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;
using ProductPriceTracker.Infrastructure.Data;
using ProductPriceTracker.Infrastructure.Data.Repositories;
using ProductPriceTracker.Infrastructure.Services;
using Serilog;

// 建立設定物件，從 appsettings.json 讀取組態
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// 建立 Serilog 記錄器，從組態讀取設定
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("🚀 啟動 ProductPriceTracker.Worker");

    // 建立主機，使用 Serilog 作為日誌記錄器
    var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((hostCtx, services) =>
    {
        // hostCtx.Configuration 已包含 appsettings.json
        var configuration = hostCtx.Configuration;

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 註冊 EF Core 的 DbContext，使用 SQL Server 並傳入連線字串
        services.AddDbContext<ScrapeDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 註冊 UnitOfWork 和 Repository
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // 註冊兩個爬蟲服務（Scoped/Transient 均可，這邊用 Transient 表示每次取得新的實例）
        services.AddTransient<IMomoCrawlerService, MomoCrawlerService>();
        services.AddTransient<IPchomeCrawlerService, PchomeCrawlerService>();

        // 註冊 BackgroundService（Worker），在主機啟動後會自動執行，且會自動注入 IConfiguration
        services.AddHostedService<Worker>();
    })
    .Build(); // 建立主機

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Worker 主機異常終止");
}
finally
{
    Log.CloseAndFlush();
}