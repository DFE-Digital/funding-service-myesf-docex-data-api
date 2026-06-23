function updateDocumentMetadata(fileString) {
    var updateObject = JSON.parse(fileString);
    var collection = getContext().getCollection();

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT * FROM c WHERE c.id = "' + updateObject.BatchId + '"',

        function (err, feed) {
            if (err)  throw  err;

            if (feed && feed.length) {
                var latestHistoryAction = updateObject.Metadata.History[updateObject.Metadata.History.length - 1].Action;
                if (latestHistoryAction === 'ViewedBySender' || latestHistoryAction === 'ViewedByReciever') {

                    var emailAction = feed[0].Files[0].History.find(history => history.Action === 'EmailSent');
                    if (emailAction) {
                        var emailDate = new Date(emailAction.ActionDateTimeUtc);
                        feed[0].ttl = getTTL(emailDate);
                    }
                }
                
                tryUpdate(feed[0], updateObject);
            }
        });

    if  (!isAccepted) throw new Error('The query was not accepted by the server.');

    function tryUpdate(document, updateObject) {
        var requestOptions = { etag: document._etag };

        var response = getContext().getResponse();
        var matchingFiles = 0;
        var fieldsUpdated = 0;

        for (let fileIndex = 0, fileCount = document.Files.length; fileIndex < fileCount; fileIndex++) {
            let file = document.Files[fileIndex];

            if (file.Filename === updateObject.ExistingFileName) {
                matchingFiles += 1;

                for (let fieldToUpdateIndex = 0, fieldToUpdateCount = updateObject.FieldsToUpdate.length; fieldToUpdateIndex < fieldToUpdateCount; fieldToUpdateIndex++) {
                    let key = updateObject.FieldsToUpdate[fieldToUpdateIndex];

                    file[key] = updateObject.Metadata[key];
                    fieldsUpdated += 1;
                }
            }
        }

        response.setBody(matchingFiles + " files matched. " + fieldsUpdated + " total field(s) updated");

        var isAccepted = collection.replaceDocument(document._self,
            document,
            requestOptions,
            function (err, updatedDocument, responseOptions) {
                //
            }
        );
    }

    function getTTL(emailDate) {
        var currentDate = new Date(new Date().toUTCString());
        return Math.floor(((94608000000 - (currentDate - emailDate)) / 1000));
    }
}