using Mapster;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Mapster.Converters;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Mapster.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Mapster
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
            TypeAdapterConfig.GlobalSettings.AllowImplicitSourceInheritance = true;
            config.Default.PreserveReference(true);
            config.Default.EnumMappingStrategy(EnumMappingStrategy.ByName);
            config.Default.AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull);

            config
                .ForType<Services.DTOs.User.UserInfo, FileMetadataUser>()
                .Ignore(dest => dest.IsEncrypted);
            config
                .ForType<FileMetadataUser, Services.DTOs.User.UserInfo>()
                .Ignore(dest => dest.OrganisationInfo)
                .Ignore(dest => dest.IsViewAsProvider);

            config
                .ForType<Services.DTOs.Filters.IFilter, Models.Filters.IFilter>()
                .TwoWays()
                .Include<Services.DTOs.Filters.ListFilter, Models.Filters.ListFilter>()
                .Include<Services.DTOs.Filters.DateRangeFilter, Models.Filters.DateRangeFilter>()
                .Include<Services.DTOs.Filters.RadioFilter, Models.Filters.RadioFilter>()
                .Include<Services.DTOs.Filters.TextBoxFilter, Models.Filters.TextBoxFilter>();

            config
                .ForType<Models.Filters.IFilterOption, Services.DTOs.Filters.IFilterOption>()
                .TwoWays()
                .Include<Models.Filters.ListFilterOption, Services.DTOs.Filters.ListFilterOption>()
                .Include<Models.Filters.DateRangeFilterOption, Services.DTOs.Filters.DateRangeFilterOption>()
                .Include<Models.Filters.RadioFilterOption, Services.DTOs.Filters.RadioFilterOption>()
                .Include<Models.Filters.TextBoxFilterOption, Services.DTOs.Filters.TextBoxFilterOption>();

            config
                .ForType<Services.DTOs.Filters.RadioFilter, Services.DTOs.Filters.RadioFilterOption>()
                .ConstructUsing(source => new Services.DTOs.Filters.RadioFilterOption
                {
                    Type = FilterOptionType.RadioFilterOption.ToString(),
                    Key = source.Key,
                    Value = source.Values.Where(value => value.Selected).Select(value => value.Value).FirstOrDefault()
                });

            config
                .ForType<Services.DTOs.AgencyDocument, Models.AgencyDocument>()
                .TwoWays()
                .Map(dest => dest.FileNameError, source => source.ErrorDescription);

            config
                .ForType<PublishedBatch, Models.SupportTools.PublishedBatch>()
                .MapWith(source =>
                    new PublishedBatchConverter(new FileMetadataUserEncryptor(new EncryptionService(), new CosmosDbConfiguration())).Convert(source));

            config
                .ForType<IEnumerable<Services.DTOs.BatchMetadata>, IEnumerable<Models.SupportTools.PublishedDocument>>()
                .MapWith(source =>
                    new BatchesToPublishedDocumentsConverter().Convert(source));

            config
                .ForType<Models.SupportTools.SpiRefreshLogRequest, SpiRefreshLog>()
                .TwoWays()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PartitionKey)
                .Ignore(dest => dest.DocumentType)
                .Ignore(dest => dest.ActionDateTimeUtc);

            config
                .ForType<IEnumerable<Provider>, IEnumerable<Organisation>>()
                .MapWith(source =>
                    new FdsOrganisationToOrganisationConverter().Convert(source));

            return config;
        }
    }
}