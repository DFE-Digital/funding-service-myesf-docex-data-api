SELECT c.InstanceId,
	c.Type,
	c.FinishedAt,
	c["From"],
	c["To"],
	c.PageNumber,
	c.NumberOfSuccessfullyProcessedOrganisations,
	c.IsSuccess,
	c["Error"]
FROM c
WHERE c.InstanceId = @instanceId
AND c.Type = @type
ORDER BY c.FinishedAt DESC