using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Container for <see cref="InventoryItem"/> instances.
    ///     Could be used for contents of a backpack, NPC inventory, etc.
    /// </summary>
    public class ItemList : IItemCollection, ICollection<InventoryItem>
    {
        readonly string m_Id;
        List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        ///     Constructor with readonly id spec.
        /// </summary>
        [ExcludeFromDocs]
        internal ItemList(string id)
        {
            m_Id = id;
        }

        /// <inheritdoc/>
        public bool hasBeenDiscarded { get; private set; }

        /// <summary>
        ///     Throws a <see cref="NullReferenceException"/> if this <see cref="ItemList"/> is discarded.
        /// </summary>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertActive()
        {
            if (hasBeenDiscarded)
            {
                throw new NullReferenceException(
                    $"Item already disposed of for id: {m_Id}. " +
                    "Be sure to release all references to inventory objects when they are removed or manager is reset.");
            }
        }

        /// <summary>
        ///     Mark this <see cref="ItemList"/> as discarded, usually due to <see cref="IInventoryManager"/> reset,
        ///     serialization, etc.
        /// </summary>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void MarkDiscardedInternal()
        {
            hasBeenDiscarded = true;
        }

        /// <inheritdoc/>
        public string id => m_Id;

        /// <summary>
        ///     Count of <see cref="InventoryItem"/>s in this <see cref="ItemList"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public int Count
        {
            get
            {
                AssertActive();
                return m_InventoryItems.Count;
            }
        }

        /// <summary>
        ///     Adds specified <see cref="InventoryItem"/>.
        ///     Note: <see cref="ItemList"/> can only contain 1 copy of any item so subsequent
        ///     <see cref="Add(InventoryItem)"/> calls are ignored.
        /// </summary>
        /// <param name="inventoryItem">
        ///     Item to add to this <see cref="ItemList"/>.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already
        ///     been removed from the <see cref="IInventoryManager"/>.
        /// </exception>
        public void Add(InventoryItem inventoryItem)
        {
            AssertActive();

            // if inventory item is not already in this collection
            // note: checking inventoryItem's list of collections instead of my collection of items
            // since it's likely that inventory item is in few to no lists so probably faster on average
            if (!inventoryItem.IsInCollection(this))
            {
                // add the item to the list of inventory items and this list to item's list of collections
                AddInternal(inventoryItem);

                // sync adding the item to the data layer
                (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemListAddItem(id, inventoryItem.id);
            }
        }

        /// <summary>
        ///     Add specified inventory item to list if items in the <see cref="ItemList"/> and add this
        ///     <see cref="ItemList"/> to item's list of <see cref="IItemCollection"/>s.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to add to this <see cref="ItemList"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddInternal(InventoryItem inventoryItem)
        {
            // add the item to my list of items in this list
            m_InventoryItems.Add(inventoryItem);

            // add this collection to list of collections referenced by the item
            inventoryItem.AddToCollectionInternal(this);
        }

        /// <summary>
        ///     Adds all <see cref="InventoryItem"/> in <see cref="IEnumerable"/>.
        ///     Note: <see cref="ItemList"/> can only contain 1 copy of any item so any
        ///     <see cref="Add(InventoryItem)"/> methods already contained in <see cref="ItemList"/> are ignored.
        /// </summary>
        /// <param name="inventoryItems">
        ///     <see cref="IEnumerable"/> of all <see cref="InventoryItem"/>s to add to this <see cref="ItemList"/>.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from th
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public void Add(IEnumerable<InventoryItem> inventoryItems)
        {
            AssertActive();

            foreach (var inventoryItem in inventoryItems)
            {
                Add(inventoryItem);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(InventoryItem inventoryItem)
        {
            AssertActive();

            // check if this list is in inventory item's list of collections.
            // note: using item for performance as there are likely less collections in 1 item than
            //  ...  there are items in 1 list.
            return inventoryItem.IsInCollection(this);
        }

        /// <inheritdoc/>
        public int GetItems(ICollection<InventoryItem> target = null, bool clearTarget = true)
        {
            AssertActive();

            return Tools.Copy(m_InventoryItems, target, clearTarget);
        }

        /// <summary>
        ///     Retrieve <see cref="InventoryItem"/> by indexer.
        /// </summary>
        /// <param name="index">
        ///     Index into <see cref="ItemList"/> to retrieve.
        /// </param>
        /// <returns>
        ///     Specified <see cref="InventoryItem"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public InventoryItem this[int index]
        {
            get
            {
                AssertActive();
                return m_InventoryItems[index];
            }
        }

        /// <inheritdoc/>
        public int Find(InventoryItemDefinition definition) => Find(definition, null);

        /// <inheritdoc/>
        public int Find(InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));

            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in m_InventoryItems)
            {
                if (ReferenceEquals(item.definition, definition))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find(Tag tag) => Find(tag, null);

        /// <inheritdoc/>
        public int Find(Tag tag, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var item in m_InventoryItems)
            {
                if (item.definition.HasTag(tag))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find(Predicate<InventoryItem> filter) => Find(filter, null);

        /// <inheritdoc/>
        public int Find<TState>(Func<TState, InventoryItem, bool> filter, TState state) => Find(filter, state, null);

        /// <inheritdoc/>
        public int Find(Predicate<InventoryItem> filter, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in m_InventoryItems)
            {
                if (filter(item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in m_InventoryItems)
            {
                if (filter(state, item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        ///     Swap 2 items in the <see cref="ItemList"/> by index.
        /// </summary>
        /// <param name="index0">
        ///     Index of first <see cref="InventoryItem"/> to swap.
        /// </param>
        /// <param name="index1">
        ///     Index of <see cref="InventoryItem"/> to swap with.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public void Swap(int index0, int index1)
        {
            AssertActive();

            // do nothing if self-swapping
            if (index0 == index1)
            {
                return;
            }

            // swap the 2 items
            var inventoryItem = m_InventoryItems[index0];
            m_InventoryItems[index0] = m_InventoryItems[index1];
            m_InventoryItems[index1] = inventoryItem;

            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemListSwapIndexes(id, index0, index1);
        }

        /// <inheritdoc/>
        public long GetTotalQuantity()
        {
            long quantity = 0;
            foreach (var item in this)
            {
                if (item is StackableInventoryItem stackable)
                {
                    quantity += stackable.quantity;
                }
                else
                {
                    ++quantity;
                }
            }

            return quantity;
        }

        /// <inheritdoc/>
        public long GetTotalQuantity(InventoryItemDefinition definition)
        {
            long quantity = 0;
            foreach (var item in this)
            {
                if (ReferenceEquals(definition, item.definition))
                {
                    if (item is StackableInventoryItem stackable)
                    {
                        quantity += stackable.quantity;
                    }
                    else
                    {
                        ++quantity;
                    }
                }
            }

            return quantity;
        }

        /// <summary>
        ///     Remove specified <see cref="ItemList"/> item by index.
        /// </summary>
        /// <param name="index">
        ///     Index to remove.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public void Remove(int index)
        {
            AssertActive();

            // remove this list from inventory item's list of IItemCollections
            var item = m_InventoryItems[index];

            // remove item's reference to this collection from item itself
            item.RemoveFromCollectionInternal(this);

            // remove item from the list
            m_InventoryItems.RemoveAt(index);

            // sync the removal with data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemListRemoveItem(id, item.id);
        }

        /// <inheritdoc/>
        public bool Remove(InventoryItem inventoryItem)
        {
            AssertActive();
            Tools.ThrowIfArgNull(inventoryItem, nameof(inventoryItem));

            var index = IndexOf(inventoryItem);
            if (index < 0)
            {
                return false;
            }

            Remove(index);

            return true;
        }

        /// <inheritdoc/>
        public bool Remove(InventoryItemDefinition definition)
        {
            AssertActive();
            Tools.ThrowIfArgNull(definition, nameof(definition));

            var removedFlag = false;

            for (int index = 0; index < m_InventoryItems.Count;)
            {
                if (ReferenceEquals(definition,
                    m_InventoryItems[index].definition))
                {
                    Remove(index);
                    removedFlag = true;
                }
                else
                {
                    ++index;
                }
            }

            return removedFlag;
        }

        /// <summary>
        ///     Called by <see cref="InventoryItem"/> when deleted to remove itself from this <see cref="ItemList"/>.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to remove from this <see cref="ItemList"/>.
        /// </param>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveInternal(InventoryItem inventoryItem)
        {
            m_InventoryItems.Remove(inventoryItem);

            // sync the removal with data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemListRemoveItem(id, inventoryItem.id);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            AssertActive();

            foreach (var item in m_InventoryItems)
            {
                item.RemoveFromCollectionInternal(this);
            }

            m_InventoryItems.Clear();

            // sync clear with data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemListClear(id);
        }

        /// <summary>
        ///     Find index of specified <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to find.
        /// </param>
        /// <returns>
        ///     Index of specified <see cref="InventoryItem"/> or -1 if not found.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(InventoryItem inventoryItem)
        {
            AssertActive();

            return m_InventoryItems.IndexOf(inventoryItem);
        }

        /// <summary>
        ///     Helper method to create a new <see cref="InventoryItem"/> and automatically add it to this
        ///     <see cref="ItemList"/>.
        /// </summary>
        /// <param name="itemDefinition">
        ///     <see cref="InventoryItemDefinition"/> to use to create <see cref="InventoryItem"/>.
        /// </param>
        /// <returns>
        ///     Newly-created <see cref="InventoryItem"/> in this <see cref="ItemList"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public InventoryItem CreateItem(InventoryItemDefinition itemDefinition)
        {
            AssertActive();

            var newItem = GameFoundationSdk.inventory.CreateItem(itemDefinition);
            Add(newItem);

            return newItem;
        }

        /// <summary>
        ///     Copy the entire <see cref="ItemList"/> to a compatible one-dimensional <see cref="Array"/>, starting at
        ///     the specified index of the target <see cref="Array"/>.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="Array"/>y that is the destination of the elements copied from thi
        ///     <see cref="ItemList"/>.
        ///     The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        ///     The zero-based index in <see cref="Array"/> at which copying begins.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public void CopyTo(InventoryItem[] array, int arrayIndex = 0)
        {
            AssertActive();
            m_InventoryItems.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Get <see cref="IEnumerator"/> to facilitate iteration through this <see cref="ItemList"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="IEnumerator"/> to permit iteration through this <see cref="ItemList"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Get <see cref="Enumerator"/> to facilitate iteration through this <see cref="ItemList"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="Enumerator"/> to permit iteration through this <see cref="ItemList"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Get <see cref="IEnumerator{InventoryItem}"/> to facilitate iteration through this <see cref="ItemList"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="IEnumerator{InventoryItem}"/> to facilitate iteration through this <see cref="ItemList"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemList"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<InventoryItem> IEnumerable<InventoryItem>.GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="ItemList"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        ///     <see cref="Enumerator"/> for <see cref="ItemList"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<InventoryItem>, IEnumerator, IDisposable
        {
            ItemList m_ItemList;
            int m_CurrentIndex;

            /// <summary>
            ///     Constructor for <see cref="Enumerator"/> class for this <see cref="ItemList"/>.
            /// </summary>
            /// <param name="itemList">
            ///     Owner of this <see cref="Enumerator"/>.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ItemList itemList)
            {
                m_ItemList = itemList;
                m_CurrentIndex = -1;
            }

            /// <summary>
            ///     Enable foreach functionality for this <see cref="Enumerator"/> for this <see cref="ItemList"/>.
            /// </summary>
            /// <returns>
            ///     <c>true</c> if there is another element in the list, else <c>false</c>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ++m_CurrentIndex;
                return m_CurrentIndex < m_ItemList.Count;
            }

            /// <summary>
            ///     Reset <see cref="Enumerator"/> to permit iterating this <see cref="ItemList"/> again from the start.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_CurrentIndex = -1;
            }

            /// <summary>
            ///     Required method for <see cref="IEnumerator"/> interface.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>
            ///     Current <see cref="InventoryItem"/> that this <see cref="Enumerator"/> refers to.
            /// </summary>
            public InventoryItem Current => m_ItemList[m_CurrentIndex];

            InventoryItem IEnumerator<InventoryItem>.Current => m_ItemList[m_CurrentIndex];

            object IEnumerator.Current => m_ItemList[m_CurrentIndex];
        }
    }
}
