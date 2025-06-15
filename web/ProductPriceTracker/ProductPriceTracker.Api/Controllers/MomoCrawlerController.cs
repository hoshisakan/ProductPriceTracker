using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;
using ProductPriceTracker.Infrastructure.Services;

namespace ProductPriceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MomoCrawlerController : ControllerBase
    {
        private readonly IMomoCrawlerService _crawlerService;
        private readonly IProductService _productService;
        private readonly ILogger<MomoCrawlerController> _logger;

        public MomoCrawlerController(IMomoCrawlerService crawlerService, IProductService productService, ILogger<MomoCrawlerController> logger)
        {
            _crawlerService = crawlerService;
            _productService = productService;
            _logger = logger;
        }

        [HttpPost("get-products")]
        public async Task GetProductsAsync(string keyword, int maxPages)
        {
            try
            {
                var products = await _crawlerService.GetProductsAsync(keyword, maxPages);
                if (products == null || !products.Any())
                {
                    _logger.LogWarning("No products found for the keyword: {Keyword}", keyword);
                    return;
                }
                foreach (var product in products)
                {
                    _logger.LogInformation("Found product: {ProductName} with price: {Price}", product.ProductName, product.Price);
                    // Save each product to the database
                    await _productService.AddProductAsync(product);
                    _logger.LogInformation("Product {ProductName} saved successfully.", product.ProductName);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                _logger.LogError(ex, "An error occurred while fetching products for keyword: {Keyword}", keyword);
            }
        }
    }
}