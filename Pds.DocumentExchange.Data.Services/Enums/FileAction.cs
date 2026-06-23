using System.Runtime.Serialization;

namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// An event that has happened to a file.
    /// </summary>
    public enum FileAction
    {
        /// <summary>
        /// [Not used].
        /// </summary>
        NA = 0,

        /// <summary>
        /// File was viewed by the sender.
        /// </summary>
        ViewedBySender = 1,

        /// <summary>
        /// File was viewed by the receiver.
        /// </summary>
        [EnumMember(Value = "ViewedByReciever")]
        ViewedByReceiver = 2,

        /// <summary>
        /// File was replaced.
        /// </summary>
        Replaced = 3,

        /// <summary>
        /// File passed a virus scan.
        /// </summary>
        FileScanGood = 4,

        /// <summary>
        /// File failed a virus scan.
        /// </summary>
        FileScanBad = 5,

        /// <summary>
        /// File was uploaded by an external user.
        /// </summary>
        UploadedExternal = 6,

        /// <summary>
        /// [Not used].
        /// </summary>
        ErrorPrePublish = 7,

        /// <summary>
        /// File was published (by an internal user).
        /// </summary>
        Published = 8,

        /// <summary>
        /// The notification email was sent.
        /// </summary>
        EmailSent = 9,

        /// <summary>
        /// There was an error.
        /// </summary>
        Error = 10,

        /// <summary>
        /// The file was about to be copied.
        /// </summary>
        Copying = 11,

        /// <summary>
        /// The file was about to be moved.
        /// </summary>
        Moving = 12,

        /// <summary>
        /// The file was about to be deleted.
        /// </summary>
        Deleting = 13,

        /// <summary>
        /// The file was scanned.
        /// </summary>
        FileScanned = 14,

        /// <summary>
        /// The view as provider download sent.
        /// </summary>
        ViewAsProviderDownloadedSent = 15,

        /// <summary>
        /// The view as provider download received.
        /// </summary>
        ViewAsProviderDownloadedReceived = 16,

        /// <summary>
        /// The file was deleted by a user.
        /// </summary>
        UserDeleted = 17
    }
}