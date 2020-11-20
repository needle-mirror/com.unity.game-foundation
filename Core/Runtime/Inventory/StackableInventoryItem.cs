using System;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Stackable item handled by the <see cref="IInventoryManager"/>.
    /// </summary>
    public class StackableInventoryItem : InventoryItem, IQuantifiable
    {
        /// <summary>
        ///     Triggered every time a <see cref="StackableInventoryItem"/> quantity is changed.
        /// </summary>
        public static event Action<StackableInventoryItem, long> quantityChanged;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StackableInventoryItem"/> class.
        /// </summary>
        /// <param name="stackableItemDefinition">
        ///     The definition used to create the item.
        ///     It stores all static data.
        /// </param>
        /// <param name="id">
        ///     The unique identifier of this item.
        ///     If <c>null</c>, the constructor will create one itself.
        /// </param>
        /// <param name="quantity">
        ///     Quantity for this stackable inventory item (must be positive).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the given <paramref name="stackableItemDefinition"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal StackableInventoryItem(StackableInventoryItemDefinition stackableItemDefinition,
            string id, long quantity = 1)
            : base(stackableItemDefinition, id)
        {
            m_Quantity = quantity;
        }

        /// <summary>
        ///     Quantity of this <see cref="StackableInventoryItem"/>.
        /// </summary>
        long m_Quantity;

        /// <summary>
        ///     Quantity of this <see cref="StackableInventoryItem"/>.
        /// </summary>
        public long quantity
        {
            get => m_Quantity;
        }

        /// <summary>
        ///     Set quantity for this <see cref="StackableInventoryItem"/> if permissible.
        /// </summary>
        /// <param name="quantity">
        ///     Requested new quantity.
        /// </param>
        /// <returns>
        ///     true if new quantity is valid and set, else false.
        /// </returns>
        public bool SetQuantity(long quantity) => GameFoundationSdk.inventory.SetQuantity(this, quantity);

        /// <summary>
        ///     Set new item quantity.
        ///     Note: internal because InventoryManager should first authorize new quantity.
        /// </summary>
        /// <param name="newQuantity">
        ///     Requested new quantity.
        /// </param>
        internal void SetQuantityInternal(long newQuantity)
        {
            if (newQuantity == m_Quantity)
            {
                return;
            }

            var oldQuantity = m_Quantity;

            m_Quantity = newQuantity;

            quantityChanged?.Invoke(this, oldQuantity);

            // directly tell the inventory manager to bubble up this event
            (GameFoundationSdk.inventory as InventoryManagerImpl)
                ?.HandleStackableQuantityChanged(this, oldQuantity);
        }
    }
}
