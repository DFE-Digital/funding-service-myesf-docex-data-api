SELECT DISTINCT
    c.id,
    c.UploadedBy,
    c.CreatedDate,
    ARRAY(select value cf) AS Files,
    c.InitialErrors,
    c.ParentBatchIdentifier
FROM c
JOIN cf in c.Files
WHERE ARRAY_CONTAINS(@ukprns, cf.FromUKPRN)
and cf.Okay = true
and cf.Processed = true
and cf.Deleted = false
ORDER BY c.CreatedDate