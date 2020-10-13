using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of an <see cref="ItemList"/>.
    /// </summary>
    [Serializable]
    public struct ItemListData : IEquatable<ItemListData>
    {
        /// <inheritdoc cref="ItemList.id"/>
        public string id;

        /// <summary>
        ///     All <see cref="InventoryItem.id"/>s in this list.
        /// </summary>
        public List<string> inventoryItems;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemListData"/> type.
        /// </summary>
        /// <param name="id">
        ///     The id of the item list.
        /// </param>
        public ItemListData(string id)
        {
            this.id = id;
            inventoryItems = new List<string>();
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ItemListData other)
        {
            if (id != other.id) return false;

            if (inventoryItems is null != other.inventoryItems is null) return false;

            if (inventoryItems != null)
            {
                if (inventoryItems.Count != other.inventoryItems.Count) return false;

                for (var i = 0; i < inventoryItems.Count; i++)
                {
                    if (inventoryItems[i] != other.inventoryItems[i]) return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Tells whether this <see cref="ItemListData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is ItemListData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="ItemListData"/> instance.
        ///     Returns the hash code of its <see cref="id"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="ItemListData"/> instance.
        /// </returns>
        public override int GetHashCode() => id?.GetHashCode() ?? 0;
    }
}
