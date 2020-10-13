using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;

#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component for completing a purchase using the <see cref="ITransactionManager"/>.
    /// </summary>
    [AddComponentMenu("Game Foundation/Purchase Button", 3)]
    [RequireComponent(typeof(Button))]
    [ExecuteInEditMode]
    public class PurchaseButton : MonoBehaviour
    {
        /// <summary>
        ///     The identifier of the Transaction Item being purchased.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        /// <inheritdoc cref="transactionKey"/>
        [SerializeField]
        internal string m_TransactionKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the price icon sprite of the Transaction
        ///     Item for displaying in the this view.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey = "purchase_button_icon";

        /// <summary>
        ///     Use to enable or disable the button.
        /// </summary>
        public bool interactable
        {
            get => m_Interactable;
            set => SetInteractable(value);
        }

        [SerializeField]
        internal bool m_Interactable = true;

        /// <summary>
        ///     The Text component to assign the price text to.
        /// </summary>
        public Text priceTextField => m_PriceTextField;

        [SerializeField]
        internal Text m_PriceTextField;

        /// <summary>
        ///     The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image priceIconImageField => m_PriceIconImageField;

        [SerializeField]
        internal Image m_PriceIconImageField;

        /// <summary>
        ///     The string to display if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;

        [SerializeField]
        internal string m_NoPriceString = kDefaultNoPriceString;

        /// <summary>
        ///     Callback that will get triggered if item purchase completes successfully.
        /// </summary>
        public TransactionSuccessEvent onPurchaseSuccess;

        /// <summary>
        ///     Callback that will get triggered if item purchase fails.
        /// </summary>
        public TransactionFailureEvent onPurchaseFailure;

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
        ///     The <see cref="BaseTransaction"/> being purchased.
        /// </summary>
        public BaseTransaction transaction => m_Transaction;

        BaseTransaction m_Transaction;

        /// <summary>
        ///     The Button component attached to this PurchaseButton.
        /// </summary>
        Button m_Button;

        /// <summary>
        ///     Specifies whether the item is available to purchase.
        /// </summary>
        public bool availableToPurchaseState => m_AvailableToPurchaseState;

        bool m_AvailableToPurchaseState = true;

        /// <summary>
        ///     Specifies whether the button is interactable internally.
        /// </summary>
        bool m_InteractableInternal = true;

        /// <summary>
        ///     Specifies whether the button is driven by other component.
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        ///     Specifies whether purchasing adapter will be initialized.
        /// </summary>
        bool m_WillPurchasingAdapterInitialized;
#endif

        /// <summary>
        ///     List of item exchange definition objects for transaction.
        /// </summary>
        List<ExchangeDefinitionObject> m_ExchangeObjectsList = new List<ExchangeDefinitionObject>();

        /// <summary>
        ///     List of item exchange definitions for transaction.
        /// </summary>
        List<ExchangeDefinition> m_ItemsList = new List<ExchangeDefinition>();

        /// <summary>
        ///     Default string to display when there is no cost defined in the Transaction Item.
        /// </summary>
        internal static readonly string kDefaultNoPriceString = "FREE";

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

            if (!ReferenceEquals(m_Transaction, null) && !m_IsDrivenByOtherComponent)
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
            GameFoundationSdk.inventory.itemAdded += OnInventoryChanged;
            GameFoundationSdk.inventory.itemDeleted += OnInventoryChanged;
            GameFoundationSdk.wallet.balanceChanged += OnWalletChanged;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_WillPurchasingAdapterInitialized)
            {
                if (GameFoundationSdk.transactions.purchasingAdapterIsInitialized)
                {
                    OnPurchasingAdapterInitializeSucceeded();
                }
                else
                {
                    GameFoundationSdk.transactions.purchasingAdapterInitializeSucceeded += OnPurchasingAdapterInitializeSucceeded;
                    GameFoundationSdk.transactions.purchasingAdapterInitializeFailed += OnPurchasingAdapterInitializeFailed;
                }
            }
#endif
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            GameFoundationSdk.inventory.itemAdded -= OnInventoryChanged;
            GameFoundationSdk.inventory.itemDeleted -= OnInventoryChanged;
            GameFoundationSdk.wallet.balanceChanged -= OnWalletChanged;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_WillPurchasingAdapterInitialized)
            {
                GameFoundationSdk.transactions.purchasingAdapterInitializeSucceeded -= OnPurchasingAdapterInitializeSucceeded;
                GameFoundationSdk.transactions.purchasingAdapterInitializeFailed -= OnPurchasingAdapterInitializeFailed;
            }
