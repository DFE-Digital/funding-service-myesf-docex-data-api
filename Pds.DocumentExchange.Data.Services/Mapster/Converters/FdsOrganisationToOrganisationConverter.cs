using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Mapster.Converters
{
    /// <summary>
    /// The fds api provider response to organisation converter.
    /// </summary>
    public class FdsOrganisationToOrganisationConverter
    {
        private const string OtherType = "OtherType";
        private const string OtherTypeDisplayValue = "Other types";
        private const string Miscellaneous = "Miscellaneous";
        private const string MgntGroupOrganisation = "ManagementGroup";

        public IEnumerable<Organisation> Convert(IEnumerable<Provider> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<Organisation>();
            }

            return source.Select(org => new Organisation
            {
                Identifiers = GetIdentifiers(org.Ukprn, org.CompanyHouseNumber),
                Name = org.Name,
                OrganisationType = org.ManagementGroup == null ? MgntGroupOrganisation : org.Type ?? OtherType,
                OrganisationTypeDisplay = org.ManagementGroup == null ? GetOrganisationDisplayValues(MgntGroupOrganisation) : GetOrganisationDisplayValues(org.Type ?? OtherTypeDisplayValue),
                OrganisationSubType = org.ManagementGroup == null ? org.Type ?? Miscellaneous : org.SubType ?? Miscellaneous,
                OrganisationSubTypeDisplay = org.ManagementGroup == null ? GetOrganisationDisplayValues(org.Type ?? Miscellaneous) : GetOrganisationDisplayValues(org.SubType ?? Miscellaneous),
                Status = org.Status,
                ParentOrganisation = GetParentOrganisation(org.ManagementGroup),
                ChildOrganisations = GetChildOrganisations(org.Child)
            });
        }

        private static Organisation GetParentOrganisation(Provider org)
        {
            if (org == null)
            {
                return null;
            }

            return new Organisation()
            {
                Identifiers = GetIdentifiers(org.Ukprn, org.CompanyHouseNumber),
                Name = org.Name,
                OrganisationType = MgntGroupOrganisation,
                OrganisationTypeDisplay = GetOrganisationDisplayValues(MgntGroupOrganisation),
                OrganisationSubType = org.Type ?? Miscellaneous,
                OrganisationSubTypeDisplay = GetOrganisationDisplayValues(org.Type ?? Miscellaneous),
                Status = org.Status
            };
        }

        private static IEnumerable<Organisation> GetChildOrganisations(IEnumerable<Provider> childProviders)
        {
            if (childProviders == null)
            {
                return Enumerable.Empty<Organisation>();
            }

            return childProviders.Select(org => new Organisation
            {
                Identifiers = GetIdentifiers(org.Ukprn, org.CompanyHouseNumber),
                Name = org.Name,
                OrganisationType = org.Type ?? OtherType,
                OrganisationTypeDisplay = GetOrganisationDisplayValues(org.Type ?? OtherTypeDisplayValue),
                OrganisationSubType = org.SubType ?? Miscellaneous,
                OrganisationSubTypeDisplay = GetOrganisationDisplayValues(org.SubType ?? Miscellaneous),
                Status = org.Status
            });
        }

        private static IEnumerable<OrganisationIdentifier> GetIdentifiers(int? ukprn, string companyHouseNumber)
        {
            var identifiers = new List<OrganisationIdentifier>();

            if (ukprn.GetValueOrDefault(0) != 0)
            {
                identifiers.Add(
                    new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = ukprn.ToString()
                    });
            }

            if (!string.IsNullOrEmpty(companyHouseNumber))
            {
                identifiers.Add(
                    new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.CompanyRegistrationNumber,
                        Value = companyHouseNumber
                    });
            }

            return identifiers;
        }

        private static DisplayValues GetOrganisationDisplayValues(string orgDisplayText)
        {
            return new DisplayValues
            {
                Plural = orgDisplayText.Equals(MgntGroupOrganisation) ? "Parent organisations" : orgDisplayText,
                Singular = orgDisplayText.Equals(MgntGroupOrganisation) ? "Parent organisation" : orgDisplayText
            };
        }
    }
}