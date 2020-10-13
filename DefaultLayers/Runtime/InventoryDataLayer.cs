using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Straightforward implementation of <see cref="IInventoryDataLayer"/>.
    /// </summary>
    class InventoryDataLayer : IInventoryDataLayer
    {
        /// <summary>
        ///     Stores the data of all the item instances.
        ///     Key: item's id.
        ///     Value: item's data.
        /// </summary>
        internal Dictionary<string, InventoryItemData> m_Items;

        /// <summary>
        ///     Stores the data of all the item list instances.
        ///     Key: item list id.
        ///     Value: item list data.
        /// </summary>
        Dictionary<string, ItemListData> m_ItemLists;

        /// <summary>
        ///     Stores the data of all the item map instances.
        ///     Key: item map id.
        ///     Value: item map data.
        /// </summary>
        Dictionary<string, ItemMapData> m_ItemMaps;

        /// <summary>
        ///     Catalog asset containing all definitions.
        /// </summary>
        CatalogAsset m_CatalogAsset;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<InventoryDataLayer>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryDataLayer"/>
        ///     class with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="data">
        ///     InventoryManager's serializable data.
        /// </param>
        /// <param name="catalogAsset">
        ///     The catalog asset to use as the source of truth.
        /// </param>
        public InventoryDataLayer(InventoryManagerData data, CatalogAsset catalogAsset)
        {
            m_CatalogAsset = catalogAsset;

            var validatedItems = GetValidItems(data, catalogAsset);

            m_Items = new Dictionary<string, InventoryItemData>(validatedItems.Count);

            foreach (var item in validatedItems)
            {
                m_Items.Add(item.id, item);
            }

            // read all item lists from inventory manager data
            var itemLists = GetItemLists(data);

            // create empty item lists
            m_ItemLists = new Dictionary<string, ItemListData>(itemLists.Count);

            // add all item lists to the inventory data layer
            foreach (var itemList in itemLists)
            {
                m_ItemLists.Add(itemList.id, itemList);
            }

            // read all item maps from inventory manager data
            var itemMaps = GetItemMaps(data);

            // create empty item maps
            m_ItemMaps = new Dictionary<string, ItemMapData>(itemMaps.Count);

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
        ///     Return true if the item exists;
        ///     return false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the given <paramref name="id"/> is null.
        /// </exception>
        internal bool TryGetItem(string id, out InventoryItemData item)
        {
            return m_Items.TryGetValue(id, out item);
        }

        /// <summary>
        ///     Get all ids of items created from the definition with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The definition's identifier to look items of.
        /// </param>
        /// <param name="target">
        ///     The collection to fill with the found items.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is <c>null</c>, empty, or whitespace.
        /// </exception>
        internal int GetItemsByDefinition(string key, ICollection<string> target = null, bool clearTarget = true)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));

            var count = 0;

            if (clearTarget) target?.Clear();

            foreach (var item in m_Items.Values)
            {
                if (item.definitionKey == key)
                {
                    count++;
                    target?.Add(item.id);
                }
            }

            return count;
        }

        /// <inheritdoc cref="CreateItem(string, string)"/>
        internal InventoryItemData CreateItem(string key, long quantity = 1)
        {
            var itemId = Guid.NewGuid().ToString();

            return CreateItem(key, itemId, quantity);
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
        internal InventoryItemData CreateItem(string key, string itemId, long quantity = 1)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            Tools.ThrowIfArgNullOrEmpty(itemId, nameof(itemId));

            var itemDefinition = m_CatalogAsset.FindItem(key) as InventoryItemDefinitionAsset;

            if (itemDefinition == null)
                throw new CatalogItemNotFoundException(key);

            if (m_Items.ContainsKey(itemId))
                throw new InvalidOperationException(
                    $"{nameof(InventoryDataLayer)}: Cannot create item because an item with the id \"{itemId}\" already exists.");

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
        /// <returns>
        ///     <c>true</c>.
        /// </returns>
        /// <exception cref="InventoryItemNotFoundException">
        ///     If the <see cref="InventoryItemData"/> is not found for
        ///     <paramref name="id"/>
        /// </exception>
        internal bool SetQuantity(string id, long quantity)
        {
            // find old entry (needed to know definitionKey later)
            if (!m_Items.TryGetValue(id, out var item))
            {
                throw new InventoryItemNotFoundException(id);
            }

            // update the quantity by replacing the entry in the map with new quantity
            m_Items[id] = new InventoryItemData
            {
                id = id,
                quantity = quantity,
                definitionKey = item.definitionKey,
                mutableProperties = item.mutableProperties
            };

            return true;
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
        internal bool DeleteItem(string id) => m_Items.Remove(id);

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
            int numDataItems = data.items.Length;
            var validatedItems = new List<InventoryItemData>(numDataItems);

            if (numDataItems > 0)
            {
                var validatedProperties = new List<PropertyData>();

                for (var itemIndex = 0; itemIndex < numDataItems; itemIndex++)
                {
                    var item = data.items[itemIndex];

                    // Check item's definition.
                    Exception error;

                    var itemDefinition = catalogAsset.FindItem(item.definitionKey) as InventoryItemDefinitionAsset;

                    if (itemDefinition == null)
                    {
                        error = new CatalogItemNotFoundException(item.definitionKey);
                        k_GFLogger.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
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
                        error = new Exception("An item with the id \"{item.id}\" already exist.");
                        k_GFLogger.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
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
                                k_GFLogger.LogWarning($"\"{item.id}\"'s \"{propertyKey}\" property has been " +
                                    $"skipped.\nReason: {error}");
                                continue;
                            }

                            // Check mutable property's type compatibility.
                            if (defaultProperty.type != property.value.type)
                            {
                                error = new PropertyInvalidCastException(
                                    propertyKey, defaultProperty.type, property.value.type);
                                k_GFLogger.LogWarning($"\"{item.id}\"'s \"{propertyKey}\" property has been " +
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
            if (data.itemLists == null)
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
            if (data.itemMaps == null)
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
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="CatalogItemNotFoundException">
        ///     If there is definition with the given <paramref name="key"/> in the catalog.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If the <paramref name="itemId"/> is already used by another item.
        /// </exception>
        void IInventoryDataLayer.CreateItem(string key, string itemId, Completer completer)
        {
            Tools.RejectIfArgNullOrEmpty(key, nameof(key), completer);
            Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), completer);

            var itemDefinition = m_CatalogAsset.FindItem(key) as InventoryItemDefinitionAsset;

            if (itemDefinition == null)
            {
                var reason = new CatalogItemNotFoundException(key);
                completer.Reject(reason);
            }

            if (m_Items.ContainsKey(itemId))
            {
                var reason = new InvalidOperationException(
                    $"{nameof(IInventoryDataLayer)}: Cannot create item because an item with the id \"{itemId}\" already exists.");
                completer.Reject(reason);
            }

            CreateItemNoCheck(itemDefinition, itemId);

            completer.Resolve();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.SetQuantity(string id, long quantity, Completer completer)
        {
            SetQuantity(id, quantity);

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
        bool IInventoryDataLayer.TryGetMutablePropertyValue(string itemId, string propertyKey, out Property propertyValue)
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
                return;

            if (Tools.RejectIfArgNullOrEmpty(propertyKey, nameof(propertyKey), completer))
                return;

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

        /// <inheritdoc cref="IInventoryDataLayer.CreateItemList(string, Completer)"/>
        internal ItemListData CreateItemList(string id)
        {
            var itemList = new ItemListData(id);

            m_ItemLists.Add(id, itemList);

            return itemList;
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

        /// <inheritdoc cref="IInventoryDataLayer.CreateItemMap(string, Completer)"/>
        internal ItemMapData CreateItemMap(string id)
        {
            var itemMap = new ItemMapData(id);

            m_ItemMaps.Add(id, itemMap);

            return itemMap;
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
