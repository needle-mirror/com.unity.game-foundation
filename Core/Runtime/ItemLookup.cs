using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Int based lookup table for fast item search.
    /// </summary>
    class ItemLookup
    {
        /// <summary>
        ///     Stores the next supposedly available instance id.
        ///     This value is checked when assigning it to a new item.
        /// </summary>
        int m_NextInstanceId = int.MinValue;

        /// <summary>
        ///     All items stored in this lookup table.
        ///     Key: Item's <see cref="InventoryItem.instanceId"/>.
        ///     Value: Stored item.
        /// </summary>
        Dictionary<int, InventoryItem> m_Items;

        /// <summary>
        ///     The number of items stored in this lookup table.
        /// </summary>
        public int Count => m_Items.Count;

        /// <summary>
        ///     The collection of all items stored in this lookup table.
        /// </summary>
        public Dictionary<int, InventoryItem>.ValueCollection Values => m_Items.Values;

        public InventoryItem this[int instanceId] => m_Items[instanceId];

        public ItemLookup(int capacity)
        {
            m_Items = new Dictionary<int, InventoryItem>(capacity);
        }

        /// <summary>
        ///     Add the given <paramref name="item"/> to this lookup table.
        /// </summary>
        /// <param name="item">
        ///     The item to register.
        /// </param>
        /// <returns>
        ///     The instance id of <paramref name="item"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="item"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if an item with the same id is already stored.
        /// </exception>
        public void Add(InventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));

            if (Contains(item.id))
                throw new ArgumentException("An item with the same id has already been added.");

            var instanceId = GetNextInstanceId();

            item.instanceId = instanceId;

            m_Items.Add(instanceId, item);
        }

        /// <summary>
        ///     Remove the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        ///     The item to remove.
        /// </param>
        /// <returns>
        ///     Return true if the given <paramref name="item"/> has been removed;
        ///     return false otherwise.
        /// </returns>
        public bool Remove(InventoryItem item)
        {
            if (ReferenceEquals(item, null))
            {
                return false;
            }

            // remove inventory item from all collections
            item.RemoveFromAllItemCollections();

            // actually remove item from list
            return m_Items.Remove(item.instanceId);
        }

        /// <summary>
        ///     Check if this lookup table contains an item
        ///     with the given <paramref name="instanceId"/>.
        /// </summary>
        /// <param name="instanceId">
        ///     The item's <see cref="InventoryItem.instanceId"/> to search for.
        /// </param>
        /// <returns>
        ///     Return true if this lookup table contains an item
        ///     with the given <paramref name="instanceId"/>;
        ///     return false otherwise.
        /// </returns>
        internal bool Contains(int instanceId)
        {
            return m_Items.ContainsKey(instanceId);
        }

        /// <summary>
        ///     Check if this lookup table contains an item
        ///     with the given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">
        ///     The item's <see cref="InventoryItem.id"/> to search for.
        /// </param>
        /// <returns>
        ///     Return true if this lookup table contains an item
        ///     with the given <paramref name="itemId"/>;
        ///     return false otherwise.
        /// </returns>
        public bool Contains(string itemId)
        {
            foreach (var item in m_Items.Values)
            {
                if (item.id == itemId)
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Check if this lookup table contains the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        ///     The item to search for.
        /// </param>
        /// <returns>
        ///     Return true if this lookup table contains the given <paramref name="item"/>;
        ///     return false otherwise.
        /// </returns>
        public bool Contains(InventoryItem item)
        {
            return m_Items.ContainsKey(item.instanceId);
        }

        /// <summary>
        ///     Remove all items from this lookup table and reset instance id generation.
        /// </summary>
        public void Clear()
        {
            m_Items.Clear();
            m_NextInstanceId = int.MinValue;
        }

        /// <summary>
        ///     Get the item with the given <paramref name="instanceId"/> if any.
        /// </summary>
        /// <param name="instanceId">
        ///     The item's <see cref="InventoryItem.instanceId"/> to search for.
        /// </param>
        /// <param name="value">
        ///     The item with the given <paramref name="instanceId"/> if any.
        /// </param>
        /// <returns>
        ///     Return true if an item with the given <paramref name="instanceId"/>
        ///     exists in this lookup table;
        ///     return false otherwise.
        /// </returns>
        internal bool TryGetValue(int instanceId, out InventoryItem value)
        {
            return m_Items.TryGetValue(instanceId, out value);
        }

        /// <summary>
        ///     Get the item with the given <paramref name="itemId"/> if any.
        /// </summary>
        /// <param name="itemId">
        ///     The item's <see cref="InventoryItem.id"/> to search for.
        /// </param>
        /// <param name="value">
        ///     The item with the given <paramref name="itemId"/> if any.
        /// </param>
        /// <returns>
        ///     Return true if an item with the given <paramref name="itemId"/>
        ///     exists in this lookup table;
        ///     return false otherwise.
        /// </returns>
        public bool TryGetValue(string itemId, out InventoryItem value)
        {
            foreach (var item in m_Items.Values)
            {
                if (item.id == itemId)
                {
                    value = item;

                    return true;
                }
            }

            value = null;

            return false;
        }

        /// <summary>
        ///     Gets the next available instance id.
        /// </summary>
        int GetNextInstanceId()
        {
            while (m_Items.ContainsKey(m_NextInstanceId))
            {
                m_NextInstanceId++;
            }

            var id = m_NextInstanceId++;
            return id;
        }
    }
}
