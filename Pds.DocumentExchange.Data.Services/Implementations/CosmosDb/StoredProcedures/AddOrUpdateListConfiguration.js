function addOrUpdateListConfiguration(section, oldIdentifier, newValueJson) {
    if (!section) throw new Error('Configuration section must be specified');
    if (!oldIdentifier) throw new Error('Old Identifier value must be specified');
    if (!newValueJson) throw new Error('New configuration value must be specified');

    var newValue = JSON.parse(newValueJson);

    if (!newValue.identifier) throw new Error('New configuration value must have an identifier');

    if (typeof newValue.identifier === 'string') {
        newValue.identifier = newValue.identifier.trim();

        if (!newValue.identifier) throw new Error('New configuration identifier cannot be only whitespace');
    }

    if (typeof oldIdentifier === 'string') {
        oldIdentifier = oldIdentifier.trim();

        if (!oldIdentifier) throw new Error('Old identifier cannot be only whitespace');
    }

    var collection = getContext().getCollection();

    var query = {
        query: 'SELECT * FROM c WHERE c.documentType = "ListConfiguration" AND c.configurationSection =  @section',
        parameters: [
            { name: "@section", value: section }
        ]
    };

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        query,
        {
            pageSize: -1
        },
        function (err, feed) {
            if (err) throw err;

            if (feed && feed.length) {
                tryAddOrUpdateConfiguration(feed, oldIdentifier, newValue);
            }
            else {
                throw new Error('Configuration section ' + section + ' does not exist');
            }
        });

    if (!isAccepted) throw new Error('The queryDocuments command was not accepted by the server.');

    function tryAddOrUpdateConfiguration(documents, oldIdentifier, newValue) {
        var existingDocuments = documents.filter(function (document) {
            return document.configuration.identifier === oldIdentifier;
        });

        if (existingDocuments && existingDocuments.length) {
            var document = existingDocuments[0];
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
        else {
            var accepted = collection.createDocument(
                collection.getSelfLink(),
                getNewConfigurationDocument(section, newValue),
                function (err, newDoc) {
                    if (err) throw err;
                    var body = newDoc.configuration;
                    getContext().getResponse().setBody(body);
                }
            );

            if (!accepted) throw new Error('The createDocument command was not accepted by the server.');
        }
    }

    function getNewConfigurationDocument(section, configuration) {
        return {
            "partitionKey": "1",
            "documentType": "ListConfiguration",
            "configurationSection": section,
            "configuration": configuration
        }
    }
}