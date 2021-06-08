using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
#endif
#if UNITY_EDITOR
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying the Transaction Items contained within a given store.
    ///     When attached to a game object, it will create a TransactionItemView (<see cref="TransactionItemView"/>) for
    ///     each store item in the designated list, with the given game object as their parent.
    /// </summary>
    [AddComponentMenu("Game Foundation/Store View", 1)]
    [ExecuteInEditMode]
    public class StoreView : MonoBehaviour
    {
        /// <summary>
        ///     The identifier of the Store being purchased.
        /// </summary>
        public string storeKey => m_StoreKey;

        /// <inheritdoc cref="storeKey"/>
        [SerializeField]
        internal string m_StoreKey;

        /// <summary>
        ///     The <see cref="Store"/> to display in the view.
        /// </summary>
        public Store store => m_Store;

        Store m_Store;

        /// <summary>
        ///     The identifier of the tag items in the specified store should be filtered to for display.
        /// </summary>
        public string tagKey => m_TagKey;

        /// <inheritdoc cref="tagKey"/>
        [SerializeField]
        internal string m_TagKey;

        /// <summary>
        ///     The Transaction Item <see cref="Tag"/> that the specified store should be filtered to for its default
        ///     display. If a <see cref="revealHiddenItemsButton"/> is provided, the entire store list will be revealed,
        ///     irrespective of this filter.
        /// </summary>
        public new Tag tag => m_Tag;

        Tag m_Tag;

        /// <summary>
        ///     The Static Property key string that should be used for getting the item icon sprites of the Transaction
        ///     Items for displaying in the Store View. The key specified here will overwrite any that
        ///     might be set in the supplied <see cref="transactionItemPrefab"/>.
        /// </summary>
        public string itemIconSpritePropertyKey => m_ItemIconSpritePropertyKey;

        [SerializeField]
        internal string m_ItemIconSpritePropertyKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the price icon sprite of the Transaction
        ///     Item for displaying in the <see cref="PurchaseButton"/>. The key specified here will overwrite any that
        ///     might be set in the supplied <see cref="transactionItemPrefab"/>.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey;

        /// <summary>
        ///     The Static Property key for the icon of the Inventory or Currency payout items of a bundle transaction,
        ///     as specified in the payout items' Static Properties. The key specified here will overwrite any that
        ///     might be set in the supplied <see cref="bundleItemPrefab"/>.
        /// </summary>
        public string payoutItemIconPropertyKey => m_PayoutItemIconPropertyKey;

        [SerializeField]
        internal string m_PayoutItemIconPropertyKey;

        /// <summary>
        ///     The Static Property key for the text to display in the badge for any Bundle Transaction Items that have
        ///     the key defined. The key specified here will overwrite any that might be set in the supplied
        ///     <see cref="bundleItemPrefab"/>.
        /// </summary>
        public string badgeTextPropertyKey => m_BadgeTextPropertyKey;

        [SerializeField]
        internal string m_BadgeTextPropertyKey;

        /// <summary>
        ///     The string to display on Purchase Button if the Transaction Item has no cost. The string specified here
        ///     will overwrite any that might be set in the supplied <see cref="transactionItemPrefab"/>.
        /// </summary>
        public string noPriceString => m_NoPriceString;

        [SerializeField]
        internal string m_NoPriceString = PurchaseButton.kDefaultNoPriceString;

        /// <summary>
        ///     The string to prefix the payout <see cref="InventoryItem"/> counts when displaying a bundle
        ///     transaction's payouts. The string specified here will overwrite any that might be set in the supplied
        ///     <see cref="bundleItemPrefab"/>.
        /// </summary>
        public string itemPayoutCountPrefix => m_ItemPayoutCountPrefix;

        [SerializeField]
        internal string m_ItemPayoutCountPrefix = DetailedTransactionItemView.kDefaultCountPrefix;

        /// <summary>
        ///     The string to prefix the payout <see cref="Currency"/> counts when displaying a bundle transaction's
        ///     payouts. The key specified here will overwrite any that might be set in the supplied
        ///     <see cref="bundleItemPrefab"/>.
        /// </summary>
        public string currencyPayoutCountPrefix => m_CurrencyPayoutCountPrefix;

        [SerializeField]
        internal string m_CurrencyPayoutCountPrefix = DetailedTransactionItemView.kDefaultCountPrefix;

        /// <summary>
        ///     Controls whether items that are by default hidden from display in the Store should be shown or not.
        ///     By default this value is false.
        /// </summary>
        public bool showHiddenItems
        {
            get => m_ShowHiddenItems;
            set => ShowHiddenItems(value);
        }

        [SerializeField]
        internal bool m_ShowHiddenItems;

        /// <summary>
        ///     Use to enable or disable interaction on the store UI.
        /// </summary>
        public bool interactable
        {
            get => m_Interactable;
            set => SetInteractable(value);
        }

        [SerializeField]
        internal bool m_Interactable = true;

        /// <summary>
        ///     The prefab with <see cref="TransactionItemView"/> component attached to use for creating the list of
        ///     TransactionItemView items.
        /// </summary>
        public TransactionItemView transactionItemPrefab
        {
            get => m_TransactionItemPrefab;
            set => SetTransactionItemPrefab(value);
        }

        [SerializeField]
        internal TransactionItemView m_TransactionItemPrefab;

        /// <summary>
        ///     The prefab with <see cref="DetailedTransactionItemView"/> component attached to use for creating the
        ///     TransactionItemView items that have more than one payout, if UseBundleTransactionItem is enabled.
        /// </summary>
        public DetailedTransactionItemView bundleItemPrefab
        {
            get => m_BundleItemPrefab;
            set => SetBundleTransactionItemPrefab(value);
        }

        [SerializeField]
        internal DetailedTransactionItemView m_BundleItemPrefab;

        /// <summary>
        ///     The prefab with <see cref="TransactionItemView"/> component attached to use for creating the first
        ///     transaction item in the Store, if <see cref="useFeaturedTransactionItem"/> is enabled.
        ///
        ///     Note: If <see cref="useFeaturedTransactionItem"/> is enabled, and the first transaction item in the
        ///     Store has more than one payout, this prefab will not be used and only
        ///     <see cref="featuredBundleItemPrefab"/> will be used.
        /// </summary>
        public TransactionItemView featuredItemPrefab
        {
            get => m_FeaturedItemPrefab;
            set => SetFeaturedItemPrefab(value);
        }

        [SerializeField]
        internal TransactionItemView m_FeaturedItemPrefab;

        /// <summary>
        ///     The prefab with <see cref="DetailedTransactionItemView"/> component attached to use for creating the first
        ///     transaction item in the Store, if <see cref="useFeaturedTransactionItem"/> is enabled, and the first
        ///     item in the Store has more than one payout.
        ///
        /// </summary>
        public DetailedTransactionItemView featuredBundleItemPrefab
        {
            get => m_FeaturedBundleItemPrefab;
            set => SetFeaturedBundleItemPrefab(value);
        }

        [SerializeField]
        internal DetailedTransactionItemView m_FeaturedBundleItemPrefab;

        /// <summary>
        ///     The Transform in which to generate the list of <see cref="TransactionItemView"/> items.
        /// </summary>
        public Transform itemContainer => m_ItemContainer;

        [SerializeField]
        internal Transform m_ItemContainer;

        /// <summary>
        ///     Optional button that, when clicked, will reveal any transaction items in the Store list that don't have
        ///     the tag specified by <see cref="tag"/>.
        /// </summary>
        public Button revealHiddenItemsButton
        {
            get => m_RevealHiddenItemsButton;
            set => SetRevealHiddenItemsButton(value);
        }

        [SerializeField]
        internal Button m_RevealHiddenItemsButton;

        /// <summary>            
        ///     Callback that will get triggered any time one of the TransactionItemViews in the store has the status
        ///     of it's <see cref="PurchaseButton.itemPurchasableStatus"/> change.
        /// </summary>
        [Space]
        public PurchasableStatusChangedEvent onPurchasableStatusChanged;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store fails.
        /// </summary>
        public TransactionFailureEvent onTransactionFailed;

        /// <summary>
        ///     A callback for when any of the store's transaction's <see cref="PurchaseButton.itemPurchasableStatus"/>
        ///     changes. Wraps UnityEvent and accepts three parameters: the <see cref="PurchaseButton"/> the status is
        ///     changing on, the old <see cref="PurchasableStatus"/> and the new <see cref="PurchasableStatus"/>.
        /// </summary>
        [Serializable]
        public class PurchasableStatusChangedEvent : UnityEvent<PurchaseButton, PurchasableStatus, PurchasableStatus> { }

        /// <summary>
        ///     A callback for when a transaction is completed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/> as a
        ///     parameter.
        /// </summary>
        [Serializable]
        public class TransactionSuccessEvent : UnityEvent<BaseTransaction> { }

        /// <summary>
        ///     A callback for when a transaction is failed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/>
        ///     and Exception as a parameter.
        /// </summary>
        [Serializable]
        public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> { }

        /// <summary>
        ///     The list of <see cref="TransactionItemView"/> items that are instantiated using Transaction Item prefab
        ///     based on the specified store and tag.
        /// </summary>
        readonly List<TransactionItemView> m_TransactionItems = new List<TransactionItemView>();

        /// <summary>
        ///     Used in custom inspector UI to specify whether <see cref="bundleItemPrefab"/> should be supplied and used.
        ///     If it is disabled, it will use <see cref="transactionItemPrefab"/> for any bundle items instead.
        /// </summary>
        internal bool useBundleTransactionItem
        {
            get => m_UseBundleTransactionItem;
            set => UseBundleTransactionItem(value);
        }

        [SerializeField]
        internal bool m_UseBundleTransactionItem = true;

        /// <summary>
        ///     Used in custom inspector UI to specify whether any featured item prefabs should be supplied and
        ///     used for displaying the first transaction item of the StoreView. The two possible featured item prefabs
        ///     that could be supplied are <see cref="featuredItemPrefab"/> and <see cref="featuredBundleItemPrefab"/>,
        ///     which one will be used depends on whether the first transaction item in the Store has more than one
        ///     payout or not.
        /// </summary>
        internal bool useFeaturedTransactionItem
        {
            get => m_UseFeaturedTransactionItem;
            set => UseFeaturedTransactionItem(value);
        }

        [SerializeField]
        internal bool m_UseFeaturedTransactionItem;

        /// <summary>
        ///     Used in custom inspector UI to specify whether <see cref="revealHiddenItemsButton"/> should be
        ///     supplied and displayed.
        /// </summary>
        internal bool useRevealHiddenItemsButton
        {
            get => m_UseRevealHiddenItemsButton;
            set => UseRevealHiddenItemsButton(value);
        }

        [SerializeField]
        internal bool m_UseRevealHiddenItemsButton;

        /// <summary>
        ///     Specifies whether the fields for Bundle Transaction Items in the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showBundleTransactionItemEditorFields = true;

        /// <summary>
        ///     Specifies whether the Featured Transaction Item fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showFeaturedItemsEditorFields = true;

        /// <summary>
        ///     The Game Object with ScrollRect component where scrollable content resides.
        /// </summary>
        ScrollRect m_ScrollRect;

        /// <summary>
        ///     The final Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        Transform itemParentTransform => m_ItemContainer ? m_ItemContainer : transform;

        /// <summary>
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        protected bool m_IsDirty;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<StoreView>();
        
        /// <summary>
        ///     Store item data that is going to be used to generate auto-generated content.
        /// </summary>
        readonly struct StoreItem
        {
            public readonly string transactionKey;
            public readonly bool isVirtualTransaction;
            public readonly int payoutCount;

            /// <summary>
            ///     Construct StoreItem.
            /// </summary>
            /// <param name="transactionKey">
            ///     The key for <see cref="BaseTransaction"/>
            /// </param>
            /// <param name="isVirtualTransaction">
            ///     Specifies whether is virtual transaction or not
            /// </param>
            /// <param name="payoutCount">
            ///     The payout count of the transaction.
            /// </param>
            public StoreItem(string transactionKey, bool isVirtualTransaction, int payoutCount = 0)
            {
                this.transactionKey = transactionKey;
                this.isVirtualTransaction = isVirtualTransaction;
                this.payoutCount = payoutCount;
            }
        }

        /// <summary>
        ///     Store item type.
        /// </summary>
        enum StoreItemType
        {
            TransactionItem,
            BundleTransactionItem,
            FeaturedTransactionItem,
            FeaturedBundleTransactionItem,
        }

        /// <summary>
        ///     Adds listeners, if the application is playing.
        /// </summary>
        void OnEnable()
        {
            GameFoundationSdk.initialized += RegisterEvents;
            GameFoundationSdk.initialized += InitializeComponentData;
            GameFoundationSdk.willUninitialize += UnregisterEvents;

            if (GameFoundationSdk.IsInitialized)
            {
                RegisterEvents();
            }

            UpdateContent();
        }

        /// <summary>
        ///     Removes listeners, if the application is playing.
        /// </summary>
        void OnDisable()
        {
            GameFoundationSdk.initialized -= RegisterEvents;
            GameFoundationSdk.initialized -= InitializeComponentData;
            GameFoundationSdk.willUninitialize -= UnregisterEvents;

            if (GameFoundationSdk.IsInitialized)
            {
                UnregisterEvents();
            }
        }

        /// <summary>
        ///     Add necessary events for this view to Game Foundation.
        /// </summary>
        void RegisterEvents()
        {
            if (GameFoundationSdk.transactions == null)
                return;

            GameFoundationSdk.transactions.transactionSucceeded += OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed += OnTransactionFailed;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            if (GameFoundationSdk.transactions == null)
                return;

            GameFoundationSdk.transactions.transactionSucceeded -= OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed -= OnTransactionFailed;
        }

        /// <summary>
        ///     Initializes StoreView with needed info.
        /// </summary>
        /// <param name="storeKey">
        ///     The key for the store that should be displayed.
        /// </param>
        /// <param name="tagKey">
        ///     The key for the tag of items that should be displayed.
        /// </param>
        /// <param name="itemIconSpritePropertyKey">
        ///     The sprite name for item icon that will be displayed on this view.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The sprite name for price icon that will be displayed on PurchaseButton.
        /// </param>
        /// <param name="noPriceString">
        ///     The string to display on Purchase Button when there is no cost defined in the Transaction Item.
        /// </param>
        internal void Init(string storeKey, string tagKey, string itemIconSpritePropertyKey, string priceIconSpritePropertyKey, string noPriceString)
        {
            m_ItemIconSpritePropertyKey = itemIconSpritePropertyKey;
            m_PriceIconSpritePropertyKey = priceIconSpritePropertyKey;
            m_NoPriceString = noPriceString;

            SetStore(storeKey, tagKey);

            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here causes a frame delay
            // when being driven by a parent component that makes this object look out of sync with its parent.
            UpdateContent();
        }

        /// <summary>
        ///     Initializes the StoreView before the first frame update if Game Foundation Sdk was already
        ///     initialized before StoreView was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        /// </summary>
        void Start()
        {
            m_ScrollRect = gameObject.GetComponentInChildren<ScrollRect>(false);

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Store is null)
            {
                InitializeComponentData();
                return;
            }

            if (Application.isPlaying && !GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Waiting for initialization.");
            }
        }

        /// <summary>
        ///     Initializes StoreView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets which <see cref="Store"/> should be displayed by this view.
        /// </summary>
        /// <param name="storeKey">
        ///     The identifier for the <see cref="Store"/> that should be displayed.
        /// </param>
        /// <param name="tagKey">
        ///     The key of <see cref="Tag"/> in specified Transaction Items of the Store to be displayed.
        /// </param>
        /// <remarks>
        ///     If the <paramref name="storeKey"/> param is null or empty, or is not found in the store catalog no
        ///     action will be taken.
        /// </remarks>
        /// <remarks>
        ///     If the <paramref name="tagKey"/> param is null or empty, all transactions in a store will be displayed.
        /// </remarks>
        internal void SetStore(string storeKey, string tagKey = null)
        {
            m_StoreKey = storeKey;
            m_TagKey = tagKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Finds <see cref="Store"/> definition in the Store catalog.
        /// </summary>
        /// <param name="definitionKey">
        ///     The definition key of <see cref="Store"/>.
        /// </param>
        /// <returns>
        ///     A reference to <see cref="Store"/>.
        /// </returns>
        Store GetStore(string definitionKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(definitionKey))
                return null;

            var storeDefinition = GameFoundationSdk.catalog?.Find<Store>(definitionKey);
            if (!(storeDefinition is null) || !m_ShowDebugLogs) return storeDefinition;

            k_GFLogger.LogWarning($"Store \"{definitionKey}\" doesn't exist in Store catalog.");
            return null;
        }

        /// <summary>
        ///     Finds <see cref="Tag"/> definition in the Tag catalog.
        /// </summary>
        /// <param name="tagKey">
        ///     The key of <see cref="Tag"/> in specified Transactions of the Store to be displayed.
        /// </param>
        /// <returns>
        ///     A reference to <see cref="Tag"/>.
        /// </returns>
        Tag GetTag(string tagKey)
        {
            if (!Application.isPlaying)
                return null;

            return !string.IsNullOrEmpty(tagKey) ? GameFoundationSdk.tags.Find(tagKey) : null;
        }

        /// <summary>
        ///     Sets which <see cref="Store"/> should be displayed by this view.
        /// </summary>
        /// <param name="store">
        ///     The identifier for the <see cref="Store"/> that should be displayed.
        /// </param>
        /// <param name="tag">
        ///     A reference to <see cref="Tag"/> in specified Transactions of the Store to be displayed.
        /// </param>
        /// <remarks>
        ///     If the <paramref name="tag"/> param is null, all transactions in a store will be displayed.
        /// </remarks>
        public void SetStore(Store store, Tag tag = null)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetStore)))
            {
                return;
            }

            m_Store = store;
            m_StoreKey = store?.key;
            m_Tag = tag;
            m_TagKey = tag?.key;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Updates which tag of items within the store should be displayed by this view.
        /// </summary>
        /// <param name="tag">
        ///     A reference to <see cref="Tag"/> in specified Transactions of the Store to be displayed.
        ///     To show all Transactions in a store (no tag filtering) null can be passed as the tag.
        /// </param>
        public void SetTag(Tag tag)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetTag)))
            {
                return;
            }

            m_Tag = tag;
            m_TagKey = tag?.key;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the value of <see cref="transactionItemPrefab"/> and updates the view if the application is playing.
        /// </summary>
        /// <param name="itemPrefab">
        ///     The prefab of type <see cref="TransactionItemView"/> to use for displaying transaction items.
        /// </param>
        public void SetTransactionItemPrefab(TransactionItemView itemPrefab)
        {
            if (m_TransactionItemPrefab == itemPrefab)
                return;

            m_TransactionItemPrefab = itemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the value of <see cref="bundleItemPrefab"/> and updates the view if the application is playing.
        ///     If null is passed in, any bundle transaction items in the store view will use
        ///     <see cref="transactionItemPrefab"/> to display their content.
        /// </summary>
        /// <param name="itemPrefab">
        ///     The prefab of type <see cref="DetailedTransactionItemView"/> to use for displaying bundle transaction items.
        /// </param>
        public void SetBundleTransactionItemPrefab(DetailedTransactionItemView itemPrefab)
        {
            if (m_BundleItemPrefab == itemPrefab)
                return;

            m_UseBundleTransactionItem = !(itemPrefab is null);

            m_BundleItemPrefab = itemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the value of <see cref="featuredItemPrefab"/> and updates the view if the application is playing.
        ///     If null is passed in, the regularly specified transaction item prefabs will be used.
        /// </summary>
        /// <param name="itemPrefab">
        ///     The prefab of type <see cref="TransactionItemView"/> to use for displaying the first item in the Store view.
        /// </param>
        private void SetFeaturedItemPrefab(TransactionItemView itemPrefab)
        {
            if (m_FeaturedItemPrefab == itemPrefab)
                return;

            m_FeaturedItemPrefab = itemPrefab;

            if (m_FeaturedItemPrefab is null && m_FeaturedBundleItemPrefab is null)
            {
                m_UseFeaturedTransactionItem = false;
            }
            else
            {
                m_UseFeaturedTransactionItem = true;
            }

            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the value of <see cref="featuredBundleItemPrefab"/> and updates the view if the application is playing.
        ///     If null is passed in, the prefab specified in <see cref="featuredItemPrefab"/> will be used.
        /// </summary>
        /// <param name="itemPrefab">
        ///     The prefab of type <see cref="DetailedTransactionItemView"/> to use for displaying the first item in the
        ///     Store view if that item has more than one payout.
        /// </param>
        private void SetFeaturedBundleItemPrefab(DetailedTransactionItemView itemPrefab)
        {
            if (m_FeaturedBundleItemPrefab == itemPrefab)
                return;

            m_FeaturedBundleItemPrefab = itemPrefab;

            if (m_FeaturedBundleItemPrefab is null && m_FeaturedItemPrefab is null)
            {
                m_UseFeaturedTransactionItem = false;
            }
            else
            {
                m_UseFeaturedTransactionItem = true;
            }

            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the value of <see cref="revealHiddenItemsButton"/> and updates the view if the application is playing.
        ///     It is expected that the button's onClick listener will trigger <see cref="ShowHiddenItems"/> with
        ///     value of true, although that is up to user discretion.
        /// </summary>
        /// <param name="hiddenItemsButton">
        ///     The Button game object to use for revealing hidden items in the store view.
        /// </param>
        private void SetRevealHiddenItemsButton(Button hiddenItemsButton)
        {
            if (m_RevealHiddenItemsButton == hiddenItemsButton)
                return;

            m_UseRevealHiddenItemsButton = !(hiddenItemsButton is null);

            m_RevealHiddenItemsButton = hiddenItemsButton;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Static Property key for Transaction Item icon that will be displayed on this view.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key that is defined on Transaction Item for item icon sprite.
        /// </param>
        public void SetItemIconSpritePropertyKey(string propertyKey)
        {
            if (m_ItemIconSpritePropertyKey == propertyKey)
            {
                return;
            }

            m_ItemIconSpritePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Static Property key for price icon that will be displayed on <see cref="PurchaseButton"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key that is defined on the Currency or Inventory Item costs for the transaction.
        /// </param>
        public void SetPriceIconSpritePropertyKey(string propertyKey)
        {
            if (m_PriceIconSpritePropertyKey == propertyKey)
            {
                return;
            }

            m_PriceIconSpritePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Static Property key for the payout item icons used by <see cref="DetailedTransactionItemView"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key that is defined on the Currency or Inventory Item payouts for the transaction.
        /// </param>
        public void SetPayoutItemIconPropertyKey(string propertyKey)
        {
            if (m_PayoutItemIconPropertyKey == propertyKey)
            {
                return;
            }

            m_PayoutItemIconPropertyKey = propertyKey;

            if (m_UseBundleTransactionItem)
            {
                m_IsDirty = true;
            }
        }

        /// <summary>
        ///     Sets the Static Property key that will be used when displaying the bundle transaction's badge.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined in the bundle transaction's static properties for the transaction's badge.
        /// </param>
        public void SetBadgeTextPropertyKey(string propertyKey)
        {
            if (m_BadgeTextPropertyKey == propertyKey)
            {
                return;
            }

            m_BadgeTextPropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the string to display on Purchase Button when there is no cost defined in the Transaction Item.
        /// </summary>
        /// <param name="noPriceString">
        ///     The string to display.
        /// </param>
        public void SetNoPriceString(string noPriceString)
        {
            if (m_NoPriceString == noPriceString)
            {
                return;
            }

            m_NoPriceString = noPriceString;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the string to prefix the payout Currency counts with when displaying Bundle Transaction Items.
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
        ///     Sets the string to prefix the payout Inventory Item counts with when displaying Bundle Transaction Items.
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
        ///     Sets whether or not to display transactions that have more than one payout with a special bundle
        ///     transaction item prefab.
        /// </summary>
        /// <param name="useBundlePrefab">
        ///     Whether to use <see cref="bundleItemPrefab"/> to display transactions with more than one payout.
        /// </param>
        private void UseBundleTransactionItem(bool useBundlePrefab)
        {
            if (m_UseBundleTransactionItem == useBundlePrefab)
            {
                return;
            }

            m_UseBundleTransactionItem = useBundlePrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets whether or not to display the first transaction item of the StoreView using a special Featured
        ///     Transaction Item prefab.
        ///
        ///     Note: If <see cref="useFeaturedTransactionItem"/> is enabled, and the first transaction item in the
        ///     Store has more than one payout, this setting will not be used.
        /// </summary>
        /// <param name="useFeaturedItemPrefab">
        ///     Whether to use <see cref="featuredItemPrefab"/> to display transactions with more than one payout.
        /// </param>
        private void UseFeaturedTransactionItem(bool useFeaturedItemPrefab)
        {
            if (m_UseFeaturedTransactionItem == useFeaturedItemPrefab)
            {
                return;
            }

            m_UseFeaturedTransactionItem = useFeaturedItemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets whether or not to display the <see cref="revealHiddenItemsButton"/> if it has been provided.
        /// </summary>
        /// <param name="useHiddenItemsButton">
        ///     Whether to display <see cref="revealHiddenItemsButton"/> at the end of the store list.
        /// </param>
        private void UseRevealHiddenItemsButton(bool useHiddenItemsButton)
        {
            if (m_UseRevealHiddenItemsButton == useHiddenItemsButton)
            {
                return;
            }

            m_UseRevealHiddenItemsButton = useHiddenItemsButton;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Method that sets <see cref="showHiddenItems"/> and controls whether to filter the Store View to the
        ///     value set in <see cref="tag"/>.
        ///
        ///     If true is passed in, all items in the Store list will be shown, and, if a
        ///     <see cref="revealHiddenItemsButton"/> has been provided, it will hide the button.
        ///     If false is passed in, only the items that have the given <see cref="tag"/> will be shown and, if a
        ///     <see cref="revealHiddenItemsButton"/> has been provided, it will be shown.
        /// </summary>
        /// <param name="showItems">
        ///     Whether or not to show items that don't have the selected <see cref="tag"/>.
        /// </param>
        public void ShowHiddenItems(bool showItems)
        {
            if (m_ShowHiddenItems == showItems)
            {
                return;
            }

            m_ShowHiddenItems = showItems;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the <see cref="PurchaseButton"/>'s interactable state if the state specified is different from the
        ///     current state.
        /// </summary>
        /// <param name="interactable">
        ///     Whether the StoreView and the StoreView's TransactionItemViews should be enabled or not.
        /// </param>
        public void SetInteractable(bool interactable)
        {
            if (m_Interactable == interactable)
            {
                return;
            }

            m_Interactable = interactable;
            UpdateInteractable();
        }

        /// <summary>
        ///     Sets the interactability of all the child TransactionItemViews. Does not effect the interactability of
        ///     the StoreView itself.
        /// </summary>
        /// <param name="interactable">
        ///     Whether the TransactionItemViews in the StoreView should be enabled or not.
        /// </param>
        public void SetInteractableTransactionItems(bool interactable)
        {
            foreach (var itemView in m_TransactionItems)
            {
                itemView.interactable = interactable;
            }
        }

        /// <summary>
        ///     Gets the list of TransactionItemViews that represents all items being displayed in this store view for the
        ///     designated store.
        /// </summary>
        /// <returns>
        ///     Array of TransactionItemViews objects for items being displayed in the store view.
        /// </returns>
        public TransactionItemView[] GetItems()
        {
            return m_TransactionItems.ToArray();
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
                UpdateInteractable();
                UpdateRuntimeObjects();
                UpdateContent();
            }
        }

        /// <summary>
        ///     At runtime, assigns the appropriate value for <see cref="m_Store"/> and <see cref="m_Tag"/> from the
        ///     Catalog if needed.
        ///     If m_Store and m_StoreKey don't currently match, this replaces m_Store with the correct store by
        ///     searching the Catalog for m_StoreKey. Similarly, if or m_Tag and m_TagKey don't currently match, m_Tag
        ///     will be replaced with the correct tag using m_TagKey.
        /// </summary>
        void UpdateRuntimeObjects()
        {
            if (!Application.isPlaying || !GameFoundationSdk.IsInitialized)
                return;

            var valueChanged = false;

            if (m_Store is null && !string.IsNullOrEmpty(m_StoreKey) ||
                !(m_Store is null) && m_Store.key != m_StoreKey)
            {
                m_Store = GetStore(m_StoreKey);
                valueChanged = true;
            }

            if (m_Tag is null && !string.IsNullOrEmpty(m_TagKey) ||
                !(m_Tag is null) && m_Tag.key != m_TagKey)
            {
                m_Tag = GetTag(m_TagKey);
                valueChanged = true;
            }

            if (valueChanged)
            {
                ResetScrollContainerPosition();
            }
        }

        /// <summary>
        ///     Generates and instantiates the list of TransactionItemViews for display in the StoreView.
        /// </summary>
        internal void UpdateContent()
        {
            if (Application.isPlaying)
            {
                UpdateContentAtRuntime();
            }
#if UNITY_EDITOR
            else
            {
                UpdateContentAtEditor();
            }
#endif
        }
        
#if UNITY_EDITOR
        /// <summary>
        ///     To generate the content at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // To prevent the prefab from baking auto-generated content in Prefab Edit Mode 
            if (!this.ShouldRegenerateGameObjects())
            {
                return;
            }

            var storeItems = new List<StoreItem>();
            
            var storeAsset = !string.IsNullOrEmpty(m_StoreKey) ? PrefabTools.GetLookUpCatalogAsset().FindItem(m_StoreKey) as StoreAsset : null;
            if (!(storeAsset is null))
            {
                var storeItemObjects = new List<StoreItemObject>();
                storeAsset.GetStoreItems(storeItemObjects);

                var tagAsset = !string.IsNullOrEmpty(m_TagKey) ? PrefabTools.GetLookUpCatalogAsset().tagCatalog.FindTag(m_TagKey) : null;

                foreach (var storeItemObject in storeItemObjects)
                {
                    var transaction = storeItemObject.transaction;
                    if (tagAsset is null || transaction.HasTag(tagAsset))
                    {
                        if (transaction is VirtualTransactionAsset)
                        {
                            storeItems.Add(new StoreItem(transaction.key, true, transaction.payout.GetItems()));
                        }
                        else
                        {
                            storeItems.Add(new StoreItem(transaction.key, false, transaction.payout.GetItems()));
                        }
                    }
                }
            }
            
            GenerateStoreItems(storeItems, true);
        }
#endif
        
        /// <summary>
        ///     To generate the content at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            var storeItems = new List<StoreItem>();
            if (!(m_Store is null))
            {
                var transactions = new List<BaseTransaction>();
                if (m_Tag is null || m_ShowHiddenItems)
                {
                    m_Store.GetStoreItems(transactions);
                }
                else
                {
                    m_Store.FindStoreItems(m_Tag, transactions);
                }

                foreach (var transaction in transactions)
                {
                    if (transaction is VirtualTransaction)
                    {
                        storeItems.Add(new StoreItem(transaction.key, true, transaction.payout.GetExchanges()));
                    }
                    else
                    {
                        storeItems.Add(new StoreItem(transaction.key, false, transaction.payout.GetExchanges()));
                    }
                }   
            }

            GenerateStoreItems(storeItems);
            
            StartCoroutine(UpdateScrollbarStatus());
        }

        /// <summary>
        ///     Generate store items under <see cref="itemContainer"/> transform
        /// </summary>
        /// <param name="storeItems">
        ///     The list of the store items.
        /// </param>
        /// <param name="forceRegenerate">
        ///     Forces to remove all the store items under <see cref="itemContainer"/>
        ///     before generating the new content.
        /// </param> 
        void GenerateStoreItems(IEnumerable<StoreItem> storeItems, bool forceRegenerate = false)
        {
            if (forceRegenerate)
            {
                RemoveItems(0);
            }

            var firstItemSeen = false;
            var index = 0;
            foreach (var storeItem in storeItems)
            {
                if (!Application.isPlaying || storeItem.isVirtualTransaction || IsIAPTransactionPurchasable(storeItem.transactionKey))
                {
                    TransactionItemView item = null;
                    if (!firstItemSeen)
                    {
                        if (m_UseFeaturedTransactionItem)
                        {
                            if (storeItem.payoutCount > 1 && m_FeaturedBundleItemPrefab)
                            {
                                item = InitItemView(
                                    GetTransactionItemView(StoreItemType.FeaturedBundleTransactionItem, index, m_FeaturedBundleItemPrefab) as
                                        DetailedTransactionItemView, storeItem.transactionKey);
                            }
                            else if (m_FeaturedItemPrefab)
                            {
                                item = InitItemView(
                                    GetTransactionItemView(StoreItemType.FeaturedTransactionItem, index, m_FeaturedItemPrefab),
                                    storeItem.transactionKey);
                            }
                        }

                        firstItemSeen = true;
                    }

                    if (item is null)
                    {
                        if (storeItem.payoutCount > 1 && m_UseBundleTransactionItem && m_BundleItemPrefab)
                        {
                            item = InitItemView(
                                GetTransactionItemView(StoreItemType.BundleTransactionItem, index, m_BundleItemPrefab) as
                                    DetailedTransactionItemView, storeItem.transactionKey);
                        }
                        else
                        {
                            item = InitItemView(GetTransactionItemView(StoreItemType.TransactionItem, index, m_TransactionItemPrefab),
                                storeItem.transactionKey);
                        }
                    }

                    if (item is null) continue;
                    
                    if (!m_Interactable) item.interactable = false;

                    if (Application.isPlaying)
                    {
                        m_TransactionItems.Add(item);   
                    }
                }

                index++;
            }

            RemoveItems(index);

            if (m_RevealHiddenItemsButton)
            {
                m_RevealHiddenItemsButton.transform.SetAsLastSibling();
                m_RevealHiddenItemsButton.gameObject.SetActive(!m_ShowHiddenItems && m_UseRevealHiddenItemsButton);
            }
        }
        
        /// <summary>
        ///     Returns a <see cref="TransactionItemView"/> to use it while generating the content.
        ///     If there is already an instantiated <see cref="TransactionItemView"/> under <see cref="itemContainer"/> transform,
        ///     it returns the first eligible <see cref="TransactionItemView"/>.
        ///     If there is no eligible <see cref="TransactionItemView"/> under <see cref="itemContainer"/> transform,
        ///     it instantiate prefabs based on StoreItemType. 
        /// </summary>
        /// <param name="itemType">
        ///     Item type that you want to initialize to generate the content. 
        /// </param>
        /// <param name="itemIndex">
        ///     Item child index in <see cref="itemContainer"/> that you try to initialize.
        /// </param>
        /// <param name="prefab">
        ///     The prefab that is going to be instantiated.
        /// </param> 
        /// <returns>
        ///     <see cref="TransactionItemView"/> in item container. 
        /// </returns>
        TransactionItemView GetTransactionItemView(StoreItemType itemType, int itemIndex, TransactionItemView prefab)
        {
            if (itemIndex < itemParentTransform.childCount)
            {
                var itemTransform = itemParentTransform.GetChild(itemIndex);
                if (itemTransform.TryGetComponent<TransactionItemView>(out var transactionItemView) &&
                    transactionItemView.gameObject.activeSelf && itemTransform.name.StartsWith(itemType.ToString()))
                {
                    itemTransform.name = GetItemName(itemType, itemIndex);
                    return transactionItemView;
                }

                // if the transform under item container doesn't have TransactionItemView or StoreType is not match,
                // we remove that transform from the container and look at the next one.
                if (RemoveItems(itemIndex, 1))
                {
                    return GetTransactionItemView(itemType, itemIndex, prefab);
                }
            }
            
            // if there is no eligible TransactionItemView, we instantiate a new one.
            return InstantiateItemView(itemType, itemIndex, prefab);
        }

        /// <summary>
        ///     Instantiate store item prefab based on the store item type.
        /// </summary>
        /// <param name="itemType">
        ///     The type of the store item that you want to instantiate. 
        /// </param>
        /// <param name="itemIndex">
        ///     The index of the store item in <see cref="itemContainer"/>
        /// </param>
        /// <param name="prefab">
        ///     The prefab that is going to be instantiated.
        /// </param> 
        /// <returns>
        ///     <see cref="TransactionItemView"/> component that is on instantiated store item GameObject.
        /// </returns>
        TransactionItemView InstantiateItemView(StoreItemType itemType, int itemIndex, TransactionItemView prefab)
        {
            var itemView = Instantiate(prefab, itemParentTransform, true).GetComponent<TransactionItemView>();
            itemView.transform.localScale = Vector3.one;
            itemView.transform.SetSiblingIndex(itemIndex);
            itemView.name = GetItemName(itemType, itemIndex);

            return itemView;
        }

        /// <summary>
        ///     Generate store item name.
        /// </summary>
        /// <param name="itemType">
        ///     Store item type.
        /// </param>
        /// <param name="itemIndex">
        ///     The index of the store item in <see cref="itemContainer"/>
        /// </param>
        /// <returns>
        ///     The name of the store item.
        /// </returns>
        string GetItemName(StoreItemType itemType, int itemIndex)
        {
            return $"{itemType.ToString()} - #{itemIndex + 1}";
        }

        /// <summary>
        ///     Initialize <see cref="DetailedTransactionItemView"/> component.
        /// </summary>
        /// <param name="itemView">
        ///     <see cref="DetailedTransactionItemView"/> that you want to be initialized.
        /// </param>
        /// <param name="transactionKey">
        ///     <see cref="BaseTransaction"/> key.
        /// </param>
        /// <returns>
        ///     <see cref="DetailedTransactionItemView"/> object that is initialized.
        /// </returns>
        DetailedTransactionItemView InitItemView(DetailedTransactionItemView itemView, string transactionKey)
        {
            itemView.Init(transactionKey, m_ItemIconSpritePropertyKey,
                m_PriceIconSpritePropertyKey, m_NoPriceString, m_PayoutItemIconPropertyKey,
                m_BadgeTextPropertyKey, m_CurrencyPayoutCountPrefix, m_ItemPayoutCountPrefix);

            if (Application.isPlaying)
            {
                itemView.onPurchasableStatusChanged.AddListener(OnPurchasableStatusChanged);
            }
            
            return itemView;
        }

        /// <summary>
        ///     Initialize <see cref="TransactionItemView"/> component.
        /// </summary>
        /// <param name="itemView">
        ///     <see cref="TransactionItemView"/> that you want to be initialized.
        /// </param>
        /// <param name="transactionKey">
        ///     <see cref="BaseTransaction"/> key.
        /// </param>
        /// <returns>
        ///     <see cref="TransactionItemView"/> object that is initialized.
        /// </returns>
        TransactionItemView InitItemView(TransactionItemView itemView, string transactionKey)
        {
            itemView.Init(transactionKey, m_ItemIconSpritePropertyKey, m_BadgeTextPropertyKey,
                m_PriceIconSpritePropertyKey, m_NoPriceString);

            if (Application.isPlaying)
            {
                itemView.onPurchasableStatusChanged.AddListener(OnPurchasableStatusChanged);    
            }
            
            return itemView;
        }

        /// <summary>
        ///     Updates whether vertical scrolling should be enabled.
        /// </summary>
        /// <returns>
        ///     IEnumerator to wait for end of frame.
        /// </returns>
        /// <remarks>
        ///     Doesn't support runtime screen orientation changes.
        /// </remarks>
        IEnumerator UpdateScrollbarStatus()
        {
            yield return new WaitForEndOfFrame();

            if (!(m_ScrollRect is null))
            {
                var scrollTransform = m_ScrollRect.GetComponent<RectTransform>();
                var containerTransform = itemParentTransform.GetComponent<RectTransform>();

                if (!(scrollTransform is null) && !(containerTransform is null))
                {
                    var scrollRect = scrollTransform.rect;
                    var containerRect = containerTransform.rect;

                    m_ScrollRect.vertical = scrollRect.height < containerRect.height;
                    m_ScrollRect.horizontal = scrollRect.width < containerRect.width;
                }
            }
        }

        /// <summary>
        ///     Sets the anchored position of the <see cref="m_ScrollRect"/>'s content to 0.
        /// </summary>
        void ResetScrollContainerPosition()
        {
            if (!(m_ScrollRect is null) && !(m_ScrollRect.content is null))
            {
                var scrollContentTransform = m_ScrollRect.content.GetComponent<RectTransform>();
                scrollContentTransform.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        ///     Updates the button's interactable state to the state specified in <see cref="m_Interactable"/>.
        /// </summary>
        void UpdateInteractable()
        {
            if (!(m_ScrollRect is null))
            {
                m_ScrollRect.enabled = m_Interactable;
            }

            if (!(m_RevealHiddenItemsButton is null))
            {
                m_RevealHiddenItemsButton.interactable = m_Interactable;
            }

            SetInteractableTransactionItems(m_Interactable);
        }

        /// <summary>
        ///     Remove store items from the StoreView GameObject.
        /// </summary>
        /// <param name="startIndex">
        ///     The index of the store item as child transform in <see cref="itemContainer"/>.
        /// </param>
        /// <param name="count">
        ///     The count of child transform in <see cref="itemContainer"/> to remove.
        /// </param>
        /// <returns></returns>
        bool RemoveItems(int startIndex, int count = -1)
        {
            if (itemParentTransform is null)
            {
                return false;
            }

            var childCount = itemParentTransform.childCount;
            if (startIndex < 0 || startIndex >= childCount - 1)
            {
                return false;
            }

            int to = count < 0 ? childCount : Math.Min(startIndex + count, childCount);
            int currentIndex = startIndex;

            bool removed = false;

            for (int i = startIndex; i < to; i++)
            {
                var item = itemParentTransform.GetChild(currentIndex);
                if (item.TryGetComponent<TransactionItemView>(out var transactionItemView) && transactionItemView.gameObject.activeSelf)
                {
                    if (Application.isPlaying)
                    {
                        transactionItemView.onPurchasableStatusChanged.RemoveListener(OnPurchasableStatusChanged);

                        // Calling DestroyImmediate is not recommended here at runtime.
                        // Therefore we'll use the activeSelf flag as a workaround to indicate that
                        // this GameObject should not be reused because it's
                        // going to be destroyed at the end of the frame.
                        
                        transactionItemView.gameObject.SetActive(false);
                        Destroy(item.gameObject);
                        currentIndex++;
                    }
                    else
                    {
                        DestroyImmediate(item.gameObject);    
                    }

                    removed = true;
                }
                else
                {
                    currentIndex++;
                }
            }

            return removed;
        }

        bool HasStoreContain(BaseTransaction transaction)
        {
            if (!(m_Store is null))
            {
                return m_Store.Contains(transaction);
            }

            return false;
        }
        
        bool IsIAPTransactionPurchasable(string iapTransactionKey)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_Store is null) return false;

            var storeItems = new List<BaseTransaction>();
            m_Store.GetStoreItems(storeItems);
            foreach (var storeItem in storeItems)
            {
                if (storeItem.key == iapTransactionKey)
                {
                    var iapTransaction = storeItem as IAPTransaction;
                    return !(iapTransaction is null) && IsIAPTransactionPurchasable(iapTransaction);
                }
            }
#endif
            return false;

        }

        bool IsIAPTransactionPurchasable(IAPTransaction iapTransaction)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (iapTransaction.product == null || iapTransaction.product.definition == null) 
            {
                k_GFLogger.LogWarning($"Store \"{m_StoreKey}\" has an IAP Transaction with a Product Id that can't be found by the Unity IAP system. Please verify that the IAP Transaction is set up correctly in both Game Foundation and the IAP Catalog. Product Id: {iapTransaction.productId}");
                return false;
            }

            return iapTransaction.product.definition.type != ProductType.NonConsumable ||
                   iapTransaction.product.definition.type == ProductType.NonConsumable &&
                   !GameFoundationSdk.transactions.IsIapProductOwned(iapTransaction.productId);
#else
            return false;
#endif
        }

        /// <summary>
        ///     Gets triggered when any item in the store is successfully purchased. Triggers the
        ///     user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            if (!HasStoreContain(transaction))
                return;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (transaction is IAPTransaction iapTransaction && iapTransaction.product.definition.type == ProductType.NonConsumable)
            {
                UpdateContent();
            }
#endif

            onTransactionSucceeded?.Invoke(transaction);
        }

        /// <summary>
        ///     Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        ///     user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (!HasStoreContain(transaction))
                return;

            onTransactionFailed?.Invoke(transaction, exception);
        }

        /// <summary>
        ///     Listener for any of the store's transactions' <see cref="TransactionItemView.onPurchasableStatusChanged"/>
        ///     callbacks. Invokes this class' <see cref="onPurchasableStatusChanged"/> callback.
        /// </summary>
        /// <param name="purchaseButton"> The <see cref="PurchaseButton"/> whose status has changed.</param>
        /// <param name="oldStatus"> The previous purchasable status of the transaction.</param>
        /// <param name="newStatus"> The current status of the transaction.</param>
        void OnPurchasableStatusChanged(PurchaseButton purchaseButton, PurchasableStatus oldStatus, PurchasableStatus newStatus)
        {
            onPurchasableStatusChanged?.Invoke(purchaseButton, oldStatus, newStatus);
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
