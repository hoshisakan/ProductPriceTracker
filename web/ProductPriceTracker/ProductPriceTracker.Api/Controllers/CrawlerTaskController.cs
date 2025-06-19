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
public class CrawlerTaskController : ControllerBase
{
    private readonly ILogger<CrawlerTaskController> _logger;
    private readonly ICrawlerTaskService _crawlerTaskService;


    public CrawlerTaskController(ILogger<CrawlerTaskController> logger, ICrawlerTaskService crawlerTaskService) // 從 DI 拿 RabbitMQ 連線
    {
        _logger = logger;
        _crawlerTaskService = crawlerTaskService;
    }

    // POST /api/crawlertask/get-crawler-task
    [Authorize]
    [HttpPost("get-crawler-task")]
    public async Task<IActionResult> GetCrawlerTask()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var crawlerTasks = await _crawlerTaskService.GetAllCrawlerTasksAsync(userId);

            if (crawlerTasks == null || !crawlerTasks.Any())
            {
                return NotFound("No crawler tasks found.");
            }

            return Ok(crawlerTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing GetCrawlerTask request.");
            return StatusCode(500, "Internal server error while processing the request.");
        }
    }
}
