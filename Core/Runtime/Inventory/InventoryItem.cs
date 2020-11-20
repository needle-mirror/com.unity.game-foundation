using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Item handled by the <see cref="IInventoryManager"/>.
    /// </summary>
    public partial class InventoryItem : IEquatable<InventoryItem>, IComparable<InventoryItem>, ITradable
    {
        /// <summary>
        ///     The unique identifier of this item.
        /// </summary>
        string m_Id;

        /// <inheritdoc cref="m_Id"/>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public string id
        {
            get
            {
                AssertActive();

                return m_Id;
            }
        }

        /// <summary>
        ///     A session-wide identifier used by <seealso cref="ItemLookup"/> to ease internal searches.
        /// </summary>
        int m_InstanceId;

        /// <inheritdoc cref="m_InstanceId"/>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        internal int instanceId
        {
            get
            {
                AssertActive();

                return m_InstanceId;
            }
            set => m_InstanceId = value;
        }

        /// <summary>
        ///     The definition used to create this item.
        /// </summary>
        InventoryItemDefinition m_Definition;

        /// <inheritdoc cref="m_Definition"/>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public InventoryItemDefinition definition
        {
            get
            {
                AssertActive();

                return m_Definition;
            }
        }

        /// <summary>
        ///     Determines if this item has been discarded (removed from Game Foundation).
        ///     Items being standard objects, they cannot be destroyed and garbage collected as long as all their
        ///     references are not set to <c>null</c>.
        ///     This property is a way for you to know if the object is still active within Game Foundation.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the item has been removed from Game Foundation.
        /// </returns>
        public bool hasBeenDiscarded { get; private set; }

        /// <summary>
        ///     List of all <see cref="IItemCollection"/> (either <see cref="ItemList"/> or <see cref="ItemMap"/>
        ///     instances that this item has been added to.
        /// </summary>
        protected List<IItemCollection> m_ItemCollections;

        /// <summary>
        ///     Add this item to specified <see cref="IItemCollection"/>.
        ///     Note: helper--does not check if already in list--caller is responsible for verifying that operation is
        ///     valid.
        /// </summary>
        /// <param name="itemCollection">
        ///     <see cref="IItemCollection"/> that this item is now a part of.
        /// </param>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToCollectionInternal(IItemCollection itemCollection)
        {
            if (m_ItemCollections is null)
            {
                m_ItemCollections = new List<IItemCollection>();
            }

            m_ItemCollections.Add(itemCollection);

            var inventory = GameFoundationSdk.inventory as InventoryManagerImpl;
            inventory?.OnItemAddedToCollection(itemCollection, this);
        }

        /// <summary>
        ///     Remove this item from specified <see cref="IItemCollection"/>.
        /// </summary>
        /// <param name="itemCollection">
        ///     <see cref="IItemCollection"/> that this item is no longer contained in.
        /// </param>
        /// <returns>
        ///     true if item was found and remove, else false.
        /// </returns>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool RemoveFromCollectionInternal(IItemCollection itemCollection)
        {
            if (m_ItemCollections is null || !m_ItemCollections.Remove(itemCollection))
            {
                return false;
            }

            var inventory = GameFoundationSdk.inventory as InventoryManagerImpl;
            inventory?.OnItemRemovedFromCollection(itemCollection, this);

            return true;
        }

        /// <summary>
        ///     Check if specified item is contained within specified <see cref="IItemCollection"/>.
        /// </summary>
        /// <param name="itemCollection">
        ///     <see cref="IItemCollection"/> to check if this <see cref="InventoryItem"/> is within.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this item is in specified collection, else <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsInCollection(IItemCollection itemCollection)
        {
            return m_ItemCollections != null &&
                m_ItemCollections.Contains(itemCollection);
        }

        /// <summary>
        ///     Fill specified <see cref="ICollection{T}"/> with all <see cref="IItemCollection"/>s that this item is
        ///     contained in.
        /// </summary>
        /// <param name="target">
        ///     The target collection the <see cref="IItemCollection"/> instances are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     count of <see cref="IItemCollection"/>s.
        /// </returns>
        public int GetItemCollections(ICollection<IItemCollection> target = null, bool clearTarget = true)
            => Tools.Copy(m_ItemCollections, target, clearTarget);

        /// <summary>
        ///     Remove this <see cref="InventoryItem"/> from all <see cref="IItemCollection"/> instances (if any).
        /// </summary>
        public void RemoveFromAllItemCollections()
        {
            if (m_ItemCollections is null || m_ItemCollections.Count == 0)
            {
                return;
            }

            var inventory = GameFoundationSdk.inventory as InventoryManagerImpl;

            foreach (var collection in m_ItemCollections)
            {
                if (collection is ItemList itemList)
                {
                    itemList.RemoveInternal(this);
                }
                else if (collection is ItemMap itemMap)
                {
                    itemMap.RemoveInternal(this);
                }

                inventory?.OnItemRemovedFromCollection(collection, this);
            }

            m_ItemCollections.Clear();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryItem"/> class.
        /// </summary>
        /// <param name="itemDefinition">
        ///     The definition used to create the item.
        ///     It stores all static data.
        /// </param>
        /// <param name="id">
        ///     The unique identifier of this item.
        /// </param>
        internal InventoryItem(InventoryItemDefinition itemDefinition, string id)
        {
            m_Definition = itemDefinition;
            m_Id = id;
        }

        /// <summary>
        ///     Throws a <see cref="NullReferenceException"/> if this <see cref="InventoryItem"/> is discarded.
        /// </summary>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertActive()
        {
            if (hasBeenDiscarded)
            {
                throw new NullReferenceException(
                    $"Item already disposed of for id: {m_Id}. Be sure to release all references to GameItems, Inventories and InventoryItems when they are removed or manager is reset.");
            }
        }

        /// <summary>
        ///     Remove all references to this item.
        /// </summary>
        internal void Discard()
        {
            if (hasBeenDiscarded) return;

            CleanMutableProperties();

            // ensure this item is not in any item collections
            RemoveFromAllItemCollections();

            // remember the item was discarded so we don't do it again
            hasBeenDiscarded = true;
        }

        /// <summary>
        ///     Gets the hash code of this <see cref="InventoryItem"/> instance.
        ///     Returns the hash code of its <see cref="id"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="InventoryItem"/> instance.
        /// </returns>
        public override int GetHashCode() => id.GetHashCode();

        /// <summary>
        ///     Tells whether this <see cref="InventoryItem"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        /// <inheritdoc cref="IEquatable{T}"/>
        bool IEquatable<InventoryItem>.Equals(InventoryItem other) => ReferenceEquals(this, other);

        /// <inheritdoc cref="IComparable{T}"/>
        int IComparable<InventoryItem>.CompareTo(InventoryItem other) => id.CompareTo(other.id);
    }
}
