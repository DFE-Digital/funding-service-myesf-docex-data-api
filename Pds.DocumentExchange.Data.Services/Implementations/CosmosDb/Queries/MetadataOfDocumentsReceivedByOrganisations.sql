SELECT DISTINCT 
    c.id,
	c.ParentBatchIdentifier,
   ARRAY(SELECT f.Filename, f.FileType, f.FromUKPRN, f.ToUKPRN, f.Metadata, f.Version, 
   ARRAY(SELECT h.Action, h.ActionDateTimeUtc, {"Principle": h.User.Principle, "FullName": h.User.FullName, "IsEncrypted": h.User.IsEncrypted} AS User FROM h IN f.History 
   WHERE ARRAY_CONTAINS(["ViewedByReciever", "UploadedExternal", "Published"], h.Action)) AS History) AS Files
FROM c 
JOIN (SELECT VALUE f FROM f IN c.Files WHERE ARRAY_CONTAINS(@ukprns, f.ToUKPRN) AND f.Okay = true AND f.Processed = true AND f.Deleted = false) f
ORDER BY c.CreatedDate