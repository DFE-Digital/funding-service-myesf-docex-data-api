namespace Pds.DocumentExchange.Data.Core.Common
{
    /// <summary>
    /// The cosmos database stored procedure names.
    /// </summary>
    public static class CosmosDbStoredProcedureNames
    {
        /// <summary>
        /// The stored procedure name to add history to a file metadata.
        /// </summary>
        public const string AddHistoryToFileMetadata = "AddHistoryToFileMetadata";

        /// <summary>
        /// The stored procedure name to get batches by parent batch id.
        /// </summary>
        public const string GetBatchesByParent = "GetBatchesByParent";

        /// <summary>
        /// The stored procedure name to get configuration data.
        /// </summary>
        public const string GetConfiguration = "GetConfiguration";

        /// <summary>
        /// The stored procedure name to get configuration data.
        /// </summary>
        public const string UpdateConfiguration = "UpdateConfiguration";

        /// <summary>
        /// The stored procedure name to get list configuration data.
        /// </summary>
        public const string GetListConfiguration = "GetListConfiguration";

        /// <summary>
        /// The stored procedure name to get list configuration data.
        /// </summary>
        public const string AddOrUpdateListConfiguration = "AddOrUpdateListConfiguration";

        /// <summary>
        /// The stored procedure name to get count of unprocessed files.
        /// </summary>
        public const string GetCountOfUnprocessedFiles = "GetCountOfUnprocessedFiles";

        /// <summary>
        /// The stored procedure name to get the current version number.
        /// </summary>
        public const string GetCurrentVersionNumber = "GetCurrentVersionNumber";

        /// <summary>
        /// The stored procedure name to get report of documents viewed.
        /// </summary>
        public const string GetReportOfDocumentsViewed = "GetReportOfDocumentsViewed";

        /// <summary>
        /// The stored procedure name to get report of documents viewed summary.
        /// </summary>
        public const string GetReportOfDocumentsViewedSummary = "GetReportOfDocumentsViewedSummary";

        /// <summary>
        /// The stored procedure name to get report of file scan bad.
        /// </summary>
        public const string GetReportOfFileScanBad = "GetReportOfFileScanBad";

        /// <summary>
        /// The stored procedure name to get report of initial upload errors.
        /// </summary>
        public const string GetReportOfInitialUploadErrors = "GetReportOfInitialUploadErrors";

        /// <summary>
        /// The stored procedure name to get report of files sent from Agency.
        /// </summary>
        public const string GetReportOfFilesSentFromAgency = "GetReportOfFilesSentFromAgency";

        /// <summary>
        /// The MI report.
        /// </summary>
        public const string GetMIReport = "GetMIReport";

        /// <summary>
        /// The stored procedure name to try and set an exclusive email lock identifier on the batches.
        /// </summary>
        public const string TryAndSetAnExclusiveEmailLockIdOnTheBatches = "TryAndSetAnExclusiveEmailLockIdOnTheBatches";

        /// <summary>
        /// The stored procedure name to update document metadata.
        /// </summary>
        public const string UpdateDocumentMetadata = "UpdateDocumentMetadata";

        /// <summary>
        /// The stored procedure name to update document metadata and version.
        /// </summary>
        public const string UpdateDocumentMetadataAndVersion = "UpdateDocumentMetadataAndVersion";

        /// <summary>
        /// The stored procedure name to get count of unseen received files.
        /// </summary>
        public const string GetCountOfUnseenReceivedFiles = "GetCountOfUnseenReceivedFiles";

        /// <summary>
        /// The stored procedure name to get a file.
        /// </summary>
        public const string GetFile = "GetFile";

        /// <summary>
        /// The query name to get the agency files to delete by UKPRN, document type and year.
        /// </summary>
        public const string GetAgencyDeleteFilesInfo = "GetAgencyDeleteFilesInfo";

        /// <summary>
        /// The query name to get the organisation files to delete by UKPRN, document type and year.
        /// </summary>
        public const string GetOrganisationDeleteFilesInfo = "GetOrganisationDeleteFilesInfo";

        /// <summary>
        /// The stored procedure name to get the batches which have been published during last month.
        /// </summary>
        public const string GetPublishedBatches = "GetPublishedBatches";

        /// <summary>
        /// The stored procedure name to get the notification recipients by a given parent batch.
        /// </summary>
        public const string GetNotificationRecipientsByParentBatch = "GetNotificationRecipientsByParentBatch";
    }
}