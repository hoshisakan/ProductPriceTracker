USE [ScrapeDbDeploy]
GO

SELECT 
p.ProductId, ph.HistoryId, p.ProductName, p.Price
FROM [dbo].Products AS p
INNER JOIN [dbo].ProductHistories AS ph
ON p.TaskId = ph.TaskId
AND p.UserId = ph.UserId
ORDER BY p.Price DESC
;


SELECT ct.[Id]
    ,ct.[TaskId]
    ,ct.[Source]
    ,ct.[Status]
    ,ct.[CreatedAt]
    ,ct.[UserId]
    ,p.ProductName
    ,p.Price
FROM [dbo].[CrawlerTasks] AS ct
INNER JOIN [dbo].Products as p
ON ct.TaskId = p.TaskId
ORDER BY ct.UserId

GO


