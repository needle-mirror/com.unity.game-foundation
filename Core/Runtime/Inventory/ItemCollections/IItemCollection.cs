using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Interface for inventory item collections (Lists and Maps).
    /// </summary>
    public interface IItemCollection : IEnumerable<InventoryItem>, IEnumerable
    {
        /// <summary>
        ///     Key used for this <see cref="ItemList"/>.
        ///     Key is read-only and is set when created through <see cref="IInventoryManager"/> or will be assigned
        ///     unique <see cref="Guid"/>.
        /// </summary>
        string id { get; }

        /// <summary>
        ///     Count of <see cref="InventoryItem"/>s in this <see cref="ItemList"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Determines if this <see cref="IItemCollection"/> has been discarded (removed from Game Foundation).
        ///     Items being standard objects, they cannot be destroyed and garbage collected as long as all their
        ///     references are not set to <c>null</c>.
        ///     This property is a way for you to know if the object is still active within Game Foundation.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the item has been removed from Game Foundation.
        /// </returns>
        bool hasBeenDiscarded { get; }

        /// <summary>
        ///     Fill <paramref name="target"/> with all <see cref="InventoryItem"/>s.
        /// </summary>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     Count of <see cref="ItemList"/>s owned by <see cref="IInventoryManager"/>.
        /// </returns>
        int GetItems(ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Check if this <see cref="ItemList"/> contains specified <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> for which to search.
        /// </param>
        /// <returns>
        ///     true if specified <see cref="InventoryItem"/> is contained in this <see cref="ItemList"/>, else false.
        /// </returns>
        bool Contains(InventoryItem inventoryItem);

        /// <summary>
        ///     Finds the <see cref="InventoryItem"/> instances of the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">
        ///     <see cref="InventoryItemDefinition"/> to search for.
        /// </param>
        /// <returns>
        ///     Count of <see cref="InventoryItem"/>s with specified <paramref name="definition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="definition"/> is <c>null</c>.
        /// </exception>
        int Find(InventoryItemDefinition definition);

        /// <inheritdoc cref="Find(InventoryItemDefinition)"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        int Find(InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget = true);

        /// <summary>
        ///     Finds the <see cref="InventoryItem"/> instances using specified <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">
        ///     <see cref="Tag"/> to search for.
        /// </param>
        /// <returns>
        ///     Count of <see cref="InventoryItem"/>s with specified <see cref="Tag"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="tag"/> is <c>null</c>.
        /// </exception>
        int Find(Tag tag);

        /// <inheritdoc cref="Find(Tag)"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        int Find(Tag tag, ICollection<InventoryItem> target, bool clearTarget = true);

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances passing the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">
        ///     The filter the <see cref="InventoryItem"/> instances have to fulfill.
        /// </param>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        int Find(Predicate<InventoryItem> filter);

        /// <inheritdoc cref="Find(Predicate{InventoryItem})"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        int Find(Predicate<InventoryItem> filter, ICollection<InventoryItem> target, bool clearTarget = true);

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances passing the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">
        ///     The filter the <see cref="InventoryItem"/> instances have to fulfill.
        /// </param>
        /// <param name="state">
        ///     The data used bny the filter.
        /// </param>
        /// <typeparam name="TState">
        ///     The type of the <paramref name="state"/>.
        /// </typeparam>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        int Find<TState>(Func<TState, InventoryItem, bool> filter, TState state);

        /// <inheritdoc cref="Find{TState}(Func{TState, InventoryItem, bool}, TState)"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TState">
        ///     The type of the <paramref name="state"/>.
        /// </typeparam>
        int Find<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target,
            bool clearTarget = true);

        /// <summary>
        ///     Count all <see cref="InventoryItem"/>s in this <see cref="IItemCollection"/> (including total quantities
        ///     of any <see cref="StackableInventoryItem"/>s) and return total quantity.
        /// </summary>
        /// <returns>
        ///     Total quantity of all items in <see cref="IItemCollection"/>.
        ///     Non-stackable items count as 1, <see cref="StackableInventoryItem"/>s count as the total stack quantity.
        /// </returns>
        long GetTotalQuantity();

        /// <summary>
        ///     Count all specified <see cref="InventoryItemDefinition"/> in this <see cref="IItemCollection"/>
        ///     (including total quantities of any <see cref="StackableInventoryItem"/>s) and return total quantity.
        /// </summary>
        /// <param name="definition">
        ///     <see cref="InventoryItemDefinition"/> to accumulate total quantity for.
        /// </param>
        /// <returns>
        ///     Total quantity of all items.
        ///     Non-stackable items count as 1, <see cref="StackableInventoryItem"/>s count as the total stack quantity.
        /// </returns>
        long GetTotalQuantity(InventoryItemDefinition definition);

        /// <summary>
        ///     Remove specified <see cref="ItemList"/> item by reference.
        /// </summary>
        /// <param name="item">
        ///     <see cref="InventoryItem"/> to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if successful, else <c>false</c>.
        /// </returns>
        bool Remove(InventoryItem item);

        /// <summary>
        ///     Remove all <see cref="InventoryItem"/>s with specified <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="definition">
        ///     <see cref="InventoryItemDefinition"/>s to find
        ///     and remove.
        /// </param>
        /// <returns>
        ///     true if any <see cref="InventoryItemDefinition"/>s
        ///     were found and removed, else false.
        /// </returns>
        bool Remove(InventoryItemDefinition definition);

        /// <summary>
        ///     Remove all items from this <see cref="IItemCollection"/>.
        /// </summary>
        void Clear();
    }
}