#endif
        }

        /// <summary>
        ///     Gets the Button component of the PurchaseButton and sets the onClick listener to call <see cref="Purchase"/>.
        /// </summary>
        void Awake()
        {
            m_Button = GetComponent<Button>();

            if (Application.isPlaying)
            {
                m_Button.onClick.AddListener(Purchase);
            }
        }

        /// <summary>
        ///     Initializes PurchaseButton with needed info.
        /// </summary>
        /// <param name="transactionKey">
        ///     The <see cref="BaseTransaction"/> identifier to be displayed.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The sprite name for price icon that will be displayed on the button.
        /// </param>
        internal void Init(string transactionKey, string priceIconSpritePropertyKey)
        {
            Init(transactionKey, priceIconSpritePropertyKey, m_NoPriceString);
        }

        /// <summary>
        ///     Initializes PurchaseButton with needed info.
        /// </summary>
        /// <param name="transactionKey">
        ///     The <see cref="BaseTransaction"/> identifier to be displayed.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The sprite name for price icon that will be displayed on the button.
        /// </param>
        /// <param name="noPriceString">
        ///     The string to display when there is no cost defined in the Transaction Item.
        /// </param>
        internal void Init(string transactionKey, string priceIconSpritePropertyKey, string noPriceString)
        {
            m_IsDrivenByOtherComponent = true;

            m_PriceIconSpritePropertyKey = priceIconSpritePropertyKey;
            m_NoPriceString = noPriceString;

            SetTransaction(transactionKey);
        }

        /// <summary>
        ///     Initializes PurchaseButton before the first frame update.
        ///     If the it's already initialized by TransactionItemView no action will be taken.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ThrowIfNotInitialized();

            if (!m_IsDrivenByOtherComponent)
            {
                SetTransaction(m_TransactionKey);
            }

            UpdateButtonStatus();
        }

        /// <summary>
        ///     Sets Transaction Item should be displayed by this view.
        /// </summary>
        /// <param name="definitionKey">
        ///     The transaction identifier that should be displayed.
        /// </param>
        internal void SetTransaction(string definitionKey)
        {
            m_Transaction = GetTransaction(definitionKey);
            m_TransactionKey = definitionKey;

            UpdateContent();
        }

        /// <summary>
        ///     Sets Transaction Item should be displayed by this view.
        /// </summary>
        /// <param name="transaction">
        ///     A reference to <see cref="BaseTransaction"/> that should be displayed.
        /// </param>
        public void SetTransaction(BaseTransaction transaction)
        {
            ThrowIfNotInitialized();

            m_Transaction = transaction;
            m_TransactionKey = transaction?.key;

            UpdateContent();
        }

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
            if (transactionItem != null || !m_ShowDebugLogs) return transactionItem;

            Debug.LogWarning($"{nameof(TransactionItemView)} - TransactionItem \"{definitionKey}\" doesn't exist in Transaction catalog.");
            return null;
        }

        /// <summary>
        ///     Calls <see cref="ITransactionManager.BeginTransaction(BaseTransaction, List{string})"/> from
        ///     <see cref="GameFoundationSdk.transactions"/> with the purchase detail of the
        ///     Transaction Item displayed in the button.
        ///     Is automatically attached to the onClick event of the PurchaseButton.
        /// </summary>
        public void Purchase()
        {
            if (ReferenceEquals(m_Transaction, null))
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item is not defined.");
                return;
            }

            SetInteractableInternal(false);

            StartCoroutine(ExecuteTransaction(m_Transaction));
        }

        /// <summary>
        ///     Execute the transaction with Coroutine since transaction uses deferred objects.
        /// </summary>
        /// <param name="transaction">
        ///     The Transaction being purchased.
        /// </param>
        IEnumerator ExecuteTransaction(BaseTransaction transaction)
        {
            Deferred<TransactionResult> deferred = GameFoundationSdk.transactions.BeginTransaction(transaction);

            if (m_ShowDebugLogs)
            {
                Debug.Log($"{nameof(PurchaseButton)} - Now processing purchase: {transaction.displayName}");
            }

            // Wait for the transaction to be processed
            int currentStep = 0;

            while (!deferred.isDone)
            {
                // Keep track of the current step and possibly show a progress UI
                if (deferred.currentStep != currentStep)
                {
                    currentStep = deferred.currentStep;

                    if (m_ShowDebugLogs)
                    {
                        Debug.Log($"{nameof(PurchaseButton)} - Transaction is now on step {currentStep} of {deferred.totalSteps}");
                    }
                }

                yield return null;
            }

            // We re-enable the button before the break even if there is an error
            SetInteractableInternal(true);

            // Now that the transaction has been processed, check for an error
            if (!deferred.isFulfilled)
            {
                if (m_ShowDebugLogs)
                {
                    Debug.LogError($"{nameof(PurchaseButton)} - Transaction Key: \"{transaction.key}\" - Error Message: {deferred.error}");
                }

                onPurchaseFailure?.Invoke(transaction, deferred.error);

                deferred.Release();
                yield break;
            }

            // Here we can assume success
            if (m_ShowDebugLogs)
            {
                Debug.Log("The purchase was successful in both the platform store and the data layer!");

                foreach (var tradable in deferred.result.payout.products)
                {
                    if (tradable is CurrencyExchange currencyExchange)
                    {
                        Debug.Log($"Player was awarded {currencyExchange.amount} of currency '{currencyExchange.currency.displayName}'");
                    }
                    else if (tradable is InventoryItem inventoryItem)
                    {
                        Debug.Log($"Player was awarded 1 of Inventory Item '{inventoryItem.definition.displayName}'");
                    }
                }
            }

            onPurchaseSuccess?.Invoke(transaction);

            // All done
            deferred.Release();
        }

        /// <summary>
        ///     Sets the button's interactable state according to Transaction Item's affordability status.
        /// </summary>
        /// <param name="state">
        ///     Whether the button should be enabled or not.
        /// </param>
        void SetAvailableToPurchaseState(bool state)
        {
            if (m_AvailableToPurchaseState != state)
            {
                m_AvailableToPurchaseState = state;
                UpdateButtonStatus();
            }
        }

        /// <summary>
        ///     Updates the button's interactable state according to Transaction Item's affordability status.
        /// </summary>
        void UpdateAvailableToPurchaseState()
        {
            if (m_Transaction == null)
            {
                SetAvailableToPurchaseState(false);
            }
            else if (m_Transaction is VirtualTransaction vTransaction)
            {
                SetAvailableToPurchaseState(IsAffordable(vTransaction));
            }
            else if (m_Transaction is IAPTransaction iapTransaction)
            {
                if (string.IsNullOrEmpty(iapTransaction.productId) || !GameFoundationSdk.transactions.purchasingAdapterIsInitialized)
                {
                    SetAvailableToPurchaseState(false);
                }
                else
                {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                    bool available = iapTransaction.product.definition.type != ProductType.NonConsumable ||
                        iapTransaction.product.definition.type == ProductType.NonConsumable &&
                        !GameFoundationSdk.transactions.IsIapProductOwned(iapTransaction.productId);

                    SetAvailableToPurchaseState(available);
#else
                    SetAvailableToPurchaseState(false);
#endif
                }
            }
        }

        /// <summary>
        ///     Sets the button's interactable state if the state specified is different from the current state.
        /// </summary>
        /// <param name="interactable">
        ///     Whether the button should be enabled or not.
        /// </param>
        public void SetInteractable(bool interactable)
        {
            if (m_Interactable != interactable)
            {
                m_Interactable = interactable;
                UpdateButtonStatus();
            }
        }

        void SetInteractableInternal(bool active)
        {
            if (m_InteractableInternal != active)
            {
                m_InteractableInternal = active;
                UpdateButtonStatus();
            }
        }

        /// <summary>
        ///     Updates button status according to user defined setting and internal status like affordability
        ///     of the Transaction Item
        /// </summary>
        void UpdateButtonStatus()
        {
            m_Button.interactable = m_AvailableToPurchaseState && m_Interactable && m_InteractableInternal;
        }

        /// <summary>
        ///     Sets the Text component to display price text on the button.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the texts on buttons
        /// </param>
        public void SetPriceTextField(Text text)
        {
            if (m_PriceTextField == text)
            {
                return;
            }

            m_PriceTextField = text;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Image component to display price icon sprite on the button.
        /// </summary>
        /// <param name="image">
        ///     The Image component to display price icon sprite.
        /// </param>
        public void SetPriceIconImageField(Image image)
        {
            if (m_PriceIconImageField == image)
            {
                return;
            }

            m_PriceIconImageField = image;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the string to display when there is no cost defined in the Transaction Item.
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
        ///     Sets the Static Property key for price icon that will be displayed on this view.
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
            UpdateContent();
        }

        /// <summary>
        ///     Sets price text and icon on the button.
        /// </summary>
        /// <param name="priceIcon">
        ///     Price icon sprite to display
        /// </param>
        /// <param name="priceText">
        ///     Price text to display
        /// </param>
        void SetContent(Sprite priceIcon, string priceText)
        {
            if (!ReferenceEquals(m_PriceIconImageField, null))
            {
                if (m_PriceIconImageField.sprite != priceIcon)
                {
                    m_PriceIconImageField.sprite = priceIcon;

                    if (!ReferenceEquals(priceIcon, null))
                    {
                        m_PriceIconImageField.gameObject.SetActive(true);
                        m_PriceIconImageField.SetNativeSize();
                    }
                    else
                    {
                        m_PriceIconImageField.gameObject.SetActive(false);
                    }

#if UNITY_EDITOR
                    EditorUtility.SetDirty(m_PriceIconImageField);
#endif
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(PurchaseButton)} - Icon Image Field is not defined.");
            }

            if (!ReferenceEquals(m_PriceTextField, null))
            {
                if (m_PriceTextField.text != priceText)
                {
                    m_PriceTextField.text = priceText;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(m_PriceTextField);
#endif
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(PurchaseButton)} - Price Text Field is not defined.");
            }
        }

        /// <summary>
        ///     Updates the price icon and the price amount field on the PurchaseButton.
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

        /// <summary>
        ///     To update the price icon image and the price amount field on the PurchaseButton at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionKey) || m_Transaction == null)
            {
                SetContent(null, null);

                SetAvailableToPurchaseState(false);
                return;
            }

            if (m_Transaction is VirtualTransaction vTransaction)
            {
                if (DoesHaveMultipleCost(vTransaction))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction item \"{vTransaction.displayName}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCost(vTransaction, 0, out var cost, out var costItem);

                if (cost > 0 && costItem != null)
                {
                    Sprite sprite = null;
                    if (costItem.TryGetStaticProperty(m_PriceIconSpritePropertyKey, out var spriteProperty))
                    {
                        sprite = spriteProperty.AsAsset<Sprite>();
                    }

                    if (ReferenceEquals(sprite, null))
                    {
                        Debug.LogWarning($"{nameof(PurchaseButton)} - \"{costItem.displayName}\" doesn't have sprite called \"{m_PriceIconSpritePropertyKey}\"");
                    }

                    SetContent(sprite, cost.ToString());
                    SetAvailableToPurchaseState(IsAffordable(vTransaction));
                }

                // Item is free
                else
                {
                    SetContent(null, noPriceString);
                    SetAvailableToPurchaseState(true);
                }
            }
            else if (m_Transaction is IAPTransaction iapTransaction)
            {
                SetContent(null, null);

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (string.IsNullOrEmpty(iapTransaction.productId))
                {
                    Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item \"{iapTransaction.displayName}\" shouldn't have empty or null product id.");
                }
                else if (GameFoundationSdk.transactions.purchasingAdapterIsInitialized)
                {
                    SetIAPTransactionPrice(iapTransaction);
                }
                else
                {
                    m_WillPurchasingAdapterInitialized = true;

                    GameFoundationSdk.transactions.purchasingAdapterInitializeSucceeded += OnPurchasingAdapterInitializeSucceeded;
                    GameFoundationSdk.transactions.purchasingAdapterInitializeFailed += OnPurchasingAdapterInitializeFailed;
                }

                UpdateAvailableToPurchaseState();
#endif
            }
        }

        static void GetVirtualCost(VirtualTransaction transaction, int indexOfCost, out long amount, out CatalogItem costItem)
        {
            var costs = transaction?.costs;
            if (costs != null && costs.GetExchanges() > 0)
            {
                var cost = costs.GetExchange(indexOfCost);
                amount = cost.amount;

                costItem = cost.tradableDefinition;

                return;
            }

            amount = 0;
            costItem = null;
        }

        bool DoesHaveMultipleCost(VirtualTransaction transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return costs.GetExchanges(m_ItemsList) > 1;
            }

            return false;
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        ///     Sets price text on the button according to localized price
        /// </summary>
        /// <param name="iapTransaction">A reference to <see cref="BaseTransaction"/></param>
        void SetIAPTransactionPrice(IAPTransaction iapTransaction)
        {
            var productMetadata = GameFoundationSdk.transactions.GetLocalizedIAPProductInfo(iapTransaction.productId);
            if (!string.IsNullOrEmpty(productMetadata.price))
            {
                SetContent(null, productMetadata.price);
            }
            else if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item \"{iapTransaction.displayName}\" localized price is empty or null.");
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        ///     To update the price icon image and the price amount field on the PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // To avoid updating the content the prefab selected in the Project window
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            var transactionAsset = !string.IsNullOrEmpty(m_TransactionKey)
                ? CatalogSettings.catalogAsset.FindItem(m_TransactionKey) as BaseTransactionAsset
                : null;

            if (ReferenceEquals(transactionAsset, null))
            {
                SetContent(null, null);
                return;
            }

            if (transactionAsset is VirtualTransactionAsset vTransactionAsset)
            {
                if (DoesHaveMultipleCost(vTransactionAsset))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction item \"{vTransactionAsset.displayName}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCostAsset(vTransactionAsset, 0, out var cost, out var itemAsset);

                if (cost > 0 && !ReferenceEquals(itemAsset, null))
                {
                    Sprite sprite = null;
                    if (itemAsset.TryGetStaticProperty(m_PriceIconSpritePropertyKey, out var spriteProperty))
                    {
                        sprite = spriteProperty.AsAsset<Sprite>();
                    }

                    if (ReferenceEquals(sprite, null))
                    {
                        Debug.LogWarning($"{nameof(PurchaseButton)} - \"{itemAsset.displayName}\" transaction item doesn't have sprite called \"{m_PriceIconSpritePropertyKey}\"");
                    }

                    SetContent(sprite, cost.ToString());
                }
                else
                {
                    SetContent(null, noPriceString);
                }
            }
            else if (transactionAsset is IAPTransactionAsset iapTransactionAsset)
            {
                if (string.IsNullOrEmpty(iapTransactionAsset.productId))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction Item \"{iapTransactionAsset.displayName}\" shouldn't have empty or null product id.");
                }

                SetContent(null, "N/A");
            }
        }

        void GetVirtualCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, out CatalogItemAsset costItemAsset)
        {
            var costs = transaction?.costs;
            if (costs?.GetItems(m_ExchangeObjectsList) > indexOfCost)
            {
                var cost = m_ExchangeObjectsList[indexOfCost];
                amount = cost.amount;
                costItemAsset = cost.catalogItem;
                return;
            }

            amount = 0;
            costItemAsset = null;
        }

        bool DoesHaveMultipleCost(VirtualTransactionAsset transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return costs.GetItems(m_ExchangeObjectsList) > 1;
            }

            return false;
        }
