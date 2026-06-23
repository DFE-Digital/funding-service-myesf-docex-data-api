function addHistoryToFileMetadata(historyString) {
    var updateObject = JSON.parse(historyString);
    var collection = getContext().getCollection();

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT * FROM c WHERE c.id = "' + updateObject.BatchId + '"',

        function (err, feed) {
            if (err) throw err;

            if (feed && feed.length) {
                var latestHistoryAction = updateObject.Files[0].History[0].Action;
                if (latestHistoryAction === 'EmailSent') {
                    feed[0].ttl = 94608000;
                }

                tryUpdate(feed[0], updateObject);
            }
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');

    function tryUpdate(document, updateObject) {
        var requestOptions = { etag: document._etag };
        var response = getContext().getResponse();

        var matchingFiles = 0;

        for (let updateFileIndex = 0, len = updateObject.Files.length; updateFileIndex < len; updateFileIndex++) {
            let updateFile = updateObject.Files[updateFileIndex];

            for (let fileIndex = 0, fileCount = document.Files.length; fileIndex < fileCount; fileIndex++) {
                let documentFile = document.Files[fileIndex];

                if (updateFile.Filename === documentFile.Filename) {
                    matchingFiles += 1;

                    documentFile.History = documentFile.History.concat(updateFile.History);
                }
            }
        }

        response.setBody(matchingFiles + " files added history to");

        var isAccepted = collection.replaceDocument(document._self,
            document,
            requestOptions,
            function (err, updatedDocument, responseOptions) {
                //
            }
        );
    }
}