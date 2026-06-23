function getCountOfUnprocessedFiles(parentBatchId) {
    var collection = getContext().getCollection();

    var query = {
        query: "SELECT value count(1) FROM Batches JOIN Files in Batches.Files WHERE Batches.ParentBatchIdentifier = @ParentBatchId and Files.Processed = false",
        parameters: [
            { name: "@ParentBatchId", value: parentBatchId }
        ]
    };

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        query,

    function (err, feed) {
        if (err) throw err;

        var response = getContext().getResponse();

        // Check the feed and if empty, set the body to 'no docs found', 
        // else take 1st element from feed
        if (!feed || !feed.length) {            
            response.setBody(0);
        }
        else {
            response.setBody(feed[0]);
        }
    });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}