using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     Helper object to store meta data for a reward.
        /// </summary>
        struct RewardMetaData
        {
            /// <summary>
            ///     The index of the last claimed reward item.
            /// </summary>
            public int highestIndexClaimed;

            /// <summary>
            ///     The number of reward items in the reward.
            /// </summary>
            public int rewardItemCount;

            /// <summary>
            ///     The number of elapsed ticks since the last claim of a reward item.
            /// </summary>
            public long ticksSinceClaim;
        }

        /// <summary>
        ///     Stores the data of all the item instances.
        ///     Key: reward's key.
        ///     Value: reward's data.
        /// </summary>
        readonly Dictionary<string, RewardData> m_Rewards = new Dictionary<string, RewardData>();

        /// <summary>
        ///     Initializes the data layer for the <see cref="IRewardManager"/>.
        /// </summary>
        /// <param name="data">
        ///     RewardManager's serializable data.
        /// </param>
        protected void InitializeRewardDataLayer(RewardManagerData data)
        {
            //Reset containers.
            m_Rewards.Clear();

            Tools.ThrowIfArgNull(data, nameof(data));

            if (data.rewards is null)
            {
                // be backward compatible with save files that didn't have rewards
                return;
            }

            foreach (var rewardData in data.rewards)
            {
                // validate that the reward still exists in the catalog
                // if it's not in the catalog, then don't keep it

                if (!(m_CatalogAsset.FindItem(rewardData.key) is RewardAsset))
                {
                    k_Logger.LogWarning($"Could not add reward with key {rewardData.key} to Reward list " +
                        "because it was not found in the reward catalog.");
                    continue;
                }

                m_Rewards.Add(rewardData.key, rewardData);
            }
        }

        /// <inheritdoc/>
        RewardManagerData IRewardDataLayer.GetData()
        {
            // before creating the data to return, validate the existing data and reset rewards where needed.
            // Give a default rejectable to enforce direct throw if the operation fails.
            EnforceResets(default);

            var data = new RewardManagerData
            {
                rewards = new RewardData[m_Rewards.Count]
            };

            var index = 0;

            foreach (var rewardEntry in m_Rewards)
            {
                var rewardData = new RewardData
                {
                    key = rewardEntry.Key,
                    claimedRewards = rewardEntry.Value.claimedRewards
                };

                data.rewards[index] = rewardData;

                index++;
            }

            return data;
        }

        /// <summary>
        ///     Loop through all reward data and reset any rewards that are due for it.
        ///     Resetting means that all its claim timestamps are deleted.
        /// </summary>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        ///     If the handle isn't valid, the exception will be directly thrown.
        /// </param>
        void EnforceResets(Rejectable rejectable)
        {
            // see if any rewards need to be reset
            foreach (var kvp in m_Rewards)
            {
                // This can only happen if m_CatalogAsset has been modified outside this
                // data access layer after InitializeRewardDataLayer has been called.
                if (!(m_CatalogAsset.FindItem(kvp.Key) is RewardAsset rewardDefinition))
                {
                    var reason = new CatalogItemNotFoundException(kvp.Key);
                    if (!rejectable.isActive)
                    {
                        throw reason;
                    }

                    rejectable.Reject(reason);

                    return;
                }

                var rewardData = kvp.Value;

                var cooldownTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;
                var expirationTicks = rewardDefinition.expirationSeconds * TimeSpan.TicksPerSecond;
                var metaData = GetRewardItemMetaData(rewardDefinition, rewardData);

                // how many items have gone unclaimed since the last claim?
                // - this does not include older missed items, only the ones since the last claim 
                var missedItemsCount = 0;

                // if we're in cooldown, then there can't be any missed items since the last claim
                if (metaData.ticksSinceClaim > cooldownTicks
                    && rewardDefinition.expirationSeconds > 0
                    && !rewardDefinition.resetIfExpired)
                {
                    missedItemsCount = Mathf.FloorToInt((float)(metaData.ticksSinceClaim - cooldownTicks) / expirationTicks);

                    // edge case ?
                    if (metaData.ticksSinceClaim == expirationTicks)
                    {
                        missedItemsCount -= 1;
                    }
                }

                var claimableIndex = metaData.highestIndexClaimed + 1 + missedItemsCount;

                // when should we reset:
                var hasCooldownFinished = metaData.ticksSinceClaim >= cooldownTicks;
                var lastRewardItemIndex = metaData.rewardItemCount - 1;

                // the highest claimable index exceeds the size of the item collection
                // and the cooldown has finished
                var isClaimableOutOfRange = claimableIndex > lastRewardItemIndex
                    && hasCooldownFinished;

                // when the last item in the reward has been claimed
                // and the cooldown has finished
                var wasLastRewardClaimed = metaData.highestIndexClaimed == lastRewardItemIndex
                    && hasCooldownFinished;

                // when resetIfExpires is true and any item expires
                var doResetAfterExpiration = rewardDefinition.expirationSeconds > 0
                    && rewardDefinition.resetIfExpired
                    && metaData.ticksSinceClaim >= cooldownTicks + expirationTicks;
                if (isClaimableOutOfRange
                    || wasLastRewardClaimed
                    || doResetAfterExpiration)
                {
                    rewardData.claimedRewards.Clear();
                }
            }
        }

        /// <inheritdoc/>
        void IRewardDataLayer.Claim(
            string rewardKey,
            string rewardItemKey,
            Completer<TransactionExchangeData> completer)
        {
            try
            {
                EnforceResets(completer);
                if (!completer.isActive)
                {
                    return;
                }

                if (!(m_CatalogAsset.FindItem(rewardKey) is RewardAsset rewardDefinition))
                {
                    completer.Reject(new CatalogItemNotFoundException(rewardKey));

                    return;
                }

                var rewardItemDefinition = rewardDefinition.FindRewardItem(rewardItemKey);
                var cooldownTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;
                var expirationTicks = rewardDefinition.expirationSeconds * TimeSpan.TicksPerSecond;

                if (!m_Rewards.ContainsKey(rewardKey))
                {
                    m_Rewards[rewardKey] = new RewardData
                    {
                        key = rewardKey,
                        claimedRewards = new List<ClaimedRewardData>()
                    };
                }

                // first find validation parameters

                var rewardItem = rewardDefinition.FindRewardItem(rewardItemKey);
                var rewardItemIndex = rewardDefinition.IndexOf(rewardItem);
                var rewardData = m_Rewards[rewardKey];
                var metaData = GetRewardItemMetaData(rewardDefinition, rewardData);

                // how many items have gone unclaimed?

                var missedItemsCount = 0;

                if (rewardDefinition.expirationSeconds > 0 && !rewardDefinition.resetIfExpired)
                {
                    missedItemsCount = Mathf.FloorToInt((float)(metaData.ticksSinceClaim - cooldownTicks) / expirationTicks);

                    // edge case ?
                    if (metaData.ticksSinceClaim == expirationTicks)
                    {
                        missedItemsCount -= 1;
                    }
                }

                var potentiallyClaimableIndex = metaData.highestIndexClaimed >= 0 ? metaData.highestIndexClaimed + 1 + missedItemsCount : -1;

                // now validate

                var rewardItemState = GetState(
                    rewardDefinition,
                    rewardData,
                    rewardItemKey,
                    rewardItemIndex,
                    metaData.ticksSinceClaim,
                    potentiallyClaimableIndex);
                switch (rewardItemState)
                {
                    case RewardItemState.Claimed:
                    {
                        var message = $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it has already been claimed.";
                        completer.Reject(new InvalidOperationException(message));

                        return;
                    }

                    case RewardItemState.Locked:
                    {
                        var message = $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it is locked.";
                        completer.Reject(new InvalidOperationException(message));

                        return;
                    }

                    case RewardItemState.Missed:
                    {
                        var message = $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it's too old.";
                        completer.Reject(new InvalidOperationException(message));

                        return;
                    }
                }

                // we have made it past the guards
                // now flag the item as claimed in the data and apply the payout

                rewardData.claimedRewards.Add(
                    new ClaimedRewardData
                    {
                        rewardItemKey = rewardItemKey,
                        timestamp = DateTime.UtcNow.Ticks
                    });

                var result = ApplyRewardItemPayout(rewardItemDefinition, completer);

                completer.Resolve(result);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <summary>
        ///     Get the latest meta data for the given reward.
        /// </summary>
        /// <param name="rewardDefinition">
        ///     The definition of the reward.
        /// </param>
        /// <param name="rewardData">
        ///     The serializable data of the reward.
        /// </param>
        /// <returns>
        ///     Return a new <see cref="RewardMetaData"/> with the most recent data.
        /// </returns>
        static RewardMetaData GetRewardItemMetaData(in RewardAsset rewardDefinition, in RewardData rewardData)
        {
            var metaData = new RewardMetaData
            {
                highestIndexClaimed = -1,
                ticksSinceClaim = long.MaxValue
            };

            using (k_RewardItemObjectListPool.Get(out var rewardItems))
            {
                var nowTicks = DateTime.UtcNow.Ticks;

                metaData.rewardItemCount = rewardDefinition.GetRewardItems(rewardItems);
                for (var i = 0; i < metaData.rewardItemCount; i++)
                {
                    var rewardItemDefinition = rewardItems[i];

                    var match = rewardData.claimedRewards
                        .Find(claimedReward => claimedReward.rewardItemKey.Equals(rewardItemDefinition.key));
                    if (match is null)
                    {
                        continue;
                    }

                    metaData.highestIndexClaimed = i;

                    metaData.ticksSinceClaim = nowTicks - match.timestamp;
                }
            }

            return metaData;
        }

        /// <summary>
        ///     Get a RewardItemState based on everything we know about the current reward state.
        ///     Similar to <see cref="UnityEngine.GameFoundation.Reward.GetState"/>, but modified to work in the data layer.
        /// </summary>
        static RewardItemState GetState(
            RewardAsset rewardDefinition,
            RewardData rewardData,
            string rewardItemKey,
            int rewardItemIndex,
            long ticksSinceClaim,
            int potentiallyClaimableIndex)
        {
            var match = rewardData.claimedRewards.Find(x => x.rewardItemKey.Equals(rewardItemKey));
            if (!(match is null))
            {
                // if a key exists, it's claimed, period

                return RewardItemState.Claimed;
            }

            if (rewardData.claimedRewards.Count == 0)
            {
                // there have been no claims on this reward
                // therefore only the first item is claimable
                return rewardItemIndex == 0 ? RewardItemState.Claimable : RewardItemState.Locked;
            }

            if (rewardItemIndex > potentiallyClaimableIndex)
            {
                return RewardItemState.Locked;
            }

            if (rewardItemIndex == potentiallyClaimableIndex)
            {
                if (ticksSinceClaim < rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond)
                {
                    // nothing is claimable because it's in cooldown
                    return RewardItemState.Locked;
                }

                return RewardItemState.Claimable;
            }

            if (rewardItemIndex < potentiallyClaimableIndex)
            {
                // this item is older than the current claimable and has not been claimed
                return RewardItemState.Missed;
            }

            // this item must be in the future
            return RewardItemState.Locked;
        }

        /// <summary>
        ///     Grants the items in a payout to the player's wallet and inventory.
        /// </summary>
        /// <param name="rewardItemDefinition">
        ///     The <see cref="RewardItemDefinition"/> to apply payout of.
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        /// </param>
        /// <returns>
        ///     A <see cref="TransactionExchangeData"/> describing the payout that was applied.
        /// </returns>
        TransactionExchangeData ApplyRewardItemPayout(RewardItemObject rewardItemDefinition, Rejectable rejectable)
        {
            var payout = rewardItemDefinition.payout;
            var result = new TransactionExchangeData();

            using (k_ExchangeDefinitionsListPool.Get(out var exchangeList))
            using (k_CurrencyDataListPool.Get(out var currencyDataList))
            using (k_ItemDataListPool.Get(out var itemDataList))
            {
                payout.GetItems(exchangeList);

                for (var exchangeIndex = 0; exchangeIndex < exchangeList.Count; exchangeIndex++)
                {
                    var exchange = exchangeList[exchangeIndex];
                    var key = exchange.catalogItem.key;

                    switch (exchange.catalogItem)
                    {
                        // [a] Increment the currencies
                        case CurrencyAsset _:
                        {
                            var balance = exchange.amount;

                            AdjustBalance(key, balance, rejectable);
                            if (!rejectable.isActive)
                            {
                                return default;
                            }

                            currencyDataList.Add(new CurrencyExchangeData
                            {
                                currencyKey = key,
                                amount = balance
                            });

                            break;
                        }

                        case InventoryItemDefinitionAsset definitionAsset:
                        {
                            // [b] Create the new stackable item
                            if (definitionAsset.isStackableFlag)
                            {
                                // create 1 stack with desired quantity
                                var item = CreateItem(key, rejectable);
                                if (!rejectable.isActive)
                                {
                                    return default;
                                }

                                var itemData = new InventoryItemData
                                {
                                    id = item.id,
                                    quantity = exchange.amount,
                                    definitionKey = key
                                };

                                itemDataList.Add(itemData);
                            }

                            // [c] Create the new item(s)
                            else
                            {
                                for (var i = 0; i < exchange.amount; i++)
                                {
                                    var item = CreateItem(key, rejectable);
                                    if (!rejectable.isActive)
                                    {
                                        return default;
                                    }

                                    var itemData = new InventoryItemData
                                    {
                                        id = item.id,
                                        quantity = 1,
                                        definitionKey = key
                                    };

                                    itemDataList.Add(itemData);
                                }
                            }

                            break;
                        }

                        default:
                        {
                            var indexInParent = -1;
                            var definitionItems = rewardItemDefinition.reward.m_RewardItems;
                            for (var rewardItemIndex = 0; rewardItemIndex < definitionItems.Count; rewardItemIndex++)
                            {
                                if (ReferenceEquals(definitionItems[rewardItemIndex], rewardItemDefinition))
                                {
                                    indexInParent = rewardItemIndex;
                                    break;
                                }
                            }

                            var message = $"The payout item #{exchangeIndex.ToString()} for the reward item " +
                                $"#{indexInParent.ToString()} \"{rewardItemDefinition.reward.key}\" isn't supported.";
                            rejectable.Reject(new NotSupportedException(message));

                            return default;
                        }
                    }
                }

                result.currencies = currencyDataList.ToArray();
                result.items = itemDataList.ToArray();
            }

            return result;
        }
    }
}
