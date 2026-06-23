SELECT f.FileType, f.Metadata.Year AS Year, f.FromUKPRN, f.Version,
(SELECT VALUE COUNT(h) > 0 FROM h IN f.History WHERE h.Action = 'ViewedByReciever' OR h.Action = 'DownloadedByReceiver') AS Viewed
FROM Batches AS b
JOIN (SELECT VALUE f FROM f IN b.Files WHERE f.ToUKPRN = -999 AND f.Okay = true AND f.Processed = true AND f.Deleted = false
AND ARRAY_CONTAINS(@allowedProducts, f.FileType)) f