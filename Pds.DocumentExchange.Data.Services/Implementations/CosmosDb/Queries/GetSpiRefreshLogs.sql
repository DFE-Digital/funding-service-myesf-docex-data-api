SELECT TOP 20 *
FROM c 
WHERE c.documentType = "SpiRefreshLog"
ORDER BY c.ActionDateTimeUtc DESC