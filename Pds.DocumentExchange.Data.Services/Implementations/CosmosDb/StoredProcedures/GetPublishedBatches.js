function getPublishedBatches(pageNumber, pageSize) {
    let context = getContext();

    let collection = context.getCollection();
    let response = context.getResponse();

    let isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT c.ParentBatchIdentifier, c.UploadedBy, COUNT(c.Files) NumberOfDocuments, MAX(c.CreatedDate) AS DateAndTime ' +
        'FROM c JOIN (SELECT VALUE f FROM f IN c.Files WHERE f.FromUKPRN = -999 AND f.Deleted = false) f ' +
        'GROUP BY c.ParentBatchIdentifier, c.UploadedBy',
        {
            pageSize: -1
        },
        function (err, feed) {
            if (err) throw err;

            let publishedDocs = feed;
            const totalItems = publishedDocs.length;
            const totalPages = Math.ceil(totalItems / pageSize);

            publishedDocs.sort(compare);

            const start = (pageNumber - 1) * pageSize;
            const end = pageNumber * pageSize;
            publishedDocs = publishedDocs.slice(start, end);

            for (i = 0; i < publishedDocs.length; i++) {
                addNumberOfEmails(publishedDocs[i]);
            }

            const result = {
                totalItems: totalItems,
                totalPages: totalPages,
                items: publishedDocs
            };

            response.setBody(result);
        }
    );

    if (!isAccepted) throw new Error('The query was not accepted by the server.');

    function compare(a, b) {
        if (a.DateAndTime < b.DateAndTime) {
            return 1;
        }
        if (a.DateAndTime > b.DateAndTime) {
            return -1;
        }
        return 0;
    }

    function addNumberOfEmails(publishedDoc) {
        let getNumberOfEmailsQuery =
        {
            'query': 'SELECT SUM(ARRAY_LENGTH(r.To)) AS NumberOfEmails ' +
                'FROM c ' +
                'JOIN r IN c.RecipientNotificationSummaries ' +
                'WHERE c.documentType = "BatchNotificationSummary" ' +
                'AND c.ParentBatchIdentifier = @parentBatchId',
            'parameters': [{ 'name': '@parentBatchId', 'value': publishedDoc.ParentBatchIdentifier }]
        };

        let isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            getNumberOfEmailsQuery,
            {
                pageSize: -1
            },
            function (err, feed) {
                if (err) throw err;

                if (feed.length > 0) {
                    publishedDoc.NumberOfEmails = feed[0].NumberOfEmails;
                }
            });

        if (!isAccepted) throw new Error('The query was not accepted by the server.');
    }
}