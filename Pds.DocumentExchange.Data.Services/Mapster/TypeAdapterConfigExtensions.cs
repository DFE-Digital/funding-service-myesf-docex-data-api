using Mapster;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using Pds.DocumentExchange.Data.Services.Mapster.Converters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Mapster
{
    /// <summary>
    /// Class to extend Mapster TypeAdapterConfig to add mappings.
    /// </summary>
    public static class TypeAdapterConfigExtensions
    {
        /// <summary>
        /// Adds mappings to the TypeAdapterConfig.
        /// </summary>
        /// <param name="config">The TypeAdapter config.</param>
        public static TypeAdapterConfig Configure(this TypeAdapterConfig config)
        {
            config
                .ForType<IEnumerable<Provider>, IEnumerable<Organisation>>()
                .MapWith(source =>
                    new FdsOrganisationToOrganisationConverter().Convert(source));
            return config;
        }
    }
}