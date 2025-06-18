// API Controller 範例 (ASP.NET Core)
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductPriceTracker.Core.Dtos;
using ProductPriceTracker.Core.Interface.IServices;
using RabbitMQ.Client;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;


    public ProductController(ILogger<ProductController> logger, IProductService productService) // 從 DI 拿 RabbitMQ 連線
    {
        _logger = logger;
        _productService = productService;
    }

    // POST /api/product/get-product
    [Authorize]
    [HttpPost("get-product")]
    public async Task<IActionResult> GetProduct([FromBody] ProductRequestDto request)
    {
        try
        {
            var products = await _productService.GetProductsByTaskIdAsync(request.TaskId);

            if (products == null || !products.Any())
            {
                return NotFound("No product found for the given task ID.");
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing GetProduct request.");
            return StatusCode(500, "Internal server error while processing the request.");
        }
    }
}
