using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying the Transaction Items contained within a given store.
    ///     When attached to a game object, it will create a TransactionItemView (<see cref="TransactionItemView"/>) for each
    ///     store item in the designated
    ///     list, with the given game object as their parent.
    /// </summary>
    [AddComponentMenu("Game Foundation/Store View", 1)]
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
        ///     The identifier of the tag items in the specified store should be filtered to for display.
        /// </summary>
        public string tagKey => m_TagKey;

        /// <inheritdoc cref="tagKey"/>
        [SerializeField]
        internal string m_TagKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the item icon sprite of the Transaction Item
        ///     for displaying in the view.
        /// </summary>
        public string itemIconSpritePropertyKey => m_ItemIconSpritePropertyKey;

        [SerializeField]
        internal string m_ItemIconSpritePropertyKey = "item_icon";

        /// <summary>
        ///     The Static Property key string that should be used for getting the price icon sprite of the Transaction Item
        ///     for displaying in the <see cref="PurchaseButton"/>.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey = "purchase_button_icon";

        /// <summary>
        ///     The string to display on Purchase Button if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;

        [SerializeField]
        internal string m_NoPriceString = PurchaseButton.kDefaultNoPriceString;

        /// <summary>
        ///     The <see cref="Store"/> to display in the view.
        /// </summary>
        public Store store => m_Store;

        Store m_Store;

        /// <summary>
        ///     The <see cref="Tag"/> in the specified store should be filtered to for display.
        /// </summary>
        public new Tag tag => m_Tag;

        Tag m_Tag;

        /// <summary>
        ///     The prefab with <see cref="TransactionItemView"/> component attached to use for creating the list of
        ///     TransactionItemView items.
        /// </summary>
        public TransactionItemView transactionItemPrefab => m_TransactionItemPrefab;

        [SerializeField]
        internal TransactionItemView m_TransactionItemPrefab;

        /// <summary>
        ///     The Transform in which to generate the list of <see cref="TransactionItemView"/> items.
        /// </summary>
        public Transform itemContainer => m_ItemContainer;

        [SerializeField]
        internal Transform m_ItemContainer;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        [Space]
        public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store fails.
        /// </summary>
        public TransactionFailureEvent onTransactionFailed;

        /// <summary>
        ///     A callback for when a transaction is completed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/> as a
        ///     parameter.
        /// </summary>
        [Serializable]
        public class TransactionSuccessEvent : UnityEvent<BaseTransaction> { }

        /// <summary>
        ///     A callback for when a transaction is failed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/> and
        ///     Exception as a parameter.
        /// </summary>
        [Serializable]
        public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> { }

        /// <summary>
        ///     The list of <see cref="TransactionItemView"/> items that are instantiated using Transaction Item prefab based on
        ///     the specified store and tag.
        /// </summary>
        readonly List<TransactionItemView> m_TransactionItems = new List<TransactionItemView>();

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
        ///     Specifies whether the Auto Populated Transaction Item fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showTransactionItemEditorFields = true;

        /// <summary>
        ///     To see if the component is being rendered in the scene.
        /// </summary>
        bool m_IsRunning;

        /// <summary>
        ///     The Game Object with ScrollRect component where scrollable content resides.
        /// </summary>
        ScrollRect m_ScrollRect;

        /// <summary>
        ///     The final Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        Transform itemParentTransform => m_ItemContainer ? m_ItemContainer : transform;

        /// <summary>
        ///     Specifies whether the button is interactable internally.
        /// </summary>
        bool m_InteractableInternal = true;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<StoreView>();

        /// <summary>
        ///     Adds listeners, if the application is playing.
        /// </summary>
        void OnEnable()
        {
            GameFoundationSdk.initialized += RegisterEvents;
            GameFoundationSdk.uninitialized += UnregisterEvents;

            if (GameFoundationSdk.IsInitialized)
            {
                RegisterEvents();
            }

            if (!(m_Store is null))
            {
                UpdateContent();
            }
        }

        /// <summary>
        ///     Removes listeners, if the application is playing.
        /// </summary>
        void OnDisable()
        {
            GameFoundationSdk.initialized -= RegisterEvents;
            GameFoundationSdk.uninitialized -= UnregisterEvents;

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
            GameFoundationSdk.transactions.transactionInitiated += OnTransactionInitiated;
            GameFoundationSdk.transactions.transactionSucceeded += OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed += OnTransactionFailed;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            GameFoundationSdk.transactions.transactionInitiated -= OnTransactionInitiated;
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

            UpdateContent();
        }

        /// <summary>
        ///     Initializes the StoreView before the first frame update.
        /// </summary>
        void Start()
        {
            ThrowIfNotInitialized(nameof(Start));

            m_Store = GetStore(m_StoreKey);
            m_Tag = GetTag(m_TagKey);

            m_IsRunning = true;
            m_ScrollRect = gameObject.GetComponentInChildren<ScrollRect>(false);

            UpdateContent();
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
        ///     If the <paramref name="storeKey"/> param is null or empty, or is not found in the store catalog no action will be
        ///     taken.
        /// </remarks>
        /// <remarks>
        ///     If the <paramref name="tagKey"/> param is null or empty, all transactions in a store will be displayed.
        /// </remarks>
        internal void SetStore(string storeKey, string tagKey = null)
        {
            m_Store = GetStore(storeKey);
            m_StoreKey = storeKey;

            m_Tag = GetTag(tagKey);
            m_TagKey = tagKey;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
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
            ThrowIfNotInitialized(nameof(SetStore));

            m_Store = store;
            m_StoreKey = store?.key;
            m_Tag = tag;
            m_TagKey = tag?.key;

            if (Application.isPlaying)
            {
                if (m_Store == null)
                {
                    k_GFLogger.LogError($"Requested store \"{storeKey}\" doesn't exist in Store Catalog.");
                    return;
                }

                UpdateContent();
            }
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
            if (storeDefinition != null || !m_ShowDebugLogs) return storeDefinition;

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
        ///     Updates which tag of items within the store should be displayed by this view.
        /// </summary>
        /// <param name="tagKey">
        ///     The key for the tag of Transactions that should be displayed.
        ///     To show all Transactions in a store (no tag filtering) null can be passed as the tag.
        /// </param>
        internal void SetTagKey(string tagKey)
        {
            m_Tag = GetTag(tagKey);
            m_TagKey = tagKey;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
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
            ThrowIfNotInitialized(nameof(SetTag));

            m_Tag = tag;
            m_TagKey = tag?.key;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
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

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }

        /// <summary>
        ///     Sets the Static Property key for price icon that will be displayed on <see cref="PurchaseButton"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The the Static Property key that is defined on Transaction Item for price icon sprite.
        /// </param>
        public void SetPriceIconSpritePropertyKey(string propertyKey)
        {
            if (m_PriceIconSpritePropertyKey == propertyKey)
            {
                return;
            }

            m_PriceIconSpritePropertyKey = propertyKey;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
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
            UpdateContent();
        }

        /// <summary>
        ///     Sets the <see cref="PurchaseButton"/>'s interactable state if the state specified is different from the current
        ///     state.
        /// </summary>
        /// <param name="interactable">
        ///     Whether the button should be enabled or not.
        /// </param>
        public void SetInteractable(bool interactable)
        {
            if (m_Interactable == interactable)
            {
                return;
            }

            m_Interactable = interactable;

            if (m_ScrollRect != null)
            {
                m_ScrollRect.enabled = interactable & m_InteractableInternal;
            }

            foreach (var itemView in m_TransactionItems)
            {
                itemView.interactable = interactable;
            }
        }

        void SetInteractableInternal(bool active)
        {
            if (m_InteractableInternal == active)
            {
                return;
            }

            m_InteractableInternal = active;

            if (m_ScrollRect != null)
            {
                m_ScrollRect.enabled = active && m_Interactable;
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
        ///     Generates and instantiates the list of TransactionItemViews for display in the StoreView.
        /// </summary>
        void UpdateContent()
        {
            if (!m_IsRunning) return;

            RemoveAllItems();

            if (m_Store is null || m_TransactionItemPrefab is null)
            {
                return;
            }

            var transactions = new List<BaseTransaction>();
            if (m_Tag is null)
            {
                m_Store.GetStoreItems(transactions);
            }
            else
            {
                m_Store.FindStoreItems(m_Tag, transactions);
            }

            if (transactions.Count == 0) return;

            foreach (var transaction in transactions)
            {
                if (transaction is VirtualTransaction || transaction is IAPTransaction iapTransaction && IsIAPTransactionPurchasable(iapTransaction))
                {
                    var item = Instantiate(m_TransactionItemPrefab, itemParentTransform, true)
                        .GetComponent<TransactionItemView>();
                    item.transform.localScale = Vector3.one;
                    item.Init(transaction.key, m_ItemIconSpritePropertyKey, m_PriceIconSpritePropertyKey, m_NoPriceString);
                    if (!m_Interactable)
                    {
                        item.interactable = false;
                    }

                    m_TransactionItems.Add(item);
                }
            }

            StartCoroutine(UpdateScrollbarStatus());
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
        ///     Resets the StoreView by removing the listeners and destroying the game object for all items in the view.
        /// </summary>
        void RemoveAllItems()
        {
            if (m_TransactionItems.Count == 0)
            {
                return;
            }

            foreach (var item in m_TransactionItems)
            {
                Destroy(item.gameObject);
            }

            m_TransactionItems.Clear();
        }

        bool HasStoreContain(BaseTransaction transaction)
        {
            if (m_Store != null)
            {
                return m_Store.Contains(transaction);
            }

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
        ///     Throws an Invalid Operation Exception if GameFoundation has not been initialized before this view is used.
        /// </summary>
        /// <param name="callingMethod">
        ///     Calling method name.
        /// </param>
        void ThrowIfNotInitialized(string callingMethod)
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                var message = $"GameFoundationSdk.Initialize() must be called before {callingMethod} is used.";
                k_GFLogger.LogException(message, new InvalidOperationException(message));
            }
        }

        /// <summary>
        ///     Gets triggered when a Transaction Item is initiated. Triggers the
        ///     user-specified onTransactionInitiated callback.
        /// </summary>
        void OnTransactionInitiated(BaseTransaction transaction)
        {
            if (!HasStoreContain(transaction))
                return;

            SetInteractableInternal(false);
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

            SetInteractableInternal(true);

            onTransactionSucceeded?.Invoke(transaction);
        }

        /// <summary>
        ///     Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        ///     suser-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (!HasStoreContain(transaction))
                return;

            SetInteractableInternal(true);

            onTransactionFailed?.Invoke(transaction, exception);
        }
    }
}