#endif

        /// <summary>
        ///     Checks the cost items and currencies to see whether there is enough quantity to complete the purchase.
        /// </summary>
        /// <returns>
        ///     True if there is enough of the items in inventory and/or wallet for the purchase,
        ///     false if there is not enough items.
        /// </returns>
        bool IsAffordable(VirtualTransaction transaction)
        {
            if (transaction == null)
            {
                return false;
            }

            ICollection<Exception> costExceptions = new List<Exception>();
            transaction.VerifyCost(costExceptions);
            transaction.VerifyPayout(costExceptions);

            return costExceptions.Count == 0;
        }

        /// <summary>
        ///     Throws an Invalid Operation Exception if GameFoundation has not been initialized before this view is used.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void ThrowIfNotInitialized()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                throw new InvalidOperationException($"Error: GameFoundation.Initialize() must be called before the {nameof(PurchaseButton)} is used.");
            }
        }

        /// <summary>
        ///     Listens to updates to the wallet
        ///     and updates the button enabled state based on the new information.
        /// </summary>
        void OnWalletChanged(IQuantifiable _, long __)
        {
            UpdateAvailableToPurchaseState();
        }

        /// <summary>
        ///     Listens to updates to the inventory
        ///     and updates the button enabled state based on the new information.
        /// </summary>
        void OnInventoryChanged(InventoryItem item)
        {
            UpdateAvailableToPurchaseState();
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        ///     Gets triggered when the Purchasing Adapter is initialized successfully,
        ///     and update the button price label and enabled state based on the information
        /// </summary>
        void OnPurchasingAdapterInitializeSucceeded()
        {
            if (m_Transaction != null && m_Transaction is IAPTransaction iapTransaction)
            {
                SetIAPTransactionPrice(iapTransaction);
            }

            m_WillPurchasingAdapterInitialized = false;
        }

        /// <summary>
        ///     Gets triggered when the Purchasing Adapter fails to be initialized.
        /// </summary>
        void OnPurchasingAdapterInitializeFailed(Exception exception)
        {
            if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Key: \"{m_TransactionKey}\" - Error Message: {exception.Message}");
            }

            m_WillPurchasingAdapterInitialized = false;
        }
#endif
    }
}
