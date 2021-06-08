using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Container for <see cref="InventoryItem"/>s.
    ///     Could be used for items attached to body parts, distributed around a room/dungeon, assigned to specific
    ///     NPCs, etc.
    /// </summary>
    public class ItemMap : IItemCollection
    {
        readonly string m_Id;
        Dictionary<string, InventoryItem> m_InventoryItems = new Dictionary<string, InventoryItem>();

        /// <summary>
        ///     Constructor with readonly id spec.
        /// </summary>
        [ExcludeFromDocs]
        internal ItemMap(string id)
        {
            m_Id = id;
        }

        /// <inheritdoc/>
        public bool hasBeenDiscarded { get; private set; }

        /// <summary>
        ///     Throws a <see cref="NullReferenceException"/> if this <see cref="ItemMap"/> is discarded.
        /// </summary>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
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
        ///     Mark this <see cref="ItemMap"/> as discarded, usually due to <see cref="IInventoryManager"/> reset,
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
        ///     Count of <see cref="InventoryItem"/>s in this <see cref="ItemMap"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
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
        ///     Find item associated with slot.
        /// </summary>
        /// <param name="slot">
        ///     Slot to find.
        /// </param>
        /// <returns>
        ///     <see cref="InventoryItem"/> associated with slot or null if not found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InventoryItem Get(string slot)
        {
            AssertActive();

            m_InventoryItems.TryGetValue(slot, out var inventoryItem);

            return inventoryItem;
        }

        /// <summary>
        ///     Finds slot (or null) for specified <see cref="InventoryItem"/> by reference.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to find.
        /// </param>
        /// <returns>
        ///     Slot used for specified <see cref="InventoryItem"/> or null if not found.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <see cref="InventoryItem"/> is <c>null</c>.
        /// </exception>
        public string GetSlot(InventoryItem inventoryItem)
        {
            AssertActive();
            Tools.ThrowIfArgNull(inventoryItem, nameof(inventoryItem));

            // find the value
            foreach (var kv in m_InventoryItems)
            {
                if (ReferenceEquals(kv.Value, inventoryItem))
                {
                    return kv.Key;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds specified <see cref="InventoryItem"/> to this <see cref="ItemMap"/> using specified slot.
        ///     Important: <see cref="InventoryItem"/>s can only have ONE instance in the <see cref="ItemMap"/> and slot
        ///     can only be Add-ed once.
        ///     Please use Indexer to change value for a slot, or <see cref="ChangeSlot(string, string)"/> to change the
        ///     slot for an <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="slot">
        ///     Slot to use for this <see cref="InventoryItem"/>.
        /// </param>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to add based on specified slot.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     If item is added again with different slot.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string slot, InventoryItem inventoryItem)
        {
            AssertActive();

            // make sure item is not added more than once
            if (CheckItemAlreadyExistsInternal(inventoryItem, slot))
            {
                return;
            }

            // add the item
            AddInternal(slot, inventoryItem);

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapAddItem(id, slot, inventoryItem.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddInternal(string slot, InventoryItem inventoryItem)
        {
            // add the item
            m_InventoryItems.Add(slot, inventoryItem);

            // add this collection to the item's list of collections
            inventoryItem.AddToCollectionInternal(this);
        }

        /// <inheritdoc/>
        public int GetItems(ICollection<InventoryItem> target = null, bool clearTarget = true)
        {
            AssertActive();

            return Tools.Copy(m_InventoryItems.Values, target, clearTarget);
        }

        /// <summary>
        ///     Check if specified slot is currently in this <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="slot">
        ///     Slot to search for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if specified slot is found, else <c>false</c>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSlotSet(string slot)
        {
            AssertActive();

            return m_InventoryItems.ContainsKey(slot);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(InventoryItem inventoryItem)
        {
            AssertActive();

            // check if this map is in inventory item's list of collections
            // note: using item for performance as there are likely less collections in 1 item than
            //  ...  there are items in 1 list.
            return inventoryItem.IsInCollection(this);
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified slot.
        /// </summary>
        /// <param name="slot">
        ///     The slot of the <see cref="InventoryItem"/> to get or set.
        /// </param>
        /// <returns>
        ///     <see cref="InventoryItem"/> associated with desired slot.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If slot or <see cref="InventoryItem"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If item is added again with different slot.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public InventoryItem this[string slot]
        {
            get { return Get(slot); }

            set
            {
                // remove item already in the slot (if any)
                Unset(slot);

                // set the new value
                Set(slot, value);
            }
        }

        /// <inheritdoc/>
        public int Find(InventoryItemDefinition definition) => Find(definition, (ICollection<InventoryItem>)null);

        /// <inheritdoc/>
        public int Find(InventoryItemDefinition definition, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));

            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var kv in m_InventoryItems)
            {
                if (ReferenceEquals(kv.Value.definition, definition))
                {
                    count++;
                    target?.Add(kv.Value);
                }
            }

            return count;
        }

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances of the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="InventoryItemDefinition"/> filter.
        /// </param>
        /// <param name="target">
        ///     The target collection the (<see cref="string"/> slot, <see cref="InventoryItem"/> item) tuples are
        ///     copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found, with their related slot.
        /// </returns>
        public int Find(
            InventoryItemDefinition definition,
            ICollection<(string slot, InventoryItem item)> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));

            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var kv in m_InventoryItems)
            {
                if (ReferenceEquals(kv.Value.definition, definition))
                {
                    count++;
                    target?.Add((kv.Key, kv.Value));
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find(Tag tag) => Find(tag, (ICollection<InventoryItem>)null);

        /// <inheritdoc/>
        public int Find(Tag tag, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var kv in m_InventoryItems)
            {
                if (kv.Value.definition.HasTag(tag))
                {
                    count++;
                    target?.Add(kv.Value);
                }
            }

            return count;
        }

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances using the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> filter.
        /// </param>
        /// <param name="target">
        ///     The target collection the (<see cref="string"/> slot, <see cref="InventoryItem"/> item) tuples are
        ///     copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found, with their related slot.
        /// </returns>
        public int Find(Tag tag, ICollection<(string slot, InventoryItem item)> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            AssertActive();

            if (clearTarget) target?.Clear();

            var count = 0;
            foreach (var kv in m_InventoryItems)
            {
                if (kv.Value.definition.HasTag(tag))
                {
                    count++;
                    target?.Add((kv.Key, kv.Value));
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find(Predicate<InventoryItem> filter) => Find(filter, (ICollection<InventoryItem>)null);

        /// <inheritdoc/>
        public int Find(Predicate<InventoryItem> filter, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var item in m_InventoryItems.Values)
            {
                if (filter(item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances passing the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">
        ///     The filter the <see cref="InventoryItem"/> instances have to fulfill.
        /// </param>
        /// <param name="target">
        ///     The target collection the (<see cref="string"/> slot, <see cref="InventoryItem"/> item) tuples are
        ///     copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of items found, with their related slot.
        /// </returns>
        public int Find(
            Predicate<InventoryItem> filter,
            ICollection<(string slot, InventoryItem item)> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(kvp.Value))
                {
                    count++;
                    target?.Add((kvp.Key, kvp.Value));
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public int Find<TState>(Func<TState, InventoryItem, bool> filter, TState state)
            => Find(filter, state, (ICollection<InventoryItem>)null);

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

            foreach (var item in m_InventoryItems.Values)
            {
                if (filter(state, item))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <inheritdoc cref="Find{TState}(Func{TState, InventoryItem, bool}, TState)"/>
        /// <param name="target">
        ///     The target collection the (<see cref="string"/> slot, <see cref="InventoryItem"/> item) tuples are
        ///     copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        public int Find<TState>(
            Func<TState, InventoryItem, bool> filter,
            TState state,
            ICollection<(string slot, InventoryItem item)> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(state, kvp.Value))
                {
                    count++;
                    target?.Add((kvp.Key, kvp.Value));
                }
            }

            return count;
        }

        /// <summary>
        ///     Finds all the <see cref="InventoryItem"/> instances passing the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">
        ///     The filter the <see cref="InventoryItem"/> instances have to fulfill.
        ///     This filter takes the slot (<see cref="string"/>) and the <see cref="InventoryItem"/> instance, and it
        ///     returns a <see cref="bool"/> if they meet its expectations.
        /// </param>
        /// <returns>
        ///     The number of items found, with their related slot.
        /// </returns>
        public int Find(Func<string, InventoryItem, bool> filter) => Find(filter, (ICollection<InventoryItem>)null);

        /// <inheritdoc cref="Find(Func{string, InventoryItem, bool})"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        public int Find
            (Func<string, InventoryItem, bool> filter, ICollection<InventoryItem> target, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(kvp.Key, kvp.Value))
                {
                    count++;
                    target?.Add(kvp.Value);
                }
            }

            return count;
        }

        /// <inheritdoc cref="Find(Func{string, InventoryItem, bool})"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        public int Find(
            Func<string, InventoryItem, bool> filter,
            ICollection<(string slot, InventoryItem item)> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(kvp.Key, kvp.Value))
                {
                    count++;
                    target?.Add((kvp.Key, kvp.Value));
                }
            }

            return count;
        }

        /// <summary>
        ///     Finds the <see cref="InventoryItem"/> passing the filter.
        /// </summary>
        /// <typeparam name="TState">
        ///     The type of the <paramref name="state"/> object.
        /// </typeparam>
        /// <param name="filter">
        ///     The filter the <see cref="InventoryItem"/> instances have to fulfill.
        ///     This filter takes the <paramref name="state"/> ob ject, the slot (<see cref="string"/>) and the
        ///     <see cref="InventoryItem"/> instance, and it returns a <see cref="bool"/> if they meet its expectations.
        /// </param>
        /// <param name="state">
        ///     The data used by the filter.
        /// </param>
        /// <returns>
        ///     The number of <see cref="InventoryItem"/> found.
        /// </returns>
        public int Find<TState>(Func<TState, string, InventoryItem, bool> filter, TState state)
            => Find(filter, state, (ICollection<InventoryItem>)null);

        /// <inheritdoc cref="Find{TState}(Func{TState, string, InventoryItem, bool}, TState)"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        public int Find<TState>(
            Func<TState, string, InventoryItem, bool> filter,
            TState state,
            ICollection<InventoryItem> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(state, kvp.Key, kvp.Value))
                {
                    count++;
                    target?.Add(kvp.Value);
                }
            }

            return count;
        }

        /// <inheritdoc cref="Find{TState}(Func{TState, string, InventoryItem, bool}, TState)"/>
        /// <param name="target">
        ///     The target collection the <see cref="InventoryItem"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        public int Find<TState>(
            Func<TState, string, InventoryItem, bool> filter,
            TState state,
            ICollection<(string slot, InventoryItem item)> target,
            bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(filter, nameof(filter));

            if (clearTarget) target?.Clear();

            var count = 0;

            foreach (var kvp in m_InventoryItems)
            {
                if (filter(state, kvp.Key, kvp.Value))
                {
                    count++;
                    target?.Add((kvp.Key, kvp.Value));
                }
            }

            return count;
        }

        /// <summary>
        ///     Change the slot for specified item in <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="oldSlot">
        /// </param>
        /// <param name="newSlot">
        /// </param>
        /// <returns>
        ///     true if item was found and swapped, else false.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If either argument is null.
        /// </exception>
        public bool ChangeSlot(string oldSlot, string newSlot)
        {
            AssertActive();
            Tools.ThrowIfArgNull(oldSlot, nameof(oldSlot));
            Tools.ThrowIfArgNull(newSlot, nameof(newSlot));

            if (String.Equals(oldSlot, newSlot))
            {
                return false;
            }

            if (IsSlotSet(newSlot))
            {
                return false;
            }

            var item = this[oldSlot];
            if (item is null)
            {
                return false;
            }

            m_InventoryItems.Remove(oldSlot);
            m_InventoryItems.Add(newSlot, item);

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapChangeSlot(id, oldSlot, newSlot);

            return true;
        }

        /// <summary>
        ///     Swap 2 items in the <see cref="ItemMap"/> by slot string.
        /// </summary>
        /// <param name="slot0">
        ///     Slot of item to swap.
        /// </param>
        /// <param name="slot1">
        ///     Slot of item to swap with.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If either argument is <c>null</c>.
        /// </exception>
        /// <returns>
        ///     <c>true</c> if the swap is successful, <c>false</c> otherwise.
        /// </returns>
        public bool SwapSlots(string slot0, string slot1)
        {
            AssertActive();
            Tools.ThrowIfArgNull(slot0, nameof(slot0));
            Tools.ThrowIfArgNull(slot1, nameof(slot1));

            if (String.Equals(slot0, slot1))
            {
                return true;
            }

            if (!m_InventoryItems.TryGetValue(slot0, out var item0) ||
                !m_InventoryItems.TryGetValue(slot1, out var item1))
            {
                return false;
            }

            m_InventoryItems[slot0] = item1;
            m_InventoryItems[slot1] = item0;

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapSwapSlots(id, slot0, slot1);

            return true;
        }

        /// <summary>
        ///     Remove specified slot from this <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="slot">
        ///     Slot to remove.
        /// </param>
        /// <returns>
        ///     true if slot was found and removed, else false.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unset(string slot)
        {
            AssertActive();

            if (!m_InventoryItems.TryGetValue(slot, out var inventoryItem))
            {
                return false;
            }

            inventoryItem.RemoveFromCollectionInternal(this);

            if (!m_InventoryItems.Remove(slot))
            {
                return false;
            }

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapRemoveItem(id, slot);

            return true;
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

        /// <inheritdoc/>
        public bool Remove(InventoryItem inventoryItem)
        {
            AssertActive();
            Tools.ThrowIfArgNull(inventoryItem, nameof(inventoryItem));

            // find the value
            foreach (var kv in m_InventoryItems)
            {
                if (ReferenceEquals(kv.Value, inventoryItem))
                {
                    var slot = kv.Key;

                    // remove item's reference to this collection from item itself
                    inventoryItem.RemoveFromCollectionInternal(this);

                    // remove the item from the dictionary
                    m_InventoryItems.Remove(slot);

                    // sync to data layer
                    (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapRemoveItem(id, slot);

                    return true;
                }
            }

            return false;
        }

        [ThreadStatic]
        static List<string> s_TempSlotsList = new List<string>();

        /// <inheritdoc/>
        public bool Remove(InventoryItemDefinition definition)
        {
            AssertActive();
            Tools.ThrowIfArgNull(definition, nameof(definition));

            // find all the matching items
            foreach (var kv in m_InventoryItems)
            {
                // for each matching item definition found
                if (ReferenceEquals(definition, kv.Value.definition))
                {
                    // remove the item's reference to this collection
                    kv.Value.RemoveFromCollectionInternal(this);

                    // add to list of slots to be removed
                    s_TempSlotsList.Add(kv.Key);
                }
            }

            // if no matches found, return failure
            if (s_TempSlotsList.Count == 0)
            {
                return false;
            }

            // for each match, remove the key from the dictionary
            foreach (var slot in s_TempSlotsList)
            {
                m_InventoryItems.Remove(slot);

                // sync to data layer
                (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapRemoveItem(id, slot);
            }

            // clear out the keys list for next time
            s_TempSlotsList.Clear();

            return true;
        }

        /// <summary>
        ///     Called by <see cref="InventoryItem"/> when deleted to remove itself from this <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> to remove from this <see cref="ItemMap"/>.
        /// </param>
        [ExcludeFromDocs]
        internal void RemoveInternal(InventoryItem inventoryItem)
        {
            foreach (var kv in m_InventoryItems)
            {
                if (!ReferenceEquals(kv.Value, inventoryItem))
                {
                    continue;
                }

                m_InventoryItems.Remove(kv.Key);

                ((InventoryManagerImpl)GameFoundationSdk.inventory).SyncItemMapRemoveItem(id, kv.Key);

                return;
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            AssertActive();

            foreach (var kv in m_InventoryItems)
            {
                kv.Value.RemoveFromCollectionInternal(this);
            }

            m_InventoryItems.Clear();

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapClear(id);
        }

        /// <summary>
        ///     Helper method to create a new <see cref="InventoryItem"/> and automatically add it to this <see cref="ItemMap"/>.
        /// </summary>
        /// <param name="slot">
        ///     Slot to use when adding new <see cref="InventoryItem"/> to this <see cref="ItemMap"/> for later retrieval.
        /// </param>
        /// <param name="itemDefinition">
        ///     <see cref="InventoryItemDefinition"/> to use to create <see cref="InventoryItem"/>.
        /// </param>
        /// <returns>
        ///     Newly-created <see cref="InventoryItem"/> added to this <see cref="ItemMap"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        public InventoryItem CreateItem(string slot, InventoryItemDefinition itemDefinition)
        {
            AssertActive();

            if (IsSlotSet(slot))
            {
                throw new InvalidOperationException("Cannot CreateItem with same slot as item already in ItemMap.");
            }

            var newItem = GameFoundationSdk.inventory.CreateItem(itemDefinition);

            // add the item
            m_InventoryItems.Add(slot, newItem);

            // add this collection to the item's list of collections
            newItem.AddToCollectionInternal(this);

            // sync to data layer
            (GameFoundationSdk.inventory as InventoryManagerImpl).SyncItemMapAddItem(id, slot, newItem.id);

            return newItem;
        }

        /// <summary>
        ///     Return false if item is NOT already in map, else throw if slot changes  or return true for item already
        ///     in map with same slot.
        /// </summary>
        /// <param name="slot">
        ///     Proposed new slot for this item.
        /// </param>
        /// <param name="inventoryItem">
        ///     <see cref="InventoryItem"/> for which to search.
        /// </param>
        /// <returns>
        ///     <c>false</c> if item is NOT already in the map, else <c>true</c> (throws if slot changes).
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     If item is already in map with a different slot.
        /// </exception>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CheckItemAlreadyExistsInternal(InventoryItem inventoryItem, string slot)
        {
            // if not already in the map then return false (ie not in collection)
            if (!inventoryItem.IsInCollection(this))
            {
                return false;
            }

            // if item already in map with exactly the same key then return false
            var oldInventoryItem = Get(slot);
            if (ReferenceEquals(inventoryItem, oldInventoryItem))
            {
                return true;
            }

            // prevent item being added repeatedly to the map with a different key
            throw new InvalidOperationException("Item can only be added once to a map");
        }

        /// <summary>
        ///     Get <see cref="IEnumerator"/> to facilitate iteration through this <see cref="ItemMap"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="IEnumerator"/> to permit iteration through this <see cref="ItemMap"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Get <see cref="Enumerator"/> to facilitate iteration through this <see cref="ItemMap"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="Enumerator"/> to permit iteration through this <see cref="ItemMap"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     Get <see cref="IEnumerator{InventoryItem}"/> to facilitate iteration through this <see cref="ItemMap"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="IEnumerator{InventoryItem}"/> to facilitate iteration through this <see cref="ItemMap"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this <see cref="ItemMap"/> is being used after it has already been removed from the
        ///     <see cref="IInventoryManager"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<InventoryItem> IEnumerable<InventoryItem>.GetEnumerator()
        {
            AssertActive();
            return new Enumerator(this);
        }

        /// <summary>
        ///     <see cref="Enumerator"/> for <see cref="ItemMap"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<InventoryItem>, IEnumerator, IDisposable
        {
            ItemMap m_ItemMap;

            Dictionary<string, InventoryItem>.Enumerator m_Enumerator;

            /// <summary>
            ///     Constructor for Enumerator class for ItemMap.
            /// </summary>
            /// <param name="itemMap">
            ///     Owner of this Enumerator.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ItemMap itemMap)
            {
                m_ItemMap = itemMap;
                m_Enumerator = itemMap.m_InventoryItems.GetEnumerator();
            }

            /// <summary>
            ///     Enable foreach functionality for this enumerator for this ItemMap.
            /// </summary>
            /// <returns>
            ///     <c>true</c> if there is another element in the map, else <c>false</c>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            /// <summary>
            ///     Reset enumerator to permit iterating the map again from the start.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_Enumerator = m_ItemMap.m_InventoryItems.GetEnumerator();
            }

            /// <summary>
            ///     Required method for IEnumerator interface.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>
            ///     Current <see cref="InventoryItem"/> that this Enumerator refers to.
            /// </summary>
            public InventoryItem Current => m_Enumerator.Current.Value;

            InventoryItem IEnumerator<InventoryItem>.Current => m_Enumerator.Current.Value;

            object IEnumerator.Current => m_Enumerator.Current.Value;
        }
    }
}
