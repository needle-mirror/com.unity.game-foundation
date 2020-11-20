using System;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects providing data to the <see cref="IInventoryManager"/>.
    /// </summary>
    public interface IInventoryDataLayer
    {
        /// <summary>
        ///     Get InventoryManager's serializable data.
        /// </summary>
        /// <returns>
        ///     The player's data for the <see cref="IInventoryManager"/>.
        /// </returns>
        InventoryManagerData GetData();

        /// <summary>
        ///     Request to create a new item with the given <paramref name="key"/> and <paramref name="id"/>.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the definition used to create the item.
        /// </param>
        /// <param name="id">
        ///     Identifier to give to the created item.
        /// </param>
        /// <param name="quantity">
        ///     Quantity of items if stackable, else 1 for non-stackable.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void CreateItem(string key, string id, long quantity, Completer completer);

        /// <summary>
        ///     Request to set quantity of item matching the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item we want to delete.
        /// </param>
        /// <param name="quantity">
        ///     New quantity of item specified.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void SetQuantity(string id, long quantity, Completer completer);

        /// <summary>
        ///     Request to delete the item matching the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item we want to delete.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void DeleteItem(string id, Completer completer);

        /// <summary>
        ///     Request to create a new item list with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     Identifier to give to the created item.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void CreateItemList(string id, Completer completer);

        /// <summary>
        ///     Request to delete the item list matching the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item list we want to delete.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void DeleteItemList(string id, Completer completer);

        /// <summary>
        ///     Request to add item to <see cref="ItemList"/>.
        /// </summary>
        /// <param name="listId">
        ///     <see cref="ItemList.id"/> to add item to.
        /// </param>
        /// <param name="itemId">
        ///     <see cref="InventoryItem.id"/> to add to list.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemListAddItem(string listId, string itemId, Completer completer);

        /// <summary>
        ///     Request to remove item from <see cref="ItemList"/>.
        /// </summary>
        /// <param name="listId">
        ///     <see cref="ItemList.id"/> to remove item from.
        /// </param>
        /// <param name="itemId">
        ///     <see cref="InventoryItem.id"/> to remove from list.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemListRemoveItem(string listId, string itemId, Completer completer);

        /// <summary>
        ///     Request to swap items in <see cref="ItemList"/>.
        /// </summary>
        /// <param name="listId">
        ///     <see cref="ItemList.id"/> to target.
        /// </param>
        /// <param name="index0">
        ///     index of first item to swap.
        /// </param>
        /// <param name="index1">
        ///     index of item to swap with.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemListSwapIndexes(string listId, int index0, int index1, Completer completer);

        /// <summary>
        ///     Request to clear all items in <see cref="ItemList"/>.
        /// </summary>
        /// <param name="listId">
        ///     <see cref="ItemList.id"/> to target.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemListClear(string listId, Completer completer);

        /// <summary>
        ///     Request to create a new item map with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     Identifier to give to the created item.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void CreateItemMap(string id, Completer completer);

        /// <summary>
        ///     Request to delete the item map matching the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item map we want to delete.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void DeleteItemMap(string id, Completer completer);

        /// <summary>
        ///     Request to add item to <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     <see cref="ItemMap.id"/> to add item to.
        /// </param>
        /// <param name="slot">
        ///     slot to use for this item in the map.
        /// </param>
        /// <param name="itemId">
        ///     <see cref="InventoryItem.id"/> to add to map.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemMapAddItem(string mapId, string slot, string itemId, Completer completer);

        /// <summary>
        ///     Request to remove item from <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     <see cref="ItemMap.id"/> to remove item from.
        /// </param>
        /// <param name="slot">
        ///     slot to remove from map.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemMapRemoveItem(string mapId, string slot, Completer completer);

        /// <summary>
        ///     Request to change item slots in <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     <see cref="ItemMap.id"/> to target.
        /// </param>
        /// <param name="oldSlot">
        ///     Old slot id of the item to swap.
        /// </param>
        /// <param name="newSlot">
        ///     New slot id of the item.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemMapChangeSlot(string mapId, string oldSlot, string newSlot, Completer completer);

        /// <summary>
        ///     Request to swap items in <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     <see cref="ItemMap.id"/> to target.
        /// </param>
        /// <param name="slot0">
        ///     slot of first item to swap.
        /// </param>
        /// <param name="slot1">
        ///     slot of item to swap with.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemMapSwapSlots(string mapId, string slot0, string slot1, Completer completer);

        /// <summary>
        ///     Request to clear all items in <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     <see cref="ItemMap.id"/> to target.
        /// </param>
        /// <param name="completer">
        ///     <see cref="Completer"/> for action.
        /// </param>
        void ItemMapClear(string mapId, Completer completer);

        /// <summary>
        ///     Get the mutable property with the given <paramref name="propertyKey"/>
        ///     of the item with the given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier.
        /// </param>
        /// <param name="propertyKey">
        ///     The mutable property's identifier.
        /// </param>
        /// <returns>
        ///     The mutable property's value.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If either <paramref name="itemId"/> or <paramref name="propertyKey"/>
        ///     is null, empty or whitespace.
        /// </exception>
        /// <exception cref="InventoryItemNotFoundException">
        ///     If there is no item with the given <paramref name="itemId"/>.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no mutable property with the given <paramref name="propertyKey"/> on the item.
        /// </exception>
        Property GetMutablePropertyValue(string itemId, string propertyKey);

        /// <summary>
        ///     Get the mutable property with the given <paramref name="propertyKey"/>
        ///     of the item with the given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier.
        /// </param>
        /// <param name="propertyKey">
        ///     The mutable property's identifier.
        /// </param>
        /// <param name="propertyValue">
        ///     The mutable property's value found for the given identifiers.
        /// </param>
        /// <returns>
        ///     True if a mutable property with the given <paramref name="propertyKey"/> exists
        ///     in the item with the given <paramref name="itemId"/>;
        ///     false otherwise.
        /// </returns>
        bool TryGetMutablePropertyValue(string itemId, string propertyKey, out Property propertyValue);

        /// <summary>
        ///     Request to update the mutable property with the given <paramref name="propertyKey"/>
        ///     of the item with the given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">
        ///     Item's id to update the mutable property of.
        /// </param>
        /// <param name="propertyKey">
        ///     Mutable property's key to update.
        /// </param>
        /// <param name="value">
        ///     Value to set to the mutable property.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void SetMutablePropertyValue(string itemId, string propertyKey, Property value, Completer completer);
    }
}
