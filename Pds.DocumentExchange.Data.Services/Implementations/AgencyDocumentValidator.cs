using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The validation service for agency documents.
    /// </summary>
    public class AgencyDocumentValidator : IAgencyDocumentValidator
    {
        private const int UkprnMinLength = 8;
        private const int FileNamePartsNumber = 3;

        private readonly Regex _fileNameRegex = new Regex(@"^[\w\.]+$", RegexOptions.Compiled);

        private readonly IProductsLookup _productsLookup;
        private readonly IConfigurationDataService _configurationDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyDocumentValidator"/> class.
        /// </summary>
        /// <param name="productsLookup">The products lookup.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        public AgencyDocumentValidator(
            IProductsLookup productsLookup,
            IConfigurationDataService configurationDataService)
        {
            _productsLookup = productsLookup;
            _configurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public async Task<AgencyDocumentErrorType> ValidateDocument(string team, string fileName, IDictionary<string, Organisation> organisations)
        {
            It.IsEmpty(team)
                .AsGuard<ArgumentNullException>();

            var errorType = ValidateFileNameFormat(fileName);
            if (IsError(errorType))
            {
                return errorType;
            }

            var fileNameParts = GetFileNameParts(fileName);

            var organisationId = fileNameParts[(int)AgencyDocumentNamePart.OrganisationId];
            errorType = ValidateOrganisationIdentifier(organisationId, organisations);

            if (IsError(errorType))
            {
                return errorType;
            }

            var productId = fileNameParts[(int)AgencyDocumentNamePart.ProductId];
            errorType = await ValidateProductIdentifier(productId);

            if (IsError(errorType))
            {
                return errorType;
            }

            var academicYear = fileNameParts[(int)AgencyDocumentNamePart.AcademicYear];
            errorType = ValidateAcademicYear(academicYear);

            if (IsError(errorType))
            {
                return errorType;
            }

            errorType = await ValidateFileExtension(fileName);
            if (IsError(errorType))
            {
                return errorType;
            }

            errorType = await ValidateTeamCanPublish(team, fileNameParts[1]);
            if (IsError(errorType))
            {
                return errorType;
            }

            return errorType;

            bool IsError(AgencyDocumentErrorType errorType) =>
                errorType != AgencyDocumentErrorType.NoError;
        }

        private AgencyDocumentErrorType ValidateFileNameFormat(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return AgencyDocumentErrorType.DocumentNameInvalidFormat;
            }

            if (!_fileNameRegex.IsMatch(fileName))
            {
                return AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters;
            }

            var fileNameParts = GetFileNameParts(fileName);

            if (fileNameParts.Length != FileNamePartsNumber || fileNameParts.Any(string.IsNullOrWhiteSpace))
            {
                return AgencyDocumentErrorType.DocumentNameInvalidFormat;
            }

            return AgencyDocumentErrorType.NoError;
        }

        private string[] GetFileNameParts(string fileName)
        {
            var fileWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            return fileWithoutExtension.Split('_');
        }

        private AgencyDocumentErrorType ValidateOrganisationIdentifier(string organisationId, IDictionary<string, Organisation> organisations)
        {
            if (string.IsNullOrEmpty(organisationId) || organisationId.Length < UkprnMinLength)
            {
                return AgencyDocumentErrorType.OrganisationIdentifierInvalidFormat;
            }

            var isOrganisationExists = organisations.ContainsKey(organisationId);

            if (!isOrganisationExists)
            {
                return AgencyDocumentErrorType.OrganisationIdentifierNotRecognised;
            }

            return AgencyDocumentErrorType.NoError;
        }

        private async Task<AgencyDocumentErrorType> ValidateProductIdentifier(string productIdentifier)
            => !await _productsLookup.Exists(productIdentifier)
                ? AgencyDocumentErrorType.ProductIdentifierInvalidFormat
                : AgencyDocumentErrorType.NoError;

        private AgencyDocumentErrorType ValidateAcademicYear(string academicYear)
        {
            if (string.IsNullOrWhiteSpace(academicYear)
                || !int.TryParse(academicYear, out _))
            {
                return AgencyDocumentErrorType.AcademicYearInvalidFormat;
            }

            return AgencyDocumentErrorType.NoError;
        }

        private async Task<AgencyDocumentErrorType> ValidateFileExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                return AgencyDocumentErrorType.DocumentExtensionNotAccepted;
            }

            var fileExtensions = await _configurationDataService.GetFileExtensions();

            var extensionWithoutDot = extension.Replace(".", string.Empty);
            var isExtensionAllowed = fileExtensions.Any(ext => ext.CanInternalUserUpload && ext.Extension.IsEqualToIgnoreCase(extensionWithoutDot));

            if (!isExtensionAllowed)
            {
                return AgencyDocumentErrorType.DocumentExtensionNotAccepted;
            }

            return AgencyDocumentErrorType.NoError;
        }

        private async Task<AgencyDocumentErrorType> ValidateTeamCanPublish(string team, string productIdentifier)
        {
            var fileProduct = await _productsLookup.Get(productIdentifier);

            var isTeamAllowedToPublishProduct = fileProduct.AgencyTeams.Contains(team);
            if (!isTeamAllowedToPublishProduct)
            {
                return AgencyDocumentErrorType.TeamNotAuthorisedToPublish;
            }

            return AgencyDocumentErrorType.NoError;
        }
    }
}