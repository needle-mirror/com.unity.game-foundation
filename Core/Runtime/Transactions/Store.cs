using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Maintains Collection of <see cref="BaseTransaction"/> items for a Store and implements methods to retrieve them.
    /// </summary>
    /// <inheritdoc/>
    public class Store : CatalogItem
    {
        [ThreadStatic]
        static List<BaseTransaction> ts_TempStoreItemList;

        static List<BaseTransaction> s_TempStoreItemList
        {
            get
            {
                if (ts_TempStoreItemList is null) ts_TempStoreItemList = new List<BaseTransaction>();
                return ts_TempStoreItemList;
            }
        }

        [ThreadStatic]
        static List<Tag> ts_TempTagList;

        static List<Tag> s_TempTagList
        {
            get
            {
                if (ts_TempTagList is null)
                {
                    ts_TempTagList = new List<Tag>();
                }

                return ts_TempTagList;
            }
        }

        /// <summary>
        ///     Available <see cref="Store"/> <see cref="BaseTransaction"/> items for this <see cref="Store"/>.
        /// </summary>
        internal BaseTransaction[] m_Items;

        /// <summary>
        ///     Tells whether or not this <see cref="Store"/> instance contains the given <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="BaseTransaction"/> instance to find.</param>
        /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
        public bool Contains(BaseTransaction transaction)
        {
            foreach (var candidate in m_Items)
            {
                if (candidate == transaction) return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets all the store items (<see cref="BaseTransaction"/> instances).
        /// </summary>
        /// <param name="target">
        ///     The target collection store items are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items of this store.
        /// </returns>
        public int GetStoreItems(ICollection<BaseTransaction> target = null, bool clearTarget = true)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var storeItem in m_Items)
            {
                count++;
                target?.Add(storeItem);
            }

            return count;
        }

        /// <summary>
        ///     Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set to target
        ///     Collection.
        /// </summary>
        /// <param name="tag">
        ///     Desired <see cref="Tag"/> to be queried by this method.
        /// </param>
        /// <param name="target">
        ///     Collection to place resultant <see cref="BaseTransaction"/> items into.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     Number of <see cref="BaseTransaction"/> items added to the target collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws if <paramref name="tag"/> is <c>true</c>.
        /// </exception>
        int FindStoreItemsInternal(Tag tag, ICollection<BaseTransaction> target = null, bool clearTarget = true)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var storeItem in m_Items)
            {
                if (storeItem.HasTag(tag))
                {
                    count++;
                    target?.Add(storeItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set to target
        ///     Collection.
        /// </summary>
        /// <param name="tags">
        ///     Collection of <see cref="Tag"/> to accept.
        /// </param>
        /// <param name="target">
        ///     Collection to place resultant <see cref="BaseTransaction"/> items into.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     Number of <see cref="BaseTransaction"/> items added to the target collection.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     Throws if tags is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throws if any <see cref="Tag"/> in tags Collection is null.
        /// </exception>
        int FindStoreItemsInternal
            (IEnumerable<Tag> tags, ICollection<BaseTransaction> target = null, bool clearTarget = true)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var tag in tags)
            {
                foreach (var storeItem in m_Items)
                {
                    if (storeItem.HasTag(tag))
                    {
                        count++;
                        target?.Add(storeItem);
                        break;
                    }
                }
            }

            return count;
        }

        /// <summary>
        ///     Adds all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified
        ///     <see cref="Tag"/> set to Collection.
        /// </summary>
        /// <param name="tag">
        ///     Desired <see cref="Tag"/> to be queried by this method.
        /// </param>
        /// <param name="target">
        ///     Collection to place resultant <see cref="BaseTransaction"/> items into.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="BaseTransaction"/> items added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws if <paramref name="tag"/> is null.
        /// </exception>
        public int FindStoreItems(Tag tag, ICollection<BaseTransaction> target = null, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            return FindStoreItemsInternal(tag, target, clearTarget);
        }

        /// <summary>
        ///     Updates Collection of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of
        ///     the specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tags">
        ///     Collection of desired <see cref="Tag"/> to be added to the output Collection.
        /// </param>
        /// <param name="target">
        ///     Collection to place resultant <see cref="BaseTransaction"/> items into.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="BaseTransaction"/> items found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws if tags or target is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Throws if any <see cref="Tag"/> in tags Collection is <c>null</c>.
        /// </exception>
        public int FindStoreItems(IEnumerable<Tag> tags, ICollection<BaseTransaction> target = null, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(tags, nameof(tags));

            foreach (var tag in tags)
            {
                if (tag is null)
                {
                    throw new ArgumentException(
                        $"{nameof(Store)}: A {nameof(Tag)} in parameter list {nameof(tags)} is null when " +
                        "trying to find store items with given tags.");
                }
            }

            return FindStoreItemsInternal(tags, target, clearTarget);
        }

        /// <summary>
        ///     Returns a summary string for this <see cref="Store"/>.
        /// </summary>
        /// <returns>Summary string for this <see cref="Store"/>.</returns>
        public override string ToString()
        {
            return $"{GetType().Name}(Key: '{key}' DisplayName: '{displayName}')";
        }
    }
}
