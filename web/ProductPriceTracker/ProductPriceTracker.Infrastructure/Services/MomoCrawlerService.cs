using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


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

            var options = new ChromeOptions();
            options.AddArgument("--headless");  // 無頭模式
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            // 建議設定timeout，避免卡住
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true; // 隱藏黑框視窗

            using var driver = new ChromeDriver(driverService, options);
            
            for (int currentPage = 1; currentPage <= maxPages; currentPage++)
            {
                var url = $"https://www.momoshop.com.tw/search/searchShop.jsp?keyword={Uri.EscapeDataString(keyword)}&curPage={currentPage}";
                _logger.LogInformation("Loading page: {PageUrl}", url);

                driver.Navigate().GoToUrl(url);

                // Selenium 沒有直接的 WaitUntilNetworkIdle，使用等待元素方式
                // 等待產品列表出現，最多等10秒
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));
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
                        // img有時候是lazy load，可能要取src或data-src，先用src
                        var imageElement = item.FindElement(By.CssSelector("img.prdImg"));

                        var productName = titleElement.Text.Trim();
                        
                        // 處理價格文字，如有逗號去除
                        var priceText = priceElement.Text.Replace(",", "").Trim();
                        decimal price = 0;
                        decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);

                        var productLink = "https://www.momoshop.com.tw" + linkElement.GetAttribute("href");
                        // 這裡沒用到 image，但可擴充
                        
                        var product = new Product
                        {
                            ProductName = productName,
                            Price = price,
                            Description = productLink,
                            CreatedAt = DateTime.UtcNow,
                            Stock = 100
                        };

                        _logger.LogInformation("Adding product: {ProductName} (Page {Page})", product.ProductName, currentPage);

                        if (await _unitOfWork.Products.IsProductExistsAsync(product.ProductName, product.Description))
                        {
                            _logger.LogInformation("Product {ProductName} already exists, skipping.", product.ProductName);
                            continue;
                        }

                        products.Add(product);
                    }
                    catch (NoSuchElementException ex)
                    {
                        _logger.LogWarning("Some product info missing, skipping item. Error: {Message}", ex.Message);
                        continue;
                    }
                }
            }

            return products;
        }
    }
}