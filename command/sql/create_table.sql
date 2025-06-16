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
    ProductId INT IDENTITY(1,1) PRIMARY KEY,      -- 主鍵，自動遞增
    ProductName NVARCHAR(100) NOT NULL,           -- 商品名稱
    Description NVARCHAR(500) NULL,                -- 商品描述，可空
    Price DECIMAL(18, 2) NOT NULL,                 -- 價格，保留兩位小數
    Stock INT NOT NULL DEFAULT 0,                   -- 庫存數量，預設0
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),  -- 建立時間
    UpdatedAt DATETIME2 NULL                        -- 更新時間，可空
);
GO
--------------------------------------------------------------