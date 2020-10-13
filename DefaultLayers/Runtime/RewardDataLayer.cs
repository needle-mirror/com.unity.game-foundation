using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Hidden classes which focuses on dealing with rewards for the <see cref="BaseMemoryDataLayer"/> object.
    /// </summary>
    class RewardDataLayer : IRewardDataLayer
    {
        /// <summary>
        ///     The existing WalletDataLayer instance which will be used to grant currency from payouts.
        /// </summary>
        readonly WalletDataLayer m_WalletDataLayer;

        /// <summary>
        ///     The existing InventoryDataLayer instance which will be used to grant items from payouts.
        /// </summary>
        readonly InventoryDataLayer m_InventoryDataLayer;

        /// <summary>
        ///     Stores the data of all the item instances.
        ///     Key: reward's key.
        ///     Value: reward's data.
        /// </summary>
        Dictionary<string, RewardData> m_Rewards;

        /// <summary>
        ///     Provides <see cref="List{ExchangeDefinition}"/> instances;
        /// </summary>
        static readonly Pool<List<ExchangeDefinition>> s_ExchangeDefinitionsListPool =
            new Pool<List<ExchangeDefinition>>(
                () => new List<ExchangeDefinition>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{InventoryItemSerializableData}"/> instances;
        /// </summary>
        static readonly Pool<List<CurrencyExchangeData>> s_CurrencyDataListPool =
            new Pool<List<CurrencyExchangeData>>(
                () => new List<CurrencyExchangeData>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{InventoryItemSerializableData}"/> instances;
        /// </summary>
        static readonly Pool<List<InventoryItemData>> s_ItemDataListPool =
            new Pool<List<InventoryItemData>>(
                () => new List<InventoryItemData>(),
                list => list.Clear());

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardDataLayer>();

        /// <summary>
        ///     Initializes a new <see cref="RewardDataLayer"/> instance.
        /// </summary>
        /// <param name="data">
        ///     The data to initialize the <see cref="RewardDataLayer"/> object with.
        /// </param>
        /// <param name="catalogAsset">
        ///     The catalog asset to be used as source of truth.
        /// </param>
        /// <param name="walletDataLayer">
        ///     An existing WalletDataLayer instance that has already been initialized.
        ///     Payouts will be granted by this wallet data layer.
        /// </param>
        /// <param name="inventoryDataLayer">
        ///     An existing InventoryDataLayer instance that has already been initialized.
        ///     Payouts will be granted by this inventory data layer.
        /// </param>
        public RewardDataLayer(
            RewardManagerData data,
            CatalogAsset catalogAsset,
            WalletDataLayer walletDataLayer,
            InventoryDataLayer inventoryDataLayer)
        {
            Tools.ThrowIfArgNull(data, nameof(data));
            Tools.ThrowIfArgNull(catalogAsset, nameof(catalogAsset));
            Tools.ThrowIfArgNull(walletDataLayer, nameof(walletDataLayer));
            Tools.ThrowIfArgNull(inventoryDataLayer, nameof(inventoryDataLayer));

            m_WalletDataLayer = walletDataLayer;
            m_InventoryDataLayer = inventoryDataLayer;

            m_Rewards = new Dictionary<string, RewardData>();

            if (data.rewards == null)
            {
                // be backward compatible with save files that didn't have rewards
                return;
            }

            foreach (var rewardData in data.rewards)
            {
                // validate that the reward still exists in the catalog
                // if it's not in the catalog, then don't keep it

                if (catalogAsset.FindItem(rewardData.key) == null)
                {
                    k_GFLogger.LogWarning($"Could not add reward with key {rewardData.key} to Reward list " +
                        "because it was not found in the reward catalog.");
                    continue;
                }

                m_Rewards.Add(rewardData.key, rewardData);
            }
        }

        /// <inheritdoc/>
        RewardManagerData IRewardDataLayer.GetData()
        {
            // before creating the data to return, validate the existing data and reset rewards where needed
            EnforceResets();

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
                    claimedRewardItemKeys = rewardEntry.Value.claimedRewardItemKeys,
                    claimedRewardItemTimestamps = rewardEntry.Value.claimedRewardItemTimestamps
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
        void EnforceResets()
        {
            var nowTicks = DateTime.UtcNow.Ticks;

            // see if any rewards need to be reset
            foreach (var kvp in m_Rewards)
            {
                var rewardDefinition = GameFoundationSdk.catalog.Find<RewardDefinition>(kvp.Key);
                var rewardData = kvp.Value;

                var cooldownTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;
                var expirationTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;

                int highestIndexClaimed = -1;
                long ticksSinceClaim = long.MaxValue;
                long biggestTimestamp = 0;

                for (var i = 0; i < rewardDefinition.m_Items.Length; i++)
                {
                    var rewardItemDefinition = rewardDefinition.m_Items[i];

                    if (rewardData.claimedRewardItemKeys.Contains(rewardItemDefinition.key)
                        && i > highestIndexClaimed)
                    {
                        highestIndexClaimed = i;

                        var claimedTimestamp = rewardData
                            .claimedRewardItemTimestamps[rewardData.claimedRewardItemKeys.IndexOf(rewardItemDefinition.key)];

                        ticksSinceClaim = nowTicks - claimedTimestamp;

                        if (biggestTimestamp < claimedTimestamp)
                        {
                            biggestTimestamp = claimedTimestamp;
                        }
                    }
                }

                var missedItemsCount = 0;

                if (rewardDefinition.expirationSeconds > 0 && !rewardDefinition.resetIfExpired)
                {
                    missedItemsCount = Mathf.FloorToInt((float)(ticksSinceClaim - cooldownTicks) / expirationTicks);

                    // edge case ?
                    if (ticksSinceClaim == expirationTicks)
                    {
                        missedItemsCount -= 1;
                    }
                }

                var claimableIndex = highestIndexClaimed + 1 + missedItemsCount;

                // when should we reset:
                var hasCooldownFinished = ticksSinceClaim > cooldownTicks;
                var lastRewardItemIndex = rewardDefinition.m_Items.Length - 1;

                // the highest claimable index exceeds the size of the item collection
                // and the cooldown has finished
                var isClaimableOutOfRange = claimableIndex > lastRewardItemIndex
                    && hasCooldownFinished;

                // when the last item in the reward has been claimed
                // and the cooldown has finished
                var wasLastRewardClaimed = highestIndexClaimed == lastRewardItemIndex
                    && hasCooldownFinished;

                // when resetIfExpires is true and any item expires
                var doResetAfterExpiration = rewardDefinition.resetIfExpired
                    && ticksSinceClaim > cooldownTicks + expirationTicks;
                if (isClaimableOutOfRange
                    || wasLastRewardClaimed
                    || doResetAfterExpiration)
                {
                    rewardData.claimedRewardItemKeys.Clear();
                    rewardData.claimedRewardItemTimestamps.Clear();
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
                EnforceResets();

                var rewardDefinition = GameFoundationSdk.catalog.Find<RewardDefinition>(rewardKey);
                var rewardItemDefinition = rewardDefinition.FindRewardItem(rewardItemKey);
                var cooldownTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;
                var expirationTicks = rewardDefinition.expirationSeconds * TimeSpan.TicksPerSecond;

                if (!m_Rewards.ContainsKey(rewardKey))
                {
                    m_Rewards[rewardKey] = new RewardData
                    {
                        key = rewardKey,
                        claimedRewardItemKeys = new List<string>(),
                        claimedRewardItemTimestamps = new List<long>()
                    };
                }

                // first find validation parameters

                var rewardItem = rewardDefinition.FindRewardItem(rewardItemKey);
                var rewardItemIndex = rewardDefinition.IndexOf(rewardItem);
                var nowTimestamp = DateTime.UtcNow.Ticks;
                int highestIndexClaimed = -1;
                long ticksSinceClaim = long.MaxValue;

                for (var i = 0; i < rewardDefinition.m_Items.Length; i++)
                {
                    if (m_Rewards[rewardKey].claimedRewardItemKeys.Contains(rewardDefinition.m_Items[i].key)
                        && i > highestIndexClaimed)
                    {
                        highestIndexClaimed = i;

                        var claimedTimestamp = m_Rewards[rewardKey]
                            .claimedRewardItemTimestamps[
                                m_Rewards[rewardKey].claimedRewardItemKeys.IndexOf(rewardDefinition.m_Items[i].key)];

                        ticksSinceClaim = nowTimestamp - claimedTimestamp;
                    }
                }

                // how many items have gone unclaimed?

                var missedItemsCount = 0;

                if (rewardDefinition.expirationSeconds > 0 && !rewardDefinition.resetIfExpired)
                {
                    missedItemsCount = Mathf.FloorToInt((float)(ticksSinceClaim - cooldownTicks) / expirationTicks);

                    // edge case ?
                    if (ticksSinceClaim == expirationTicks)
                    {
                        missedItemsCount -= 1;
                    }
                }

                var potentiallyClaimableIndex = highestIndexClaimed >= 0 ? highestIndexClaimed + 1 + missedItemsCount : -1;

                // now validate

                switch (
                    GetState(
                        rewardDefinition,
                        m_Rewards[rewardKey],
                        rewardItemKey,
                        rewardItemIndex,
                        ticksSinceClaim,
                        potentiallyClaimableIndex))
                {
                    case RewardItemState.Claimed:
                        throw new InvalidOperationException(
                            $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it has already been claimed.");

                    case RewardItemState.Locked:
                        throw new InvalidOperationException(
                            $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it is locked.");

                    case RewardItemState.Missed:
                        throw new InvalidOperationException(
                            $"Reward {rewardKey} item {rewardItemKey} cannot be claimed because it's too old.");
                }

                // we have made it past the guards
                // now flag the item as claimed in the data and apply the payout

                m_Rewards[rewardKey].claimedRewardItemKeys.Add(rewardItemKey);
                m_Rewards[rewardKey].claimedRewardItemTimestamps.Add(DateTime.UtcNow.Ticks);

                var result = ApplyRewardItemPayout(rewardItemDefinition.payout);

                completer.Resolve(result);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <summary>
        ///     Get a RewardItemState based on everything we know about the current reward state.
        ///     Similar to <see cref="UnityEngine.GameFoundation.Reward.GetState"/>, but modified to work in the data layer.
        /// </summary>
        static RewardItemState GetState(
            RewardDefinition rewardDefinition,
            RewardData rewardData,
            string rewardItemKey,
            int rewardItemIndex,
            long ticksSinceClaim,
            int potentiallyClaimableIndex)
        {
            var indexOfClaimedTimestamp = rewardData.claimedRewardItemKeys.IndexOf(rewardItemKey);

            if (indexOfClaimedTimestamp >= 0)
            {
                // if a key exists, it's claimed, period

                return RewardItemState.Claimed;
            }

            if (rewardData.claimedRewardItemKeys.Count == 0)
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
        /// <param name="payout">
        ///     The payout to apply.
        /// </param>
        /// <returns>
        ///     A <see cref="TransactionExchangeData"/> describing the payout that was applied.
        /// </returns>
        TransactionExchangeData ApplyRewardItemPayout(TransactionExchangeDefinition payout)
        {
            var result = new TransactionExchangeData();

            var exchangeListHandle = s_ExchangeDefinitionsListPool.Get(out var exchangeList);
            var currencyDataListHandle = s_CurrencyDataListPool.Get(out var currencyDataList);
            var itemDataListHandle = s_ItemDataListPool.Get(out var itemDataList);

            try
            {
                payout.GetExchanges(exchangeList);

                foreach (var exchange in exchangeList)
                {
                    var key = exchange.tradableDefinition.key;

                    switch (exchange.tradableDefinition)
                    {
                        // [a] Increment the currencies
                        case Currency _:
                        {
                            var balance = exchange.amount;

                            m_WalletDataLayer.AdjustBalance(key, balance);

                            currencyDataList.Add(new CurrencyExchangeData
                            {
                                currencyKey = key,
                                amount = balance
                            });

                            break;
                        }

                        // [b] Create the new stackable item
                        case StackableInventoryItemDefinition _:
                        {
                            // create 1 stack with desired quantity
                            var item = m_InventoryDataLayer.CreateItem(key);

                            var itemData = new InventoryItemData
                            {
                                id = item.id,
                                quantity = exchange.amount,
                                definitionKey = key
                            };

                            itemDataList.Add(itemData);

                            break;
                        }

                        // [c] Create the new item(s)
                        case InventoryItemDefinition inventoryItemDefinition:
                        {
                            for (var i = 0; i < exchange.amount; i++)
                            {
                                var item = m_InventoryDataLayer.CreateItem(key);

                                var itemData = new InventoryItemData
                                {
                                    id = item.id,
                                    quantity = 1,
                                    definitionKey = key
                                };

                                itemDataList.Add(itemData);
                            }

                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                result.currencies = currencyDataList.ToArray();
                result.items = itemDataList.ToArray();
            }
            finally
            {
                itemDataListHandle.Release();
                currencyDataListHandle.Release();
                exchangeListHandle.Release();
            }

            return result;
        }
    }
}
