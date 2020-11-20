using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contains all the runtime <see cref="CatalogItem"/> instances and provide query methods to get them.
    /// </summary>
    public class Catalog
    {
        /// <summary>
        ///     Store all <see cref="CatalogItem"/> available to Game Foundation
        ///     using their <see cref="CatalogItem.key"/>.
        /// </summary>
        internal Dictionary<string, CatalogItem> items { get; }

        /// <summary>
        ///     Create an empty <see cref="Catalog"/>.
        /// </summary>
        public Catalog()
            : this(new Dictionary<string, CatalogItem>()) { }

        /// <summary>
        ///     Create a <see cref="Catalog"/> containing all the given <paramref name="items"/>.
        /// </summary>
        /// <param name="items">
        ///     The collection of <see cref="CatalogItem"/> classified with their <see cref="CatalogItem.key"/>.
        /// </param>
        internal Catalog(Dictionary<string, CatalogItem> items)
        {
            this.items = items;
        }

        /// <summary>
        ///     Looks for a <see cref="CatalogItem"/> instance by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItem"/> to find.
        /// </param>
        /// <typeparam name="TCatalogItem">
        ///     The sub-type of catalog item to get.
        /// </typeparam>
        /// <returns>
        ///     The requested <see cref="CatalogItem"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty or whitespace.
        /// </exception>
        public TCatalogItem Find<TCatalogItem>(string key)
            where TCatalogItem : CatalogItem
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            if (items.TryGetValue(key, out var rawItem)
                && rawItem is TCatalogItem typedItem)
            {
                return typedItem;
            }

            return default;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all <see cref="CatalogItem"/> instances
        ///     of this catalog and returns their count.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="CatalogItem"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItem">
        ///     The sub-type of catalog item to get.
        /// </typeparam>
        /// <returns>
        ///     The number of <see cref="CatalogItem"/> instances of this catalog.
        /// </returns>
        public int GetItems<TCatalogItem>(ICollection<TCatalogItem> target = null, bool clearTarget = true)
            where TCatalogItem : CatalogItem
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var item in items.Values)
            {
                if (item is TCatalogItem typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all <see cref="CatalogItem"/> instances
        ///     of this catalog matching the given <paramref name="tag"/> and returns their count.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> instance used as a filter.
        /// </param>
        /// <param name="target">
        ///     The target container of all the matching <see cref="CatalogItem"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItem">
        ///     Limit the search to items of the specified type.
        /// </typeparam>
        /// <returns>
        ///     The number of <see cref="CatalogItem"/> instances matching the tag filter in this catalog.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> parameter is <c>null</c>.
        /// </exception>
        public int FindItems<TCatalogItem>(Tag tag, ICollection<TCatalogItem> target = null, bool clearTarget = true)
            where TCatalogItem : CatalogItem
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            var count = 0;

            if (clearTarget) target?.Clear();

            foreach (var item in items.Values)
            {
                if (item.HasTag(tag) && item is TCatalogItem typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all <see cref="CatalogItem"/> instances
        ///     of this catalog matching the given <paramref name="tag"/> and returns their count.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> instance used as a filter.
        /// </param>
        /// <param name="target">
        ///     The target container of all the matching <see cref="CatalogItem"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItem">
        ///     Limit the search to items of the specified type.
        /// </typeparam>
        /// <returns>
        ///     The number of <see cref="CatalogItem"/> instances matching the tag filter in this catalog.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> parameter is <c>null</c>.
        /// </exception>
        public int FindItems<TCatalogItem>(Tag tag, ICollection<CatalogItem> target = null, bool clearTarget = true)
            where TCatalogItem : CatalogItem
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            var count = 0;

            if (clearTarget) target?.Clear();

            foreach (var item in items.Values)
            {
                if (item.HasTag(tag) && item is TCatalogItem typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Gets filtered items.
        /// </summary>
        /// <param name="filter">
        ///     The predicate filtering the <see cref="CatalogItem"/> instances.
        /// </param>
        /// <param name="target">
        ///     The target collection the filtered <see cref="CatalogItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of filtered items.
        /// </returns>
        public int FindItems(Predicate<CatalogItem> filter, ICollection<CatalogItem> target = null, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in items.Values)
            {
                if (filter(item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        ///     Gets filtered items.
        /// </summary>
        /// <param name="filter">
        ///     The predicate filtering the <see cref="CatalogItem"/> instances.
        /// </param>
        /// <param name="state">
        ///     An object representing data to be used by the filter.
        /// </param>
        /// <param name="target">
        ///     The target collection the filtered <see cref="CatalogItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TState">
        ///     The type of the data provided to the <paramref name="filter"/>.
        /// </typeparam>
        /// <returns>
        ///     The number of filtered items.
        /// </returns>
        public int FindItems<TState>(
            Func<TState, CatalogItem, bool> filter,
            TState state,
            ICollection<CatalogItem> target = null,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in items.Values)
            {
                if (filter(state, item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        ///     Get a <see cref="BaseTransaction"/> from its product id.
        /// </summary>
        /// <param name="productId">
        ///     The product id of the IAP transaction definition to find.
        /// </param>
        /// <returns>
        ///     If found, returns the <see cref="IAPTransaction"/> object, otherwise null
        /// </returns>
        public IAPTransaction FindIAPTransactionByProductId(string productId)
        {
            Tools.ThrowIfArgNull(productId, nameof(productId));

            var iapTransactions = new List<IAPTransaction>();
            GetItems(iapTransactions);

            foreach (var iapTransaction in iapTransactions)
            {
                if (iapTransaction.productId == productId)
                {
                    return iapTransaction;
                }
            }

            return null;
        }
    }
}
