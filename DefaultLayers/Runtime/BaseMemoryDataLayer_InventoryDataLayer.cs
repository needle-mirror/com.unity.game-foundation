using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     Stores the data of all the item instances.
        ///     Key: item's id.
        ///     Value: item's data.
        /// </summary>
        readonly Dictionary<string, InventoryItemData> m_Items = new Dictionary<string, InventoryItemData>();

        /// <summary>
        ///     Stores the data of all the item list instances.
        ///     Key: item list id.
        ///     Value: item list data.
        /// </summary>
        readonly Dictionary<string, ItemListData> m_ItemLists = new Dictionary<string, ItemListData>();

        /// <summary>
        ///     Stores the data of all the item map instances.
        ///     Key: item map id.
        ///     Value: item map data.
        /// </summary>
        readonly Dictionary<string, ItemMapData> m_ItemMaps = new Dictionary<string, ItemMapData>();

        /// <summary>
        ///     Initializes the data layer for <see cref="IInventoryManager"/>.
        /// </summary>
        /// <param name="data">
        ///     InventoryManager's serializable data.
        /// </param>
        protected void InitializeInventoryDataLayer(InventoryManagerData data)
        {
            //Reset containers.
            m_Items.Clear();
            m_ItemLists.Clear();
            m_ItemMaps.Clear();

            var validatedItems = GetValidItems(data, m_CatalogAsset);

            foreach (var item in validatedItems)
            {
                m_Items.Add(item.id, item);
            }

            // read all item lists from inventory manager data
            var itemLists = GetItemLists(data);

            // add all item lists to the inventory data layer
            foreach (var itemList in itemLists)
            {
                m_ItemLists.Add(itemList.id, itemList);
            }

            // read all item maps from inventory manager data
            var itemMaps = GetItemMaps(data);

            // add all item maps to the inventory data layer
            foreach (var itemMap in itemMaps)
            {
                m_ItemMaps.Add(itemMap.id, itemMap);
            }
        }

        /// <summary>
        ///     Try to find the <paramref name="item"/> with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The item's id to look for.
        /// </param>
        /// <param name="item">
        ///     The item with the given <paramref name="id"/> if it exists.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the item exists;
        ///     return <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="id"/> is null.
        /// </exception>
        bool TryGetItem(string id, out InventoryItemData item)
        {
            return m_Items.TryGetValue(id, out item);
        }

        /// <inheritdoc cref="CreateItem(string, string, Rejectable, long)"/>
        InventoryItemData CreateItem(string key, Rejectable rejectable, long quantity = 1)
        {
            var itemId = Guid.NewGuid().ToString();

            return CreateItem(key, itemId, rejectable, quantity);
        }

        /// <summary>
        ///     Create a new item.
        /// </summary>
        /// <param name="key">
        ///     The definition's identifier used to create the item.
        /// </param>
        /// <param name="itemId">
        ///     The item's id to use.
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case the creation fails.
        /// </param>
        /// <param name="quantity">
        ///     The number of item to create.
        /// </param>
        /// <returns>
        ///     The id of the newly created item.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If either of the given <paramref name="key"/> or
        ///     <paramref name="itemId"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="CatalogItemNotFoundException">
        ///     If there is definition with the given <paramref name="key"/> in the catalog.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If the <paramref name="itemId"/> is already used by another item.
        /// </exception>
        InventoryItemData CreateItem(string key, string itemId, Rejectable rejectable, long quantity = 1)
        {
            if (Tools.RejectIfArgNullOrEmpty(key, nameof(key), rejectable)
                || Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), rejectable))
            {
                return default;
            }

            if (!(m_CatalogAsset.FindItem(key) is InventoryItemDefinitionAsset itemDefinition))
            {
                rejectable.Reject(new CatalogItemNotFoundException(key));

                return default;
            }

            if (m_Items.ContainsKey(itemId))
            {
                var message = $"{nameof(BaseMemoryDataLayer)}: Cannot create item because an item with the id \"{itemId}\" already exists.";
                rejectable.Reject(new InvalidOperationException(message));

                return default;
            }

            return CreateItemNoCheck(itemDefinition, itemId, quantity);
        }

        /// <summary>
        ///     Create an item from the given <paramref name="itemDefinition"/> with
        ///     the given <paramref name="itemId"/> without checking their validity nor the item existence.
        /// </summary>
        /// <param name="itemDefinition">
        ///     The definition to use to create the item.
        /// </param>
        /// <param name="itemId">
        ///     The id to give to the created item.
        /// </param>
        /// <param name="quantity">
        ///     The number of item to create.
        /// </param>
        /// <returns>
        ///     Return the created item.
        /// </returns>
        InventoryItemData CreateItemNoCheck(InventoryItemDefinitionAsset itemDefinition, string itemId, long quantity = 1)
        {
            var item = new InventoryItemData
            {
                id = itemId,
                quantity = quantity,
                definitionKey = itemDefinition.key
            };

            // Set up mutableProperties.
            {
                item.mutableProperties = new PropertyData[itemDefinition.mutableProperties.Count];
                var i = 0;
                foreach (var propertyEntry in itemDefinition.mutableProperties)
                {
                    item.mutableProperties[i] = new PropertyData
                    {
                        key = propertyEntry.Key,
                        value = propertyEntry.Value
                    };
                    ++i;
                }
            }

            m_Items.Add(itemId, item);

            return item;
        }

        /// <summary>
        ///     Set quantity for the item with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     Identifier of the item to delete.
        /// </param>
        /// <param name="quantity">
        ///     New quantity of item specified.
        /// </param>
        /// <param name="rejectable">
        ///     A handle to the rejectable promise in case the operation fails.
        /// </param>
        /// <exception cref="InventoryItemNotFoundException">
        ///     Thrown if the <see cref="InventoryItemData"/> is not found for <paramref name="id"/>.
        /// </exception>
        void SetQuantity(string id, long quantity, Rejectable rejectable)
        {
            // find old entry (needed to know definitionKey later)
            if (!m_Items.TryGetValue(id, out var item))
            {
                rejectable.Reject(new InventoryItemNotFoundException(id));
                return;
            }

            // update the quantity by replacing the entry in the map with new quantity
            m_Items[id] = new InventoryItemData
            {
                id = id,
                quantity = quantity,
                definitionKey = item.definitionKey,
                mutableProperties = item.mutableProperties
            };
        }

        /// <summary>
        ///     Delete the item with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     Identifier of the item to delete.
        /// </param>
        /// <returns>
        ///     <c>true</c> if deleted; <c>false</c> otherwise.
        /// </returns>
        bool DeleteItem(string id) => m_Items.Remove(id);

        /// <inheritdoc cref="IInventoryDataLayer.CreateItemList(string, Completer)"/>
        ItemListData CreateItemList(string id)
        {
            var itemList = new ItemListData(id);

            m_ItemLists.Add(id, itemList);

            return itemList;
        }

        /// <inheritdoc cref="IInventoryDataLayer.CreateItemMap(string, Completer)"/>
        ItemMapData CreateItemMap(string id)
        {
            var itemMap = new ItemMapData(id);

            m_ItemMaps.Add(id, itemMap);

            return itemMap;
        }

        /// <summary>
        ///     Create a list of valid items from the given <paramref name="data"/>
        ///     using the given <paramref name="catalogAsset"/> as a source of truth.
        /// </summary>
        /// <param name="data">
        ///     Data to parse and check for validity.
        /// </param>
        /// <param name="catalogAsset">
        ///     The catalog asset to use as the source of truth.
        /// </param>
        /// <returns>
        ///     Return a list all items from the given <paramref name="data"/> that have a valid definition.
        ///     Their mutableProperties have also been validated to match their matching definition.
        /// </returns>
        static List<InventoryItemData> GetValidItems(InventoryManagerData data, CatalogAsset catalogAsset)
        {
            var numDataItems = data.items.Length;
            var validatedItems = new List<InventoryItemData>(numDataItems);

            if (numDataItems > 0)
            {
                var validatedProperties = new List<PropertyData>();

                for (var itemIndex = 0; itemIndex < numDataItems; itemIndex++)
                {
                    var item = data.items[itemIndex];

                    // Check item's definition.
                    Exception error;

                    if (!(catalogAsset.FindItem(item.definitionKey) is InventoryItemDefinitionAsset itemDefinition))
                    {
                        error = new CatalogItemNotFoundException(item.definitionKey);
                        k_Logger.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
                        continue;
                    }

                    var isItemIdAlreadyUsed = false;
                    foreach (var validatedItem in validatedItems)
                    {
                        if (validatedItem.id == item.id)
                        {
                            isItemIdAlreadyUsed = true;
                            break;
                        }
                    }

                    if (isItemIdAlreadyUsed)
                    {
                        error = new GameFoundationException($"An item with the id \"{item.id}\" already exist.");
                        k_Logger.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
                        continue;
                    }

                    // get item definition's default values of mutable properties
                    var defaultProperties = itemDefinition.mutableProperties;

                    // validate mutable properties
                    if (item.mutableProperties != null)
                    {
                        for (var propertyIndex = 0; propertyIndex < item.mutableProperties.Length; propertyIndex++)
                        {
                            var property = item.mutableProperties[propertyIndex];
                            var propertyKey = property.key;

                            // Check mutable property's default value.
                            if (!defaultProperties.TryGetValue(propertyKey, out var defaultProperty))
                            {
                                error = new PropertyNotFoundException(item.id, propertyKey);
                                k_Logger.LogWarning($"\"{item.id}\"'s \"{propertyKey}\" property has been " +
                                    $"skipped.\nReason: {error}");
                                continue;
                            }

                            // Check mutable property's type compatibility.
                            if (defaultProperty.currentValue.type != property.value.type)
                            {
                                error = new PropertyInvalidCastException(
                                    propertyKey, defaultProperty.currentValue.type, property.value.type);
                                k_Logger.LogWarning($"\"{item.id}\"'s \"{propertyKey}\" property has been " +
                                    $"reset.\nReason: {error}");
                                property.value = defaultProperties[propertyKey];
                            }

                            validatedProperties.Add(property);
                        }
                    }

                    // add default values of missing mutable properties.
                    foreach (var defaultProperty in defaultProperties)
                    {
                        var isDefaultPropertyValidated = false;
                        foreach (var validatedProperty in validatedProperties)
                        {
                            if (defaultProperty.Key == validatedProperty.key)
                            {
                                isDefaultPropertyValidated = true;
                                break;
                            }
                        }

                        if (!isDefaultPropertyValidated)
                        {
                            var validProperty = new PropertyData
                            {
                                key = defaultProperty.Key,
                                value = defaultProperty.Value
                            };
                            validatedProperties.Add(validProperty);
                        }
                    }

                    // set item mutable properties
                    item.mutableProperties = validatedProperties.ToArray();

                    // clear validated mutable properties list for reuse
                    validatedProperties.Clear();

                    // add this item to list of validated items
                    validatedItems.Add(item);
                }
            }

            return validatedItems;
        }

        /// <summary>
        ///     Create a list of item lists from the given <see cref="InventoryManagerData"/>.
        /// </summary>
        /// <param name="data">
        ///     Data to parse.
        /// </param>
        /// <returns>
        ///     Return a list all <see cref="ItemListData"/> from the given <paramref name="data"/>.
        /// </returns>
        static List<ItemListData> GetItemLists(InventoryManagerData data)
        {
            // handle null for item lists (permits upgrade from old data layer without item lists)
            if (data.itemLists is null)
            {
                return new List<ItemListData>();
            }

            // setup item lists based on InventoryManagerData
            var itemLists = new List<ItemListData>(data.itemLists.Length);

            foreach (var itemList in data.itemLists)
            {
                itemLists.Add(itemList);
            }

            return itemLists;
        }

        /// <summary>
        ///     Create a map of item maps from the given <see cref="InventoryManagerData"/>.
        /// </summary>
        /// <param name="data">
        ///     Data to parse.
        /// </param>
        /// <returns>
        ///     Return a map all <see cref="ItemMapData"/> from the given <paramref name="data"/>.
        /// </returns>
        static List<ItemMapData> GetItemMaps(InventoryManagerData data)
        {
            // handle null for item maps (permits upgrade from old data layer without item maps)
            if (data.itemMaps is null)
            {
                return new List<ItemMapData>();
            }

            // setup item maps based on InventoryManagerData
            var itemMaps = new List<ItemMapData>(data.itemMaps.Length);

            foreach (var itemMap in data.itemMaps)
            {
                itemMaps.Add(itemMap);
            }

            return itemMaps;
        }

        /// <inheritdoc/>
        InventoryManagerData IInventoryDataLayer.GetData()
        {
            var items = new InventoryItemData[m_Items.Count];
            m_Items.Values.CopyTo(items, 0);

            var itemLists = new ItemListData[m_ItemLists.Count];
            m_ItemLists.Values.CopyTo(itemLists, 0);

            var itemMaps = new ItemMapData[m_ItemMaps.Count];
            m_ItemMaps.Values.CopyTo(itemMaps, 0);

            var data = new InventoryManagerData
            {
                items = items,
                itemLists = itemLists,
                itemMaps = itemMaps
            };

            return data;
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItem(string key, string itemId, long quantity, Completer completer)
        {
            if (Tools.RejectIfArgNullOrEmpty(key, nameof(key), completer)
                || Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), completer))
            {
                return;
            }

            if (!(m_CatalogAsset.FindItem(key) is InventoryItemDefinitionAsset itemDefinition))
            {
                var reason = new CatalogItemNotFoundException(key);
                completer.Reject(reason);

                return;
            }

            if (m_Items.ContainsKey(itemId))
            {
                var reason = new InvalidOperationException(
                    $"{nameof(IInventoryDataLayer)}: Cannot create item because an item with the id \"{itemId}\" already exists.");
                completer.Reject(reason);

                return;
            }

            try
            {
                CreateItemNoCheck(itemDefinition, itemId, quantity);

                completer.Resolve();
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.SetQuantity(string id, long quantity, Completer completer)
        {
            SetQuantity(id, quantity, completer);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            // Requesting the deletion of a non existing item is a silent error.
            DeleteItem(itemId);

            completer.Resolve();
        }

        /// <inheritdoc/>
        Property IInventoryDataLayer.GetMutablePropertyValue(string itemId, string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(itemId, nameof(itemId));
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            if (!m_Items.TryGetValue(itemId, out var item))
                throw new InventoryItemNotFoundException(itemId);

            foreach (var itemProperty in item.mutableProperties)
            {
                if (itemProperty.key == propertyKey)
                    return itemProperty.value;
            }

            throw new PropertyNotFoundException(itemId, propertyKey);
        }

        /// <inheritdoc/>
        bool IInventoryDataLayer.TryGetMutablePropertyValue(
            string itemId, string propertyKey, out Property propertyValue)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || string.IsNullOrWhiteSpace(propertyKey)
                || !m_Items.TryGetValue(itemId, out var item))
            {
                propertyValue = default;

                return false;
            }

            foreach (var itemProperty in item.mutableProperties)
            {
                if (itemProperty.key == propertyKey)
                {
                    propertyValue = itemProperty.value;

                    return true;
                }
            }

            propertyValue = default;

            return false;
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.SetMutablePropertyValue(string itemId, string propertyKey, Property value, Completer completer)
        {
            if (Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), completer))
            {
                return;
            }

            if (Tools.RejectIfArgNullOrEmpty(propertyKey, nameof(propertyKey), completer))
            {
                return;
            }

            if (!m_Items.TryGetValue(itemId, out var item))
            {
                completer.Reject(new InventoryItemNotFoundException(itemId));

                return;
            }

            for (var i = 0; i < item.mutableProperties.Length; i++)
            {
                var itemProperty = item.mutableProperties[i];
                if (itemProperty.key == propertyKey)
                {
                    if (itemProperty.value.type != value.type)
                    {
                        completer.Reject(
                            new PropertyInvalidCastException(
                                propertyKey, itemProperty.value.type, value.type));
                    }
                    else
                    {
                        // Don't forget to reapply values to collections since we are working with structs.
                        itemProperty.value = value;
                        item.mutableProperties[i] = itemProperty;
                        m_Items[item.id] = item;

                        completer.Resolve();
                    }

                    return;
                }
            }

            completer.Reject(new PropertyNotFoundException(itemId, propertyKey));
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItemList(string id, Completer completer)
        {
            CreateItemList(id);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItemList(string id, Completer completer)
        {
            m_ItemLists.Remove(id);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListAddItem(string listId, string itemId, Completer completer)
        {
            var itemList = m_ItemLists[listId];
            itemList.inventoryItems.Add(itemId);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListRemoveItem(string listId, string itemId, Completer completer)
        {
            var itemList = m_ItemLists[listId];
            itemList.inventoryItems.Remove(itemId);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListSwapIndexes(string listId, int index0, int index1, Completer completer)
        {
            // swap the 2 items
            var itemList = m_ItemLists[listId];

            var inventoryItem = itemList.inventoryItems[index0];
            itemList.inventoryItems[index0] = itemList.inventoryItems[index1];
            itemList.inventoryItems[index1] = inventoryItem;

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListClear(string listId, Completer completer)
        {
            m_ItemLists[listId].inventoryItems.Clear();

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItemMap(string id, Completer completer)
        {
            CreateItemMap(id);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItemMap(string id, Completer completer)
        {
            m_ItemMaps.Remove(id);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapAddItem(string mapId, string slot, string itemId, Completer completer)
        {
            var itemMap = m_ItemMaps[mapId];
            itemMap.inventoryItems.Add(new ItemMapData.ItemData(slot, itemId));

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapRemoveItem(string mapId, string slot, Completer completer)
        {
            var itemMap = m_ItemMaps[mapId];
            var slotIndex = itemMap.inventoryItems.FindIndex(i => i.slot == slot);
            itemMap.inventoryItems.RemoveAt(slotIndex);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapChangeSlot(string mapId, string oldSlot, string newSlot, Completer completer)
        {
            var itemMap = m_ItemMaps[mapId];
            var slotIndex = itemMap.inventoryItems.FindIndex(i => i.slot == oldSlot);

            var item = itemMap.inventoryItems[slotIndex].id;

            itemMap.inventoryItems[slotIndex] = new ItemMapData.ItemData(newSlot, item);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapSwapSlots(string mapId, string slot0, string slot1, Completer completer)
        {
            var itemMap = m_ItemMaps[mapId];
            var slotIndex0 = itemMap.inventoryItems.FindIndex(i => i.slot == slot0);
            var slotIndex1 = itemMap.inventoryItems.FindIndex(i => i.slot == slot1);

            var item0 = itemMap.inventoryItems[slotIndex0].id;
            var item1 = itemMap.inventoryItems[slotIndex1].id;

            itemMap.inventoryItems[slotIndex0] = new ItemMapData.ItemData(slot0, item1);
            itemMap.inventoryItems[slotIndex1] = new ItemMapData.ItemData(slot1, item0);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapClear(string mapId, Completer completer)
        {
            m_ItemMaps[mapId].inventoryItems.Clear();

            completer.Resolve();
        }
    }
}
