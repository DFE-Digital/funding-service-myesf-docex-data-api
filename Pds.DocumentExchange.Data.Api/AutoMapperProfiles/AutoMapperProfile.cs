using AutoMapper;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Api.AutoMapperProfiles.Converters;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.AutoMapperProfiles
{
    /// <summary>
    /// The AutoMapper profile for this Api.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperProfile"/> class.
        /// </summary>
        public AutoMapperProfile()
        {
            CreateMap<Services.DTOs.Product, Models.Product>()
                .ReverseMap();
            CreateMap<Services.DTOs.FileExtensionInfo, Models.FileExtensionInfo>()
                .ReverseMap();
            CreateMap<Services.DTOs.AgencyTeam, Models.AgencyTeam>()
                .ReverseMap();
            CreateMap<Services.DTOs.DocumentReference, Models.DocumentReference>()
                .ReverseMap();
            CreateMap<Services.DTOs.DocumentReferenceWithPreviousVersions, Models.DocumentReferenceWithPreviousVersions>()
                .ReverseMap();

            CreateMap<Services.DTOs.User.UserInfo, Models.UserInfo>()
                .ReverseMap();
            CreateMap<Services.DTOs.User.OrganisationInfo, Models.OrganisationInfo>()
                .ReverseMap();
            CreateMap<OrganisationIdentifier, Models.OrganisationIdentifier>()
                .ReverseMap();

            CreateMap<Services.DTOs.User.UserInfo, Services.DTOs.FileMetadataUser>()
                .ForMember(dest => dest.IsEncrypted, opt => opt.Ignore());
            CreateMap<Services.DTOs.FileMetadataUser, Services.DTOs.User.UserInfo>()
                .ForMember(dest => dest.OrganisationInfo, opt => opt.Ignore())
                .ForMember(dest => dest.IsViewAsProvider, opt => opt.Ignore());

            CreateMap<Services.DTOs.Summary, Models.Summary>()
                .ReverseMap();
            CreateMap<Services.DTOs.FileShareSummary, Models.FileShareSummary>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.IFilter, Models.Filters.IFilter>()
                .Include<Services.DTOs.Filters.ListFilter, Models.Filters.ListFilter>()
                .Include<Services.DTOs.Filters.DateRangeFilter, Models.Filters.DateRangeFilter>()
                .Include<Services.DTOs.Filters.RadioFilter, Models.Filters.RadioFilter>()
                .Include<Services.DTOs.Filters.TextBoxFilter, Models.Filters.TextBoxFilter>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.ListFilter, Models.Filters.ListFilter>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.DateRangeFilter, Models.Filters.DateRangeFilter>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.RadioFilter, Models.Filters.RadioFilter>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.TextBoxFilter, Models.Filters.TextBoxFilter>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.FilterValue, Models.Filters.FilterValue>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.FilterGroup, Models.Filters.FilterGroup>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.RadioFilterValue, Models.Filters.RadioFilterValue>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.IFilterOption, Models.Filters.IFilterOption>()
                .Include<Services.DTOs.Filters.ListFilterOption, Models.Filters.ListFilterOption>()
                .Include<Services.DTOs.Filters.DateRangeFilterOption, Models.Filters.DateRangeFilterOption>()
                .Include<Services.DTOs.Filters.RadioFilterOption, Models.Filters.RadioFilterOption>()
                .Include<Services.DTOs.Filters.TextBoxFilterOption, Models.Filters.TextBoxFilterOption>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.ListFilterOption, Models.Filters.ListFilterOption>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.DateRangeFilterOption, Models.Filters.DateRangeFilterOption>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.RadioFilterOption, Models.Filters.RadioFilterOption>()
                .ReverseMap();
            CreateMap<Services.DTOs.Filters.TextBoxFilterOption, Models.Filters.TextBoxFilterOption>()
                .ReverseMap();

            CreateMap<Services.DTOs.Filters.RadioFilter, Services.DTOs.Filters.RadioFilterOption>()
                .ConvertUsing(source => new Services.DTOs.Filters.RadioFilterOption
                {
                    Type = FilterOptionType.RadioFilterOption.ToString(),
                    Key = source.Key,
                    Value = source.Values.Where(value => value.Selected).Select(value => value.Value).FirstOrDefault()
                });

            CreateMap(typeof(Services.DTOs.ListResult<>), typeof(Models.ListResult<>))
                .ReverseMap();

            CreateMap<Services.DTOs.AgencyDocument, Models.AgencyDocument>()
                .ForMember(dest => dest.FileNameError, opt => opt.MapFrom(src => src.ErrorDescription))
                .ReverseMap();
            CreateMap<Services.Enums.AgencyDocumentValidity, AgencyDocumentValidity>()
                .ReverseMap();

            CreateMap<Services.DTOs.ExchangeDocument, Models.ExchangeDocument>()
                .ReverseMap();
            CreateMap<Services.DTOs.ExchangeDocumentEvent, Models.ExchangeDocumentEvent>()
                .ReverseMap();
            CreateMap<Services.Enums.ExchangeDocumentDirection, ExchangeDocumentDirection>()
                .ReverseMap();

            CreateMap<Services.DTOs.AgencyListDocumentOptions, Models.AgencyListDocumentOptions>()
                .ReverseMap();
            CreateMap<Services.DTOs.ExchangeListDocumentOptions, Models.ExchangeListDocumentOptions>()
                .ReverseMap();
            CreateMap<Services.DTOs.ExchangeListOrganisationDocumentOptions, Models.ExchangeListOrganisationDocumentOptions>()
                .ReverseMap();

            CreateMap<Services.DTOs.AgencyPublishRequest, Models.AgencyPublishRequest>()
                .ReverseMap();
            CreateMap<Services.DTOs.UploadDocumentRequest, Models.UploadDocumentRequest>()
                .ReverseMap();
            CreateMap<Services.DTOs.ExchangeDocumentDownloadRequest, Models.ExchangeDocumentDownloadRequest>()
                .ReverseMap();
            CreateMap<Services.DTOs.ExchangeDocumentDeleteRequest, Models.ExchangeDocumentDeleteRequest>()
                .ReverseMap();

            CreateMap<Services.DTOs.EmailSetting, Models.EmailSetting>()
                .ReverseMap();

            CreateMap<PublishedBatch, Models.SupportTools.PublishedBatch>()
                .ConvertUsing<PublishedBatchConverter>();

            CreateMap<IEnumerable<BatchMetadata>, IEnumerable<Models.SupportTools.PublishedDocument>>()
                .ConvertUsing<BatchesToPublishedDocumentsConverter>();

            CreateMap<Models.SupportTools.SpiRefreshLogRequest, SpiRefreshLog>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.PartitionKey, opt => opt.Ignore())
                .ForMember(x => x.DocumentType, opt => opt.Ignore())
                .ForMember(x => x.ActionDateTimeUtc, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SpiRefreshLog, Models.SupportTools.SpiRefreshLogResponse>()
                .ReverseMap();
        }
    }
}