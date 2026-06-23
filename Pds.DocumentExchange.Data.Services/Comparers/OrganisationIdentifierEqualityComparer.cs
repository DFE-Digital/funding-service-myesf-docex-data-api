using Pds.Core.Common.Organisation.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Comparers
{
    /// <summary>
    /// The organisation identifier equality comparer.
    /// </summary>
    public class OrganisationIdentifierEqualityComparer : IEqualityComparer<OrganisationIdentifier>
    {
        /// <inheritdoc/>
        public bool Equals([AllowNull] OrganisationIdentifier x, [AllowNull] OrganisationIdentifier y)
        {
            return !(x == null || y == null) && x.Type.Equals(y.Type) && x.Value.Equals(y.Value);
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] OrganisationIdentifier obj) =>
            obj.Type.GetHashCode() + (obj.Value ?? string.Empty).GetHashCode();
    }
}