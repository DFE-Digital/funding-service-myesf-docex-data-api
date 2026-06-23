using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.Services.Common.Helpers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Comparers
{
    /// <summary>
    /// The organisation identifier equality comparer.
    /// </summary>
    public class ProductEqualityComparer : IEqualityComparer<Product>
    {
        /// <inheritdoc/>
        public bool Equals([AllowNull] Product x, [AllowNull] Product y)
        {
            return !(x == null || y == null)
                && x.Identifier.Equals(y.Identifier)
                && x.Name.Equals(y.Name)
                && x.CanOrganisationsUpload.Equals(y.CanOrganisationsUpload)
                && Enumerable.SequenceEqual(x.AgencyTeams, y.AgencyTeams);
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] Product obj)
        {
            var hash = obj.Identifier.GetHashCode();
            hash += (obj.Name ?? string.Empty).GetHashCode();
            hash += obj.CanOrganisationsUpload.GetHashCode();

            int agencyTeamsHash = 0;
            obj.AgencyTeams.ForEach(team => agencyTeamsHash += (team ?? string.Empty).GetHashCode());
            hash += agencyTeamsHash;

            return hash;
        }
    }
}