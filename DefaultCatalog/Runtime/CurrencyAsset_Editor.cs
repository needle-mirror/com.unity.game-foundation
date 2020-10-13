#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CurrencyAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "Currency";

        /// <summary>
        ///     Before this currency is destroyed,
        ///     remove any references to it from any transactions and rewards.
        /// </summary>
        protected override void OnBeforeDestroy()
        {
            var rewardAssets = new List<RewardAsset>();
            CatalogSettings.catalogAsset.GetItems(rewardAssets);

            foreach (var rewardAsset in rewardAssets)
            {
                rewardAsset.Editor_HandleCurrencyRemoved(this);
            }

            var transactionAssets = new List<BaseTransactionAsset>();
            CatalogSettings.catalogAsset.GetItems(transactionAssets);

            foreach (var transactionAsset in transactionAssets)
            {
                transactionAsset.Editor_HandleCurrencyRemoved(this);
            }

            base.OnBeforeDestroy();
        }

        /// <summary>
        ///     Sets the <see cref="initialBalance"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="balance">
        ///     The initial balance.
        ///     Negative values are considered as 0.
        /// </param>
        internal void Editor_SetInitialBalance(long balance)
        {
            m_InitialBalance = Math.Max(0, balance);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Sets the <see cref="maximumBalance"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="balance">
        ///     The maximum balance.
        ///     Negative values are considered as 0.
        /// </param>
        internal void Editor_SetMaximumBalance(long balance)
        {
            m_MaximumBalance = Math.Max(0, balance);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Sets the <see cref="type"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="value">
        ///     The type of this <see cref="CurrencyAsset"/> instance.
        /// </param>
        internal void Editor_SetType(CurrencyType value)
        {
            m_Type = value;

            EditorUtility.SetDirty(this);
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(CurrencyAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            var currencyTarget = target as CurrencyAsset;

            if (currencyTarget == null)
            {
                throw new InvalidCastException(
                    $"{nameof(CurrencyAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            currencyTarget.m_InitialBalance = m_InitialBalance;
            currencyTarget.m_MaximumBalance = m_MaximumBalance;
            currencyTarget.m_Type = m_Type;

            base.CopyValues(currencyTarget);
        }
    }
}

#endif
