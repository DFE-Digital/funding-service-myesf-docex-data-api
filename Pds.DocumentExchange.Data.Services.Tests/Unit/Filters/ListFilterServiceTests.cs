using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Filters.Models;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using It = Moq.It;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class ListFilterServiceTests
    {
        private readonly Mock<IListFilter<Country>> _filterByName = new Mock<IListFilter<Country>>(MockBehavior.Strict);
        private readonly Mock<IListFilter<Country>> _filterByContinent = new Mock<IListFilter<Country>>(MockBehavior.Strict);
        private readonly Mock<IListFilter<Country>> _filterByPopulation = new Mock<IListFilter<Country>>(MockBehavior.Strict);

        private readonly IEnumerable<IListFilter<Country>> _filters;

        private readonly ListFilterService<Country> _filteringService = new ListFilterService<Country>();

        private readonly Country[] _countries = new[]
            {
                new Country
                {
                    Name = "UK",
                    Continent = "Europe",
                    Population = 66000000
                },
                new Country
                {
                    Name = "US",
                    Continent = "America",
                    Population = 328000000
                },
                new Country
                {
                    Name = "Italy",
                    Continent = "Europe",
                    Population = 60000000
                },
                new Country
                {
                    Name = "Germany",
                    Continent = "Europe",
                    Population = 83000000
                },
                new Country
                {
                    Name = "China",
                    Continent = "Asia",
                    Population = 1400050000
                }
            };

        public ListFilterServiceTests()
        {
            InitializeFilterByName();
            InitializeFilterByContinent();
            InitializeFilterByPopulation();

            _filters = new[] { _filterByName.Object, _filterByContinent.Object, _filterByPopulation.Object };
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterListIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var filterByCountryName = new ListFilterOption
            {
                Key = "countryname",
                Values = new string[] { "Italy" }
            };

            // Act
            Func<Task<FilterResult<Country>>> func = () => _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                Collection.Empty<IListFilter<Country>>(),
                new[] { filterByCountryName });

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionContainsNonExistingKey_RemovesKey()
        {
            // Arrange
            var nonExistingOption = new ListFilterOption
            {
                Key = "non-existing-key",
                Values = new string[] { "some-value" }
            };

            var sourceList = Collection.Empty<Country>();

            var filterByCountryName = new ListFilterOption
            {
                Key = "countryname",
                Values = new string[] { "Italy" }
            };

            var filterOptions = new List<ListFilterOption> { nonExistingOption, filterByCountryName };


            _filterByName
                .Setup(f => f.GetFilterValues())
                .ReturnsAsync(Collection.EmptyAndReadOnly<string>());


            var expectedResult = new FilterResult<Country>
            {
                Items = Collection.Empty<Country>(),
                Filters = new[]
                {
                    new ListFilter
                    {
                        Title = "Filter by country name",
                        Key = "countryname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by continent name",
                        Key = "continentname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by population",
                        Key = "population",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    }
                }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                sourceList,
                _filters,
                filterOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenSourceListIsEmpty_ReturnsEmptyListAndFilterWithNoValues()
        {
            // Arrange
            var sourceList = Collection.Empty<Country>();

            var filterByCountryName = new ListFilterOption
            {
                Key = "countryname",
                Values = new string[] { "Italy" }
            };

            _filterByName
                .Setup(f => f.GetFilterValues())
                .ReturnsAsync(Collection.EmptyAndReadOnly<string>());

            _filterByContinent
               .Setup(f => f.GetFilterValues())
               .ReturnsAsync(Collection.EmptyAndReadOnly<string>());

            _filterByPopulation
               .Setup(f => f.GetFilterValues())
               .ReturnsAsync(Collection.EmptyAndReadOnly<string>());

            var expectedResult = new FilterResult<Country>
            {
                Items = Collection.Empty<Country>(),
                Filters = new[]
                {
                    new ListFilter
                    {
                        Title = "Filter by country name",
                        Key = "countryname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by continent name",
                        Key = "continentname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by population",
                        Key = "population",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = Enumerable.Empty<FilterGroup>()
                    }
                }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                sourceList,
                _filters,
                new[] { filterByCountryName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsNull_ReturnSourceListAndFilters()
        {
            // Arrange
            var expectedResult = new FilterResult<Country>
            {
                Items = _countries,
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                null);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsEmpty_ReturnSourceListAndFilters()
        {
            // Arrange
            var expectedResult = new FilterResult<Country>
            {
                Items = _countries,
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                Collection.Empty<ListFilterOption>());

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenNoFilterValueIsSelected_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByCountryName = new ListFilterOption
            {
                Key = "countryname",
                Values = new string[] { }
            };

            var expectedResult = new FilterResult<Country>
            {
                Items = _countries,
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                new[] { filterByCountryName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenSingleValueIsSelected_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByContinentName = new ListFilterOption
            {
                Key = "continentname",
                Values = new string[] { "Europe" }
            };

            var expectedResult = new FilterResult<Country>
            {
                Items = _countries.Where(country => country.Continent == "Europe"),
                Filters = new[]
                {
                    new ListFilter
                    {
                        Title = "Filter by country name",
                        Key = "countryname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = new[]
                        {
                            new FilterGroup
                            {
                                Title = "Asia",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "China",
                                        Value = "China",
                                        Selected = false,
                                        Count = 0,
                                        Category = "Asia"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "America",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "US",
                                        Value = "US",
                                        Selected = false,
                                        Count = 0,
                                        Category = "America"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "Europe",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "Italy",
                                        Value = "Italy",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "Germany",
                                        Value = "Germany",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "UK",
                                        Value = "UK",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    }
                                }
                            }
                        }
                    },
                    new ListFilter
                    {
                        Title = "Filter by continent name",
                        Key = "continentname",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "America",
                                Value = "America",
                                Selected = false,
                                Count = 1,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Asia",
                                Value = "Asia",
                                Selected = false,
                                Count = 1,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Europe",
                                Value = "Europe",
                                Selected = true,
                                Count = 3,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by population",
                        Key = "population",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "Less than 100 million",
                                Value = "less-100-million",
                                Selected = false,
                                Count = 3,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "More than 100 million",
                                Value = "more-100-million",
                                Selected = false,
                                Count = 0,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    }
                }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                new[] { filterByContinentName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenMultipleValuesSelectedInSingleFilter_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByContinentName = new ListFilterOption
            {
                Key = "continentname",
                Values = new string[] { "Europe", "America" }
            };

            var expectedResult = new FilterResult<Country>
            {
                Items = _countries.Where(country => country.Continent == "Europe" || country.Continent == "America"),
                Filters = new[]
                {
                    new ListFilter
                    {
                        Title = "Filter by country name",
                        Key = "countryname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = new[]
                        {
                            new FilterGroup
                            {
                                Title = "Asia",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "China",
                                        Value = "China",
                                        Selected = false,
                                        Count = 0,
                                        Category = "Asia"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "America",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "US",
                                        Value = "US",
                                        Selected = false,
                                        Count = 1,
                                        Category = "America"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "Europe",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "Italy",
                                        Value = "Italy",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "Germany",
                                        Value = "Germany",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "UK",
                                        Value = "UK",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    }
                                }
                            }
                        }
                    },
                    new ListFilter
                    {
                        Title = "Filter by continent name",
                        Key = "continentname",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "America",
                                Value = "America",
                                Selected = true,
                                Count = 1,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Asia",
                                Value = "Asia",
                                Selected = false,
                                Count = 1,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Europe",
                                Value = "Europe",
                                Selected = true,
                                Count = 3,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by population",
                        Key = "population",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "Less than 100 million",
                                Value = "less-100-million",
                                Selected = false,
                                Count = 3,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "More than 100 million",
                                Value = "more-100-million",
                                Selected = false,
                                Count = 1,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    }
                }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                new[] { filterByContinentName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenMultipleValuesSelectedInDifferentFilters_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByCountryName = new ListFilterOption
            {
                Key = "countryname",
                Values = new string[] { "Italy", "Germany", "US" }
            };

            var filterByContinentName = new ListFilterOption
            {
                Key = "continentname",
                Values = new string[] { "Europe", "America" }
            };

            var expectedResult = new FilterResult<Country>
            {
                Items = _countries.Where(country => country.Name == "Italy" || country.Name == "Germany" || country.Name == "US"),
                Filters = new[]
                {
                    new ListFilter
                    {
                        Title = "Filter by country name",
                        Key = "countryname",
                        Values = Enumerable.Empty<FilterValue>(),
                        Groups = new[]
                        {
                            new FilterGroup
                            {
                                Title = "Asia",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "China",
                                        Value = "China",
                                        Selected = false,
                                        Count = 0,
                                        Category = "Asia"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "America",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "US",
                                        Value = "US",
                                        Selected = true,
                                        Count = 1,
                                        Category = "America"
                                    }
                                }
                            },
                            new FilterGroup
                            {
                                Title = "Europe",
                                Values = new[]
                                {
                                    new FilterValue
                                    {
                                        Title = "Italy",
                                        Value = "Italy",
                                        Selected = true,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "Germany",
                                        Value = "Germany",
                                        Selected = true,
                                        Count = 1,
                                        Category = "Europe"
                                    },
                                    new FilterValue
                                    {
                                        Title = "UK",
                                        Value = "UK",
                                        Selected = false,
                                        Count = 1,
                                        Category = "Europe"
                                    }
                                }
                            }
                        }
                    },
                    new ListFilter
                    {
                        Title = "Filter by continent name",
                        Key = "continentname",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "America",
                                Value = "America",
                                Selected = true,
                                Count = 1,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Europe",
                                Value = "Europe",
                                Selected = true,
                                Count = 2,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "Asia",
                                Value = "Asia",
                                Selected = false,
                                Count = 0,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    },
                    new ListFilter
                    {
                        Title = "Filter by population",
                        Key = "population",
                        Values = new[]
                        {
                            new FilterValue
                            {
                                Title = "Less than 100 million",
                                Value = "less-100-million",
                                Selected = false,
                                Count = 2,
                                Category = string.Empty
                            },
                            new FilterValue
                            {
                                Title = "More than 100 million",
                                Value = "more-100-million",
                                Selected = false,
                                Count = 1,
                                Category = string.Empty
                            }
                        },
                        Groups = Enumerable.Empty<FilterGroup>()
                    }
                }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _countries,
                _filters,
                new[] { filterByCountryName, filterByContinentName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private void InitializeFilterByName()
        {
            _filterByName
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by country name");

            _filterByName
                .SetupGet(f => f.FilterKey)
                .Returns("countryname");

            _filterByName
                .SetupGet(f => f.GetFilterTitleFromValue)
                .Returns((countryName) => Task.FromResult(countryName));

            _filterByName
                .Setup(f => f.GetFilterValues())
                .ReturnsAsync(() =>
                {
                    var countryNames = _countries.Select(country => country.Name).ToList();
                    return new ReadOnlyCollection<string>(countryNames);
                });

            _filterByName
               .Setup(f => f.GetElementsByFilterValue(It.IsAny<string>()))
               .ReturnsAsync((string countryName) =>
               {
                   return new ReadOnlyCollection<Country>(GetCountryByName(countryName));
               });

            _filterByName
               .Setup(f => f.GetElementsByFilterValue(It.IsAny<IEnumerable<string>>()))
               .ReturnsAsync((IEnumerable<string> countryNames) =>
               {
                   var countries = countryNames.SelectMany(countryName => GetCountryByName(countryName)).ToList();
                   return new ReadOnlyCollection<Country>(countries);
               });

            _filterByName
                .SetupGet(f => f.ListFilterType)
                .Returns(Enums.ListFilterType.Group);

            _filterByName
                .SetupGet(f => f.GetFilterCategoryFromValue)
                .Returns((string value) =>
                {
                    var category = string.Empty;

                    switch (value)
                    {
                        case "UK":
                        case "Italy":
                        case "Germany":
                            category = "Europe";
                            break;

                        case "US":
                            category = "America";
                            break;

                        case "China":
                            category = "Asia";
                            break;

                        default:
                            break;
                    }

                    return Task.FromResult(category);
                });

            List<Country> GetCountryByName(string countryName)
                => _countries.Where(country => country.Name == countryName).ToList();
        }

        private void InitializeFilterByContinent()
        {
            _filterByContinent
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by continent name");

            _filterByContinent
                .SetupGet(f => f.FilterKey)
                .Returns("continentname");

            _filterByContinent
                .SetupGet(f => f.GetFilterTitleFromValue)
                .Returns((continentName) => Task.FromResult(continentName));

            _filterByContinent
                .Setup(f => f.GetFilterValues())
                .ReturnsAsync(() =>
                {
                    var continentNames = _countries.Select(country => country.Continent).Distinct().ToList();
                    return new ReadOnlyCollection<string>(continentNames);
                });

            _filterByContinent
               .Setup(f => f.GetElementsByFilterValue(It.IsAny<string>()))
               .ReturnsAsync((string continentName) =>
               {
                   return new ReadOnlyCollection<Country>(GetCountryByContinent(continentName));
               });

            _filterByContinent
               .Setup(f => f.GetElementsByFilterValue(It.IsAny<IEnumerable<string>>()))
               .ReturnsAsync((IEnumerable<string> continentNames) =>
               {
                   var countries = continentNames.SelectMany(continentName => GetCountryByContinent(continentName)).ToList();
                   return new ReadOnlyCollection<Country>(countries);
               });

            _filterByContinent
                .SetupGet(f => f.ListFilterType)
                .Returns(Enums.ListFilterType.List);

            _filterByContinent
               .SetupGet(f => f.GetFilterCategoryFromValue)
               .Returns(value => Task.FromResult(string.Empty));

            List<Country> GetCountryByContinent(string continentName)
                => _countries.Where(country => country.Continent == continentName).ToList();
        }

        private void InitializeFilterByPopulation()
        {
            _filterByPopulation
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by population");

            _filterByPopulation
                .SetupGet(f => f.FilterKey)
                .Returns("population");

            _filterByPopulation
                .Setup(f => f.GetFilterValues())
                .ReturnsAsync(() =>
                {
                    var populationValues = new[] { "less-100-million", "more-100-million" };
                    return new ReadOnlyCollection<string>(populationValues);
                });

            _filterByPopulation
                .SetupGet(f => f.GetFilterTitleFromValue)
                .Returns((populationValue) =>
                {
                    var title = populationValue switch
                    {
                        "less-100-million" => "Less than 100 million",
                        _ => "More than 100 million"
                    };

                    return Task.FromResult(title);
                });

            _filterByPopulation
               .Setup(f => f.GetElementsByFilterValue(It.IsAny<string>()))
               .ReturnsAsync((string populationValue) =>
               {
                   return new ReadOnlyCollection<Country>(GetCountryByPopulation(populationValue));
               });

            _filterByPopulation
              .Setup(f => f.GetElementsByFilterValue(It.IsAny<IEnumerable<string>>()))
              .ReturnsAsync((IEnumerable<string> populationValues) =>
              {
                  var countries = populationValues.SelectMany(population => GetCountryByPopulation(population)).ToList();
                  return new ReadOnlyCollection<Country>(countries);
              });

            _filterByPopulation
                .SetupGet(f => f.ListFilterType)
                .Returns(Enums.ListFilterType.List);

            _filterByPopulation
               .SetupGet(f => f.GetFilterCategoryFromValue)
               .Returns(value => Task.FromResult(string.Empty));

            List<Country> GetCountryByPopulation(string populationValue)
            {
                var matches = populationValue switch
                {
                    "less-100-million" => _countries.Where(country => country.Population < 100000000),
                    _ => _countries.Where(country => country.Population >= 100000000)
                };

                return matches.ToList();
            }
        }

        private IEnumerable<ListFilter> GetInitialFiltersResult()
            => new[]
            {
                new ListFilter
                {
                    Title = "Filter by country name",
                    Key = "countryname",
                    Values = Collection.Empty<FilterValue>(),
                    Groups = new[]
                    {
                        new FilterGroup
                        {
                            Title = "Asia",
                            Values = new[]
                            {
                                new FilterValue
                                {
                                    Title = "China",
                                    Value = "China",
                                    Selected = false,
                                    Count = 1,
                                    Category = "Asia"
                                }
                            }
                        },
                        new FilterGroup
                        {
                            Title = "America",
                            Values = new[]
                            {
                                new FilterValue
                                {
                                    Title = "US",
                                    Value = "US",
                                    Selected = false,
                                    Count = 1,
                                    Category = "America"
                                }
                            }
                        },
                        new FilterGroup
                        {
                            Title = "Europe",
                            Values = new[]
                            {
                                new FilterValue
                                {
                                    Title = "Italy",
                                    Value = "Italy",
                                    Selected = false,
                                    Count = 1,
                                    Category = "Europe"
                                },
                                new FilterValue
                                {
                                    Title = "Germany",
                                    Value = "Germany",
                                    Selected = false,
                                    Count = 1,
                                    Category = "Europe"
                                },
                                new FilterValue
                                {
                                    Title = "UK",
                                    Value = "UK",
                                    Selected = false,
                                    Count = 1,
                                    Category = "Europe"
                                }
                            }
                        }
                    }
                },
                new ListFilter
                {
                    Title = "Filter by continent name",
                    Key = "continentname",
                    Values = new[]
                    {
                        new FilterValue
                        {
                            Title = "America",
                            Value = "America",
                            Selected = false,
                            Count = 1,
                            Category = string.Empty
                        },
                        new FilterValue
                        {
                            Title = "Asia",
                            Value = "Asia",
                            Selected = false,
                            Count = 1,
                            Category = string.Empty
                        },
                        new FilterValue
                        {
                            Title = "Europe",
                            Value = "Europe",
                            Selected = false,
                            Count = 3,
                            Category = string.Empty
                        }
                    },
                    Groups = Collection.Empty<FilterGroup>()
                },
                new ListFilter
                {
                    Title = "Filter by population",
                    Key = "population",
                    Values = new[]
                    {
                        new FilterValue
                        {
                            Title = "Less than 100 million",
                            Value = "less-100-million",
                            Selected = false,
                            Count = 3,
                            Category = string.Empty
                        },
                        new FilterValue
                        {
                            Title = "More than 100 million",
                            Value = "more-100-million",
                            Selected = false,
                            Count = 2,
                            Category = string.Empty
                        }
                    },
                    Groups = Collection.Empty<FilterGroup>()
                }
            };
    }
}