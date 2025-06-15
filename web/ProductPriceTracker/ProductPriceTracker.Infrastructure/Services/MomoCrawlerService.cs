using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;
using Microsoft.Playwright;


namespace ProductPriceTracker.Infrastructure.Services
{
    public class MomoCrawlerService : IMomoCrawlerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MomoCrawlerService> _logger;

        public MomoCrawlerService(IUnitOfWork unitOfWork, ILogger<MomoCrawlerService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Product>> GetProductsAsync(string keyword, int maxPages = 3)
        {
            var products = new List<Product>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            for (int currentPage = 1; currentPage <= maxPages; currentPage++)
            {
                var url = $"https://www.momoshop.com.tw/search/searchShop.jsp?keyword={Uri.EscapeDataString(keyword)}&curPage={currentPage}";
                _logger.LogInformation("Loading page: {PageUrl}", url);

                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });
                await page.WaitForTimeoutAsync(1000); // 加一點等待比較穩

                var productElements = await page.QuerySelectorAllAsync("li.listAreaLi");

                if (productElements.Count == 0)
                {
                    _logger.LogWarning("No products found on page {Page}, stopping.", currentPage);
                    break;
                }

                foreach (var item in productElements)
                {
                    var title = await item.QuerySelectorAsync("h3.prdName");
                    var price = await item.QuerySelectorAsync("span.price b");
                    var link = await item.QuerySelectorAsync("a.goods-img-url");
                    var image = await item.QuerySelectorAsync("img.prdImg");

                    var product = new Product
                    {
                        ProductName = (await title?.InnerTextAsync())?.Trim() ?? "",
                        Price = decimal.TryParse((await price?.InnerTextAsync())?.Replace(",", ""), out var p) ? p : 0,
                        Description = "https://www.momoshop.com.tw" + (await link?.GetAttributeAsync("href") ?? ""),
                        CreatedAt = DateTime.UtcNow,
                        Stock = 100
                    };

                    _logger.LogInformation("Adding product: {ProductName} (Page {Page})", product.ProductName, currentPage);

                    // 檢查產品是否已存在
                    if (await _unitOfWork.Products.IsProductExistsAsync(product.ProductName, product.Description))
                    {
                        _logger.LogInformation("Product {ProductName} already exists, skipping.", product.ProductName);
                        continue;
                    }
                    
                    products.Add(product);
                }
            }

            return products;
        }
        // public async Task<List<Product>> GetProductsAsync(string keyword)
        // {
        //     var products = new List<Product>();
        //     var url = $"https://www.momoshop.com.tw/search/searchShop.jsp?keyword={Uri.EscapeDataString(keyword)}";

        //     using var playwright = await Playwright.CreateAsync();
        //     await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        //     var page = await browser.NewPageAsync();
        //     await page.GotoAsync(url);

        //     // 等待商品載入
        //     await page.WaitForSelectorAsync("li.listAreaLi");

        //     var productElements = await page.QuerySelectorAllAsync("li.listAreaLi");

        //     foreach (var item in productElements)
        //     {
        //         var title = await item.QuerySelectorAsync("h3.prdName");
        //         var price = await item.QuerySelectorAsync("span.price b");
        //         var link = await item.QuerySelectorAsync("a.goods-img-url");
        //         var image = await item.QuerySelectorAsync("img.prdImg");

        //         _logger.LogInformation("Processing product: {Title}", await title?.InnerTextAsync());
        //         _logger.LogInformation("Price: {Price}", await price?.InnerTextAsync());
        //         _logger.LogInformation("Link: {Link}", await link?.GetAttributeAsync("href"));
        //         _logger.LogInformation("Image: {Image}", await image?.GetAttributeAsync("src"));

        //         var product = new Product
        //         {
        //             ProductName = (await title?.InnerTextAsync())?.Trim() ?? "",
        //             Price = decimal.TryParse((await price?.InnerTextAsync())?.Replace(",", ""), out var p) ? p : 0,
        //             Description = "https://www.momoshop.com.tw" + (await link?.GetAttributeAsync("href") ?? ""),
        //             CreatedAt = DateTime.UtcNow,
        //             Stock = 100
        //         };

        //         _logger.LogInformation("Adding product: {ProductName} with price: {Price}", product.ProductName, product.Price);

        //         products.Add(product);
        //     }

        //     return products;
        // }
    }
}