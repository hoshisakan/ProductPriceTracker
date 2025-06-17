using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client; // RabbitMQ 連線與通道處理
using RabbitMQ.Client.Events; // RabbitMQ 消費者事件處理
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ProductPriceTracker.Infrastructure.Services; // 注入 Momo 與 PCHome 爬蟲服務
using ProductPriceTracker.Core.Dtos;
using ProductPriceTracker.Core.Interface.IServices;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories; // CrawlRequest 請求 DTO


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger, IServiceProvider services, IConfiguration configuration)
    {
        _logger = logger;
        _services = services;
        _configuration = configuration;

        var rabbitConfig = _configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory()
        {
            HostName = rabbitConfig["HostName"],
            Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
            UserName = rabbitConfig["UserName"],
            Password = rabbitConfig["Password"],
            VirtualHost = rabbitConfig["VirtualHost"]
        };

        const int maxRetries = 5;
        int retriesLeft = maxRetries;
        while (true)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: "product_price_updates_queue",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("成功連線到 RabbitMQ");
                break;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                retriesLeft--;
                _logger.LogWarning("連線 RabbitMQ 失敗，剩餘嘗試次數：{RetriesLeft}，錯誤訊息：{Message}", retriesLeft, ex.Message);
                if (retriesLeft == 0)
                {
                    _logger.LogError("無法連線至 RabbitMQ，結束啟動。");
                    throw;  // 可考慮改成其他錯誤處理機制
                }
                Thread.Sleep(3000); // 等待 3 秒再重試
            }
        }
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 建立一個 RabbitMQ 消費者，監聽該頻道上的訊息
        var consumer = new EventingBasicConsumer(_channel);
        // var consumer = new AsyncEventingBasicConsumer(_channel);

        // 訂閱收到訊息的事件處理函式
        consumer.Received += async (model, ea) =>
        {
            // 取得訊息內容（byte 陣列）
            var body = ea.Body.ToArray();
            // 將 byte 轉成字串（JSON 格式）
            var message = Encoding.UTF8.GetString(body);

            // 記錄收到的訊息
            _logger.LogInformation("Received message: {Message}", message);

            try
            {
                // // 將 JSON 字串反序列化成 CrawlRequest 物件
                var crawlRequest = System.Text.Json.JsonSerializer.Deserialize<CrawlRequest>(message);

                // 檢查反序列化是否成功
                if (crawlRequest == null)
                {
                    _logger.LogWarning("Failed to deserialize CrawlRequest.");
                    // 手動拒絕並不重新放回佇列
                    _channel.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                // 產生任務編號（例如 Task-20250617-0001）
                var taskId = $"Task-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

                var crawlerTask = new CrawlerTask
                {
                    TaskId = taskId,
                    Source = "{" + $"Mode: {crawlRequest.Mode}, Keyword: {crawlRequest.Keyword}, MaxPage: {crawlRequest.MaxPage}" + "}", // 可以視為來源網站或內容
                    // Source = message, // 可以視為來源網站或內容
                    Status = "Received"
                };

                using var scope = _services.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.CrawlerTasks.AddAsync(crawlerTask);

                // 使用依賴注入解析 Momo 與 PCHome 爬蟲服務
                var momoCrawlerService = scope.ServiceProvider.GetRequiredService<IMomoCrawlerService>();
                var pchomeCrawlerService = scope.ServiceProvider.GetRequiredService<IPchomeCrawlerService>();

                // 根據 Mode 執行不同爬蟲
                switch (crawlRequest.Mode.ToLower())
                {
                    case "momo":
                        await momoCrawlerService.GetProductsAsync(crawlRequest.Keyword, crawlRequest.MaxPage, taskId);
                        break;
                    case "pchome":
                        await pchomeCrawlerService.GetProductsAsync(crawlRequest.Keyword, crawlRequest.MaxPage, taskId);
                        break;
                    default:
                        _logger.LogWarning("Unknown mode: {Mode}", crawlRequest.Mode);
                        break;
                 }
                _logger.LogInformation("Processing crawl request for mode: {Mode}, keyword: {Keyword}, maxPage: {MaxPage}",
                    crawlRequest.Mode, crawlRequest.Keyword, crawlRequest.MaxPage);

                // 處理成功，手動回覆 ack
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
                // 處理失敗，拒絕並且不重新排隊（避免死循環）
                _channel.BasicReject(ea.DeliveryTag, false);
            }
        };

        // 開始消費指定佇列的訊息，autoAck 設為 false 表示由程式手動回覆訊息已處理完成
        _channel.BasicConsume(queue: "product_price_updates_queue",
                            autoAck: false,  // 手動回覆 ack，確保穩定處理
                            consumer: consumer);

        // 此方法為非同步執行，返回已完成的 Task，等待事件觸發處理訊息
        return Task.CompletedTask;
    }
}
