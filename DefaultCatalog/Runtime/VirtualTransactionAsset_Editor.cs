#if UNITY_EDITOR

using System;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class VirtualTransactionAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "VirtualTransaction";

        /// <inheritdoc/>
        internal override void Editor_HandleInventoryItemRemoved(InventoryItemDefinitionAsset item)
        {
            var itemExchanges = m_Costs.m_Exchanges;
            for (var i = 0; i < itemExchanges.Count;)
            {
                var exchange = itemExchanges[i];
                if (exchange.catalogItem == item)
                {
                    itemExchanges.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }

            base.Editor_HandleInventoryItemRemoved(item);
        }

        /// <inheritdoc/>
        internal override void Editor_HandleCurrencyRemoved(CurrencyAsset currency)
        {
            var currencyExchanges = m_Costs.m_Exchanges;
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

            base.Editor_HandleCurrencyRemoved(currency);
        }

        /// <summary>
        ///     Add a <paramref name="catalogItem"/> cost to this instance.
        /// </summary>
        /// <param name="catalogItem">
        ///     The catalog item to add.
        /// </param>
        /// <param name="amount">
        ///     The amount of the <paramref name="catalogItem"/> to add.
        /// </param>
        internal void Editor_AddCost(TradableDefinitionAsset catalogItem, long amount)
        {
            GameFoundation.Tools.ThrowIfArgNull(catalogItem, nameof(catalogItem));

            var exchange = new ExchangeDefinitionObject
            {
                m_CatalogItem = catalogItem,
                m_Amount = amount
            };
            m_Costs.m_Exchanges.Add(exchange);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Remove the given <paramref name="item"/> from the cost.
        /// </summary>
        /// <param name="item">
        ///     The cost entry to remove.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the item has been removed from cost;
        ///     return <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveCost(ExchangeDefinitionObject item)
        {
            var isRemoved = m_Costs.m_Exchanges.Remove(item);
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
                    $"{nameof(VirtualTransactionAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            if (!(target is VirtualTransactionAsset virtualTransactionTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(VirtualTransactionAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            virtualTransactionTarget.m_Costs = m_Costs.Clone();

            base.CopyValues(virtualTransactionTarget);
        }

        /// <inheritdoc/>
        internal override void RefreshReferences(CatalogAsset owner)
        {
            base.RefreshReferences(owner);

            m_Costs.RefreshReferences(owner);
        }
    }
}

#endif
