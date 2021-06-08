#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class BaseTransactionAsset
    {
        /// <summary>
        ///     Before this transaction is destroyed, remove any references to it from any stores.
        /// </summary>
        protected sealed override void OnBeforeDestroy()
        {
            var storeAssets = new List<StoreAsset>();
            CatalogSettings.catalogAsset.GetItems(storeAssets);

            foreach (var storeAsset in storeAssets)
            {
                storeAsset.Editor_HandleTransactionRemoved(this);
            }

            base.OnBeforeDestroy();
        }

        /// <summary>
        ///     If an inventory item is about to be destroyed, then remove any references to it from this transaction.
        /// </summary>
        /// <param name="inventoryItem">
        ///     The inventory item to remove from this transaction.
        /// </param>
        internal virtual void Editor_HandleInventoryItemRemoved(InventoryItemDefinitionAsset inventoryItem)
        {
            var itemExchanges = m_Payout.m_Exchanges;
            for (var i = 0; i < itemExchanges.Count;)
            {
                var exchange = itemExchanges[i];
                if (exchange.catalogItem == inventoryItem)
                {
                    itemExchanges.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        ///     If a currency is about to be destroyed, then remove any references to it from this transaction.
        /// </summary>
        /// <param name="currency">
        ///     The currency to remove from this transaction.
        /// </param>
        internal virtual void Editor_HandleCurrencyRemoved(CurrencyAsset currency)
        {
            var currencyExchanges = m_Payout.m_Exchanges;
            for (var i = 0; i < currencyExchanges.Count;)
            {
                var exchange = currencyExchanges[i];
                if (exchange.catalogItem == currency)
                {
                    currencyExchanges.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        ///     Adds a <paramref name="catalogItem"/> payout to this instance.
        /// </summary>
        /// <param name="catalogItem">
        ///     The catalog item to add.
        /// </param>
        /// <param name="amount">
        ///     The amount of the <paramref name="catalogItem"/> to add.
        /// </param>
        internal void Editor_AddPayout(TradableDefinitionAsset catalogItem, long amount)
        {
            GameFoundation.Tools.ThrowIfArgNull(catalogItem, nameof(catalogItem));

            // NonConsumable and Subscription type IAP products don't support automatic payouts
            if (this is IAPTransactionAsset iapTransactionAsset
                && iapTransactionAsset.productType != IAPProductType.Consumable)
            {
                Debug.LogWarning(
                    $"Tried to add a payout to IAP Transaction '{iapTransactionAsset.m_Key}', " +
                    $"but the transaction type of '{iapTransactionAsset.productType}' " +
                    "does not support automatic payouts.");
                return;
            }

            var exchange = new ExchangeDefinitionObject
            {
                m_CatalogItem = catalogItem,
                m_Amount = amount
            };
            m_Payout.m_Exchanges.Add(exchange);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Remove the given <paramref name="item"/> from the payout.
        /// </summary>
        /// <param name="item">
        ///     The payout entry to remove.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the item has been removed from payout;
        ///     return <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemovePayout(ExchangeDefinitionObject item)
        {
            var isRemoved = m_Payout.m_Exchanges.Remove(item);

            if (isRemoved)
            {
                EditorUtility.SetDirty(this);
            }

            return isRemoved;
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(BaseTransactionAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            if (!(target is BaseTransactionAsset baseTransactionTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(BaseTransactionAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            baseTransactionTarget.m_Payout = m_Payout.Clone();

            base.CopyValues(baseTransactionTarget);
        }

        /// <inheritdoc/>
        internal override void RefreshReferences(CatalogAsset owner)
        {
            base.RefreshReferences(owner);

            m_Payout.RefreshReferences(owner);
        }
    }
}

#endif
