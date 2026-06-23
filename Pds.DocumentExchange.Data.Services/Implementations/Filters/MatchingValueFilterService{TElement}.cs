using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The class implementing the methods to filter elements by applying multiple date filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public abstract class MatchingValueFilterService<TElement>
    {
        /// <summary>
        /// Filters the elements collection by the specified filters.
        /// </summary>
        /// <param name="elements">The elements collection.</param>
        /// <param name="filters">The collection of filters.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <returns>The filtered collection of elements.</returns>
        protected async Task<IEnumerable<TElement>> Filter(
            IEnumerable<TElement> elements,
            IEnumerable<IMatchingValueFilter<TElement>> filters,
            IEnumerable<ISingleValueFilterOption> filterOptions)
        {
            if (It.IsEmpty(filterOptions) || filterOptions.All(option => string.IsNullOrWhiteSpace(option.Value)))
            {
                return elements;
            }

            var elementsByFilterOption = new List<IEnumerable<TElement>>();
            var applicableFilterOptions = filterOptions.Where(option => !string.IsNullOrWhiteSpace(option.Value));

            foreach (var currentFilterOption in applicableFilterOptions)
            {
                var matchingElements = Collection.Empty<TElement>();
                var filter = filters.Single(filter => filter.FilterKey.IsEqualToIgnoreCase(currentFilterOption.Key));

                foreach (var currentElement in elements)
                {
                    if (await filter.DoesElementMatchValue(currentElement, currentFilterOption.Value))
                    {
                        matchingElements.Add(currentElement);
                    }
                }

                if (matchingElements.Any())
                {
                    elementsByFilterOption.Add(matchingElements);
                }
                else
                {
                    return Collection.Empty<TElement>();
                }
            }

            return elementsByFilterOption.Any()
                ? elementsByFilterOption.Aggregate((x, y) => x.Intersect(y))
                : Collection.Empty<TElement>();
        }
    }
}