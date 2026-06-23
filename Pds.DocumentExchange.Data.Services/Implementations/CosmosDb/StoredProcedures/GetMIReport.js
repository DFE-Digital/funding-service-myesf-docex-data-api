function getMIReport(fromdate, todate, token) {
    var query = {
        query: `SELECT
            d.FileType,
            d.Metadata.Year,
            d.ToUKPRN,
            d.FromUKPRN,
            d.Version,
            d.IsFromAgency,
            ARRAY(
                    SELECT e.ActionDateTimeUtc, { "FullName": e.User.FullName, "IsEncrypted": e.User.IsEncrypted } as User
                    from e in d.History
                    WHERE e.Action = 'ViewedByReciever'
                ) as History
            From c
            join d in c.Files
            where ARRAY_LENGTH(d.History) > 0
            and isdefined(c.Files)
            and c.CreatedDate >= '`+ fromdate + `'
            and c.CreatedDate <= '`+ todate + `'`
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