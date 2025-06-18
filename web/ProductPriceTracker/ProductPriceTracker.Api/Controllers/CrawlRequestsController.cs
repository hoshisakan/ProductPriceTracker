// API Controller 範例 (ASP.NET Core)
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductPriceTracker.Core.Dtos;
using RabbitMQ.Client;

[ApiController]
[Route("api/[controller]")]
public class CrawlRequestsController : ControllerBase
{
    private readonly IConnection _connection;
    private readonly string _queueName = "product_price_updates_queue"; // 定義 RabbitMQ 隊列名稱
    private readonly ILogger<CrawlRequestsController> _logger;


    public CrawlRequestsController(IConnection connection, ILogger<CrawlRequestsController> logger) // 從 DI 拿 RabbitMQ 連線
    {
        _connection = connection;
        _logger = logger;
    }

    // POST /api/crawlrequests
    [Authorize]
    [HttpPost] // 指定這個方法回應 HTTP POST 請求
    public IActionResult Create([FromBody] CrawlRequest request) // 從 HTTP 請求的 body 中接收一個 JSON 格式的 CrawlRequest 物件
    {
        // 建立一個新的 RabbitMQ Channel（通道）來與 Broker 溝通，使用完自動釋放資源
        using var channel = _connection.CreateModel();

        // 確保隊列存在，如果不存在就建立，參數說明如下：
        channel.QueueDeclare(
            queue: _queueName, // 隊列名稱，這裡固定為 product_price_updates_queue
            durable: true,                        // 設為 durable 表示 RabbitMQ 重啟後保留此隊列
            exclusive: false,                     // 設為 false 表示多個連線可共用此隊列
            autoDelete: false,                    // 設為 false 表示當沒有消費者時不自動刪除隊列
            arguments: null                       // 無額外的參數設定
        );

        // 從使用者的 Claims 中取得 UserId，這裡假設 UserId 存在於 NameIdentifier 欄位
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        CrawlStorageDto crawlStorageDto = new CrawlStorageDto
        {
            UserId = userId, // 設定使用者 ID
            Mode = request.Mode, // 設定爬蟲模式（例如 "pchome" 或 "momo"）
            Keyword = request.Keyword, // 設定關鍵字
            MaxPage = request.MaxPage // 設定最大頁數
        };

        // 將接收到的 CrawlRequest 物件序列化成 JSON 字串
        var json = JsonSerializer.Serialize(crawlStorageDto);

        // 將 JSON 字串轉成 UTF-8 編碼的位元組陣列，準備傳送
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true; // 設定訊息為持久化，確保 RabbitMQ 重啟後仍然存在

        // 發佈訊息到 RabbitMQ，參數說明如下：
        channel.BasicPublish(
            exchange: "",                          // 使用預設的 exchange（default exchange）
            routingKey: _queueName, // 指定 routing key 為隊列名稱，讓訊息送進指定的隊列
            basicProperties: properties,                 // 不設定額外屬性（像是持久性、標頭等）
            body: body                             // 訊息內容本體，為 byte[] 格式
        );

        // 回傳 200 OK 結果給前端，包含訊息與送出的 request 內容
        return Ok(new
        {
            Message = "Crawl request created successfully", // 自訂訊息
            Request = request                               // 回傳剛剛送出的請求資料
        });
    }
}
