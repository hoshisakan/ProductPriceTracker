using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Infrastructure.Data;
using ProductPriceTracker.Infrastructure.Repositories;
using ProductPriceTracker.Infrastructure.Services;
using ProductPriceTracker.Worker.Services;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<ScrapeDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ProductRepository>();
        services.AddScoped<CrawlerService>();
        services.AddHostedService<CrawlTaskWorkerService>();
    })
    .Build();

await host.RunAsync();
