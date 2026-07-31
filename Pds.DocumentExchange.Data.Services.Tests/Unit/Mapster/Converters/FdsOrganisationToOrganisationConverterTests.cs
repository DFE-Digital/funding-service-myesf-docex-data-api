using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using Pds.DocumentExchange.Data.Services.Mapster.Converters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Mapster.Converters
{
    [TestClass]
    [TestCategory("Unit")]
    public class FdsOrganisationToOrganisationConverterTests
    {
        private readonly FdsOrganisationToOrganisationConverter _converter = new FdsOrganisationToOrganisationConverter();

        [TestMethod]
        public void Convert_FromNull_ReturnsAPI()
        {
            // Act
            var result = _converter.Convert(null);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Convert_Returns()
        {
            //Arrange
            var provider1 = new Provider()
            {
                Name = "test",
                Ukprn = 1,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = new List<Provider>()
                {
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 2,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    },
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 3,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    }
                }
            };

            var provider2 = new Provider()
            {
                Name = "test",
                Ukprn = 4,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = new List<Provider>()
                {
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 5,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    },
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 6,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    }
                }
            };

            var providers = new List<Provider>();
            providers.Add(provider1);
            providers.Add(provider2);

            var expected = new List<Organisation>()
            {
                new Organisation()
                {
                    Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "1"
                        }
                    },
                    Name = "test",
                    OrganisationType = "ManagementGroup",
                    OrganisationSubType = "test",
                    OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                    OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                    Status = "Open",
                    ParentOrganisation = null,
                    ChildOrganisations = new List<Organisation>()
                    {
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "2"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationSubType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" }
                        },
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "3"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationSubType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" }
                        }
                    }
                },

                new Organisation()
                {
                    Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "4"
                        }
                    },
                    Name = "test",
                    OrganisationType = "ManagementGroup",
                    OrganisationSubType = "test",
                    OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                    OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                    Status = "Open",
                    ParentOrganisation = null,
                    ChildOrganisations = new List<Organisation>()
                    {
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "5"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationSubType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" }
                        },
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "6"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationSubType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" }
                        }
                    }
                }
            };

            // Act
            var result = _converter.Convert(providers);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
