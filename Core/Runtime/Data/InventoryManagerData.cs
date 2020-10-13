using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure that contains the state of the
    ///     <see cref="InventoryItem"/> instances.
    /// </summary>
    [Serializable]
    public struct InventoryManagerData : IEquatable<InventoryManagerData>
    {
        /// <summary>
        ///     The <see cref="InventoryItem"/> instance data.
        /// </summary>
        public InventoryItemData[] items;

        /// <summary>
        ///     The <see cref="ItemList"/> instance data.
        /// </summary>
        public ItemListData[] itemLists;

        /// <summary>
        ///     The <see cref="ItemMap"/> instance data.
        /// </summary>
        public ItemMapData[] itemMaps;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(InventoryManagerData other)
        {
            if (items is null != other.items is null
                || itemLists is null != other.itemLists is null
                || itemMaps is null != other.itemMaps is null)
            {
                return false;
            }

            if (items != null)
            {
                if (items.Length != other.items.Length) return false;

                for (var i = 0; i < items.Length; i++)
                {
                    if (!items[i].Equals(other.items[i])) return false;
                }
            }

            if (itemLists != null)
            {
                if (itemLists.Length != other.itemLists.Length) return false;

                for (var i = 0; i < itemLists.Length; i++)
                {
                    if (!itemLists[i].Equals(other.itemLists[i])) return false;
                }
            }

            if (itemMaps != null)
            {
                if (itemMaps.Length != other.itemMaps.Length) return false;

                for (var i = 0; i < itemMaps.Length; i++)
                {
                    if (!itemMaps[i].Equals(other.itemMaps[i])) return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Tells whether this <see cref="InventoryManagerData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is InventoryManagerData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="InventoryManagerData"/> instance.
        ///     Returns 0.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="InventoryManagerData"/> instance.
        /// </returns>
        public override int GetHashCode() => 0;
    }
}
