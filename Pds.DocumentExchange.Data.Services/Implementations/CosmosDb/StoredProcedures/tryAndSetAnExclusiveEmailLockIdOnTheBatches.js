function tryAndSetAnExclusiveEmailLockIdOnTheBatches(fileString) {
    var updateObject = JSON.parse(fileString);
    var collection = getContext().getCollection();

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT * FROM c WHERE c.ParentBatchIdentifier = "' + updateObject.ParentBatchIdentifier + '"',

        function (err, feed) {
            if (err)  throw  err;

            if (feed && feed.length) {
                tryUpdate(feed, updateObject);
            }
        });

    if  (!isAccepted) throw new Error('The query was not accepted by the server.');

    function tryUpdate(documents, updateObject) {
        var response = getContext().getResponse();
        var set = false;

        for (let batchIndex = 0, batchCount = documents.length; batchIndex < batchCount; batchIndex++) {
            var document = documents[batchIndex];
            var requestOptions = { etag: document._etag };

            if (document.EmailLockId === null || document.EmailLockId === undefined) {
                document.EmailLockId = updateObject.EmailLockId;
                set = true;

                var isAccepted = collection.replaceDocument(document._self,
                    document,
                    requestOptions,
                    function (err, updatedDocument, responseOptions) {
                        //
                    }
                );
            }
        }

        response.setBody(set);
    }
}