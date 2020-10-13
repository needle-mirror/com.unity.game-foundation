using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of an <see cref="ItemMap"/>.
    /// </summary>
    [Serializable]
    public struct ItemMapData : IEquatable<ItemMapData>
    {
        /// <inheritdoc cref="ItemMap.id"/>
        public string id;

        /// <summary>
        ///     All <see cref="InventoryItem.id"/>s in this map.
        /// </summary>
        public List<ItemData> inventoryItems;

        /// <summary>
        ///     Construct <see cref="ItemMapData"/> with specified id.
        /// </summary>
        /// <param name="id">
        ///     Id for the new <see cref="ItemMapData"/>.
        /// </param>
        public ItemMapData(string id)
        {
            this.id = id;
            inventoryItems = new List<ItemData>();
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ItemMapData other)
        {
            if (id != other.id) return false;

            if (inventoryItems is null != other.inventoryItems is null) return false;

            if (inventoryItems != null)
            {
                if (inventoryItems.Count != other.inventoryItems.Count) return false;

                for (var i = 0; i < inventoryItems.Count; i++)
                {
                    if (!inventoryItems[i].Equals(other.inventoryItems[i])) return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Tells whether this <see cref="ItemMapData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is ItemMapData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="ItemMapData"/> instance.
        ///     Returns the hash code of its <see cref="id"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="ItemMapData"/> instance.
        /// </returns>
        public override int GetHashCode() => id?.GetHashCode() ?? 0;

        /// <summary>
        ///     Item data for 1 item in the <see cref="ItemMapData"/>.
        /// </summary>
        [Serializable]
        public struct ItemData : IEquatable<ItemData>
        {
            /// <summary>
            ///     Slot used for this item.
            /// </summary>
            public string slot;

            /// <summary>
            ///     <see cref="InventoryItem.id"/> for this item.
            /// </summary>
            public string id;

            /// <summary>
            ///     Create a new <see cref="ItemData"/> for 1 item in
            ///     the <see cref="ItemMapData"/>.
            /// </summary>
            /// <param name="slot">
            ///     Slot to use for this <see cref="InventoryItem"/>.
            /// </param>
            /// <param name="id">
            ///     <see cref="InventoryItem.id"/> for this item.
            /// </param>
            public ItemData(string slot, string id)
            {
                this.slot = slot;
                this.id = id;
            }

            /// <inheritdoc cref="IEquatable{T}"/>
            public bool Equals(ItemData other) => id == other.id && slot == other.slot;

            /// <summary>
            ///     Tells whether this <see cref="ItemData"/> instance equals <paramref name="obj"/>.
            /// </summary>
            /// <param name="obj">
            ///     The other object to compare this instance with.
            /// </param>
            /// <returns>
            ///     <c>true</c> if equals, <c>false</c> otherwise.
            /// </returns>
            public override bool Equals(object obj) => obj is ItemData other && Equals(other);

            /// <summary>
            ///     Gets the hash code of this <see cref="ItemData"/> instance.
            ///     Returns the hash code of its <see cref="id"/>.
            /// </summary>
            /// <returns>
            ///     The hash code of this <see cref="ItemData"/> instance.
            /// </returns>
            public override int GetHashCode() => id?.GetHashCode() ?? 0;
        }
    }
}
