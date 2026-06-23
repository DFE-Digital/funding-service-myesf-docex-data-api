function getBatchesByParent(parentBatchId, token) {
    var query = {
        query: "SELECT * " +
            "FROM Batches " +
            "WHERE Batches.ParentBatchIdentifier = @ParentBatchIdentifier " +
            "AND IS_DEFINED(Batches.Files)",
        parameters: [
            { name: "@ParentBatchIdentifier", value: parentBatchId }
        ]
    };
    var responseBody = [];
    var responseSize = 0;
    var lastContinuationToken;

    tryQuery(token);

    function tryQuery(continuationToken) {
        var requestOptions = {
            continuation: continuationToken,
            pageSize: 100
        };

        var accepted = __.queryDocuments(__.getSelfLink(), query, requestOptions,
            function (err, feed, responseOptions) {
                // The size of the current query response page.
                var queryPageSize = JSON.stringify(feed).length;

                // DocumentDB has a response size limit of 4 MB.
                if (responseSize + queryPageSize < 1024 * 1024 * 4) {
                    // Append query results to responseBody.
                    responseBody = responseBody.concat(feed);

                    // Keep track of the response size.
                    responseSize += queryPageSize;

                    if (responseOptions.continuation) {
                        // If there is a continuation token... Run the query again to get the next page of results
                        lastContinuationToken = responseOptions.continuation;
                        tryQuery(responseOptions.continuation);
                    } else {
                        // If there is no continutation token, we are done. Return the response.
                        __.response.setBody({
                            "Message": "Query completed succesfully.",
                            "LastContinuationToken": "",
                            "Documents": responseBody
                        });
                    }
                } else {
                    // If the response size limit reached; run the script again with the lastContinuationToken as a script parameter.
                    __.response.setBody({
                        "Message": "Response size limit reached.",
                        "LastContinuationToken": lastContinuationToken,
                        "Documents": responseBody
                    });
                }
            });

        if (!accepted) {
            // If the execution limit reached; run the script again with the lastContinuationToken as a script parameter.
            __.response.setBody({
                "Message": "Execution limit reached.",
                "LastContinuationToken": lastContinuationToken,
                "Documents": responseBody
            });
        }
    }
}