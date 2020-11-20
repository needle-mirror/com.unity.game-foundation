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
            if (m_MaximumBalance > 0)
            {
                balance = Math.Min(m_MaximumBalance, balance);
            }

            m_InitialBalance = Math.Max(0, balance);

            m_InitialBalanceWrapper = new ExternalizableValue<long>(m_InitialBalance);

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
            if (m_MaximumBalance > 0
                && m_MaximumBalance < m_InitialBalance)
            {
                Editor_SetInitialBalance(m_MaximumBalance);
            }

            m_MaximumBalanceWrapper = new ExternalizableValue<long>(m_MaximumBalance);

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
            m_TypeWrapper = new ExternalizableValue<CurrencyType>(m_Type);

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

            if (!(target is CurrencyAsset currencyTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(CurrencyAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            currencyTarget.Editor_SetInitialBalance(m_InitialBalance);
            currencyTarget.Editor_SetMaximumBalance(m_MaximumBalance);
            currencyTarget.Editor_SetType(m_Type);

            base.CopyValues(currencyTarget);
        }
    }
}

#endif
