using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        ///     The <see cref="PurchasableStatus"/> of the Transaction attached to this PurchaseButton.
        /// </summary>
        public PurchasableStatus itemPurchasableStatus => m_ItemPurchasableStatus;

        PurchasableStatus m_ItemPurchasableStatus = PurchasableStatus.GameFoundationUnavailable;

        /// <summary>
        ///     The Static Property key string that should be used for getting the price icon sprite of the Transaction
        ///     Item for displaying in the this view.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey;

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
        public TextMeshProUGUI priceTextField => m_PriceTextField;

        [SerializeField]
        internal TextMeshProUGUI m_PriceTextField;

        /// <summary>
        ///     The Image component to assign the Transaction Item's cost icon to.
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
        ///     Callback that will get triggered any time <see cref="itemPurchasableStatus"/> changes.
        /// </summary>
        public PurchasableStatusChangedEvent onPurchasableStatusChanged;

        /// <summary>
        ///     Callback that will get triggered if item purchase completes successfully.
        /// </summary>
        public TransactionSuccessEvent onPurchaseSuccess;

        /// <summary>
        ///     Callback that will get triggered if item purchase fails.
        /// </summary>
        public TransactionFailureEvent onPurchaseFailure;

        /// <summary>
        ///     A callback for when a transaction's <see cref="itemPurchasableStatus"/> changes. Wraps UnityEvent and
        ///     accepts three parameters: the <see cref="PurchaseButton"/> the status is changing on, the old
        ///     <see cref="PurchasableStatus"/> and the new <see cref="PurchasableStatus"/>.
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
        ///     Flag for determining whether there is currently a purchase in progress.
        /// </summary>
        bool m_PurchaseInProgress = false;

        /// <summary>
        ///     Specifies whether the button is driven by other component.
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;

        /// <summary>
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        bool m_IsDirty;

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
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<PurchaseButton>();

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
            if (GameFoundationSdk.inventory != null)
            {
                GameFoundationSdk.inventory.itemAdded += OnInventoryChanged;
                GameFoundationSdk.inventory.itemDeleted += OnInventoryChanged;
            }

            if (GameFoundationSdk.wallet != null)
            {
                GameFoundationSdk.wallet.balanceChanged += OnWalletChanged;
            }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_WillPurchasingAdapterInitialized)
            {
                if (GameFoundationSdk.transactions.purchasingAdapterIsInitialized)
                {
                    OnPurchasingAdapterInitializeSucceeded();
                }
                else if (GameFoundationSdk.transactions != null)
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
            if (GameFoundationSdk.inventory != null)
            {
                GameFoundationSdk.inventory.itemAdded -= OnInventoryChanged;
                GameFoundationSdk.inventory.itemDeleted -= OnInventoryChanged;
            }

            if (GameFoundationSdk.wallet != null)
            {
                GameFoundationSdk.wallet.balanceChanged -= OnWalletChanged;
            }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_WillPurchasingAdapterInitialized
                && GameFoundationSdk.transactions != null)
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

            if (Application.isPlaying && m_Button.onClick.GetPersistentEventCount() <= 0)
            {
                k_GFLogger.LogWarning("There are no onClick listeners attached to the PurchaseButton named " 
                                      + m_Button.name + " via the Inspector UI. This may cause unexpected behavior " +
                                      "when trying to purchase transaction.");
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

            m_TransactionKey = transactionKey;
            m_Transaction = GetTransaction(m_TransactionKey);
            UpdateInteractable();
            UpdatePurchasableStatus();
            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here causes a frame delay
            // when being driven by a parent component that makes this object look out of sync with its parent.
            UpdateContent();
        }

        /// <summary>
        ///     Initializes PurchaseButton before the first frame update if Game Foundation Sdk was already
        ///     initialized before PurchaseButton was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        ///     If the it's already initialized by TransactionItemView no action will be taken.
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
                m_IsDirty = true;
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
        ///     Initializes PurchaseButton data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            UpdatePurchasableStatus();
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets Transaction Item should be displayed by this view.
        /// </summary>
        /// <param name="definitionKey">
        ///     The transaction identifier that should be displayed.
        /// </param>
        internal void SetTransaction(string definitionKey)
        {
            m_TransactionKey = definitionKey;
            m_IsDirty = true;
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

            k_GFLogger.LogWarning($"TransactionItem \"{definitionKey}\" doesn't exist in Transaction catalog.");
            return null;
        }

        /// <summary>
        ///     Sets Transaction Item should be displayed by this view.
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
        ///     Calls <see cref="ITransactionManager.BeginTransaction(BaseTransaction, List{string})"/> from
        ///     <see cref="GameFoundationSdk.transactions"/> with the purchase detail of the
        ///     Transaction Item displayed in the button.
        ///     Is automatically attached to the onClick event of the PurchaseButton.
        /// </summary>
        public void Purchase()
        {
            if (m_ItemPurchasableStatus == PurchasableStatus.PurchaseButtonMisconfigured)
            {
                k_GFLogger.LogError("Purchase attempted, but the purchase button is misconfigured.");
                return;
            }

            if (m_ItemPurchasableStatus == PurchasableStatus.ItemPurchaseInProgress)
            {
                onPurchaseFailure?.Invoke(m_Transaction,
                    new InvalidOperationException("A purchase for this item is already in progress."));
                return;
            }

            m_PurchaseInProgress = true;
            UpdatePurchasableStatus();
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
            using (Deferred<TransactionResult> deferred = GameFoundationSdk.transactions.BeginTransaction(transaction))
            {
                if (m_ShowDebugLogs)
                {
                    k_GFLogger.Log($"Now processing purchase: {transaction.displayName}");
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
                            k_GFLogger.Log($"Transaction is now on step {currentStep} of {deferred.totalSteps}");
                        }
                    }

                    yield return null;
                }

                // Set purchaseInProgress to false and update purchasableStatus before the break even if there's an error
                m_PurchaseInProgress = false;
                UpdatePurchasableStatus();

                // Now that the transaction has been processed, check for an error
                if (!deferred.isFulfilled)
                {
                    if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogError($"Transaction Key: \"{transaction.key}\" - Error Message: {deferred.error}");
                    }

                    onPurchaseFailure?.Invoke(transaction, deferred.error);

                    yield break;
                }

                // Here we can assume success
                if (m_ShowDebugLogs)
                {
                    k_GFLogger.Log("The purchase was successful in both the platform store and the data layer!");

                    foreach (var tradable in deferred.result.payout.products)
                    {
                        if (tradable is CurrencyExchange currencyExchange)
                        {
                            k_GFLogger.Log($"Player was awarded {currencyExchange.amount} of currency " +
                                           $"'{currencyExchange.currency.displayName}'");
                        }
                        else if (tradable is InventoryItem inventoryItem)
                        {
                            k_GFLogger.Log($"Player was awarded 1 of Inventory Item " +
                                           $"'{inventoryItem.definition.displayName}'");
                        }
                    }
                }

                onPurchaseSuccess?.Invoke(transaction);
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
            if (m_Interactable == interactable)
                return;

            m_Interactable = interactable;
            UpdateInteractable();
        }

        /// <summary>
        ///     Sets the Text component to display price text on the button.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the texts on buttons
        /// </param>
        public void SetPriceTextField(TextMeshProUGUI text)
        {
            if (m_PriceTextField == text)
            {
                return;
            }

            m_PriceTextField = text;
            m_IsDirty = true;
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
            m_IsDirty = true;
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
            m_IsDirty = true;
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
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets price text and icon on the button.
        /// </summary>
        /// <param name="priceText">
        ///     Price text to display
        /// </param>
        void SetTextContent(string priceText)
        {
            if (!(m_PriceTextField is null))
            {
                if (m_PriceTextField.text != priceText)
                {
                    m_PriceTextField.text = priceText;
#if UNITY_EDITOR
                    // Setting dirty here for the case where the TransactionItemView is being driven from a parent
                    // component, instead of it's own inspector
                    EditorUtility.SetDirty(this);	
#endif
                }
            }
            else
            {
                k_GFLogger.LogWarning("Price Text Field is not defined.");
            }
        }

        /// <summary>
        ///     Sets price icon on the button.
        /// </summary>
        /// <param name="priceIcon">
        ///     Price icon sprite to display.
        /// </param>
        void SetIconSprite(Sprite priceIcon)
        {
            if (!(m_PriceIconImageField is null))
            {
                if (m_PriceIconImageField.sprite != priceIcon)
                {
                    m_PriceIconImageField.sprite = priceIcon;

                    if (!(priceIcon is null))
                    {
                        m_PriceIconImageField.gameObject.SetActive(true);
                        m_PriceIconImageField.preserveAspect = true;
                    }
                    else
                    {
                        m_PriceIconImageField.gameObject.SetActive(false);
                    }
#if UNITY_EDITOR
                    // Setting dirty here for the case where the TransactionItemView is being driven from a parent
                    // component, instead of it's own inspector
                    EditorUtility.SetDirty(this);	
#endif
                }
            }
            else
            {
                k_GFLogger.LogWarning("Icon Image Field is not defined.");
            }
        }

        /// <summary>
        ///     Checks to see whether any properties have been changed (by checking <see cref="m_IsDirty"/>) and
        ///     if so, calls <see cref="UpdateContent"/> before resetting the flag.
        ///
        ///     At runtime, also assigns the appropriate value for <see cref="m_Transaction"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;

                if (GameFoundationSdk.IsInitialized &&
                    (m_Transaction is null && !string.IsNullOrEmpty(m_TransactionKey) ||
                     !(m_Transaction is null) && m_Transaction.key != m_TransactionKey))
                {
                    m_Transaction = GetTransaction(m_TransactionKey);
                }

                UpdateInteractable();
                UpdateContent();
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
            if (!GameFoundationSdk.IsInitialized || string.IsNullOrEmpty(m_TransactionKey) || m_Transaction is null)
            {
                SetTextContent(null);
                SetIconSprite(null);
                UpdatePurchasableStatus();
                return;
            }

            if (m_Transaction is VirtualTransaction vTransaction)
            {
                if (DoesHaveMultipleCost(vTransaction))
                {
                    k_GFLogger.LogWarning($"Transaction item \"{vTransaction.displayName}\" has multiple item " +
                                          $"costs. {nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCost(vTransaction, 0, out var cost, out var costItem);

                if (cost > 0 && costItem != null)
                {
                    if (!string.IsNullOrEmpty(m_PriceIconSpritePropertyKey))
                    {
                        if (costItem.TryGetStaticProperty(m_PriceIconSpritePropertyKey, out var spriteProperty))
                        {
                            PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                        }
                        else
                        {
                            k_GFLogger.LogWarning($"\"{costItem.displayName}\" doesn't have static property " +
                                                  $"called \"{m_PriceIconSpritePropertyKey}\"");
                        }    
                    }
                    else
                    {
                        SetIconSprite(null);
                    }

                    SetTextContent(cost.ToString());
                }
                else
                {
                    // Item is free
                    SetTextContent(noPriceString);
                    SetIconSprite(null);
                }
            }
            else if (m_Transaction is IAPTransaction iapTransaction)
            {
                SetTextContent(null);
                SetIconSprite(null);

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (string.IsNullOrEmpty(iapTransaction.productId))
                {
                    k_GFLogger.LogError($"Transaction Item \"{iapTransaction.displayName}\" shouldn't have empty or null product id.");
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
#endif
            }

            UpdatePurchasableStatus();
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
                SetTextContent(productMetadata.price);
                SetIconSprite(null);
            }
            else if (m_ShowDebugLogs)
            {
                k_GFLogger.LogError($"Transaction Item \"{iapTransaction.displayName}\" localized price is empty or null.");
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        ///     To update the price icon image and the price amount field on the PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            var transactionAsset = !string.IsNullOrEmpty(m_TransactionKey)
                ? PrefabTools.GetLookUpCatalogAsset().FindItem(m_TransactionKey) as BaseTransactionAsset
                : null;

            if (transactionAsset is null)
            {
                SetTextContent(null);
                SetIconSprite(null);
                return;
            }

            if (transactionAsset is VirtualTransactionAsset vTransactionAsset)
            {
                if (DoesHaveMultipleCost(vTransactionAsset))
                {
                    k_GFLogger.LogWarning(
                        $"Transaction item \"{vTransactionAsset.displayName}\" has multiple item costs. " +
                        $"{nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCostAsset(vTransactionAsset, 0, out var cost, out var itemAsset);

                if (cost > 0 && !(itemAsset is null))
                {
                    if (!string.IsNullOrEmpty(m_PriceIconSpritePropertyKey))
                    {
                        if (itemAsset.TryGetStaticProperty(m_PriceIconSpritePropertyKey, out var spriteProperty))
                        {
                            PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                        }
                        else
                        {
                            k_GFLogger.LogWarning($"\"{itemAsset.displayName.currentValue}\" transaction item " +
                                                  $"doesn't have sprite called \"{m_PriceIconSpritePropertyKey}\"");
                        }
                    }
                    else
                    {
                        SetIconSprite(null);
                    }

                    SetTextContent(cost.ToString());
                }
                else
                {
                    SetTextContent(noPriceString);
                    SetIconSprite(null);
                }
            }
            else if (transactionAsset is IAPTransactionAsset iapTransactionAsset)
            {
                if (string.IsNullOrEmpty(iapTransactionAsset.productId))
                {
                    k_GFLogger.LogWarning($"Transaction Item \"{iapTransactionAsset.displayName}\" " +
                                          $"shouldn't have empty or null product id.");
                }

                SetTextContent("N/A");
                SetIconSprite(null);
            }
        }

        void GetVirtualCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, 
            out CatalogItemAsset costItemAsset)
        {
            if (transaction == null)
            {
                amount = 0;
                costItemAsset = null;
                return;
            }

            var costs = transaction.costs;
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
            if (transaction == null)
            {
                return false;
            }

            var costs = transaction.costs;
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
        bool IsAffordable(VirtualTransaction virtualTransaction)
        {
            if (virtualTransaction == null)
            {
                return false;
            }

            ICollection<Exception> costExceptions = new List<Exception>();
            virtualTransaction.VerifyCost(costExceptions);
            virtualTransaction.VerifyPayout(costExceptions);

            return costExceptions.Count == 0;
        }

        /// <summary>
        ///     Determines the current Purchasable Status of the transaction based on several possible
        ///     configurations at runtime.
        /// </summary>
        PurchasableStatus GetPurchasableStatus()
        {
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                return PurchasableStatus.PurchaseButtonMisconfigured;
            }

            if (!GameFoundationSdk.IsInitialized)
            {
                return PurchasableStatus.GameFoundationUnavailable;
            }

            if (m_Transaction is null)
            {
                return PurchasableStatus.PurchaseButtonMisconfigured;
            }

            if (m_PurchaseInProgress)
            {
                return PurchasableStatus.ItemPurchaseInProgress;
            }

            if (m_Transaction is VirtualTransaction virtualTransaction)
            {
                if (!IsAffordable(virtualTransaction))
                {
                    return PurchasableStatus.ItemUnaffordable;
                }
            }

            if (m_Transaction is IAPTransaction iapTransaction)
            {
                if (string.IsNullOrEmpty(iapTransaction.productId))
                {
                    return PurchasableStatus.TransactionMisconfigured;
                }

                if (!GameFoundationSdk.transactions.purchasingAdapterIsInitialized)
                {
                    return PurchasableStatus.PurchasingAdapterUnavailable;
                }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (iapTransaction.product.definition.type == ProductType.NonConsumable
                    && GameFoundationSdk.transactions.IsIapProductOwned(iapTransaction.productId))
                {
                    return PurchasableStatus.ItemOwned;
                }

                if (iapTransaction.product.definition.type != ProductType.Consumable
                         && iapTransaction.product.definition.type != ProductType.NonConsumable)
                {
                    return PurchasableStatus.TransactionMisconfigured;
                }
#else
                return PurchasableStatus.PurchasingAdapterUnavailable;
#endif
            }

            return PurchasableStatus.AvailableToPurchase;
        }

        /// <summary>
        ///     Determines the <see cref="PurchasableStatus"/> of the transaction based on several possible
        ///     configurations and whether or not the determination is being made during runtime or not.
        ///
        ///     If the newly determined status is different from the current status, it updates
        ///     <see cref="itemPurchasableStatus"/> to the new status and triggers the onPurchasableStatusChanged callback.
        /// </summary>
        void UpdatePurchasableStatus()
        {
            if (!Application.isPlaying)
                return;

            var newStatus = GetPurchasableStatus();

            if (m_ItemPurchasableStatus == newStatus)
                return;

            var oldStatus = m_ItemPurchasableStatus;
            m_ItemPurchasableStatus = newStatus;

            onPurchasableStatusChanged?.Invoke(this, oldStatus, m_ItemPurchasableStatus);
        }

        /// <summary>
        ///     Updates the button's interactable state to the state specified in <see cref="m_Interactable"/>.
        /// </summary>
        void UpdateInteractable()
        {
            if (!(m_Button is null))
            {
                m_Button.interactable = m_Interactable;
            }
        }
        
        /// <summary>
        ///     Callback for if there is an error while trying to load a sprite from its Property.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message returned by <see cref="PrefabTools.LoadSprite"/>.
        /// </param>
        void OnSpriteLoadFailed(string errorMessage)
        {
            k_GFLogger.LogWarning(errorMessage);
        }

        /// <summary>
        ///     Listens to updates to the wallet
        ///     and updates the transaction's purchasable status based on the new information.
        /// </summary>
        void OnWalletChanged(IQuantifiable _, long __)
        {
            UpdatePurchasableStatus();
        }

        /// <summary>
        ///     Listens to updates to the inventory
        ///     and updates the transaction's purchasable status based on the new information.
        /// </summary>
        void OnInventoryChanged(InventoryItem item)
        {
            UpdatePurchasableStatus();
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        ///     Gets triggered when the Purchasing Adapter is initialized successfully,
        ///     and update the button price label and purchasable status based on the information
        /// </summary>
        void OnPurchasingAdapterInitializeSucceeded()
        {
            if (m_Transaction != null && m_Transaction is IAPTransaction iapTransaction)
            {
                SetIAPTransactionPrice(iapTransaction);
            }

            UpdatePurchasableStatus();
            m_WillPurchasingAdapterInitialized = false;
        }

        /// <summary>
        ///     Gets triggered when the Purchasing Adapter fails to be initialized.
        /// </summary>
        void OnPurchasingAdapterInitializeFailed(Exception exception)
        {
            if (m_ShowDebugLogs)
            {
                k_GFLogger.LogError($"Transaction Key: \"{m_TransactionKey}\" - Error Message: {exception.Message}");
            }

            UpdatePurchasableStatus();
            m_WillPurchasingAdapterInitialized = false;
        }
#endif

        /// <summary>
        ///     When changes are made via the Inspector, trigger <see cref="UpdateContent"/>
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
