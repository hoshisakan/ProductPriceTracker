SELECT h.HistoryId, p.ProductId, p.ProductName, p.Description, p.Price FROM dbo.Product AS p
INNER JOIN dbo.ProductHistory AS h
ON p.ProductId = h.ProductId
ORDER BY p.Price;
