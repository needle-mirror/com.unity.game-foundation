using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying all the payout items in a Transaction.
    /// </summary>
    [AddComponentMenu("Game Foundation/Payout View", 2)]
    [ExecuteInEditMode]
    public class PayoutView : MonoBehaviour
    {
        /// <summary>
        ///     The key of the Transaction Item being displayed.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        [SerializeField]
        internal string m_TransactionKey;

        /// <summary>
        ///     The <see cref="BaseTransaction"/> to display in the view at runtime.
        /// </summary>
        public BaseTransaction transaction => m_Transaction;

        BaseTransaction m_Transaction;

        /// <summary>
        ///     The string to prefix the payout <see cref="InventoryItem"/> counts when displaying the
        ///     transaction's payouts.
        /// </summary>
        public string itemPayoutCountPrefix => m_ItemPayoutCountPrefix;

        [SerializeField]
        internal string m_ItemPayoutCountPrefix;

        /// <summary>
        ///     The string to prefix the payout <see cref="Currency"/> counts when displaying the transaction's payouts.
        /// </summary>
        public string currencyPayoutCountPrefix => m_CurrencyPayoutCountPrefix;

        [SerializeField]
        internal string m_CurrencyPayoutCountPrefix;

        /// <summary>
        ///     The Static Property key for the icon of the Inventory or Currency payout items, as
        ///     specified in their Static Properties.
        /// </summary>
        public string payoutItemIconPropertyKey => m_PayoutItemIconPropertyKey;

        [SerializeField]
        internal string m_PayoutItemIconPropertyKey;

        /// <summary>
        ///     The Payout Item prefab to use when displaying the transaction's payouts.
        /// </summary>
        public ImageInfoView payoutItemPrefab => m_PayoutItemPrefab;

        [SerializeField]
        internal ImageInfoView m_PayoutItemPrefab;

        /// <summary>
        ///     The GameObject to use as a separator between Payout Items when auto generating promotion image.
        /// </summary>
        public GameObject separatorPrefab => m_SeparatorPrefab;

        [SerializeField]
        internal GameObject m_SeparatorPrefab;

        /// <summary>
        ///     Default count prefix for inventory item and currency.
        /// </summary>
        internal const string kDefaultCountPrefix = "x";

        /// <summary>
        ///     Specifies whether this view is driven by other component
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;

#if UNITY_EDITOR
        /// <summary>
        ///     The <see cref="BaseTransactionAsset"/> to display in the view at editor time.
        /// </summary>
        BaseTransactionAsset m_TransactionAsset;
#endif

        /// <summary>
        ///     A name to use when generating PayoutItem GameObjects under Payout Items Image Container.
        /// </summary>
        const string kPayoutItemGameObjectName = "PayoutItem";

        /// <summary>
        ///     A name to use when generating Separator GameObjects under Auto Generated Image Container
        /// </summary>
        const string kSeparatorGameObjectName = "Separator";

        /// <summary>
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        bool m_IsDirty;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<PayoutView>();

        /// <summary>
        ///     Adds listeners, if the application is playing.
        /// </summary>
        void OnEnable()
        {
            GameFoundationSdk.initialized += InitializeComponentData;

            if (!(m_Transaction is null) && !m_IsDrivenByOtherComponent)
            {
                UpdateContent();
            }
        }

        /// <summary>
        ///     Removes listeners, if the application is playing.
        /// </summary>
        void OnDisable()
        {
            GameFoundationSdk.initialized -= InitializeComponentData;
        }

        /// <summary>
        ///     Initializes PayoutView content before the first frame update if Game Foundation Sdk is already
        ///     initialized. If Game Foundation SDK is not yet initialized, it sets the view to a blank state and
        ///     waits for Game Foundation Sdk to initialize.
        /// 
        ///     If this PayoutView instance is already initialized by another component no action will be taken.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Waiting for initialization.");
                UpdateContent();
                return;
            }

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk
            // initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Transaction is null)
            {
                InitializeComponentData();
            }
        }

        /// <summary>
        ///     Initializes PayoutView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            m_IsDirty = true;
        }

        /// <summary>
        ///     Initializes PayoutView with needed info.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="BaseTransaction"/> the payouts are attached to.
        /// </param>
        /// <param name="payoutItemIconPropertyKey">
        ///     The key to use for looking up the payout icons as specified in the static properties of their respective
        ///     items or currencies.
        /// </param>
        /// <param name="itemPayoutCountPrefix">
        ///     The string to prepend the count of how much of an item is distributed as part of this payout.
        /// </param>
        /// <param name="currencyPayoutCountPrefix">
        ///     The string to prepend the count of how much currency is distributed as part of this payout.
        /// </param>
        internal void Init(BaseTransaction transaction, string payoutItemIconPropertyKey,
            string itemPayoutCountPrefix = kDefaultCountPrefix, string currencyPayoutCountPrefix = kDefaultCountPrefix)
        {
            m_Transaction = transaction;
#if UNITY_EDITOR
            m_TransactionAsset = null;
#endif
            Init(transaction?.key, payoutItemIconPropertyKey, itemPayoutCountPrefix, currencyPayoutCountPrefix);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Initializes PayoutView with needed info.
        /// </summary>
        /// <param name="transactionAsset">
        ///     The <see cref="BaseTransactionAsset"/> the payouts are attached to.
        /// </param>
        /// <param name="payoutItemIconPropertyKey">
        ///     The key to use for looking up the payout icons as specified in the static properties of their respective
        ///     items or currencies.
        /// </param>
        /// <param name="itemPayoutCountPrefix">
        ///     The string to prepend the count of how much of an item is distributed as part of this payout.
        /// </param>
        /// <param name="currencyPayoutCountPrefix">
        ///     The string to prepend the count of how much currency is distributed as part of this payout.
        /// </param>
        internal void Init(BaseTransactionAsset transactionAsset,string payoutItemIconPropertyKey,
            string itemPayoutCountPrefix = kDefaultCountPrefix, string currencyPayoutCountPrefix = kDefaultCountPrefix)
        {
            m_TransactionAsset = transactionAsset;
            m_Transaction = null;
            Init(transactionAsset?.key, payoutItemIconPropertyKey, itemPayoutCountPrefix, currencyPayoutCountPrefix);
        }
#endif

        /// <summary>
        ///     Initializes PayoutView with blank state, used when parent component doesn't want to generate payouts,
        ///     but wants <see cref="m_IsDrivenByOtherComponent"/> to be true.
        /// </summary>
        internal void Init()
        {
            m_Transaction = null;

#if UNITY_EDITOR
            m_TransactionAsset = null;
#endif
            Init(string.Empty, payoutItemIconPropertyKey, itemPayoutCountPrefix, currencyPayoutCountPrefix);
        }

        /// <summary>
        ///     Initializes PayoutView with needed info.
        /// </summary>
        /// <param name="transactionKey">
        ///     The transaction key whose payouts will be displayed.
        /// </param>
        /// <param name="payoutItemIconPropertyKey">
        ///     The key to use for looking up the payout icons as specified in the static properties of their respective
        ///     items or currencies.
        /// </param>
        /// <param name="itemPayoutCountPrefix">
        ///     The string to prepend the count of how much of an item is distributed as part of this payout.
        /// </param>
        /// <param name="currencyPayoutCountPrefix">
        ///     The string to prepend the count of how much currency is distributed as part of this payout.
        /// </param>
        void Init(string transactionKey, string payoutItemIconPropertyKey,
            string itemPayoutCountPrefix = kDefaultCountPrefix, string currencyPayoutCountPrefix = kDefaultCountPrefix)
        {
            m_TransactionKey = transactionKey;
            m_PayoutItemIconPropertyKey = payoutItemIconPropertyKey;
            m_ItemPayoutCountPrefix = itemPayoutCountPrefix;
            m_CurrencyPayoutCountPrefix = currencyPayoutCountPrefix;

            m_IsDrivenByOtherComponent = true;

            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here causes a frame delay
            // when being driven by a parent component that makes this object look out of sync with its parent.
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Transaction Item that should be displayed by this view.
        /// </summary>
        /// <param name="transaction">
        ///     A reference to <see cref="BaseTransaction"/> that should be displayed.
        /// </param>
        public void SetTransaction(BaseTransaction transaction)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetTransaction)))
            {
                return;
            }

            m_Transaction = transaction;
            m_TransactionKey = transaction?.key;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Static Property key that will be used when displaying the transaction's payout item images.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined in the static properties of the transaction's payouts.
        /// </param>
        public void SetPayoutItemIconPropertyKey(string propertyKey)
        {
            if (m_PayoutItemIconPropertyKey == propertyKey)
            {
                return;
            }

            m_PayoutItemIconPropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the string with which to prefix the Currency payout counts.
        /// </summary>
        /// <param name="prefix">
        ///     The string to prefix.
        /// </param>
        public void SetCurrencyPayoutCountPrefix(string prefix)
        {
            if (m_CurrencyPayoutCountPrefix == prefix)
            {
                return;
            }

            m_CurrencyPayoutCountPrefix = prefix;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the string with which to prefix the Inventory Item payout counts.
        /// </summary>
        /// <param name="prefix">
        ///     The string to prefix.
        /// </param>
        public void SetInventoryItemPayoutCountPrefix(string prefix)
        {
            if (m_ItemPayoutCountPrefix == prefix)
            {
                return;
            }

            m_ItemPayoutCountPrefix = prefix;
            m_IsDirty = true;
        }

        /// <summary>
        ///     The prefab to use for each payout item when auto generating the promotion image.
        /// </summary>
        /// <param name="prefab">
        ///     The prefab to use when displaying the Payout Items.
        /// </param>
        public void SetPayoutItemPrefab(ImageInfoView prefab)
        {
            if (m_PayoutItemPrefab == prefab)
            {
                return;
            }

            m_PayoutItemPrefab = prefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the prefab to use for as a separator when when auto generating the promotion image.
        /// </summary>
        /// <param name="prefab">
        ///     The prefab to use when displaying the separator between Payout Items.
        /// </param>
        public void SetSeparatorPrefab(GameObject prefab)
        {
            if (m_SeparatorPrefab == prefab)
            {
                return;
            }

            m_SeparatorPrefab = prefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     At runtime, assigns the appropriate value for <see cref="m_Transaction"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void UpdateRuntimeObject()
        {
            if (!GameFoundationSdk.IsInitialized)
                return;

            if (m_Transaction is null && !string.IsNullOrEmpty(m_TransactionKey) ||
                !(m_Transaction is null) && m_Transaction.key != m_TransactionKey)
            {
                m_Transaction = GetTransaction(m_TransactionKey);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     At editor time, assigns the appropriate value for <see cref="m_Transaction"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void UpdateEditorTimeAsset()
        {
            if (m_TransactionAsset is null && !string.IsNullOrEmpty(m_TransactionKey) ||
                !(m_TransactionAsset is null) && m_TransactionAsset.key != m_TransactionKey)
            {
                m_TransactionAsset = PrefabTools.GetLookUpCatalogAsset()
                    .FindItem(m_TransactionKey) as BaseTransactionAsset;
            }
        }
#endif

        /// <summary>
        ///     Finds Base Transaction definition in the Transaction catalog.
        /// </summary>
        /// <param name="definitionKey">
        ///     The definition key of <see cref="BaseTransaction"/>.
        /// </param>
        /// <returns>
        ///     A reference to <see cref="BaseTransaction"/>.
        /// </returns>
        BaseTransaction GetTransaction(string definitionKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(definitionKey))
                return null;

            var transactionItem = GameFoundationSdk.catalog?.Find<BaseTransaction>(definitionKey);
            return transactionItem;
        }

        /// <summary>
        ///     Checks to see whether any properties have been changed (by checking <see cref="m_IsDirty"/>) and
        ///     if so, calls <see cref="UpdateContent"/> before resetting the flag.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;

                if (Application.isPlaying)
                {
                    UpdateRuntimeObject();
                }
#if UNITY_EDITOR
                else
                {
                    UpdateEditorTimeAsset();
                }
#endif
                UpdateContent();
            }
        }

        /// <summary>
        ///     Updates the item name, item icon, badge text, and PurchaseButton.
        /// </summary>
        internal void UpdateContent()
        {
            ClearContent();
            if (Application.isPlaying)
            {
                if (m_PayoutItemPrefab is null && GameFoundationSdk.IsInitialized)
                {
                    k_GFLogger.LogWarning("PayoutItem Prefab needs to be defined to display payout images.");
                }

                UpdateContentAtRuntime();
            }
#if UNITY_EDITOR
            else
            {
                UpdateContentAtEditor();
            }
#endif
        }

        /// <summary>
        ///     Clears payout image container.
        /// </summary>
        internal void ClearContent()
        {
            var toRemove = new List<Transform>();

            foreach (Transform child in transform)
            {
                if (child.name == kPayoutItemGameObjectName || child.name == kSeparatorGameObjectName)
                {
                    toRemove.Add(child);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(toRemove[i].gameObject);
                }
                else
                {
                    DestroyImmediate(toRemove[i].gameObject);
                }
            }
        }

        /// <summary>
        ///     Gets a list of sprites, one for each Inventory or Currency item listed in <see cref="m_Transaction"/>'s
        ///     Payouts list that have a static property key for <see cref="m_PayoutItemIconPropertyKey"/>, at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            if (m_Transaction is null)
            {
                return;
            }

            var exchanges = new List<ExchangeDefinition>();
            m_Transaction.payout.GetExchanges(exchanges);

            for (var i = 0; i < exchanges.Count; i++)
            {
                var exchange = exchanges[i];
                var shouldPrependSeparator = i > 0 && exchanges.Count > 1 && !(m_SeparatorPrefab is null);
                var quantity = "";

                switch (exchange.tradableDefinition)
                {
                    case Currency _:
                        quantity = (m_CurrencyPayoutCountPrefix ?? "") + exchange.amount;
                        break;
                    case InventoryItemDefinition _:
                        quantity = (m_ItemPayoutCountPrefix ?? "") + exchange.amount;
                        break;
                }

                InstantiateSeparatorIfNecessary(shouldPrependSeparator);
                var imageInfoView = InstantiateImageInfoView();

                if (exchange.tradableDefinition.TryGetStaticProperty(m_PayoutItemIconPropertyKey, out var iconProperty))
                {
                    SetImageInfoView(imageInfoView, quantity, iconProperty);
                }
                else
                {
                    SetImageInfoView(imageInfoView, quantity);
                    k_GFLogger.LogWarning($"The \"{m_Transaction.displayName}\" transaction's " +
                                          $"\"{exchange.tradableDefinition.displayName}\" payout does not have a " +
                                          $"static property with the name \"{m_PayoutItemIconPropertyKey}\" so it " +
                                          $"will not be shown in the transaction's view.");
                }
                // To force rebuilt Layouts at Editor and Runtime
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) transform.parent);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Gets a list of sprites, one for each Inventory or Currency item listed in
        ///     <see cref="m_TransactionAsset"/>'s Payouts list that have a static property key for
        ///     <see cref="m_PayoutItemIconPropertyKey"/>, at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            if (m_TransactionAsset is null)
            {
                return;
            }

            var exchangeObjects = new List<ExchangeDefinitionObject>();
            m_TransactionAsset.payout.GetItems(exchangeObjects);

            for (var i = 0; i < exchangeObjects.Count; i++)
            {
                var exchangeObject = exchangeObjects[i];
                var shouldPrependSeparator = i > 0 && exchangeObjects.Count > 1 && !(m_SeparatorPrefab is null);
                var quantity = "";

                switch (exchangeObject.catalogItem)
                {
                    case CurrencyAsset _:
                        quantity = (m_CurrencyPayoutCountPrefix ?? "") + exchangeObject.amount;
                        break;

                    case InventoryItemDefinitionAsset _:
                        quantity = (m_ItemPayoutCountPrefix ?? "") + exchangeObject.amount;
                        break;
                }

                InstantiateSeparatorIfNecessary(shouldPrependSeparator);
                var imageInfoView = InstantiateImageInfoView();

                if (exchangeObject.catalogItem.TryGetStaticProperty(m_PayoutItemIconPropertyKey, out var iconProperty))
                {
                    SetImageInfoView(imageInfoView, quantity, iconProperty);
                }
                else
                {
                    SetImageInfoView(imageInfoView, quantity);
                    k_GFLogger.LogWarning(
                        $"The \"{m_TransactionAsset.displayName.currentValue}\" transaction's " +
                        $"\"{exchangeObject.catalogItem.displayName.currentValue}\" payout does not have a static " +
                        $"property with the name \"{m_PayoutItemIconPropertyKey}\" so it will not be " +
                        $"shown in the transaction's view.");
                }
                // To force rebuilt Layouts at Editor and Runtime
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) transform.parent);
            }
        }
