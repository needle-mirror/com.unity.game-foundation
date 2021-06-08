#if UNITY_EDITOR

using System;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class RewardAsset
    {
        internal void Editor_HandleInventoryItemRemoved(InventoryItemDefinitionAsset item)
        {
            foreach (var rewardItem in m_RewardItems)
            {
                if (rewardItem == null)
                {
                    continue;
                }

                var itemExchanges = rewardItem.m_Payout.m_Exchanges;

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
            }
        }

        internal void Editor_HandleCurrencyRemoved(CurrencyAsset currency)
        {
            foreach (var rewardItem in m_RewardItems)
            {
                if (rewardItem == null)
                {
                    continue;
                }

                var currencyExchanges = rewardItem.m_Payout.m_Exchanges;

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
        }

        /// <inheritdoc/>
        internal override string editorAssetPrefix => "Reward";

        /// <summary>
        ///     Set the <see cref="cooldownSeconds"/> and the <see cref="cooldownDisplayUnits"/>  of this instance.
        /// </summary>
        /// <param name="value">
        ///     The cooldown to set.
        ///     Negative value are considered as 0.
        /// </param>
        /// <param name="unit">
        ///     The display unit to set.
        /// </param>
        internal void Editor_SetCooldown(int value, TimeUnit unit = TimeUnit.Seconds)
        {
            value = unit.ConvertToSeconds(value);
            m_CooldownSeconds = Math.Max(0, value);
            m_CooldownWrapper = new ExternalizableValue<int>(m_CooldownSeconds);
            m_CooldownDisplayUnits = unit;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the <see cref="expirationSeconds"/> and the <see cref="expirationDisplayUnits"/> of this instance.
        /// </summary>
        /// <param name="value">
        ///     The expiration delay, in seconds, to set.
        ///     Negative value are considered as 0.
        /// </param>
        /// <param name="unit">
        ///     The display unit to set.
        /// </param>
        internal void Editor_SetExpiration(int value, TimeUnit unit = TimeUnit.Seconds)
        {
            value = unit.ConvertToSeconds(value);
            m_ExpirationSeconds = Math.Max(0, value);
            m_ExpirationWrapper = new ExternalizableValue<int>(m_ExpirationSeconds);
            m_ExpirationDisplayUnits = unit;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the <see cref="resetIfExpired"/> of this instance.
        /// </summary>
        /// <param name="value">
        ///     The flag to set.
        /// </param>
        internal void Editor_SetResetIfExpired(bool value)
        {
            m_ResetIfExpired = value;
            m_ResetIfExpiredWrapper = new ExternalizableValue<bool>(m_ResetIfExpired);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Adds a <paramref name="rewardItem"/> to the reward.
        /// </summary>
        /// <param name="rewardItem">
        ///     The <see cref="RewardItemObject"/> to add.
        /// </param>
        internal void Editor_AddItem(RewardItemObject rewardItem)
        {
            GFTools.ThrowIfArgNull(rewardItem, nameof(rewardItem));

            if (Contains(rewardItem))
            {
                throw new ArgumentException
                ($"{nameof(RewardAsset)} The {nameof(RewardItemObject)} '{rewardItem.key}' cannot be added because " +
                    $"it is already registered in this {nameof(RewardAsset)} '{displayName}'.");
            }

            rewardItem.reward = this;

            m_RewardItems.Add(rewardItem);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Removes a <paramref name="rewardItem"/> from the reward.
        /// </summary>
        /// <param name="rewardItem">
        ///     The <see cref="RewardItemObject"/> to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if removed, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveItem(RewardItemObject rewardItem)
        {
            GFTools.ThrowIfArgNull(rewardItem, nameof(rewardItem));

            var removed = m_RewardItems.Remove(rewardItem);

            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }

        /// <summary>
        ///     Swaps <paramref name="rewardItem1"/> with <paramref name="rewardItem2"/> in the store.
        /// </summary>
        /// <param name="rewardItem1">
        ///     The <see cref="RewardItemObject"/> to swap.
        /// </param>
        /// <param name="rewardItem2">
        ///     The <see cref="RewardItemObject"/> to swap the first with.
        /// </param>
        internal void Editor_SwapItemsListOrder(RewardItemObject rewardItem1, RewardItemObject rewardItem2)
        {
            GFTools.ThrowIfArgNull(rewardItem1, nameof(rewardItem1));
            GFTools.ThrowIfArgNull(rewardItem2, nameof(rewardItem2));

            // TODO: IndexOf needs to compare by unique key instead of by reference
            var index1 = m_RewardItems.IndexOf(rewardItem1);
            var index2 = m_RewardItems.IndexOf(rewardItem2);

            m_RewardItems[index1] = rewardItem2;
            m_RewardItems[index2] = rewardItem1;

            EditorUtility.SetDirty(this);
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(RewardAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            if (!(target is RewardAsset rewardTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(RewardAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            rewardTarget.Editor_SetCooldown(m_CooldownSeconds);
            rewardTarget.m_CooldownDisplayUnits = m_CooldownDisplayUnits;
            rewardTarget.Editor_SetExpiration(m_ExpirationSeconds);
            rewardTarget.m_ExpirationDisplayUnits = m_ExpirationDisplayUnits;
            rewardTarget.Editor_SetResetIfExpired(m_ResetIfExpired);

            foreach (var rewardItem in m_RewardItems)
            {
                rewardTarget.Editor_AddItem(rewardItem.Clone());
            }

            base.CopyValues(rewardTarget);
        }

        /// <inheritdoc/>
        internal override void RefreshReferences(CatalogAsset owner)
        {
            base.RefreshReferences(owner);

            foreach (var rewardItem in m_RewardItems)
            {
                rewardItem.RefreshReferences(owner);
            }
        }
    }
}

#endif
