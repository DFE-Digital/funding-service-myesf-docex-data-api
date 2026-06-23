function getNotificationRecipientsByParentBatch(parentBatchId) {
	let context = getContext();

	let collection = context.getCollection();
	let response = context.getResponse();

	var query = {
		query: 'SELECT DISTINCT r.Ukprn, t AS EmailAddress ' +
				'FROM c ' +
				'JOIN r IN c.RecipientNotificationSummaries ' +
				'JOIN t IN r.To ' +
				'WHERE c.ParentBatchIdentifier = @parentBatchId ' +
				'AND c.documentType = "BatchNotificationSummary"',
		parameters: [
			{ name: "@parentBatchId", value: parentBatchId }
		]
	};

	let isAccepted = collection.queryDocuments(
		collection.getSelfLink(),
		query,
		{
			pageSize: -1
		},
		function (err, feed) {
			if (err) throw err;
			response.setBody(feed);
		});

	if (!isAccepted) throw new Error('The query was not accepted by the server.');
}