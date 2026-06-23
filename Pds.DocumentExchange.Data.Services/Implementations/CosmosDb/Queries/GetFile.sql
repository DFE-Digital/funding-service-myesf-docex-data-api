SELECT VALUE Files 
FROM Batches 
JOIN 
(SELECT VALUE f FROM f IN Batches.Files WHERE f.Filename = @fileName AND f.Okay = true AND f.Processed = true) Files
WHERE Batches.id = @batchId
AND Batches.ParentBatchIdentifier = @parentBatchId