namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// The possible results of a file scan.
    /// </summary>
    public enum FileSafety
    {
        /// <summary>
        /// Unusual state.
        /// </summary>
        NA = 0,

        /// <summary>
        /// The virus scan was okay, and no virus was found.
        /// </summary>
        Okay = 1,

        /// <summary>
        /// The virus scan found a virus.
        /// </summary>
        Virus = 2,

        /// <summary>
        /// There was an error performing the virus scan.
        /// </summary>
        InternalError = 3,

        /// <summary>
        /// The file was already scanned.
        /// </summary>
        AlreadyScanned = 4
    }
}