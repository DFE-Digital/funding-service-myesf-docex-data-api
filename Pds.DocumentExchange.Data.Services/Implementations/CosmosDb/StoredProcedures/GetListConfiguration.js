function getListConfiguration(section) {
    if (!section) throw new Error('Configuration section must be specified');
    var collection = getContext().getCollection();

    var query = {
        query: 'SELECT c.configuration FROM c WHERE c.documentType = "ListConfiguration" AND c.configurationSection =  @section',
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
                var body = feed.map(feedItem => feedItem.configuration);
                response.setBody(body);
            }
        });

    if (!isAccepted) throw new Error('The queryDocuments command was not accepted by the server.');

    function createDefaultConfiguration(context, section) {
        var items = [];
        var defaultConfiguration = getDefaultConfiguration(section);

        if (defaultConfiguration == null) throw new Error('Configuration section ' + section + ' does not exist');

        var collection = context.getCollection();

        defaultConfiguration.forEach(function (configurationItem) {
            var accepted = collection.createDocument(
                collection.getSelfLink(),
                getNewConfigurationDocument(section, configurationItem),
                function (err, newDoc) {
                    if (err) throw err;
                    items.push(newDoc.configuration);
                }
            );

            if (!accepted) throw new Error('The createDocument command was not accepted by the server.');
        });

        context.getResponse().setBody(items);
    }

    function getNewConfigurationDocument(section, configuration) {
        return {
            "partitionKey": "1",
            "documentType": "ListConfiguration",
            "configurationSection": section,
            "configuration": configuration
        }
    }

    function getDefaultConfiguration(section) {
        var allDefaultConfiguration = {
            "Teams": [
                {
                    "identifier": "DocumentExchangeAdministratorFundingCentre",
                    "name": "Funding",
                    "emailAddress": "pds.sfs.test.dat+updated1@gmail.com"
                },
                {
                    "identifier": "DocumentExchangeAdministratorRiskAssurance",
                    "name": "Provider risk and assurance",
                    "emailAddress": "pds.sfs.test.dat+updated2@gmail.com"
                }
            ],
            "AllowedFileExtensions": [
                {
                    "identifier": "csv",
                    "extension": "csv",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "doc",
                    "extension": "doc",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "docx",
                    "extension": "docx",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "jpg",
                    "extension": "jpg",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "ods",
                    "extension": "ods",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "odt",
                    "extension": "odt",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "pdf",
                    "extension": "pdf",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "xls",
                    "extension": "xls",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "xlsx",
                    "extension": "xlsx",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": true,
                    "products": [

                    ]
                },
                {
                    "identifier": "zip",
                    "extension": "zip",
                    "canInternalUserUpload": true,
                    "canExternalUserUpload": false,
                    "products": [

                    ]
                }
            ],
            "Products": [
                {
                    "identifier": 10001,
                    "name": "Allocation calculation toolkit",
                    "pluralName": "Allocation calculation toolkits",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10002,
                    "name": "Data sharing protocol",
                    "pluralName": "Data sharing protocols",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10003,
                    "name": "Data and MI report",
                    "pluralName": "Data and MI reports",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10004,
                    "name": "14 to 16 revenue funding allocation statement",
                    "pluralName": "14 to 16 revenue funding allocation statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10005,
                    "name": "16 to 19 reconciliation statement",
                    "pluralName": "16 to 19 reconciliation statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10006,
                    "name": "Non maintained special school allocation statement",
                    "pluralName": "Non maintained special school allocation statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10007,
                    "name": "LA student number summary",
                    "pluralName": "LA student number summaries",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10008,
                    "name": "General annual grant",
                    "pluralName": "General annual grants",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10009,
                    "name": "Draft general annual grant",
                    "pluralName": "Draft general annual grants",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10010,
                    "name": "LA allocation statement summary",
                    "pluralName": "LA allocation statement summaries",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10011,
                    "name": "19+ allocation statement",
                    "pluralName": "19+ allocation statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10012,
                    "name": "Pupil premium return",
                    "pluralName": "Pupil premium returns",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10013,
                    "name": "Payment schedule",
                    "pluralName": "Payment schedules",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10014,
                    "name": "Business case audit evidence request",
                    "pluralName": "Business case audit evidence requests",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10015,
                    "name": "Business case audit evidence return",
                    "pluralName": "Business case audit evidence returns",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10016,
                    "name": "Business case template",
                    "pluralName": "Business case templates",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10017,
                    "name": "Dedicated schools grant assurance",
                    "pluralName": "Dedicated schools grant assurances",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10018,
                    "name": "Note to accounts",
                    "pluralName": "Notes to accounts",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10019,
                    "name": "School financial value assurance statement",
                    "pluralName": "School financial value assurance statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10020,
                    "name": "Pupil premium certification",
                    "pluralName": "Pupil premium certifications",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10021,
                    "name": "FE and sports certification",
                    "pluralName": "FE and sports certifications",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10022,
                    "name": "Infant free meals certification",
                    "pluralName": "Infant free meals certifications",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10023,
                    "name": "Year 7 catch up certification",
                    "pluralName": "Year 7 catch up certifications",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10024,
                    "name": "AP free school template",
                    "pluralName": "AP free school templates",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10025,
                    "name": "LA infrastructure changes",
                    "pluralName": "LAs infrastructure changes",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10026,
                    "name": "Free meals data return",
                    "pluralName": "Free meal data returns",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10027,
                    "name": "RPA certificates",
                    "pluralName": "RPA certificates",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10028,
                    "name": "LA RAT extract",
                    "pluralName": "LA RAT extracts",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10029,
                    "name": "Student eligibility workbook",
                    "pluralName": "Student eligibility workbooks",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10030,
                    "name": "Residential support scheme verification",
                    "pluralName": "Residential support scheme verifications",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10031,
                    "name": "Policy or allocation change letter",
                    "pluralName": "Policy or allocation change letters",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10032,
                    "name": "Monitoring information",
                    "pluralName": "Monitoring information",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10033,
                    "name": "Authority proforma tool",
                    "pluralName": "Authority proforma tools",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10034,
                    "name": "Adult business case",
                    "pluralName": "Adult business cases",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10035,
                    "name": "Fire safety survey",
                    "pluralName": "Fire safety surveys",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10036,
                    "name": "Post 16 grant assurance",
                    "pluralName": "Post 16 grant assurances",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorRiskAssurance"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10037,
                    "name": "16 to 19 grant assurance",
                    "pluralName": "16 to 19 grant assurances",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorRiskAssurance"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10038,
                    "name": "Capital CFO assurance statement",
                    "pluralName": "Capital CFO assurance statements",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorRiskAssurance"
                    ],
                    "canOrganisationsUpload": "false"
                },
                {
                    "identifier": 10083,
                    "name": "Alternative completion",
                    "pluralName": "Alternative completions",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10084,
                    "name": "Business case audit evidence",
                    "pluralName": "Business cases audit evidence",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10085,
                    "name": "Business case",
                    "pluralName": "Business cases",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10086,
                    "name": "Data sharing protocol",
                    "pluralName": "Data sharing protocols",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10087,
                    "name": "Financial report",
                    "pluralName": "Financial reports",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10088,
                    "name": "Funding monitoring report",
                    "pluralName": "Funding monitoring reports",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10089,
                    "name": "Infrastructure return",
                    "pluralName": "Infrastructure returns",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10090,
                    "name": "Reconcilliation return",
                    "pluralName": "Reconcilliation returns",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorFundingCentre"
                    ],
                    "canOrganisationsUpload": "true"
                },
                {
                    "identifier": 10091,
                    "name": "Grant assurance",
                    "pluralName": "Grants assurance",
                    "agencyTeams": [
                        "DocumentExchangeAdministratorRiskAssurance"
                    ],
                    "canOrganisationsUpload": "true"
                }
            ],
            "EmailSettings": [
                {
                    "emailMessageType": "ESFAPublicationInfected"
                },
                {
                    "emailMessageType": "ExternalUploadInfected"
                },
                {
                    "emailMessageType": "ESFAPublishedMultiple"
                },
                {
                    "emailMessageType": "ESFAPublishedSingle"
                },
                {
                    "emailMessageType": "ProviderReceivedMultipleTypes"
                },
                {
                    "emailMessageType": "ProviderReceivedSingleType"
                },
                {
                    "emailMessageType": "ProviderReceivedTwoTypes"
                },
                {
                    "emailMessageType": "ESFAReceivedMultiple"
                },
                {
                    "emailMessageType": "ESFAReceivedSingle"
                },
                {
                    "emailMessageType": "ExternalUploadCompleted"
                }
            ]
        };

        return allDefaultConfiguration[section];
    }
}