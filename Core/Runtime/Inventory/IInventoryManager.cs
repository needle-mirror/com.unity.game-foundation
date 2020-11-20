using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Manages all <see cref="InventoryItem"/>s and <see cref="IItemCollection"/>s.
    ///     Can subscribe to events relevant to <see cref="InventoryItem"/>s and create and delete them here.
    ///     The <see cref="IInventoryManager"/> can create <see cref="InventoryItem"/>s from
    ///     <see cref="InventoryItemDefinition"/>s as well as <see cref="IItemCollection"/>s as needed.
    /// </summary>
    public interface IInventoryManager
    {
        /// <summary>
        ///     Event fired whenever a new Inventory Item is added.
        /// </summary>
        event Action<InventoryItem> itemAdded;

        /// <summary>
        ///     Event fired whenever an Inventory Item is deleted.
        /// </summary>
        event Action<InventoryItem> itemDeleted;

        /// <summary>
        ///     Event fired whenever a <see cref="StackableInventoryItem"/> quantity changes.
        /// </summary>
        event Action<IQuantifiable, long> itemQuantityChanged;

        /// <summary>
        ///     Event fired whenever any <see cref="InventoryItem"/> <see cref="Property"/> changes.
        /// </summary>
        event Action<PropertyChangedEventArgs> itemMutablePropertyChanged;

        /// <summary>
        ///     Event fired whenever any <see cref="IItemCollection"/> is added.
        /// </summary>
        event Action<IItemCollection> collectionAdded;

        /// <summary>
        ///     Event fired whenever any <see cref="IItemCollection"/> is deleted.
        /// </summary>
        event Action<IItemCollection> collectionDeleted;

        /// <summary>
        ///     Event fired whenever an <see cref="InventoryItem"/> is added to an
        ///     <see cref="IItemCollection"/>.
        /// </summary>
        event Action<IItemCollection, InventoryItem> itemAddedToCollection;

        /// <summary>
        ///     Event fired whenever an <see cref="InventoryItem"/> is removed from an
        ///     <see cref="IItemCollection"/>.
        /// </summary>
        event Action<IItemCollection, InventoryItem> itemRemovedFromCollection;

        /// <summary>
        ///     This will create a new <see cref="InventoryItem"/> based on the given
        ///     <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <returns>
        ///     The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        InventoryItem CreateItem(InventoryItemDefinition definition);

        /// <summary>
        ///     This will create a new <see cref="StackableInventoryItem"/> based on the given
        ///     <see cref="StackableInventoryItemDefinition"/>.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="StackableInventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <param name="quantity">
        ///     Quantity of the newly-created stackable item which defaults to 1.
        /// </param>
        /// <returns>
        ///     The newly created item based on specified <see cref="StackableInventoryItemDefinition"/>.
        /// </returns>
        StackableInventoryItem CreateItem(StackableInventoryItemDefinition definition, long quantity = 1);

        /// <summary>
        ///     Create an empty <see cref="ItemList"/>.
        /// </summary>
        /// <returns>
        ///     Empty <see cref="ItemList"/> to store <see cref="InventoryItem"/>s.
        /// </returns>
        ItemList CreateList();

        /// <summary>
        ///     Create an empty <see cref="ItemMap"/>.
        /// </summary>
        /// <returns>
        ///     Empty <see cref="ItemMap"/> to store <see cref="InventoryItem"/>s.
        /// </returns>
        ItemMap CreateMap();

        /// <summary>
        ///     Fills the given list with all items in the manager.
        /// </summary>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        int GetItems(ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Fills target <see cref="ICollection{IItemCollection}"/> with all <see cref="IItemCollection"/>s.
        /// </summary>
        /// <param name="target">
        ///     The target collection the <see cref="IItemCollection"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     Count of <see cref="IItemCollection"/>s found.
        /// </returns>
        int GetCollections(ICollection<IItemCollection> target = null, bool clearTarget = true);

        /// <summary>
        ///     Returns an item with the Id wanted.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item wanted.
        /// </param>
        /// <returns>
        ///     Returns the item with the given <paramref name="id"/> if it exists; return <c>null</c> otherwise.
        /// </returns>
        InventoryItem FindItem(string id);

        /// <summary>
        ///     Gets items filtered with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">
        ///     The tag used to filter the items.
        /// </param>
        /// <param name="target">
        ///     The target collection where filtered items are copied.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="tag"/> is <c>null</c>.
        /// </exception>
        int FindItems(Tag tag, ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> used to filter the items.
        /// </param>
        /// <param name="target">
        ///     The target collection where filtered items are copied.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="InventoryItem"/> instances found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="definition"/> is <c>null</c>.
        /// </exception>
        int FindItems(InventoryItemDefinition definition, ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Gets filtered items.
        /// </summary>
        /// <param name="filter">
        ///     The predicate to filter the items.
        /// </param>
        /// <param name="target">
        ///     The target collection the filtered items are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items passing the filter.
        /// </returns>
        int FindItems(Predicate<InventoryItem> filter, ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Gets filtered items.
        /// </summary>
        /// <typeparam name="TState">
        ///     The type of the <paramref name="state"/> parameter.
        /// </typeparam>
        /// <param name="filter">
        ///     The predicate to filter the items.
        /// </param>
        /// <param name="state">
        ///     An object representing data to be used by the filter.
        /// </param>
        /// <param name="target">
        ///     The target collection the filtered items are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items passing the filter.
        /// </returns>
        int FindItems<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target = null,
            bool clearTarget = true);

        /// <summary>
        ///     Finds collection of specified type using id.
        /// </summary>
        /// <typeparam name="TCollection">
        ///     Type of collection to search for.
        /// </typeparam>
        /// <param name="id">
        ///     Id to search for.
        /// </param>
        /// <returns>
        ///     <see cref="IItemCollection"/> found or null if collection does not exist.
        /// </returns>
        TCollection FindCollection<TCollection>(string id) where TCollection : class, IItemCollection;

        /// <summary>
        ///     Deletes specified <see cref="IItemCollection"/>.
        /// </summary>
        /// <param name="collection">
        ///     <see cref="IItemCollection"/> to delete from <see cref="IInventoryManager"/>.
        /// </param>
        /// <returns>
        ///     true if item map is found and deleted, else false.
        /// </returns>
        bool Delete(IItemCollection collection);

        /// <summary>
        ///     This method will delete the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        ///     The Item instance we want to delete.
        /// </param>
        /// <returns>
        ///     Whether or not the Item was successfully deleted.
        /// </returns>
        bool Delete(InventoryItem item);

        /// <summary>
        ///     This method will delete the items that uses the given <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">
        ///     The InventoryItemDefinition we want to delete.
        /// </param>
        /// <param name="target">
        ///     The target collection where deleted items are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The amount of items that were deleted.
        /// </returns>
        int Delete
            (InventoryItemDefinition definition, ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Deletes all <see cref="IItemCollection"/>s.
        /// </summary>
        /// <param name="target">
        ///     The target collection te deleted collections are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it cleats the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of collections deleted.
        /// </returns>
        int DeleteAllCollections(ICollection<IItemCollection> target = null, bool clearTarget = true);

        /// <summary>
        ///     Deletes all the items from the player inventory.
        /// </summary>
        /// <param name="target">
        ///     The target collection the deleted items are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items deleted.
        /// </returns>
        int DeleteAllItems(ICollection<InventoryItem> target = null, bool clearTarget = true);

        /// <summary>
        ///     Get specified <see cref="StackableInventoryItem"/>'s quantity.
        /// </summary>
        /// <param name="item">
        ///     <see cref="StackableInventoryItem"/> to get quantity of.
        /// </param>
        /// <returns>
        ///     Quantity of items in <see cref="StackableInventoryItem"/>.
        /// </returns>
        long GetQuantity(StackableInventoryItem item);

        /// <summary>
        ///     Set specified <see cref="StackableInventoryItem"/>'s quantity.
        ///     Ensures new quanitity is permissible before setting; returns false on failure.
        /// </summary>
        /// <param name="item">
        ///     <see cref="StackableInventoryItem"/> to change quantity upon.
        /// </param>
        /// <param name="quantity">
        ///     Requested new quantity.
        /// </param>
        /// <returns>
        ///     <c>true</c> if quantity change is permissible and changed, else <c>false</c>.
        /// </returns>
        bool SetQuantity(StackableInventoryItem item, long quantity);

        /// <summary>
        ///     Iterate inventory and return total quantity of specified <see cref="InventoryItemDefinition"/>.
        ///     Note: if items are <see cref="StackableInventoryItem"/> then total of all items in all stacks is
        ///     calculated.
        /// </summary>
        /// <param name="itemDefinition">
        ///     <see cref="InventoryItemDefinition"/> to total.
        /// </param>
        /// <returns>
        ///     Total quantity of specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        long GetTotalQuantity(InventoryItemDefinition itemDefinition);
    }
}
