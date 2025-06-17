CREATE DATABASE ScrapeDb;
GO

USE ScrapeDb;
GO

CREATE TABLE ProductHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,  -- 主鍵，自動遞增
    ProductId INT NOT NULL,                   -- 對應 Product 表的主鍵
    Price DECIMAL(18, 2) NOT NULL,            -- 當時價格
    Stock INT NOT NULL,                       -- 當時庫存
    CapturedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),  -- 記錄時間
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
);
GO
-------------------------------------------------------------
USE ScrapeDb;
GO

CREATE TABLE dbo.Product
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,                        -- 主鍵，自動遞增
    ProductName NVARCHAR(100) NOT NULL,                             -- 商品名稱
    Description NVARCHAR(500) NULL,                                 -- 商品描述
    Price DECIMAL(18, 2) NOT NULL,                                  -- 價格
    Stock INT NOT NULL DEFAULT 0,                                   -- 庫存數量
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),          -- 建立時間
    UpdatedAt DATETIME2 NULL,                                       -- 更新時間
    TaskId NVARCHAR(50) NOT NULL,                                   -- 外鍵欄位，指向任務
    CONSTRAINT FK_Product_CrawlerTasks
    FOREIGN KEY (TaskId) REFERENCES dbo.CrawlerTasks(TaskId)    -- 外鍵約束
);
GO

-- 新增 TaskId 欄位，用來紀錄該商品是在哪個爬蟲任務中擷取到的
ALTER TABLE Product
ADD TaskId NVARCHAR(50);

-- 設定 TaskId 為外鍵，參考 CrawlerTasks 的 TaskId 欄位
ALTER TABLE Product
ADD CONSTRAINT FK_Product_CrawlerTasks
FOREIGN KEY (TaskId) REFERENCES CrawlerTasks(TaskId);

--------------------------------------------------------------
CREATE TABLE dbo.CrawlerTasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,                     -- 主鍵，自動遞增
    TaskId NVARCHAR(50) NOT NULL UNIQUE,                  -- 任務編號，禁止重複
    Source NVARCHAR(100) NOT NULL,                        -- 任務來源（例如 momo、pchome）
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',       -- 任務狀態（Pending / Running / Completed / Failed）
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()     -- 建立時間（UTC）
);