namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Class containing the filter keys used by filtering services.
    /// </summary>
    public enum FilterKey
    {
        /// <summary>
        /// The status filter key.
        /// </summary>
        Status,

        /// <summary>
        /// The product id filter key for a list filter.
        /// </summary>
        ProductIdList,

        /// <summary>
        /// The product id filter key for a radio filter.
        /// </summary>
        ProductIdRadio,

        /// <summary>
        /// The document name error filter key.
        /// </summary>
        DocumentNameError,

        /// <summary>
        /// The academic year filter key.
        /// </summary>
        AcademicYear,

        /// <summary>
        /// The organisation filter key.
        /// </summary>
        Organisation,

        /// <summary>
        /// The team filter key.
        /// </summary>
        Team,

        /// <summary>
        /// The provider type key.
        /// </summary>
        ProviderType,

        /// <summary>
        /// The upload date key.
        /// </summary>
        UploadDate,

        /// <summary>
        /// The UKPRN filter.
        /// </summary>
        Ukprn
    }
}