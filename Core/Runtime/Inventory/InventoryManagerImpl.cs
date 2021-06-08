using System;
using System.Collections.Generic;
using UnityEngine.Internal;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Implementation of <see cref="IInventoryManager"/>
    /// </summary>
    [ExcludeFromDocs]
    partial class InventoryManagerImpl : ManagerImplementation
    {
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(InventoryManagerImpl));

        /// <summary>
        ///     Gets the <see cref="IInventoryDataLayer"/> aspect of the data layer.
        /// </summary>
        static IInventoryDataLayer dataLayer => GameFoundationSdk.dataLayer;

        /// <inheritdoc cref="IInventoryManager.itemAdded"/>
        public event Action<InventoryItem> itemAdded;

        /// <inheritdoc cref="IInventoryManager.itemDeleted"/>
        public event Action<InventoryItem> itemDeleted;

        /// <summary>
        ///     Triggered every time a <see cref="StackableInventoryItem"/>
        ///     quantity is changed.
        /// </summary>
        public event Action<IQuantifiable, long> itemQuantityChanged;

        /// <inheritdoc cref="IInventoryManager.itemMutablePropertyChanged"/>
        public event Action<PropertyChangedEventArgs> itemMutablePropertyChanged;

        /// <inheritdoc cref="IInventoryManager.collectionAdded"/>
        public event Action<IItemCollection> collectionAdded;

        /// <inheritdoc cref="IInventoryManager.collectionDeleted"/>
        public event Action<IItemCollection> collectionDeleted;

        /// <inheritdoc cref="IInventoryManager.itemAddedToCollection"/>
        public event Action<IItemCollection, InventoryItem> itemAddedToCollection;

        /// <inheritdoc cref="IInventoryManager.itemRemovedFromCollection"/>
        public event Action<IItemCollection, InventoryItem> itemRemovedFromCollection;

        /// <summary>
        ///     Stores the <see cref="InventoryItem"/> instances, indexed by their <see cref="InventoryItem.id"/>.
        /// </summary>
        Dictionary<string, InventoryItem> m_Items = new Dictionary<string, InventoryItem>();

        /// <summary>
        ///     Stores the <see cref="IItemCollection"/> instances, indexed by their <see cref="IItemCollection.id"/>.
        /// </summary>
        Dictionary<string, IItemCollection> m_ItemCollections = new Dictionary<string, IItemCollection>();

        /// <inheritdoc/>
        protected override void InitializeData(Completer completer, GameFoundationInitOptions initOptions = null)
        {
            var inventoryManagerData = dataLayer.GetData();
            var catalog = GameFoundationSdk.catalog;

            foreach (var itemData in inventoryManagerData.items)
            {
                if (string.IsNullOrEmpty(itemData.definitionKey)
                    || string.IsNullOrEmpty(itemData.id))
                {
                    continue;
                }

                var itemDefinition = catalog.Find<InventoryItemDefinition>(itemData.definitionKey);
                if (itemDefinition is null)
                {
                    continue;
                }

                var item = itemDefinition.CreateInventoryItem(itemData);
                m_Items.Add(item.id, item);
            }

            // read data from inventoryManagerData for ItemLists and ItemMaps
            if (!(inventoryManagerData.itemLists is null))
            {
                foreach (var itemListData in inventoryManagerData.itemLists)
                {
                    var itemList = new ItemList(itemListData.id);
                    m_ItemCollections.Add(itemList.id, itemList);

                    if (!(itemListData.inventoryItems is null))
                    {
                        foreach (var itemId in itemListData.inventoryItems)
                        {
                            if (m_Items.TryGetValue(itemId, out var item))
                            {
                                itemList.AddInternal(item);
                            }
                        }
                    }
                }
            }

            if (!(inventoryManagerData.itemMaps is null))
            {
                foreach (var itemMapData in inventoryManagerData.itemMaps)
                {
                    var itemMap = new ItemMap(itemMapData.id);
                    m_ItemCollections.Add(itemMap.id, itemMap);

                    if (!(itemMapData.inventoryItems is null))
                    {
                        foreach (var itemData in itemMapData.inventoryItems)
                        {
                            if (m_Items.TryGetValue(itemData.id, out var item))
                            {
                                itemMap.AddInternal(itemData.slot, item);
                            }
                        }
                    }
                }
            }

            completer.Resolve();
        }

        /// <inheritdoc/>
        internal override void Uninitialize()
        {
            DeleteAllCollectionsInternal(default, default);
            DeleteAllItemsInternal(default, default);

            m_Items = null;
            m_ItemCollections = null;
        }

        /// <summary>
        ///     Used primarily for testing, this retrieves internal data layer <see cref="Data.InventoryItemData"/>
        ///     for specified item by <see cref="InventoryItem.id"/>.
        /// </summary>
        /// <param name="id">
        ///     <see cref="InventoryItem.id"/> of <see cref="Data.InventoryItemData"/> to retrieve.
        /// </param>
        /// <returns>
        ///     <see cref="Data.InventoryItemData"/> for item requested or default if not found.
        /// </returns>
        internal Data.InventoryItemData GetDataLayerItem(string id)
        {
            var data = dataLayer.GetData();
            foreach (var item in data.items)
            {
                if (item.id == id)
                {
                    return item;
                }
            }

            return default;
        }

        /// <summary>
        ///     Creates an item internally.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> the item is created from.
        /// </param>
        /// <param name="id">
        ///     The identifier of the item to create.
        /// </param>
        /// <returns>
        ///     The newly created item.
        /// </returns>
        public InventoryItem CreateInternal(InventoryItemDefinition definition, string id)
        {
            var item = definition.CreateInventoryItem(id);
            m_Items.Add(item.id, item);

            itemAdded?.Invoke(item);

            return item;
        }

        /// <summary>
        ///     Creates an item internally.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> the item is created from.
        /// </param>
        /// <param name="id">
        ///     The identifier of the item to create.
        /// </param>
        /// <param name="quantity">
        ///     Quantity of the newly-created stackable item which defaults to 1.
        /// </param>
        /// <returns>
        ///     The newly created item.
        /// </returns>
        public StackableInventoryItem CreateInternal(StackableInventoryItemDefinition definition, string id, long quantity = 1)
        {
            var item = definition.CreateInventoryItem(id, quantity);

            m_Items.Add(item.id, item);

            itemAdded?.Invoke(item);

            return item;
        }

        /// <summary>
        ///     Creates a list internally.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the list to create.
        /// </param>
        /// <returns>
        ///     The newly created list.
        /// </returns>
        public ItemList CreateListInternal(string id)
        {
            var list = new ItemList(id);
            m_ItemCollections.Add(list.id, list);

            collectionAdded?.Invoke(list);

            return list;
        }

        /// <summary>
        ///     Creates a map internally.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the map to create.
        /// </param>
        /// <returns>
        ///     The newly created map.
        /// </returns>
        public ItemMap CreateMapInternal(string id)
        {
            var map = new ItemMap(id);
            m_ItemCollections.Add(map.id, map);

            collectionAdded?.Invoke(map);

            return map;
        }

        /// <summary>
        ///     Deletes the <paramref name="item"/> internally.
        /// </summary>
        /// <param name="item">
        ///     The item to delete.
        /// </param>
        /// <returns>
        ///     <c>true</c> if deleted, <c>false</c> otherwise.
        /// </returns>
        public bool DeleteInternal(InventoryItem item)
        {
            if (item.hasBeenDiscarded) return false;

            if (!m_Items.Remove(item.id))
            {
                return false;
            }

            //Events need to happen before the item is discarded for them to access to the item's data.
            itemDeleted?.Invoke(item);

            //Cleanup the removed item to make sure it can't be used anymore.
            item.Discard();

            return true;
        }

        /// <summary>
        ///     Discards all the <see cref="InventoryItem"/> instances by their <see cref="InventoryItem.definition"/>
        ///     internally.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> instance used to find the <see cref="InventoryItem"/>
        ///     instances to disccard.
        /// </param>
        /// <param name="target">
        ///     The target collection all the discarded <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="InventoryItem"/> instances discarded.
        /// </returns>
        public int DeleteInternal(InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;

            var items = Tools.Pools.inventoryItemList.Get();
            try
            {
                FindItem((itemDef, item) => item.definition == definition, definition, items, false);
                foreach (var item in items)
                {
                    var deleted = DeleteInternal(item);
                    if (deleted)
                    {
                        count++;
                        target?.Add(item);
                    }
                }
            }
            finally
            {
                Tools.Pools.inventoryItemList.Release(items);
            }

            return count;
        }

        /// <summary>
        ///     Discards the specified <paramref name="collection"/> internally.
        /// </summary>
        /// <param name="collection">
        ///     The collection to discard.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="collection"/> is discarded, <c>false</c> otherwise.
        /// </returns>
        public bool DeleteInternal(IItemCollection collection)
        {
            if (collection.hasBeenDiscarded) return false;

            collectionDeleted?.Invoke(collection);

            // Clear the collection
            collection.Clear();

            var deleted = m_ItemCollections.Remove(collection.id);

            if (deleted)
            {
                // mark the collection as discarded
                if (collection is ItemList itemList)
                {
                    itemList.MarkDiscardedInternal();
                }
                else if (collection is ItemMap itemMap)
                {
                    itemMap.MarkDiscardedInternal();
                }
            }

            return deleted;
        }

        /// <summary>
        ///     Discards all the <see cref="IItemCollection"/> instances internally.
        /// </summary>
        /// <param name="target">
        ///     The target collection the discarded collections are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, if clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="IItemCollection"/> instances discarded.
        /// </returns>
        public int DeleteAllCollectionsInternal(ICollection<IItemCollection> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;

            var collectionsToRemove = Tools.Pools.collectionList.Get();
            try
            {
                GetCollections(collectionsToRemove, false);
                foreach (var collection in collectionsToRemove)
                {
                    var deleted = DeleteInternal(collection);
                    if (deleted)
                    {
                        count++;
                        target?.Add(collection);
                    }
                }
            }
            finally
            {
                Tools.Pools.collectionList.Release(collectionsToRemove);
            }

            return count;
        }

        /// <summary>
        ///     Deletes all the <see cref="InventoryItem"/> instances internally.
        /// </summary>
        /// <param name="target">
        ///     The target collections the discarded <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="InventoryItem"/> instances discarded.
        /// </returns>
        public int DeleteAllItemsInternal(ICollection<InventoryItem> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            var itemsToRemove = Tools.Pools.inventoryItemList.Get();
            try
            {
                Tools.Copy(m_Items.Values, itemsToRemove, false);
                foreach (var item in itemsToRemove)
                {
                    var deleted = DeleteInternal(item);
                    if (deleted)
                    {
                        count++;
                        target?.Add(item);
                    }
                }
            }
            finally
            {
                Tools.Pools.inventoryItemList.Release(itemsToRemove);
            }

            return count;
        }

        /// <inheritdoc cref="IInventoryManager.CreateList"/>
        public ItemList CreateList()
        {
            // create the ItemList and add it to our collection of all ItemLists
            var id = Guid.NewGuid().ToString();

            var list = CreateListInternal(id);

            SyncCreateItemList(id);

            return list;
        }

        /// <inheritdoc cref="IInventoryManager.CreateMap"/>
        public ItemMap CreateMap()
        {
            // create the ItemMap and add it to our collection of all ItemMaps
            var id = Guid.NewGuid().ToString();

            var map = CreateMapInternal(id);

            SyncCreateItemMap(id);

            return map;
        }

        /// <inheritdoc cref="IInventoryManager.CreateItem(InventoryItemDefinition)"/>
        public InventoryItem CreateItem(InventoryItemDefinition definition)
        {
            // create the ItemMap and add it to our collection of all ItemMaps
            var id = Guid.NewGuid().ToString();

            var newItem = CreateInternal(definition, id);

            SyncCreateItem(definition.key, newItem.id);

            return newItem;
        }

        /// <inheritdoc cref="IInventoryManager.CreateItem(StackableInventoryItemDefinition, long)"/>
        public StackableInventoryItem CreateItem(StackableInventoryItemDefinition definition, long quantity = 1)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid quantity for stackable item; quantity must be positive.");
            }

            // create the ItemMap and add it to our collection of all ItemMaps
            var id = Guid.NewGuid().ToString();

            var newItem = CreateInternal(definition, id, quantity);

            SyncCreateItem(definition.key, newItem.id, quantity);

            return newItem;
        }

        /// <inheritdoc cref="IInventoryManager.GetCollections(ICollection{IItemCollection}, bool)"/>
        public int GetCollections(ICollection<IItemCollection> target, bool clearTarget)
            => Tools.Copy(m_ItemCollections.Values, target, clearTarget);

        /// <inheritdoc cref="IInventoryManager.GetItems(ICollection{InventoryItem}, bool)"/>
        public int GetItems(ICollection<InventoryItem> target, bool clearTarget)
            => Tools.Copy(m_Items.Values, target, clearTarget);

        /// <inheritdoc cref="IInventoryManager.FindCollection{TCollection}(string)"/>
        public TCollection FindCollection<TCollection>(string id) where TCollection : class, IItemCollection
        {
            foreach (var collection in m_ItemCollections.Values)
            {
                if (collection.id == id)
                {
                    return collection as TCollection;
                }
            }

            return default;
        }

        /// <inheritdoc cref="IInventoryManager.FindItem(string)"/>
        public InventoryItem FindItem(string id)
        {
            m_Items.TryGetValue(id, out var item);
            return item;
        }

        /// <inheritdoc cref="IInventoryManager.FindItems(Predicate{InventoryItem}, ICollection{InventoryItem}, bool)"/>
        public int FindItem(Predicate<InventoryItem> filter, ICollection<InventoryItem> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var item in m_Items.Values)
            {
                if (filter(item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc
        ///     cref="IInventoryManager.FindItems{TState}(Func{TState, InventoryItem, bool}, TState, ICollection{InventoryItem}, bool)"/>
        public int FindItem<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target,
            bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var item in m_Items.Values)
            {
                if (filter(state, item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc cref="IInventoryManager.Delete(IItemCollection)"/>
        public bool Delete(IItemCollection collection)
        {
            // mark collection 'discarded' so it throws if used again
            var removed = DeleteInternal(collection);

            if (removed)
            {
                if (collection is ItemList list)
                {
                    SyncDeleteItemList(list.id);
                }
                else if (collection is ItemMap map)
                {
                    SyncDeleteItemMap(map.id);
                }
            }

            return removed;
        }

        /// <inheritdoc cref="IInventoryManager.Delete(InventoryItem)"/>
        public bool Delete(InventoryItem item)
        {
            //Store item's id since it will be discarded.
            var itemId = item.id;
            var itemWasRemoved = DeleteInternal(item);

            if (itemWasRemoved)
            {
                SyncDeleteItem(itemId);
            }

            return itemWasRemoved;
        }

        /// <inheritdoc cref="IInventoryManager.Delete(InventoryItemDefinition, ICollection{InventoryItem}, bool)"/>
        public int Delete(InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;

            var itemToRemove = Tools.Pools.inventoryItemList.Get();
            try
            {
                foreach (var item in m_Items.Values)
                {
                    if (ReferenceEquals(item.definition, definition))
                    {
                        itemToRemove.Add(item);
                    }
                }

                foreach (var item in itemToRemove)
                {
                    var deleted = Delete(item);
                    if (deleted)
                    {
                        count++;
                        target?.Add(item);
                    }
                }
            }
            finally
            {
                Tools.Pools.inventoryItemList.Release(itemToRemove);
            }

            return count;
        }

        /// <inheritdoc cref="IInventoryManager.DeleteAllItems(ICollection{InventoryItem}, bool)"/>
        public int DeleteAllItems(ICollection<InventoryItem> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = m_Items.Count;

            var itemToRemove = Tools.Pools.inventoryItemList.Get();
            try
            {
                Tools.Copy(m_Items.Values, itemToRemove);

                foreach (var item in itemToRemove)
                {
                    var deleted = Delete(item);
                    if (deleted)
                    {
                        target?.Add(item);
                    }
                }
            }
            finally
            {
                itemToRemove.Clear();
            }

            return count;
        }

        /// <inheritdoc cref="IInventoryManager.DeleteAllCollections(ICollection{IItemCollection}, bool)"/>
        public int DeleteAllCollections(ICollection<IItemCollection> target, bool clearTarget)
        {
            if (clearTarget) target?.Clear();

            var count = 0;

            var collections = Tools.Pools.collectionList.Get();

            try
            {
                var collectionCount = GetCollections(collections, false);

                foreach (var collection in collections)
                {
                    var deleted = Delete(collection);
                    if (deleted)
                    {
                        count++;
                        target?.Add(collection);
                    }
                }
            }
            finally
            {
                Tools.Pools.collectionList.Release(collections);
            }

            return count;
        }

        /// <summary>
        ///     Get specified <see cref="StackableInventoryItem"/>'s quantity.
        /// </summary>
        /// <param name="item">
        ///     <see cref="StackableInventoryItem"/> to get quantity of.
        /// </param>
        /// <returns>
        ///     Quantity of items in <see cref="StackableInventoryItem"/>.
        /// </returns>
        public long GetQuantity(StackableInventoryItem item) => item.quantity;

        /// <summary>
        ///     Set specified <see cref="StackableInventoryItem"/>'s quantity.
        ///     Ensures new quantity is permissible before setting; returns false on failure.
        /// </summary>
        /// <param name="item">
        ///     <see cref="StackableInventoryItem"/> to change quantity upon.
        /// </param>
        /// <param name="quantity">
        ///     Requested new quantity.
        /// </param>
        /// <returns>
        ///     true if quantity change is permissible and changed, else false.
        /// </returns>
        public bool SetQuantity(StackableInventoryItem item, long quantity)
        {
            var oldQuantity = item.quantity;
            if (quantity == oldQuantity)
            {
                return false;
            }

            SyncItemQuantity(item.id, quantity);

            item.SetQuantityInternal(quantity);

            return true;
        }

        /// <summary>
        ///     Iterate inventory and return total quantity of specified
        ///     <see cref="InventoryItemDefinition"/>.
        ///     Note: if items are <see cref="StackableInventoryItem"/>
        ///     then total of all items in all stacks is calculated.
        /// </summary>
        /// <param name="itemDefinition">
        ///     <see cref="InventoryItemDefinition"/> to total.
        /// </param>
        /// <returns>
        ///     Total quantity of specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        public long GetTotalQuantity(InventoryItemDefinition itemDefinition)
        {
            long quantity = 0;
            foreach (var item in m_Items.Values)
            {
                if (ReferenceEquals(item.definition, itemDefinition))
                {
                    if (item is StackableInventoryItem stackableItem)
                    {
                        quantity += stackableItem.quantity;
                    }
                    else
                    {
                        ++quantity;
                    }
                }
            }

            return quantity;
        }

        internal void HandleStackableQuantityChanged(StackableInventoryItem item, long oldQuantity)
        {
            itemQuantityChanged?.Invoke(item, oldQuantity);
        }

        /// <summary>
        ///     Called by <see cref="InventoryItem"/> whenever properties change.
        /// </summary>
        /// <param name="args">
        ///     Specific values for item and property that was changed.
        /// </param>
        internal void OnMutablePropertyChanged(PropertyChangedEventArgs args) =>
            itemMutablePropertyChanged?.Invoke(args);

        /// <summary>
        ///     Called by <see cref="InventoryItem"/> whenever it is added to
        ///     an <see cref="IItemCollection"/>.
        /// </summary>
        /// <param name="collection">
        ///     <see cref="IItemCollection"/> that <see cref="InventoryItem"/> was added to.
        /// </param>
        /// <param name="item">
        ///     <see cref="InventoryItem"/> that was added.
        /// </param>
        internal void OnItemAddedToCollection(IItemCollection collection, InventoryItem item) =>
            itemAddedToCollection?.Invoke(collection, item);

        /// <summary>
        ///     Called by <see cref="InventoryItem"/> whenever it is removed from
        ///     an <see cref="IItemCollection"/>.
        /// </summary>
        /// <param name="collection">
        ///     <see cref="IItemCollection"/> that <see cref="InventoryItem"/> was removed from.
        /// </param>
        /// <param name="item">
        ///     <see cref="InventoryItem"/> that was removed.
        /// </param>
        internal void OnItemRemovedFromCollection(IItemCollection collection, InventoryItem item) =>
            itemRemovedFromCollection?.Invoke(collection, item);

        /// <summary>
        ///     Synchronizes the creation of the item with the data layer.
        /// </summary>
        /// <param name="key">
        ///     The Identifier of the InventoryItemDefinition to assign this Item.
        /// </param>
        /// <param name="id">
        ///     The Identifier this item will have.
        /// </param>
        /// <param name="quantity">
        ///     Quantity for this item (must be positive).
        /// </param>
        internal void SyncCreateItem(string key, string id, long quantity = 1) =>
            dataLayer.CreateItem(key, id, quantity, Completer.None);

        /// <summary>
        ///     Synchronizes the removal of item from an inventory with the data layer
        /// </summary>
        /// <param name="itemId">
        ///     The Identifier of the item to delete.
        /// </param>
        internal void SyncDeleteItem(string itemId) => dataLayer.DeleteItem(itemId, Completer.None);

        /// <summary>
        ///     Synchronizes the creation of an item list with the data layer.
        /// </summary>
        /// <param name="id">
        ///     The Identifier this item list will have.
        /// </param>
        internal void SyncCreateItemList(string id) => dataLayer.CreateItemList(id, Completer.None);

        /// <summary>
        ///     Synchronizes the removal of an item list with the data layer.
        /// </summary>
        /// <param name="itemId">
        ///     The Identifier of the item list to delete.
        /// </param>
        internal void SyncDeleteItemList(string id) => dataLayer.DeleteItemList(id, Completer.None);

        /// <summary>
        ///     Synchronizes the addition of an item to an item list with the data layer.
        /// </summary>
        /// <param name="listId">
        ///     The identifier of the item list.
        /// </param>
        /// <param name="itemId">
        ///     The identifier of the item to add to the list.
        /// </param>
        internal void SyncItemListAddItem(string listId, string itemId)
            => dataLayer.ItemListAddItem(listId, itemId, Completer.None);

        /// <summary>
        ///     Synchronizes the removal of an item from an item list with the data layer.
        /// </summary>
        /// <param name="listId">
        ///     The identifier of the item list.
        /// </param>
        /// <param name="itemId">
        ///     The identifier of the item to remove from the list.
        /// </param>
        internal void SyncItemListRemoveItem(string listId, string itemId)
            => dataLayer.ItemListRemoveItem(listId, itemId, Completer.None);

        /// <summary>
        ///     Synchronizes the swapping of 2 items in an <see cref="ItemList"/> with the data layer.
        /// </summary>
        /// <param name="listId">
        ///     The identifier of the item list.
        /// </param>
        /// <param name="index0">
        ///     The index of the first item to swap.
        /// </param>
        /// <param name="index1">
        ///     The index of the item to swap with.
        /// </param>
        internal void SyncItemListSwapIndexes(string listId, int index0, int index1)
            => dataLayer.ItemListSwapIndexes(listId, index0, index1, Completer.None);

        /// <summary>
        ///     Synchronizes the clearing of items in an <see cref="ItemList"/> with the data layer.
        /// </summary>
        /// <param name="listId">
        ///     The identifier of the item list.
        /// </param>
        internal void SyncItemListClear(string listId) => dataLayer.ItemListClear(listId, Completer.None);

        /// <summary>
        ///     Synchronizes the creation of an item map with the data layer.
        /// </summary>
        /// <param name="id">
        ///     The Identifier this item map will have.
        /// </param>
        internal void SyncCreateItemMap(string id) => dataLayer.CreateItemMap(id, Completer.None);

        /// <summary>
        ///     Synchronizes the removal of an item map with the data layer.
        /// </summary>
        /// <param name="itemId">
        ///     The Identifier of the item map to delete.
        /// </param>
        internal void SyncDeleteItemMap(string id) => dataLayer.DeleteItemMap(id, Completer.None);

        /// <summary>
        ///     Synchronizes the addition of an item to an item map with the data layer.
        /// </summary>
        /// <param name="mapId">
        ///     The identifier of the item map.
        /// </param>
        /// <param name="slot">
        ///     slot to use for this item in the map.
        /// </param>
        /// <param name="itemId">
        ///     The identifier of the item to add to the map.
        /// </param>
        internal void SyncItemMapAddItem(string mapId, string slot, string itemId)
            => dataLayer.ItemMapAddItem(mapId, slot, itemId, Completer.None);

        /// <summary>
        ///     Synchronizes the removal of an item from an item map with the data layer.
        /// </summary>
        /// <param name="mapId">
        ///     The identifier of the item map.
        /// </param>
        /// <param name="slot">
        ///     slot to remove from map.
        /// </param>
        internal void SyncItemMapRemoveItem(string mapId, string slot)
            => dataLayer.ItemMapRemoveItem(mapId, slot, Completer.None);

        /// <summary>
        ///     Request to change item slots in <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="mapId">
        ///     The identifier of the item map.
        /// </param>
        /// <param name="oldSlot">
        ///     Old slot id of the item to swap.
        /// </param>
        /// <param name="newSlot">
        ///     New slot id of the item.
        /// </param>
        internal void SyncItemMapChangeSlot(string mapId, string oldSlot, string newSlot)
            => dataLayer.ItemMapChangeSlot(mapId, oldSlot, newSlot, Completer.None);

        /// <summary>
        ///     Synchronizes the swapping of 2 items in an <see cref="ItemMap"/> with the data layer.
        /// </summary>
        /// <param name="mapId">
        ///     The identifier of the item map.
        /// </param>
        /// <param name="slot0">
        ///     slot of first item to swap.
        /// </param>
        /// <param name="slot1">
        ///     slot of item to swap with.
        /// </param>
        internal void SyncItemMapSwapSlots(string mapId, string slot0, string slot1)
            => dataLayer.ItemMapSwapSlots(mapId, slot0, slot1, Completer.None);

        /// <summary>
        ///     Synchronizes the clearing of items in an <see cref="ItemMap"/> with the data layer.
        /// </summary>
        /// <param name="mapId">
        ///     The identifier of the item map.
        /// </param>
        internal void SyncItemMapClear(string mapId) => dataLayer.ItemMapClear(mapId, Completer.None);

        /// <summary>
        ///     Synchronizes the new item quantity with the data layer.
        /// </summary>
        /// <param name="itemId">
        ///     Item id upon which to set quantity.
        /// </param>
        /// <param name="quantity">
        ///     New quantity to set.
        /// </param>
        internal void SyncItemQuantity(string itemId, long quantity)
            => dataLayer.SetQuantity(itemId, quantity, Completer.None);
    }
}
