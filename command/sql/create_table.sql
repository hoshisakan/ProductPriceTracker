
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,          -- 主鍵，自動遞增
    Username NVARCHAR(100) NOT NULL UNIQUE,    -- 使用者名稱，不可重複
    PasswordHash NVARCHAR(256) NOT NULL,       -- 密碼雜湊值
    Role NVARCHAR(20) NOT NULL DEFAULT 'User', -- 使用者角色，預設為 User
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE dbo.RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,             -- 主鍵，自動遞增
    Token NVARCHAR(256) NOT NULL,                 -- Token 值
    ExpiresAt DATETIME2 NOT NULL,                 -- 過期時間
    IsRevoked BIT NOT NULL DEFAULT 0,             -- 是否已撤銷，預設為 false
    UserId INT NOT NULL,                          -- 所屬使用者 Id（外鍵）
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) -- 外鍵關聯至 Users 表
);

CREATE TABLE dbo.CrawlerTasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,                     
    TaskId NVARCHAR(50) NOT NULL UNIQUE,                  
    Source NVARCHAR(100) NOT NULL,                        
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',       
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),    
    UserId INT NOT NULL,                                  -- 新增 UserId 欄位
    CONSTRAINT FK_CrawlerTasks_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)  -- 外鍵約束
);


CREATE TABLE dbo.Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,                        
    ProductName NVARCHAR(100) NOT NULL,                             
    Description NVARCHAR(500) NULL,                                 
    Price DECIMAL(18, 2) NOT NULL,                                  
    Stock INT NOT NULL DEFAULT 0,                                   
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),          
    UpdatedAt DATETIME2 NULL,                                       
    TaskId NVARCHAR(50) NOT NULL,                                   
    UserId INT NOT NULL,        -- 這是新加的欄位
    CONSTRAINT FK_Product_CrawlerTasks FOREIGN KEY (TaskId) REFERENCES dbo.CrawlerTasks(TaskId),
    CONSTRAINT FK_Product_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.ProductHistories ( 
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,       -- 主鍵，自動遞增
    ProductId INT NOT NULL,                         -- 對應 Products 表的主鍵
    TaskId NVARCHAR(50) NOT NULL,   
    UserId INT NOT NULL,                            -- 對應 Users 表的主鍵
    Price DECIMAL(18, 2) NOT NULL,                  -- 當時價格
    Stock INT NOT NULL,                             -- 當時庫存
    CapturedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),  -- 記錄時間

    CONSTRAINT FK_ProductHistories_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_ProductHistories_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_ProductHistories_CrawlerTasks FOREIGN KEY (TaskId) REFERENCES CrawlerTasks(TaskId)
);
