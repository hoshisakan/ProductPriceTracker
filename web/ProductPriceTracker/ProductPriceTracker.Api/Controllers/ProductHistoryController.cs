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
public class ProductHistoryController : ControllerBase
{
    private readonly ILogger<ProductHistoryController> _logger;
    private readonly IProductHistoryService _productHistoryService;


    public ProductHistoryController(ILogger<ProductHistoryController> logger, IProductHistoryService productHistoryService) // 從 DI 拿 RabbitMQ 連線
    {
        _logger = logger;
        _productHistoryService = productHistoryService;
    }

    // POST /api/producthistory/get-history
    [Authorize]
    [HttpPost("get-history")]
    public async Task<IActionResult> GetHistory([FromBody] ProductHistoryRequestDto request)
    {
        try
        {
            var productHistories = await _productHistoryService.GetProductHistoriesByTaskIdAsync(request.TaskId);

            if (productHistories == null || !productHistories.Any())
            {
                return NotFound("No product history found for the given task ID.");
            }

            return Ok(productHistories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing GetHistory request.");
            return StatusCode(500, "Internal server error while processing the request.");
        }
    }
}
