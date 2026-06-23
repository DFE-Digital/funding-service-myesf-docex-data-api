using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>The ICosmosDbService interface - abstracts methods required to access azure cosmos db.</summary>
    public interface ICosmosDbService
    {
        /// <summary>Adds the history to file metadata.</summary>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="fileInfo">The file metadata.</param>
        /// <returns>An async task.</returns>
        Task<string> AddHistoryToFileMetadata(string batchIdentifier, FileMetadata fileInfo);

        /// <summary>Gets the metadata of documents received by organisation(s).</summary>
        /// <param name="identifiers">The organisation identifier(s).</param>
        /// <returns>The list of batches of metadata received by the organisation(s).</returns>
        Task<IEnumerable<BatchMetadata>> MetadataOfDocumentsReceivedByOrganisations(IEnumerable<OrganisationIdentifier> identifiers);

        /// <summary>Gets the metadata of documents sent by organisation(s).</summary>
        /// <param name="identifiers">The organisation identifier(s).</param>
        /// <returns>The list of batches of metadata sent by the organisation(s).</returns>
        Task<IEnumerable<BatchMetadata>> MetadataOfDocumentsSentByOrganisations(IEnumerable<OrganisationIdentifier> identifiers);

        /// <summary>Get batches contained within the given parent batch ID.</summary>
        /// <param name="parentBatchIdentifier">The parent batch ID.</param>
        /// <returns>A collection of batches that have the given parent.</returns>
        Task<IEnumerable<BatchMetadata>> GetBatchesByParent(string parentBatchIdentifier);

        /// <summary>
        /// Adds a batch notification summary.
        /// </summary>
        /// <param name="batchNotificationSummary">The summary to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddBatchNotificationSummary(BatchNotificationSummary batchNotificationSummary);

        /// <summary>Gets the specified configuration section.</summary>
        /// <typeparam name="TValue">The type of the configuration section.</typeparam>
        /// <param name="section">The configuration section to get.</param>
        /// <returns>The specified configuration section.</returns>
        Task<TValue> GetConfiguration<TValue>(ConfigurationSection section);

        /// <summary>
        /// Updates the specified configuration section with the given new value.
        /// </summary>
        /// <typeparam name="TValue">The type of the configuration section.</typeparam>
        /// <param name="section">The configuration section to update.</param>
        /// <param name="newValue">The new configuration value.</param>
        /// <returns>The updated config value.</returns>
        Task<TValue> UpdateConfiguration<TValue>(ConfigurationSection section, TValue newValue);

        /// <summary>Gets the specified list configuration section.</summary>
        /// <typeparam name="TListItems">The type of the list items in the configuration section.</typeparam>
        /// <param name="section">The configuration section to get.</param>
        /// <returns>The specified configuration section.</returns>
        Task<IEnumerable<TListItems>> GetListConfiguration<TListItems>(ConfigurationSection section);

        /// <summary>
        /// Adds or updates an item in the given list configuration section.
        /// </summary>
        /// <typeparam name="TListItems">The type of the list items in the configuration section.</typeparam>
        /// <typeparam name="TIdentifier">The type of the identifier.</typeparam>
        /// <param name="section">The list configuration section for which to add or update an item.</param>
        /// <param name="oldIdentifier">The old identifier value.</param>
        /// <param name="newValue">The new configuration value.</param>
        /// <returns>The new or updated config list item.</returns>
        Task<TListItems> AddOrUpdateListConfiguration<TListItems, TIdentifier>(ConfigurationSection section, TIdentifier oldIdentifier, TListItems newValue);

        /// <summary>Gets the count of unprocessed files.</summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>An integer value for the number of unprocessed files.</returns>
        Task<int> GetCountOfUnprocessedFiles(string parentBatchIdentifier);

        /// <summary>Gets the current version number.</summary>
        /// <param name="fromIdentifier">From identifier.</param>
        /// <param name="toIdentifier">To identifier.</param>
        /// <param name="year">The academic year.</param>
        /// <param name="fileType">The ID of the file type.</param>
        /// <returns>The current version number as an integer.</returns>
        Task<int> GetCurrentVersionNumber(OrganisationIdentifier fromIdentifier, OrganisationIdentifier toIdentifier, int year, int fileType);

        /// <summary>Gets the report of documents viewed.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<KeyValuePair<string, Dictionary<string, string>>> GetReportOfDocumentsViewed(DateTime from, DateTime to);

        /// <summary>Gets the report of documents viewed summary.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<IEnumerable<ClickSummary>> GetReportOfDocumentsViewedSummary(DateTime from, DateTime to);

        /// <summary>Gets the MI report.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<IEnumerable<MIReport>> GetMIReport(DateTime from, DateTime to);

        /// <summary>Gets the report of file scan bad.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<IEnumerable<ClickDetail>> GetReportOfFileScanBad(DateTime from, DateTime to);

        /// <summary>Gets the report of initial upload errors.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<IEnumerable<ErrorDetail>> GetReportOfInitialUploadErrors(DateTime from, DateTime to);

        /// <summary>Gets the report of files sent from agency.</summary>
        /// <param name="from">The start of the period to look at.</param>
        /// <param name="to">The end of the period to look at.</param>
        /// <returns>An enumerable collection of the data.</returns>
        Task<IEnumerable<ClickDetail>> GetReportOfFilesSentFromAgency(DateTime from, DateTime to);

        /// <summary>
        /// Gets the agency files delete information.
        /// </summary>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="year">The year.</param>
        /// <returns>A collection with the delete files information.</returns>
        Task<IEnumerable<BatchMetadata>> GetAgencyDeleteFilesInfo(int ukprn, string fileType, string year);

        /// <summary>
        /// Gets the organisation files delete information.
        /// </summary>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="fileType">The file type.</param>
        /// <returns>A collection with the delete files information.</returns>
        Task<IEnumerable<BatchMetadata>> GetOrganisationDeleteFilesInfo(int ukprn, string fileType);

        /// <summary>Try and set an exclusive email lock id on the batches.</summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <param name="lockIdentifier">A string to lock on.</param>
        /// <returns>True if the lock was set, false if it wasn't (i.e. something else already had the lock).</returns>
        Task<bool> TryAndSetAnExclusiveEmailLockIdOnTheBatches(string parentBatchIdentifier, string lockIdentifier);

        /// <summary>Updates the document metadata.</summary>
        /// <param name="metadata">The file info.</param>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="fieldsToUpdate">The fields to update.</param>
        /// <param name="existingFileName">The existing file name that can be used to identify the file within the batch.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateDocumentMetadata(FileMetadata metadata, string batchIdentifier, IEnumerable<string> fieldsToUpdate, string existingFileName = null);

        /// <summary>Updates the document metadata and version.</summary>
        /// <param name="metadata">The file info.</param>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="fieldsToUpdate">The fields to update.</param>
        /// <param name="existingFileName">The existing file name that can be used to identify the file within the batch.</param>
        /// <returns>A <see cref="Task"/> returning the calculated document version.</returns>
        Task<int> UpdateDocumentMetadataAndVersion(FileMetadata metadata, string batchIdentifier, IEnumerable<string> fieldsToUpdate, string existingFileName = null);

        /// <summary>
        /// Adds a new documents batch.
        /// </summary>
        /// <param name="batchMetadata">The batch metadata to be stored.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddBatch(BatchMetadata batchMetadata);

        /// <summary>Ensures all stored procedures up to date.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EnsureAllStoredProceduresUpToDate();

        /// <summary>
        /// Gets a list of received files with Seen status for Agency users.
        /// </summary>
        /// <param name="allowedProducts">Allowed products collection.</param>
        /// <returns>A list of received files with Seen status for Agency users.</returns>
        Task<IEnumerable<DocumentWithViewedStatus>> ReceivedFilesForAgencyWithSeenHistory(IEnumerable<Product> allowedProducts);

        /// <summary>
        /// Gets the specified file.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The file metadata.</returns>
        Task<FileMetadata> GetFile(DocumentReference documentReference);

        /// <summary>
        /// Gets the batches which have been published.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>The published batches list result.</returns>
        Task<ListResult<PublishedBatch>> GetPublishedBatches(int pageNumber, int pageSize);

        /// <summary>
        /// Gets the notification recipients by parent batch.
        /// </summary>
        /// <param name="parentBatchId">The parent batch identifier.</param>
        /// <returns>A collection of notification recipients.</returns>
        Task<IEnumerable<NotificationRecipient>> GetNotificationRecipientsByParentBatch(Guid parentBatchId);

        /// <summary>
        /// Adds a new SPI refresh log.
        /// </summary>
        /// <param name="log">The log information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddSpiRefreshLog(SpiRefreshLog log);

        /// <summary>
        /// Gets the SPI refresh logs.
        /// </summary>
        /// <returns>A collection of SPI refresh logs.</returns>
        Task<IEnumerable<SpiRefreshLog>> GetSpiRefreshLogs();
    }
}