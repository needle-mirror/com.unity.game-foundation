using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    partial class InventoryManagerImpl : IInventoryManager
    {
        event Action<InventoryItem> IInventoryManager.itemAdded
        {
            add => itemAdded += value;
            remove => itemAdded -= value;
        }

        event Action<InventoryItem> IInventoryManager.itemDeleted
        {
            add => itemDeleted += value;
            remove => itemDeleted -= value;
        }

        event Action<IQuantifiable, long> IInventoryManager.itemQuantityChanged
        {
            add => itemQuantityChanged += value;
            remove => itemQuantityChanged -= value;
        }

        event Action<PropertyChangedEventArgs> IInventoryManager.itemMutablePropertyChanged
        {
            add => itemMutablePropertyChanged += value;
            remove => itemMutablePropertyChanged -= value;
        }

        event Action<IItemCollection> IInventoryManager.collectionAdded
        {
            add => collectionAdded += value;
            remove => collectionAdded -= value;
        }

        event Action<IItemCollection> IInventoryManager.collectionDeleted
        {
            add => collectionDeleted += value;
            remove => collectionDeleted -= value;
        }

        event Action<IItemCollection, InventoryItem> IInventoryManager.itemAddedToCollection
        {
            add => itemAddedToCollection += value;
            remove => itemAddedToCollection -= value;
        }

        event Action<IItemCollection, InventoryItem> IInventoryManager.itemRemovedFromCollection
        {
            add => itemRemovedFromCollection += value;
            remove => itemRemovedFromCollection -= value;
        }

        InventoryItem IInventoryManager.CreateItem(InventoryItemDefinition definition)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));
            return CreateItem(definition);
        }

        StackableInventoryItem IInventoryManager.CreateItem(StackableInventoryItemDefinition definition, long quantity)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));
            return CreateItem(definition, quantity);
        }

        ItemList IInventoryManager.CreateList() => CreateList();

        ItemMap IInventoryManager.CreateMap() => CreateMap();

        int IInventoryManager.GetCollections(ICollection<IItemCollection> target, bool clearTarget)
            => GetCollections(target, clearTarget);

        int IInventoryManager.GetItems(ICollection<InventoryItem> target, bool clearTarget)
            => GetItems(target, clearTarget);

        TCollection IInventoryManager.FindCollection<TCollection>(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            return FindCollection<TCollection>(id);
        }

        InventoryItem IInventoryManager.FindItem(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            return FindItem(id);
        }

        int IInventoryManager.FindItems(Predicate<InventoryItem> filter, ICollection<InventoryItem> target, bool clearTarget)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));
            return FindItem(filter, target, clearTarget);
        }

        int IInventoryManager.FindItems<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target,
            bool clearTarget)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));
            return FindItem(filter, state, target, clearTarget);
        }

        int IInventoryManager.FindItems(Tag tag, ICollection<InventoryItem> target, bool clearTarget)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return FindItem((tagFilter, item) => item.definition.HasTag(tagFilter), tag, target, clearTarget);
        }

        int IInventoryManager.FindItems
            (InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));
            return FindItem((definitionFilter, item) => item.definition == definitionFilter, definition, target, clearTarget);
        }

        bool IInventoryManager.Delete(IItemCollection collection)
        {
            Tools.ThrowIfArgNull(collection, nameof(collection));
            return Delete(collection);
        }

        bool IInventoryManager.Delete(InventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));
            return Delete(item);
        }

        int IInventoryManager.Delete
            (InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));
            return Delete(definition, target, clearTarget);
        }

        int IInventoryManager.DeleteAllItems(ICollection<InventoryItem> target, bool clearTarget)
            => DeleteAllItems(target, clearTarget);

        int IInventoryManager.DeleteAllCollections(ICollection<IItemCollection> target, bool clearTarget)
            => DeleteAllCollections(target, clearTarget);

        long IInventoryManager.GetQuantity(StackableInventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));
            return GetQuantity(item);
        }

        bool IInventoryManager.SetQuantity(StackableInventoryItem item, long quantity)
        {
            Tools.ThrowIfArgNull(item, nameof(item));
            Tools.ThrowIfArgNegative(quantity, nameof(quantity));
            return SetQuantity(item, quantity);
        }

        long IInventoryManager.GetTotalQuantity(InventoryItemDefinition itemDefinition)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));
            return GetTotalQuantity(itemDefinition);
        }
    }
}
