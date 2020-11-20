#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     This consolidates all catalogs into one asset.
    /// </summary>
    public partial class CatalogAsset : ScriptableObject, ICatalogConfigurator, ISerializationCallbackReceiver
    {
        const string k_MainStoreDefinitionName = "Main";
        internal const string k_MainStoreDefinitionKey = "main";

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_Logger = GameFoundationDebug.Get<CatalogAsset>();

        /// <inheritdoc cref="tagCatalog"/>
        [SerializeField]
        internal TagCatalogAsset m_TagCatalog;

        /// <summary>
        ///     A reference to a <see cref="TagCatalogAsset"/>.
        /// </summary>
        public TagCatalogAsset tagCatalog => m_TagCatalog;

        /// <summary>
        ///     The list of <see cref="CatalogItemAsset"/> this catalog stores.
        /// </summary>
        [SerializeField]
        internal List<CatalogItemAsset> m_Items = new List<CatalogItemAsset>();

        void Awake()
        {
            var doSave = false;

            // ensure tag catalog exists

            if (m_TagCatalog is null)
            {
                m_TagCatalog = CreateInstance<TagCatalogAsset>();
                m_TagCatalog.name = "_Catalog_Tags";
                m_TagCatalog.m_CatalogAsset = this;
                doSave = true;
            }

            // ensure "main" store exists

            var mainStore = FindItem(k_MainStoreDefinitionKey) as StoreAsset;

            if (mainStore == null)
            {
                mainStore = CreateInstance<StoreAsset>();
                mainStore.m_Key = k_MainStoreDefinitionKey;
                mainStore.SetDisplayName(k_MainStoreDefinitionName);

#if UNITY_EDITOR

                // the Scriptable Object name that appears in the Project window
                mainStore.name = mainStore.editorAssetName;
#else
                mainStore.name = $"Store_{mainStore.m_Key}";
#endif

                m_Items.Insert(0, mainStore);

                doSave = true;
            }

            if (doSave)
            {
#if UNITY_EDITOR
                var path = AssetDatabase.GetAssetPath(this);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    Editor_Save(path);
                }
#endif
            }
        }

        /// <summary>
        ///     Tells whether an item with the specified <paramref name="key"/> exists or not.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItemAsset"/> instance to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the item is found, <c>false</c> otherwise.
        /// </returns>
        public bool ContainsItem(string key) => FindItem(key) != null;

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all
        ///     <see cref="CatalogItemAsset"/> instances of this catalog and returns their count.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="CatalogItemAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="CatalogItemAsset"/> instances of this catalog.
        /// </returns>
        public int GetItems(ICollection<CatalogItemAsset> target = null, bool clearTarget = true)
            => GFTools.Copy(m_Items, target, clearTarget);

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all
        ///     <typeparamref name="TCatalogItemAsset"/> instances of this catalog and returns their count.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <typeparamref name="TCatalogItemAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItemAsset">
        ///     The type of items to find.
        /// </typeparam>
        /// <returns>
        ///     The number of <typeparamref name="TCatalogItemAsset"/> instances of this catalog.
        /// </returns>
        /// <remarks>
        ///     There is no constraint on the generic so the given collection can be as basic as needed and reused with
        ///     different catalogs.
        /// </remarks>
        public int GetItems<TCatalogItemAsset>(ICollection<TCatalogItemAsset> target = null, bool clearTarget = true)
            where TCatalogItemAsset : CatalogItemAsset
        {
            if (clearTarget)
            {
                target?.Clear();
            }

            var count = 0;
            foreach (var item in m_Items)
            {
                if (item is TCatalogItemAsset typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Looks for a <see cref="CatalogItemAsset"/> instance by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItemAsset"/> to find.
        /// </param>
        /// <returns>
        ///     The requested <see cref="CatalogItemAsset"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty or whitespace
        /// </exception>
        public CatalogItemAsset FindItem(string key)
        {
            GFTools.ThrowIfArgNull(key, nameof(key));

            foreach (var item in m_Items)
            {
                if (item.key == key)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all
        ///     <typeparamref name="TCatalogItemAsset"/> instances of this catalog matching the given
        ///     <paramref name="tag"/> and returns their count.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> instance used as a filter.
        /// </param>
        /// <param name="target">
        ///     The target container of all the matching <typeparamref name="TCatalogItemAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItemAsset">
        ///     Type inherited from <see cref="CatalogItemAsset"/> to look for.
        /// </typeparam>
        /// <returns>
        ///     The number of <typeparamref name="TCatalogItemAsset"/>
        ///     instances matching the tag filter in this catalog.
        /// </returns>
        internal int FindItemsInternal<TCatalogItemAsset>
            (TagAsset tag, ICollection<CatalogItemAsset> target = null, bool clearTarget = true)
            where TCatalogItemAsset : CatalogItemAsset
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            var count = 0;

            if (clearTarget)
            {
                target?.Clear();
            }

            foreach (var item in m_Items)
            {
                if (item.HasTag(tag) && item is TCatalogItemAsset typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all
        ///     <see cref="CatalogItemAsset"/> instances of this catalog matching the given <paramref name="tag"/>
        ///     and returns their count.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> instance used as a filter.
        /// </param>
        /// <param name="target">
        ///     The target container of all the matching <see cref="CatalogItemAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="CatalogItemAsset"/> instances matching the tag filter in this catalog.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> parameter is <c>null</c>.
        /// </exception>
        public int FindItems(TagAsset tag, ICollection<CatalogItemAsset> target = null, bool clearTarget = true)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return FindItemsInternal<CatalogItemAsset>(tag, target, clearTarget);
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all
        ///     <typeparamref name="TCatalogItemAsset"/> instances of this catalog matching the given
        ///     <paramref name="tag"/> and returns their count.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> instance used as a filter.
        /// </param>
        /// <param name="target">
        ///     The target container of all the matching <typeparamref name="TCatalogItemAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <typeparam name="TCatalogItemAsset">
        ///     Type inherited from <see cref="CatalogItemAsset"/> to look for.
        /// </typeparam>
        /// <returns>
        ///     The number of <typeparamref name="TCatalogItemAsset"/> instances matching the tag filter in this
        ///     catalog.
        /// </returns>
        public int FindItems<TCatalogItemAsset>
            (TagAsset tag, ICollection<CatalogItemAsset> target = null, bool clearTarget = true)
            where TCatalogItemAsset : CatalogItemAsset
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return FindItemsInternal<TCatalogItemAsset>(tag, target, clearTarget);
        }

        /// <summary>
        ///     Configures the specified <paramref name="builder"/>
        ///     with the specific content of this catalog.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data with an external source.
        /// </param>
        protected void ConfigureCatalog(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            foreach (var itemAsset in m_Items)
            {
                itemAsset.Configure(builder, valueProvider);
            }
        }

        /// <summary>
        ///     Override all data that are used by Game Foundation systems
        ///     before <see cref="ICatalogConfigurator.Configure"/> is called.
        /// </summary>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data with an external source.
        /// </param>
        void OverridePreConfigurationData(IExternalValueProvider valueProvider)
        {
            foreach (var itemAsset in m_Items)
            {
                itemAsset.OverridePreConfigurationData(valueProvider);
            }
        }

        /// <summary>
        ///     A value provider to override some catalog item's data
        ///     with an external source when configuring them.
        /// </summary>
        IExternalValueProvider m_ValueProvider;

        /// <summary>
        ///     Creates a default data structure for a new player.
        /// </summary>
        /// <returns>
        ///     Returns the default Game Foundation data.
        /// </returns>
        public GameFoundationData CreateDefaultData()
        {
            // count quantity of unique items which requested InitialAllocation
            var initialAllocations = 0;

            var inventoryItemDefs = new List<InventoryItemDefinitionAsset>();
            GetItems(inventoryItemDefs);

            foreach (var definition in inventoryItemDefs)
            {
                if (definition.initialAllocation < 0)
                {
                    definition.initialAllocation.SetExternalValue(0);

                    k_Logger.LogWarning(
                        $"The inventory item definition \"{definition.key}\" has a negative " +
                        $"{nameof(definition.initialAllocation)}, it has been set to 0 to avoid errors.");
                }

                initialAllocations += definition.initialAllocation;
            }

            var currencyAssets = new List<CurrencyAsset>();

            // setup GameFoundationData with correct sizes
            var data = new GameFoundationData
            {
                inventoryManagerData = new InventoryManagerData
                {
                    items = new InventoryItemData[initialAllocations]
                },
                walletData = new WalletData
                {
                    balances = new BalanceData[GetItems(currencyAssets)]
                },
                rewardManagerData = RewardManagerData.Empty
            };

            // add all inventory item initial allocations
            var inventoryManagerDataOn = 0;
            foreach (var inventory in inventoryItemDefs)
            {
                for (int i = 0; i < inventory.initialAllocation; ++i)
                {
                    data.inventoryManagerData.items[inventoryManagerDataOn] = new InventoryItemData
                    {
                        definitionKey = inventory.key,
                        quantity = inventory.initialQuantityPerStack,
                        id = Guid.NewGuid().ToString()
                    };

                    ++inventoryManagerDataOn;
                }
            }

            // add all currency initial balances
            for (var i = 0; i < currencyAssets.Count; i++)
            {
                var currency = currencyAssets[i];
                if (currency.maximumBalance > 0
                    && currency.initialBalance > currency.maximumBalance)
                {
                    currency.initialBalance.SetExternalValue(currency.maximumBalance);

                    var message = $"The initial balance for {currency.key} has been" +
                        $" clamped to {currency.maximumBalance.currentValue.ToString()} to respect the maximum balance.";
                    k_Logger.LogWarning(message);
                }

                data.walletData.balances[i] = new BalanceData
                {
                    currencyKey = currency.key,
                    balance = currency.initialBalance
                };
            }

            return data;
        }

        /// <summary>
        ///     Set the given <paramref name="valueProvider"/> as the external value provider for this database.
        ///     Also override catalog item's data that are not configured.
        /// </summary>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data with an external source.
        /// </param>
        public void SetValueProvider(IExternalValueProvider valueProvider)
        {
            m_ValueProvider = valueProvider;

            OverridePreConfigurationData(m_ValueProvider);
        }

        /// <inheritdoc/>
        void ICatalogConfigurator.Configure(CatalogBuilder builder)
        {
            m_TagCatalog.Configure(builder);

            ConfigureCatalog(builder, m_ValueProvider);
        }

        // We are not using SerializedObject or SerializedProperty.DeleteArrayElementAtIndex
        // in our editors, therefore nulls can result when we delete an item from the catalog,
        // even though we are using RemoveObjectFromAsset and SetDirty properly.
        // We can ensure no nulls remain when we serialize and deserialize.

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            // repair the list in case of a null value
            for (var i = m_Items.Count - 1; i >= 0; i--)
            {
                var item = m_Items[i];
                if (ReferenceEquals(null, item))
                {
                    m_Items.RemoveAt(i);
                }
            }
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            // repair the list in case of a null value
            for (var i = m_Items.Count - 1; i >= 0; i--)
            {
                var item = m_Items[i];
                if (ReferenceEquals(null, item))
                {
                    m_Items.RemoveAt(i);
                }
            }
        }
    }
}
