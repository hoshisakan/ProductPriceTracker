CREATE DATABASE ScrapeDb;
GO

USE ScrapeDb;
GO

CREATE TABLE ScrapeResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Url NVARCHAR(1000) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
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