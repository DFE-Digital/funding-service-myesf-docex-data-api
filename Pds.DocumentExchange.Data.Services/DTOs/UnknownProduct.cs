using System.Linq;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Class representing an unknown product.
    /// </summary>
    public class UnknownProduct : Product
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownProduct"/> class.
        /// </summary>
        public UnknownProduct()
        {
            Identifier = -1;
            Name = "[Unknown product]";
            PluralName = "[Unknown products]";
            AgencyTeams = Enumerable.Empty<string>();
            CanOrganisationsUpload = false;
        }
    }
}