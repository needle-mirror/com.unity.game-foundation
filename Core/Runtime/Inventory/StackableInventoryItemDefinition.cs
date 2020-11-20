using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Preset values and rules for an InventoryItem.
    ///     During runtime, it may be useful to refer back to the InventoryItemDefinition for
    ///     the presets and rules, but the values cannot be changed at runtime.
    ///     InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    public class StackableInventoryItemDefinition : InventoryItemDefinition
    {
        internal StackableInventoryItemDefinition() { }

        internal StackableInventoryItemDefinition(IDictionary<string, Property> properties)
            :
            base(properties) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StackableInventoryItem"/> from this
        ///     <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of this item.
        ///     If <c>null</c>, the constructor will create one itself.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal override InventoryItem CreateInventoryItem(string id)
        {
            return new StackableInventoryItem(this, id);
        }

        internal StackableInventoryItem CreateInventoryItem(string id, long quantity = 1)
        {
            return new StackableInventoryItem(this, id, quantity);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StackableInventoryItem"/> from this
        ///     <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="inventoryItemData">
        ///     Values to use to create the new <see cref="InventoryItem"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal override InventoryItem CreateInventoryItem(InventoryItemData inventoryItemData)
        {
            return new StackableInventoryItem(this, inventoryItemData.id, inventoryItemData.quantity);
        }
    }
}
