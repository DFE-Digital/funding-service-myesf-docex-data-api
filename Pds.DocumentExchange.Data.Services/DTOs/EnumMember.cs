namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// A serializable structure for storing an enum member's value and name.
    /// </summary>
    public class EnumMember
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}