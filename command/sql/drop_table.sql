DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += '
ALTER TABLE [' + OBJECT_NAME(parent_object_id) + '] 
DROP CONSTRAINT [' + name + '];'
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('dbo.Product');

EXEC sp_executesql @sql;

-- Step 2: 刪除 Product 資料表
DROP TABLE dbo.Product;