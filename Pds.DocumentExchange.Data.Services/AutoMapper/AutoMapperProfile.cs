using AutoMapper;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.AutoMapper.Converters;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.AutoMapper
{
    /// <summary>
    /// Automapper profile for organisation.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperProfile"/> class.
        /// </summary>
        public AutoMapperProfile()
        {
            CreateMap<IEnumerable<Provider>, IEnumerable<Organisation>>()
                .ConvertUsing<FdsOrganisationToOrganisationConverter>();
        }
    }
}
