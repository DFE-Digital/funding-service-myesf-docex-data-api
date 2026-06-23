function updateConfiguration(section, newValueJson) {
    if (!section) throw new Error('Configuration section must be specified');
    if (!newValueJson) throw new Error('New configuration value must be specified');

    var newValue = JSON.parse(newValueJson);
    var collection = getContext().getCollection();

    var query = {
        query: 'SELECT * FROM c WHERE c.documentType = "Configuration" AND c.configurationSection =  @section',
        parameters: [
            { name: "@section", value: section }
        ]
    };

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        query,
        function (err, feed) {
            if (err) throw err;

            if (feed && feed.length) {
                tryUpdateConfiguration(feed[0], newValue);
            }
            else {
                throw new Error('Configuration section ' + section + ' does not exist');
            }
        });

    if (!isAccepted) throw new Error('The queryDocuments command was not accepted by the server.');

    function tryUpdateConfiguration(document, newValue) {
        var requestOptions = { etag: document._etag };
        document.configuration = newValue;

        var isAccepted = collection.replaceDocument(
            document._self,
            document,
            requestOptions,
            function (err, updatedDoc) {
                if (err) throw err;
                var body = updatedDoc.configuration;
                getContext().getResponse().setBody(body);
            }
        );

        if (!isAccepted) throw new Error('The replaceDocument command was not accepted by the server.');
    }
}