#endif

        void InstantiateSeparatorIfNecessary(bool shouldPrependSeparator)
        {
            if (!shouldPrependSeparator || separatorPrefab == null)
            {
                return;
            }

            var separator = Instantiate(m_SeparatorPrefab, transform);
            separator.transform.localScale = Vector3.one;
            separator.name = kSeparatorGameObjectName;
        }

        ImageInfoView InstantiateImageInfoView()
        {
            if (m_PayoutItemPrefab == null)
            {
                return null;
            }

            var imageInfoView = Instantiate(m_PayoutItemPrefab, transform).GetComponent<ImageInfoView>();
            imageInfoView.transform.localScale = Vector3.one;
            imageInfoView.name = kPayoutItemGameObjectName;

            return imageInfoView;
        }

        void SetImageInfoView(ImageInfoView imageInfoView, string infoText)
        {
            if (imageInfoView == null)
            {
                return;
            }

            imageInfoView.SetText(infoText);
        }

        void SetImageInfoView(ImageInfoView imageInfoView, string infoText, Property imageProperty)
        {
            if (imageInfoView == null)
            {
                return;
            }

            imageInfoView.SetView(infoText, imageProperty);
        }

        /// <summary>
        ///     When changes are made in the Inspector, set <see cref="m_IsDirty"/> to true
        ///     in order to trigger <see cref="UpdateContent"/>
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
