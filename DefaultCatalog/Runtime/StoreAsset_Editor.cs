#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine.GameFoundation.Exceptions;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class StoreAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "Store";

        /// <summary>
        ///     Adds a <paramref name="transaction"/> to the store.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="BaseTransactionAsset"/> to add.
        /// </param>
        /// <param name="enabled">
        ///     A flag to determine if the added item is enabled.
        /// </param>
        internal void Editor_AddItem(BaseTransactionAsset transaction, bool enabled = true)
        {
            GFTools.ThrowIfArgNull(transaction, nameof(transaction));

            if (Contains(transaction))
            {
                throw new GameFoundationException($"{nameof(StoreAsset)}: {nameof(BaseTransactionAsset)} " +
                    $"'{transaction.displayName}' cannot be added because it already exists in this store.");
            }

            var storeItem = new StoreItemObject();
            storeItem.store = this;
            storeItem.m_Transaction = transaction;
            storeItem.m_Enabled = enabled;

            m_StoreItems.Add(storeItem);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Enables or disables the transaction.
        /// </summary>
        /// <param name="transaction">
        ///     The transaction to enable/disable.
        /// </param>
        /// <param name="enabled">
        ///     The enable value.
        /// </param>
        public void Editor_SetEnable(BaseTransactionAsset transaction, bool enabled = true)
        {
            GFTools.ThrowIfArgNull(transaction, nameof(transaction));

            foreach (var item in m_StoreItems)
            {
                if (item.m_Transaction == transaction)
                {
                    if (item.m_Enabled != enabled)
                    {
                        item.m_Enabled = enabled;
                        EditorUtility.SetDirty(this);
                    }
                }
            }
        }

        /// <summary>
        ///     Removes a <paramref name="storeItem"/> from the store.
        /// </summary>
        /// <param name="storeItem">
        ///     The <see cref="StoreItemObject"/> to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if removed, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveItem(StoreItemObject storeItem)
        {
            GFTools.ThrowIfArgNull(storeItem, nameof(storeItem));
            var removed = m_StoreItems.Remove(storeItem);

            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }

        /// <summary>
        ///     Swaps <paramref name="storeItem1"/> with <paramref name="storeItem2"/> in the store.
        /// </summary>
        /// <param name="storeItem1">
        ///     The <see cref="StoreItemObject"/> to swap.
        /// </param>
        /// <param name="storeItem2">
        ///     The <see cref="StoreItemObject"/> to swap the first with.
        /// </param>
        internal void Editor_SwapItemsListOrder(StoreItemObject storeItem1, StoreItemObject storeItem2)
        {
            GFTools.ThrowIfArgNull(storeItem1, nameof(storeItem1));
            GFTools.ThrowIfArgNull(storeItem2, nameof(storeItem2));

            var index1 = m_StoreItems.IndexOf(storeItem1);
            var index2 = m_StoreItems.IndexOf(storeItem2);

            m_StoreItems[index1] = storeItem2;
            m_StoreItems[index2] = storeItem1;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Clean the store by removing the <see cref="StoreItemObject"/> if
        ///     they contain the reference to the removed <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">
        ///     The transaction that has just been removed.
        /// </param>
        internal void Editor_HandleTransactionRemoved(BaseTransactionAsset transaction)
        {
            for (var i = 0; i < m_StoreItems.Count;)
            {
                var storeItem = m_StoreItems[i];
                if (storeItem.transaction == transaction)
                {
                    m_StoreItems.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(StoreAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            if (!(target is StoreAsset storeTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(StoreAsset)}: The target object {target.displayName} of type '{target.GetType()}' " +
                    $"could not be cast to {GetType()}.");
            }

            foreach (var storeItem in m_StoreItems)
            {
                storeTarget.Editor_AddItem(storeItem.transaction, storeItem.enabled);
            }

            base.CopyValues(storeTarget);
        }

        /// <inheritdoc/>
        internal override void RefreshReferences(CatalogAsset owner)
        {
            base.RefreshReferences(owner);

            foreach (var storeItem in m_StoreItems)
            {
                storeItem.RefreshReferences(owner);
            }
        }
    }
}

#endif
