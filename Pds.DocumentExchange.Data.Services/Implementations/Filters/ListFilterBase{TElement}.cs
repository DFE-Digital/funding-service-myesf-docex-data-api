using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The base class that implements the common list filtering functionality.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public abstract class ListFilterBase<TElement> : IListFilter<TElement>
    {
        private readonly IEnumerable<TElement> _elements;
        private Dictionary<string, IReadOnlyCollection<TElement>> _valueItemsDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilterBase{TElement}"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        protected ListFilterBase(IEnumerable<TElement> elements)
        {
            It.IsNull(elements)
                 .AsGuard<ArgumentNullException>();

            _elements = elements;
        }

        /// <inheritdoc/>
        public abstract string FilterTitle { get; }

        /// <inheritdoc/>
        public abstract string FilterKey { get; }

        /// <inheritdoc/>
        public abstract Func<TElement, Task<string>> GetFilterValueFromElement { get; }

        /// <inheritdoc/>
        public abstract Func<string, Task<string>> GetFilterTitleFromValue { get; }

        /// <inheritdoc/>
        public virtual Func<string, Task<string>> GetFilterCategoryFromValue { get; } = value => Task.FromResult(string.Empty);

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<TElement>> GetElementsByFilterValue(string value)
        {
            var valueItemsDict = await GetValueItemsDictionary();

            return valueItemsDict.TryGetValue(value, out IReadOnlyCollection<TElement> items)
                  ? items
                  : Collection.EmptyAndReadOnly<TElement>();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<TElement>> GetElementsByFilterValue(IEnumerable<string> values)
        {
            var elements = new List<TElement>();

            foreach (var currentValue in values)
            {
                var elementsByValue = await GetElementsByFilterValue(currentValue);
                elements.AddRange(elementsByValue);
            }

            return elements.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetFilterValues()
        {
            var valueItemsDict = await GetValueItemsDictionary();
            return valueItemsDict.Keys;
        }

        /// <inheritdoc/>
        public virtual FilterType FilterType => FilterType.ListFilter;

        /// <inheritdoc/>
        public virtual ListFilterType ListFilterType => ListFilterType.List;

        private async Task<Dictionary<string, IReadOnlyCollection<TElement>>> GetValueItemsDictionary()
        {
            if (_valueItemsDictionary != null)
            {
                return _valueItemsDictionary;
            }

            var tasks = _elements.Select(async element => (Element: element, Value: await GetFilterValueFromElement(element)));
            var entries = await Task.WhenAll(tasks);

            _valueItemsDictionary = entries.GroupBy(entry => entry.Value, entry => entry.Element)
                .ToDictionary(
                    keySelector: group => group.Key,
                    elementSelector: group => Collection.AsSafeReadOnlyList(group));

            return _valueItemsDictionary;
        }
    }
}