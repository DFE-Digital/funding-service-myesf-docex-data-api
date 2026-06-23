function getConfiguration(section) {
    if (!section) throw new Error('Configuration section must be specified');
    var collection = getContext().getCollection();

    var query = {
        query: 'SELECT c.configuration FROM c WHERE c.documentType = "Configuration" AND c.configurationSection =  @section',
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
        function (err, feed, options) {
            if (err) throw err;

            if (!feed || !feed.length) {
                createDefaultConfiguration(getContext(), section);
            }
            else {
                var response = getContext().getResponse();
                var body = feed[0].configuration;
                response.setBody(body);
            }
        });

    if (!isAccepted) throw new Error('The queryDocuments command was not accepted by the server.');

    function createDefaultConfiguration(context, section) {
        var defaultConfiguration = getDefaultConfiguration(section);

        if (defaultConfiguration == null) throw new Error('Configuration section ' + section + ' does not exist');

        var collection = context.getCollection();
        var accepted = collection.createDocument(
            collection.getSelfLink(),
            getNewConfigurationDocument(section, defaultConfiguration),
            function (err, newDoc) {
                if (err) throw err;
                var body = newDoc.configuration;
                context.getResponse().setBody(body);
            }
        );

        if (!accepted) throw new Error('The createDocument command was not accepted by the server.');
    }

    function getNewConfigurationDocument(section, configuration) {
        return {
            "partitionKey": "1",
            "documentType": "Configuration",
            "configurationSection": section,
            "configuration": configuration
        }
    }

    function getDefaultConfiguration(section) {
        var allDefaultConfiguration = {
            "DocumentExchangeEnabled": true,
            "DocumentExchangeOrganisationUploadEnabled": true,
            "ServiceReplyEmail": "reply_to@docex.education.gov.uk",
            "ServiceNoReplyEmail": "service_relay_noreply@docex.education.gov.uk",
            "AgencyPublishCompleteProviderNotificationEnabled": true,
            "MaxFileUploadSize": 5000,
            "CacheWarmUpRejectSpiInvalidData": false
        };

        return allDefaultConfiguration[section];
    }
}