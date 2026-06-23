namespace Pds.DocumentExchange.Data.Populator.Models
{
    /// <summary>
    /// The bulk delete result.
    /// </summary>
    internal class BulkDeleteResult
    {
        public int Deleted { get; set; }

        public bool Continuation { get; set; }
    }
}