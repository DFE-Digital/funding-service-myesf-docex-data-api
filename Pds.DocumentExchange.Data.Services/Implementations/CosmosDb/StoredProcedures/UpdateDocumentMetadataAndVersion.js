function updateDocumentMetadataAndVersion(fileString) {
    var updateObject = JSON.parse(fileString);
    let collection = getContext().getCollection();

    var getBatchQuery =
    {
        'query': 'SELECT * FROM c WHERE c.id = @batchId',
        'parameters': [{ 'name': '@batchId', 'value': updateObject.BatchId }]
    };

    let isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        getBatchQuery,
        function (err, feed) {
            if (err) throw err;

            if (feed && feed.length) {
                tryUpdate(feed[0], updateObject);
            }
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');

    function tryUpdate(batch, updateObject) {
        let response = getContext().getResponse();

        let file = batch.Files.find(file => file.Filename === updateObject.ExistingFileName);

        if (file === undefined) {
            response.setBody(0);
        } else {
            updateObject.FieldsToUpdate.forEach(fieldName => file[fieldName] = updateObject.Metadata[fieldName]);

            var getVersionQuery =
            {
                'query': 'SELECT VALUE COUNT(1) '
                    + 'FROM Batches JOIN Files IN Batches.Files '
                    + 'WHERE Files.FromUKPRN = @fromUKPRN '
                    + 'AND Files.ToUKPRN = @toUKPRN '
                    + 'AND Files.Metadata.Year = @year '
                    + 'AND Files.FileType = @fileType',
                'parameters': [
                    { 'name': '@fromUKPRN', 'value': file.FromUKPRN },
                    { 'name': '@toUKPRN', 'value': file.ToUKPRN },
                    { 'name': '@year', 'value': file.Metadata.Year },
                    { 'name': '@fileType', 'value': file.FileType }
                ]
            };

            let isAccepted = collection.queryDocuments(
                collection.getSelfLink(),
                getVersionQuery,
                function (err, feed) {
                    if (err) throw err;

                    if (feed && feed.length) {
                        let version = feed[0];

                        file["Version"] = version;
                        saveBatch(batch);

                        response.setBody(version);
                    }
                });

            if (!isAccepted) throw new Error('The query was not accepted by the server.');
        }
    }

    function saveBatch(batch) {
        let collection = getContext().getCollection();
        let requestOptions = {
            etag: batch._etag
        };

        let isAccepted = collection.replaceDocument(batch._self,
            batch,
            requestOptions,
            function (err, updatedBatch, responseOptions) { }
        );

        if (!isAccepted) throw new Error('The query was not accepted by the server.');
    }
}