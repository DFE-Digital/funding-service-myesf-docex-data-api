# Manage Your Education and Skills Funding Document Exchange Data Api

The Manage Your Education and Skills Funding (MYESF) Document Exchange Data Api is used by the MYESF Document Exchange web application to allow the following:

- Perform requests for internal agency teams to publish documents for external users to receive
- Perform requests for external users to upload documents for internal agency teams to receive
- Retrieve organisation/provider information to be used in the web application
- Retrieve internal persisted settings to be used in the web application
- Send requests for documents to supporting virus scan service
- Send requests for notification emails to be sent to supporting email service

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is an ASP.NET Core 8 web api utilising Azure App Service for deployment.

The web api runs on an Azure App service on Azure.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `appsettings.json` file will need to be created in the `Pds.DocumentExchange.Data.Api` project. Below, and included in the repo, there is `appsettings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

## Application Settings (`appsettings.json`)

```json
{
  "AzureAd": {
    "Audience": "",
    "ClientId": "",
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": ""
  },
  "AzureCosmosDb": {
    "AuthKeyOrResourceToken": "",
    "ServiceEndpoint": ""
  },
  "Cache": {
    "AllOrganisationsMemoryCacheTimeMinutes": "60",
    "DocumentListCacheTimeMinutes": "3",
    "DocumentValidationCacheTimeMinutes": "1",
    "Redis": {
      "ConnectionString": "redis:63789"
    }
  },
  "DfESignin": {
    "PublicApi": {
      "ClientID": "",
      "ClientSecret": "",
      "TokenIssuer": "",
      "Url": ""
    }
  },
  "DocumentExchangeServices": {
    "AgencyService": {
      "ServiceBaseURL": ""
    },
    "CosmosDb": {
      "DataEncryptionKey": ""
    },
    "DirectoriesManager": {
      "BlobContainers": {
        "ConnectionString": ""
      },
      "FileShareDirectories": {
        "ConnectionString": ""
      },
      "TeamsFileShareConnectionString": ""
    },
    "Notification": {
      "UseDfeSignInContactsApi": "true"
    },
    "ServiceBusQueueManager": {
      "ConnectionString": ""
    },
    "WindowsDefenderAntivirus:Url": ""
  },
  "FdsApiClientConfiguration": {
    "ApimSubscriptionKey": "",
    "Url": ""
  },
  "Logging": {
    "ApplicationInsights": {
      "Loglevel": {
        "Default": "Information",
        "Microsoft": "Error"
      }
    },
    "LogLevel": {
      "Default": "Information"
    }
  },
  "NotificationServiceBusOptions": {
    "ServiceBusQueueName": "pds-shared-emailprocessor",
    "ConnectionString": ""
  },
  "PdsApplicationInsights": {
    "Environment": "local",
    "InstrumentationKey": ""
  }
}
```

### Setting Details

- **`AzureAd:Audience`**  
  The intended recipient of the azure authentication token.
 
- **`AzureAd:ClientId`**  
  The application (client) ID registered in azure ad.

- **`AzureAd:Instance`**  
  The URL of the azure ad service used to authenticate.

- **`AzureAd:TenantId`**  
  The unique identifier for your azure ad tenant.

- **`AzureCosmosDb:AuthKeyOrResourceToken`**  
  The secret value for document exchange cosmos db resource.

- **`AzureCosmosDb:ServiceEndpoint`**  
  The uri for the document exchange cosmos db resource.

- **`Cache:AllOrganisationsMemoryCacheTimeMinutes`**  
  The cache timeout value, in minutes, for how long the distributed cache will persist all organisation data.

- **`Cache:DocumentListCacheTimeMinutes`**  
  The cache timeout value, in minutes, for how long the distributed cache will persist retrieved documents for user.

- **`Cache:DocumentValidationCacheTimeMinutes`**  
  The cache timeout value, in minutes, for how long the distributed cache will persist pending documents to be published for user.

- **`Cache:Redis:ConnectionString`**  
  The connection string for the redis cache resource. (redis:6379 points to local redis container spun up by docker-compose).

- **`DfESignin:PublicApi:ClientID`**  
  The application (client) ID for DfE sign in public api service.

- **`DfESignin:PublicApi:ClientSecret`**  
  The application (client) secret for DfE sign in public api service.

- **`DfESignin:PublicApi:TokenIssuer`**  
  The document exchange token identifier for DfE sign in public api service.

- **`DfESignin:PublicApi:Url`**  
  The url used to access DfE sign in public api service.

- **`DocumentExchangeServices:AgencyService:ServiceBaseURL`**  
  The base url for the document exchange web application.

- **`DocumentExchangeServices:CosmosDb:DataEncryptionKey`**  
  The data encryption key used for encrypting/decrypting sensitive information in the cosmos db.

- **`DocumentExchangeServices:DirectoriesManager:BlobContainers:ConnectionString`**  
  The connection string for the document exchange related azure storage blob containers.

- **`DocumentExchangeServices:DirectoriesManager:FileShareDirectories:ConnectionString`**  
  The connection string for the document exchange related azure storage file share directories for external upload process.

- **`DocumentExchangeServices:DirectoriesManager:TeamFileShareConnectionString`**  
  The connection string for the document exchange related azure storage file share directories for internal publish process.

- **`DocumentExchangeServices:Notification:UseDfeSignInContactsApi`**  
  The boolean flag to state whether DfE Sign In public api should be used to retrieve user contact information. Should always be `true` now.

- **`DocumentExchangeServices:ServiceBusQueueManager:ConnectionString`**  
  The connection string for the service bus resource for sending and processing messages related to upload, publish and virus scan processes.

- **`DocumentExchangeServices:WindowsDefenderAntivirus:Url`**  
  The url for antivirus service.

- **`FdsApiClientConfiguration:ApimSubscriptionKey`**  
  The Apim subscription key for FDS organisation/provider data service.

- **`FdsApiClientConfiguration:Url`**  
  The url for FDS organisation/provider data service.

- **`Logging:ApplicationInsights:LogLevel:Default`**
  The default logging level for the service when logging to Application Insights; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`Logging:ApplicationInsights:LogLevel:Microsoft`**
  The default logging level for Microsoft specific information when logging to Application Insights; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`Logging:LogLevel:Default`**
  The default logging level for the service; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`NotificationServiceBusOptions:ServiceBusQueueName`**  
  The name of the service bus queue resource for email processor service. Configuration settings bound to `Pds.Core.Notification` package. Should always be `pds-shared-emailprocessor`

- **`NotificationServiceBusOptions:ConnectionString`**  
  The connection string for the service bus resource for sending requests to email processor service. Configuration settings bound to `Pds.Core.Notification` package.

- **`PdsApplicationInsights:Environment`**  
  The environment which the app is running on for Application Insights for logging purposes.

- **`PdsApplicationInsights:InstrumentationKey`**  
  The key value for Application Insights resource for logging purposes.

## docker-compose

This project depends on a redis distributed cache resource for storing api requests/responses. We are unable to connect to deployed cloud resources and so a local redis container must be created via Docker in order to test full functionality local.

The docker-compose.yml file includeds the orchestration for starting both the api and redis containers.

You must select docker-compose as the startup project to ensure that all dependent resources are running in Docker to run this solution locally.

## Test execution

In order to test the application locally a valid `appsettings.json` file will need to be created in the `Pds.DocumentExchange.Data.Api.Tests` project. `appsettings.example.json`, in `Pds.DocumentExchange.Data.Api.Tests` can be used as a base and populated with appropriate values which can be found in Azure Portal. The local environment resources should be utilised.

## Test Application Settings (`appsettings.json`)

```json
{
  "CosmosDb": {
    "ServiceEndpoint": "",
    "AuthKeyOrResourceToken": ""
  },
  "DocumentExchangeServices": {
    "CosmosDb": {
      "DataEncryptionKey": ""
    }
  },
  "BlobContainers": {
    "ConnectionString": "",
    "ContainerName": "testdata"
  },
  "TeamsFileShareConnectionString": "",
  "ExternalFileShareConnectionString": "",
  "ServiceBusConnectionString": ""
}
```

### Setting Details

- **`CosmosDb:ServiceEndpoint`**  
  The uri for the document exchange cosmos db resource. (Use `pds-cosmos-local`)

- **`CosmosDb:AuthKeyOrResourceToken`**  
  The secret value for document exchange cosmos db resource. (Use `pds-cosmos-local`)

- **`DocumentExchangeServices:CosmosDb:DataEncryptionKey`**  
  The data encryption key used for encrypting/decrypting sensitive information in the cosmos db.

- **`BlobContainers:ConnectionString`**  
  The connection string for the document exchange related azure storage blob containers. (Use `pdsiexcosmoslocal`)

- **`BlobContainers:ContainerName`**  
  The connection string for the document exchange related azure storage blob containers. Always use `testdata`

- **`TeamsFileShareConnectionString`**  
  The connection string for the document exchange related azure storage file share directories for internal publish process. (Use `pdsiexintlocal`)

- **`ExternalFileShareConnectionString`**  
  The connection string for the document exchange related azure storage file share directories for external upload process. (Use `pdsiexextuploadlocal`)

- **`ServiceBusConnectionString`**  
  The connection string for the service bus resource for sending and processing messages related to upload, publish and virus scan processes. (Use `pds-sfs-servicebus`)

  