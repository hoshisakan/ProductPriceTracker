using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;

namespace ProductPriceTracker.Infrastructure.Services
{
    public class PchomeCrawlerService : IPchomeCrawlerService
    {
        private readonly ILogger<PchomeCrawlerService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PchomeCrawlerService(ILogger<PchomeCrawlerService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Product>> GetProductsAsync(string keyword, int maxPages, string taskId, int userId)
        {
            _logger.LogInformation("User {UserId} is searching for products with keyword: {Keyword}, max pages: {MaxPages}, taskId: {TaskId}",
                userId, keyword, maxPages, taskId);
            var products = new List<Product>();

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            using var driver = new ChromeDriver(driverService, options);

            for (int currentPage = 1; currentPage <= maxPages; currentPage++)
            {
                var url = $"https://24h.pchome.com.tw/search/?q={Uri.EscapeDataString(keyword)}&p={currentPage}";
                _logger.LogInformation("Loading page: {PageUrl}", url);

                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                try
                {
                    wait.Until(drv => drv.FindElements(By.CssSelector("li.c-listInfoGrid__item")).Count > 0);
                }
                catch (WebDriverTimeoutException)
                {
                    _logger.LogWarning("No products found on page {Page}, stopping.", currentPage);
                    break;
                }

                var productElements = driver.FindElements(By.CssSelector("li.c-listInfoGrid__item"));

                foreach (var item in productElements)
                {
                    try
                    {
                        var linkElem = item.FindElement(By.CssSelector("a.c-prodInfoV2__link"));
                        var imgElem = item.FindElement(By.CssSelector("img"));
                        var priceElem = item.FindElement(By.CssSelector("div.c-prodInfoV2__priceValue--m"));

                        string productName = imgElem.GetAttribute("alt")?.Trim() ?? "無名稱";
                        string relativeLink = linkElem.GetAttribute("href")?.Trim() ?? "";
                        string productLink = relativeLink.StartsWith("http") ? relativeLink : "https://24h.pchome.com.tw" + relativeLink;

                        var priceText = priceElem.Text.Replace("$", "").Replace(",", "").Trim();
                        decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
                        int stock = 100; // PChome 預設

                        var existingProduct = await _unitOfWork.Products
                            .GetByNameAndDescriptionAsync(productName, productLink);

                        if (existingProduct != null)
                        {
                            _logger.LogInformation("Found existing product: {ProductName}", productName);

                            var history = new ProductHistory
                            {
                                UserId = userId,
                                ProductId = existingProduct.ProductId,
                                Price = price,
                                Stock = existingProduct.Stock,
                                CapturedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.ProductHistories.AddAsync(history);
                        }
                        else
                        {
                            var newProduct = new Product
                            {
                                ProductName = productName,
                                Price = price,
                                Description = productLink,
                                CreatedAt = DateTime.UtcNow,
                                Stock = stock,
                                TaskId = taskId,
                                UserId = userId
                            };

                            await _unitOfWork.Products.AddAsync(newProduct);
                            await _unitOfWork.SaveAsync(); // 拿到 ProductId

                            var history = new ProductHistory
                            {
                                UserId = userId,
                                ProductId = newProduct.ProductId,
                                Price = price,
                                Stock = stock,
                                CapturedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.ProductHistories.AddAsync(history);
                            products.Add(newProduct);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    catch (NoSuchElementException ex)
                    {
                        _logger.LogWarning("Missing product element data: {Message}", ex.Message);
                        continue;
                    }
                }
            }

            driver.Quit();
            return products;
        }

    }
}
