using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


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

        public async Task<List<Product>> GetProductsAsync(string keyword, int maxPages, string taskId)
        {
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
                var url = $"https://www.momoshop.com.tw/search/searchShop.jsp?keyword={Uri.EscapeDataString(keyword)}&curPage={currentPage}";
                _logger.LogInformation("Loading page: {PageUrl}", url);

                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                try
                {
                    wait.Until(drv => drv.FindElements(By.CssSelector("li.listAreaLi")).Count > 0);
                }
                catch (WebDriverTimeoutException)
                {
                    _logger.LogWarning("No products found on page {Page}, stopping.", currentPage);
                    break;
                }

                var productElements = driver.FindElements(By.CssSelector("li.listAreaLi"));

                foreach (var item in productElements)
                {
                    try
                    {
                        var titleElement = item.FindElement(By.CssSelector("h3.prdName"));
                        var priceElement = item.FindElement(By.CssSelector("span.price b"));
                        var linkElement = item.FindElement(By.CssSelector("a.goods-img-url"));

                        var productName = titleElement.Text.Trim();
                        var priceText = priceElement.Text.Replace(",", "").Trim();
                        decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
                        var productLink = "https://www.momoshop.com.tw" + linkElement.GetAttribute("href");

                        var existingProduct = await _unitOfWork.Products
                            .GetByNameAndDescriptionAsync(productName, productLink);

                        if (existingProduct != null)
                        {
                            _logger.LogInformation("Found existing product: {ProductName}", productName);

                            var history = new ProductHistory
                            {
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
                                Stock = 100,
                                TaskId = taskId,
                            };

                            await _unitOfWork.Products.AddAsync(newProduct);
                            await _unitOfWork.SaveAsync(); // 必須先存起來取得 ProductId

                            var history = new ProductHistory
                            {
                                ProductId = newProduct.ProductId,
                                Price = price,
                                Stock = 100,
                                CapturedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.ProductHistories.AddAsync(history);
                            products.Add(newProduct);
                        }

                        // 可選擇：如果你要立即保存歷史也可以加這一行
                        await _unitOfWork.SaveAsync();
                    }
                    catch (NoSuchElementException ex)
                    {
                        _logger.LogWarning("Missing data. Error: {Message}", ex.Message);
                        continue;
                    }
                }
            }
            return products;
        }
    }
}