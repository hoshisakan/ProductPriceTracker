using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Infrastructure.Data;
using ProductPriceTracker.Infrastructure.Data.Repositories;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Infrastructure.Services;
using ProductPriceTracker.Core.Interface.IServices;
using Serilog;
using Microsoft.Playwright;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using ProductPriceTracker.Infrastructure.Services.Jobs;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// 加入 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000") // ✅ 單一 origin
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // 監聽 0.0.0.0:5000
});

// 1️⃣ 加入資料庫 DbContext
builder.Services.AddDbContext<ScrapeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var rabbitConfig = configuration.GetSection("RabbitMQ");

// 2️⃣ 註冊 DI
var factory = new ConnectionFactory()
{
    HostName = rabbitConfig["HostName"],
    Port = int.Parse(rabbitConfig["Port"]),
    UserName = rabbitConfig["UserName"],
    Password = rabbitConfig["Password"],
    VirtualHost = rabbitConfig["VirtualHost"]
};

const int maxRetries = 5;
int retriesLeft = maxRetries;
IConnection connection = null;

while (retriesLeft > 0)
{
    try
    {
        connection = factory.CreateConnection();
        Log.Information("成功連線 RabbitMQ");
        break; // 成功跳出迴圈
    }
    catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
    {
        retriesLeft--;
        Log.Error($"連線 RabbitMQ 失敗，剩餘嘗試次數：{retriesLeft}，錯誤訊息：{ex.Message}");
        if (retriesLeft == 0)
        {
            throw; // 用盡重試次數仍失敗，丟出例外
        }
        Thread.Sleep(3000); // 等待 3 秒後再重試
    }
}

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("MSSQLBackupJob");

    q.AddJob<SqlServerBackupJob>(opts => opts.WithIdentity(jobKey));

    // q.AddTrigger(t => t
    //     .ForJob(jobKey)
    //     .WithIdentity("MSSQLBackupJob-trigger")
    //     .WithCronSchedule("0 * * ? * *")); // ⏱️ 每分鐘執行一次

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("MSSQLBackupJob-trigger")
        .WithCronSchedule("0 0/30 * ? * *")); // 每半小時執行一次
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddSingleton<IConnection>(connection);
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductHistoryRepository, ProductHistoryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMomoCrawlerService, MomoCrawlerService>();
builder.Services.AddScoped<IPchomeCrawlerService, PchomeCrawlerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductHistoryService, ProductHistoryService>();
builder.Services.AddScoped<ICrawlerTaskRepository, CrawlerTaskRepository>();
builder.Services.AddScoped<ICrawlerTaskService, CrawlerTaskService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 加入 JWT 驗證
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// 3️⃣ 加入 Controller 與 Swagger（OpenAPI）
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Price Tracker API", Version = "v1" });

    // 加入 JWT Bearer 認證設定
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請輸入 JWT Token（格式：Bearer {your token})",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    };

    options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// 自動創建資料庫與資料表
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<ScrapeDbContext>();
        logger.LogInformation("正在檢查並套用資料庫 migration...");
        dbContext.Database.Migrate();
        logger.LogInformation("資料庫 migration 已套用");
        dbContext.Database.EnsureCreated(); // 確保資料庫已創建
        logger.LogInformation("資料庫已創建或已存在");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "資料庫 migration 發生錯誤");
    }
}

// 4️⃣ 設定開發環境使用 Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
