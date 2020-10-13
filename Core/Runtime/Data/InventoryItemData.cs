using System;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of an <see cref="InventoryItem"/>.
    /// </summary>
    [Serializable]
    public struct InventoryItemData : IEquatable<InventoryItemData>
    {
        /// <summary>
        ///     Unique identifier of the item's definition.
        /// </summary>
        public string definitionKey;

        /// <inheritdoc cref="InventoryItem.id"/>
        public string id;

        /// <summary>
        ///     Quantity in stack (if <see cref="InventoryItem"/> is stackable).
        /// </summary>
        public long quantity;

        /// <summary>
        ///     Item's serializable mutable properties data.
        /// </summary>
        [FormerlySerializedAs("properties")]
        public PropertyData[] mutableProperties;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(InventoryItemData other)
        {
            if (definitionKey != other.definitionKey
                || id != other.id
                || quantity != other.quantity)
            {
                return false;
            }

            if (mutableProperties is null != other.mutableProperties is null) return false;

            if (mutableProperties != null)
            {
                if (mutableProperties.Length == other.mutableProperties.Length) return false;

                for (var i = 0; i < mutableProperties.Length; i++)
                {
                    if (!mutableProperties[i].Equals(other.mutableProperties[i])) return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Tells whether this <see cref="InventoryItemData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is InventoryItemData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="InventoryItemData"/> instance.
        ///     Returns the hash code of its <see cref="id"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="InventoryItemData"/> instance.
        /// </returns>
        public override int GetHashCode() => id?.GetHashCode() ?? 0;
    }
}
