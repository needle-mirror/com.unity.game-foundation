// #define DEBUG_EDITOR_ANALYTICS

using System;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using GFTools = UnityEngine.GameFoundation.Tools;
using DCTools = UnityEngine.GameFoundation.DefaultCatalog.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    [InitializeOnLoad]
    static class GameFoundationAnalytics
    {
        const string k_Prefix = "gameFoundation";
        const string k_OpenTabEvent = k_Prefix + "OpenTab";
        const string k_SnapshotEvent = k_Prefix + "CatalogSnapshot";

        const string k_LastSyncTimeSessionStateKey = "game-foundation.catalogSnapshot.lastSync.Time";

        const double k_SnapshotFrequency = 5 * 60;

        static readonly CatalogSnapshotContainer s_CatalogSnapshotContainer = new CatalogSnapshotContainer();

        static DateTime s_LastSyncTime;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(GameFoundationAnalytics));

        [Serializable]
        struct AnalyticsData
        {
            public string TabName;
        }

        public enum TabName
        {
            InventoryItems,
            Tags,
            Stores,
            Currencies,
            Transactions,
            GameParameters,
            Rewards
        }

        const int k_MaxEventsPerHour = 100;
        const int k_MaxNumberOfElements = 100;
        const string k_VendorKey = "unity.gamefoundation.editor";
        const int k_Version = 1;

        static GameFoundationAnalytics()
        {
            EditorAnalytics.RegisterEventWithLimit(k_OpenTabEvent, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey, k_Version);
            EditorAnalytics.RegisterEventWithLimit(k_SnapshotEvent, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey, k_Version);

            EditorApplication.update += SyncCatalogSnapshot;
        }

        public static bool DoSnapshot(out CatalogSnapshot snapshot)
        {
            try
            {
                var catalogAsset = CatalogSettings.catalogAsset;

                if (catalogAsset == null)
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new CatalogSnapshot();

                // Inventory item definitions

                var inventoryItemDefinitionAssets = new List<InventoryItemDefinitionAsset>();
                snapshot.inventoryItemCount = catalogAsset.GetItems(inventoryItemDefinitionAssets);

                // Stores definitions

                var storeAssets = new List<StoreAsset>();
                snapshot.storeCount = catalogAsset.GetItems(storeAssets);

                // Reward definitions

                var rewardAssets = new List<RewardAsset>();
                snapshot.rewardCount = catalogAsset.GetItems(rewardAssets);

                // RewardItems

                {
                    var rewardItemCount = 0;

                    foreach (var reward in rewardAssets)
                    {
                        rewardItemCount += reward.m_RewardItems.Count;
                    }

                    snapshot.rewardItemCount = rewardItemCount;
                }

                // Properties

                {
                    var staticPropertyKeys = new HashSet<string>();

                    void FillStaticPropertyKeysHash(IEnumerable<CatalogItemAsset> catalogItemList)
                    {
                        foreach (var catalogItemAsset in catalogItemList)
                        {
                            foreach (var staticPropertyKey in catalogItemAsset.staticProperties.Keys)
                            {
                                staticPropertyKeys.Add(staticPropertyKey);
                            }
                        }
                    }

                    var currencyAssets = new List<CurrencyAsset>();
                    catalogAsset.GetItems(currencyAssets);
                    FillStaticPropertyKeysHash(currencyAssets);

                    var transactionAssets = new List<BaseTransactionAsset>();
                    catalogAsset.GetItems(transactionAssets);
                    FillStaticPropertyKeysHash(transactionAssets);

                    FillStaticPropertyKeysHash(storeAssets);

                    FillStaticPropertyKeysHash(rewardAssets);

                    FillStaticPropertyKeysHash(inventoryItemDefinitionAssets);

                    var propertyKeys = new HashSet<string>();

                    foreach (var item in inventoryItemDefinitionAssets)
                    {
                        foreach (var itemProperty in item.mutableProperties)
                        {
                            propertyKeys.Add(itemProperty.Key);
                        }
                    }

                    snapshot.propertyCount = staticPropertyKeys.Count + propertyKeys.Count;
                }

                // Transaction definitions

                {
                    var virtualTransactionAssets = new List<VirtualTransactionAsset>();
                    snapshot.virtualTransactionCount = catalogAsset.GetItems(virtualTransactionAssets);

                    var iapTransactionAssets = new List<IAPTransactionAsset>();
                    snapshot.iapTransactionCount = catalogAsset.GetItems(iapTransactionAssets);
                }

                return true;
            }
            catch (Exception e)
            {
                k_GFLogger.LogException($"{nameof(GameFoundationAnalytics)}: Failed to create {nameof(CatalogSnapshot)}.", e);
                snapshot = default;
                return false;
            }
        }

        public static AnalyticsResult SendOpenTabEvent(TabName name)
        {
            return EditorAnalytics.SendEventWithLimit(k_OpenTabEvent, new AnalyticsData { TabName = name.ToString() });
        }

        static DateTime GetLastSyncInfo()
        {
            if (s_LastSyncTime != default)
            {
                return s_LastSyncTime;
            }

            var lastSyncString = SessionState.GetString(k_LastSyncTimeSessionStateKey, "0");
#if DEBUG_EDITOR_ANALYTICS
            k_GFLogger.Log($"{k_LastSyncTimeSessionStateKey} is {lastSyncString}");
#endif
            var parsed = long.TryParse(lastSyncString, out var lastSyncBinary);
            if (!parsed)
            {
                lastSyncBinary = 0;
            }

            return DateTime.FromBinary(lastSyncBinary);
        }

        static void SyncCatalogSnapshot()
        {
            var now = DateTime.UtcNow;

            var lastSyncDate = GetLastSyncInfo();

            var duration = now - lastSyncDate;

            if (duration.TotalSeconds > k_SnapshotFrequency)
            {
#if DEBUG_EDITOR_ANALYTICS
                k_GFLogger.Log("Trying to sync catalog snapshot");
#endif
                var done = DoSnapshot(out var snapshot);

                if (!done)
                {
#if DEBUG_EDITOR_ANALYTICS
                    k_GFLogger.LogError($"{nameof(GameFoundationAnalytics)} encountered a problem when trying to take snapshot");
#endif
                    return;
                }

                if (s_CatalogSnapshotContainer.CatalogSnapshot != snapshot)
                {
                    s_CatalogSnapshotContainer.CatalogSnapshot = snapshot;

                    // The snapshot "looks" different than last time we synced.
                    var result = EditorAnalytics.SendEventWithLimit(
                        k_SnapshotEvent,
                        s_CatalogSnapshotContainer);

#if DEBUG_EDITOR_ANALYTICS
                    if (result == AnalyticsResult.Ok)
                    {
                        k_GFLogger.Log("Successfully sent snapshot");
                    }
                    else
                    {
                        k_GFLogger.LogError($"Failed to send snapshot: {result}");
                    }
#endif
                }

                s_LastSyncTime = now;
                var syncBinary = now.ToBinary();
                SessionState.SetString(k_LastSyncTimeSessionStateKey, syncBinary.ToString());
            }
        }
    }
}
