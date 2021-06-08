using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying a <see cref="BaseTransaction"/>'s icon, display name and
    ///     <see cref="PurchaseButton"/>.
    ///     When attached to a game object, it will display the Transaction Item's icon and displayName and create and display
    ///     a
    ///     PurchaseButton (<see cref="PurchaseButton"/>) to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Transaction Item View", 2)]
    [ExecuteInEditMode]
    public class TransactionItemView : MonoBehaviour
    {
        /// <summary>
        ///     The key of the <see cref="BaseTransaction"/> being displayed.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        [SerializeField] internal string m_TransactionKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the item icon sprite of the Transaction Item
        ///     for displaying in the view.
        /// </summary>
        public string itemIconSpritePropertyKey => m_ItemIconSpritePropertyKey;

        [SerializeField] internal string m_ItemIconSpritePropertyKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the price icon sprite of the Transaction Item
        ///     for displaying in the <see cref="PurchaseButton"/>.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField] internal string m_PriceIconSpritePropertyKey;

        /// <summary>
        ///     The Static Property key for the badge to be displayed in callout.
        /// </summary>
        public string badgeTextPropertyKey => m_BadgeTextPropertyKey;

        [SerializeField] internal string m_BadgeTextPropertyKey;

        /// <summary>
        ///     The string to display on <see cref="PurchaseButton"/> if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;

        [SerializeField] internal string m_NoPriceString = PurchaseButton.kDefaultNoPriceString;

        /// <summary>
        ///     Use to enable or disable interaction on the TransactionItemView UI.
        /// </summary>
        public bool interactable
        {
            get => m_Interactable;
            set => SetInteractable(value);
        }

        [SerializeField] internal bool m_Interactable = true;

        /// <summary>
        ///     The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image itemIconImageField => m_ItemIconImageField;

        [SerializeField] internal Image m_ItemIconImageField;

        /// <summary>
        ///     The TextMeshProUGUI component to assign the item's display name to.
        /// </summary>
        public TextMeshProUGUI itemNameTextField => m_ItemNameTextField;

        [SerializeField] internal TextMeshProUGUI m_ItemNameTextField;

        /// <summary>
        ///     The PurchaseButton to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;

        [SerializeField] internal PurchaseButton m_PurchaseButton;

        /// <summary>
        ///     The <see cref="ImageInfoView"/> to assign the badge to.
        /// </summary>
        public ImageInfoView badgeField => m_BadgeField;

        [SerializeField] internal ImageInfoView m_BadgeField;

        /// <summary>
        ///     Callback that will get triggered any time <see cref="PurchaseButton.itemPurchasableStatus"/> changes.
        /// </summary>
        public PurchasableStatusChangedEvent onPurchasableStatusChanged;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        [Space] public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        ///     Callback that will get triggered if a purchase for any item in the store fails.
        /// </summary>
        public TransactionFailureEvent onTransactionFailed;

        /// <summary>
        ///     A callback for when a transaction's <see cref="PurchaseButton.itemPurchasableStatus"/> changes. Wraps
        ///     UnityEvent and accepts three parameters: the <see cref="PurchaseButton"/> the status is changing on, the
        ///     old <see cref="PurchasableStatus"/> and the new <see cref="PurchasableStatus"/>.
        /// </summary>
        [Serializable]
        public class PurchasableStatusChangedEvent : UnityEvent<PurchaseButton, PurchasableStatus, PurchasableStatus>
        {
        }

        /// <summary>
        ///     A callback for when a transaction is completed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/> as a
        ///     parameter.
        /// </summary>
        [Serializable]
        public class TransactionSuccessEvent : UnityEvent<BaseTransaction>
        {
        }

        /// <summary>
        ///     A callback for when a transaction is failed. Wraps UnityEvent and accepts a <see cref="BaseTransaction"/> and
        ///     Exception as a parameter.
        /// </summary>
        [Serializable]
        public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception>
        {
        }

        /// <summary>
        ///     The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        public BaseTransaction transaction => m_Transaction;

        /// <summary>
        ///     The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        protected BaseTransaction m_Transaction;

        /// <summary>
        ///     Specifies whether this view is driven by other component
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;

        /// <summary>
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        protected bool m_IsDirty;

        /// <summary>
        ///     Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField] internal bool showButtonEditorFields = true;

        /// <summary>
        ///     Specifies whether the badge related fields in the custom editor are visible.
        /// </summary>
        [SerializeField] internal bool showBadgeEditorFields = true;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        protected bool m_ShowDebugLogs = false;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<TransactionItemView>();

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
            if (GameFoundationSdk.transactions == null)
                return;

            GameFoundationSdk.transactions.transactionSucceeded += OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed += OnTransactionFailed;

            if (m_PurchaseButton)
            {
                m_PurchaseButton.onPurchasableStatusChanged.AddListener(OnPurchasableStatusChanged);
            }
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

            if (m_PurchaseButton)
            {
                m_PurchaseButton.onPurchasableStatusChanged.RemoveListener(OnPurchasableStatusChanged);
            }
        }

        /// <summary>
        ///     Initializes TransactionItemView with needed info.
        /// </summary>
        /// <param name="transactionKey">
        ///     The transaction key to be displayed.
        /// </param>
        /// <param name="itemIconSpritePropertyKey">
        ///     The Static Property key for item icon that will be displayed on this view.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The Static Property key for price icon that will be displayed on PurchaseButton.
        /// </param>
        internal void Init(string transactionKey, string itemIconSpritePropertyKey, string badgeTextPropertyKey,
            string priceIconSpritePropertyKey)
        {
            Init(transactionKey, itemIconSpritePropertyKey, badgeTextPropertyKey, priceIconSpritePropertyKey,
                PurchaseButton.kDefaultNoPriceString);
        }

        /// <summary>
        ///     Initializes TransactionItemView with needed info.
        /// </summary>
        /// <param name="transactionKey">
        ///     The transaction key to be displayed.
        /// </param>
        /// <param name="itemIconSpritePropertyKey">
        ///     The Static Property key for item icon that will be displayed on this view.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The Static Property key for price icon that will be displayed on <see cref="PurchaseButton"/>.
        /// </param>
        /// <param name="noPriceString">
        ///     The string to display on <see cref="PurchaseButton"/> when there is no cost defined in the Transaction Item.
        /// </param>
        internal void Init(string transactionKey, string itemIconSpritePropertyKey, string badgeTextPropertyKey,
            string priceIconSpritePropertyKey, string noPriceString)
        {
            m_IsDrivenByOtherComponent = true;

            m_ItemIconSpritePropertyKey = itemIconSpritePropertyKey;
            m_BadgeTextPropertyKey = badgeTextPropertyKey;
            m_PriceIconSpritePropertyKey = priceIconSpritePropertyKey;
            m_NoPriceString = noPriceString;
            m_TransactionKey = transactionKey;
            m_Transaction = GetTransaction(m_TransactionKey);
            UpdateInteractable();

            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here causes a frame delay
            // when being driven by a parent component that makes this object look out of sync with its parent.
            UpdateContent();

        }

        /// <summary>
        ///     Initializes TransactionItemView before the first frame update if Game Foundation Sdk was already
        ///     initialized before TransactionItemView was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        ///     If it's already initialized by StoreView no action will be taken.
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

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Transaction is null)
            {
                InitializeComponentData();
            }
        }

        /// <summary>
        ///     Initializes TransactionItemView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

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
        ///     Sets the TextMeshProUGUI component to display item name on this view.
        /// </summary>
        /// <param name="text">
        ///     The TextMeshProUGUI component to display the item name.
        /// </param>
        public void SetItemNameTextField(TextMeshProUGUI text)
        {
            if (m_ItemNameTextField == text)
            {
                return;
            }

            m_ItemNameTextField = text;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Image component to display item icon sprite on this view.
        /// </summary>
        /// <param name="image">
        ///     The Image component to display item icon sprite.
        /// </param>
        public void SetItemIconImageField(Image image)
        {
            if (m_ItemIconImageField == image)
            {
                return;
            }

            m_ItemIconImageField = image;
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
        ///     Sets the Static Property key that will be used when displaying the transaction's badge.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined in the transaction's static properties for the transaction's badge.
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
        ///     Sets the string to display on <see cref="PurchaseButton"/> when there is no cost defined in the Transaction Item.
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
        ///     Sets the ImageInfoView component to display the transaction's badge on this view.
        /// </summary>
        /// <param name="badge">
        ///     The ImageInfoView component to display the transaction's badge.
        /// </param>
        public void SetBadgeField(ImageInfoView badge)
        {
            if (m_BadgeField == badge)
            {
                return;
            }

            m_BadgeField = badge;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets PurchaseButton to be able to purchase Transaction Item by UI.
        /// </summary>
        /// <param name="purchaseButton">
        ///     The PurchaseButton to display price and price icon and
        ///     to be able to purchase the TransactionItem by using UI.
        /// </param>
        public void SetPurchaseButton(PurchaseButton purchaseButton)
        {
            m_PurchaseButton = purchaseButton;
            m_IsDirty = true;
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
                UpdateRuntimeObject();
                UpdateContent();
            }
        }

        /// <summary>
        ///     At runtime, assigns the appropriate value for <see cref="m_Transaction"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void UpdateRuntimeObject()
        {
            if (!Application.isPlaying || !GameFoundationSdk.IsInitialized)
                return;

            if (m_Transaction is null && !string.IsNullOrEmpty(m_TransactionKey) ||
                !(m_Transaction is null) && m_Transaction.key != m_TransactionKey)
            {
                m_Transaction = GetTransaction(m_TransactionKey);
            }
        }

        /// <summary>
        ///     Updates the item name, item icon, badge text, and PurchaseButton.
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
        ///     To update the item name, item icon, and PurchaseButton at runtime.
        /// </summary>
        protected virtual void UpdateContentAtRuntime()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                SetTextContent(string.Empty, string.Empty);
                SetIconSprite(null);
                InitPurchaseButton();
                return;
            }

            string displayName = null;
            string badgeText = null;

            // Icon and Display name
            if (m_Transaction != null)
            {
                displayName = m_Transaction.displayName;
                if (m_Transaction.TryGetStaticProperty(m_ItemIconSpritePropertyKey, out var spriteProperty))
                {
                    PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                }
                else
                {
                    SetIconSprite(null);
                }

                if (m_Transaction.TryGetStaticProperty(m_BadgeTextPropertyKey, out var badgeProperty))
                {
                    badgeText = badgeProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning(
                        $"\"{m_Transaction.displayName}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                }
            }

            SetTextContent(displayName, badgeText);
            InitPurchaseButton();

        }

#if UNITY_EDITOR
        /// <summary>
        ///     To update the item name, item icon, and PurchaseButton at editor time.
        /// </summary>
        protected virtual void UpdateContentAtEditor()
        {
            string displayName = null;
            string badgeText = null;
            BaseTransactionAsset transactionAsset = null;

            if (!(string.IsNullOrEmpty(m_TransactionKey)))
            {
                transactionAsset =
                    PrefabTools.GetLookUpCatalogAsset().FindItem(m_TransactionKey) as BaseTransactionAsset;
            }

            if (!(transactionAsset is null))
            {
                displayName = transactionAsset.displayName;
                if (transactionAsset.TryGetStaticProperty(m_ItemIconSpritePropertyKey, out var spriteProperty))
                {
                    PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                }
                else
                {
                    SetIconSprite(null);
                }

                if (!string.IsNullOrEmpty(m_BadgeTextPropertyKey))
                {
                    if (transactionAsset.TryGetStaticProperty(m_BadgeTextPropertyKey, out var badgeProperty))
                    {
                        badgeText = badgeProperty.AsString();
                    }
                    else if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogWarning($"\"{transactionAsset.displayName.currentValue}\" transaction " +
                                              $"doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                    }
                }
            }

            SetTextContent(displayName, badgeText);
            InitPurchaseButton();
        }
#endif

        /// <summary>
        ///     Sets the transaction's displayName and badge text in their respective fields.
        /// </summary>
        /// <param name="itemDisplayName">
        ///     Transaction's display name.
        /// </param>
        /// <param name="badgeText">
        ///     Badge text to be displayed.
        /// </param>
        protected void SetTextContent(string itemDisplayName, string badgeText)
        {
            if (m_ItemNameTextField == null)
            {
                k_GFLogger.LogWarning("Item Name Text Field is not defined.");
            }
            else if (m_ItemNameTextField.text != itemDisplayName)
            {
                m_ItemNameTextField.text = itemDisplayName;
#if UNITY_EDITOR
                // Setting dirty here for the case where the TransactionItemView is being driven from a parent
                // component, instead of it's own inspector
                EditorUtility.SetDirty(this);
#endif
            }

            if (!(m_BadgeField is null))
            {
                m_BadgeField.SetText(badgeText);
                m_BadgeField.gameObject.SetActive(!string.IsNullOrEmpty(badgeText));
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_BadgeField);
#endif
            }
        }

        /// <summary>
        ///     Sets the transaction's icon with the given sprite.
        /// </summary>
        /// <param name="itemSprite">
        ///     The sprite to show on the transaction game object. If null, a blank image will be shown.
        /// </param>
        protected void SetIconSprite(Sprite itemSprite)
        {
            if (m_ItemIconImageField == null)
            {
                k_GFLogger.LogWarning("Item Icon Image Field is not defined.");
            }
            else if (m_ItemIconImageField.sprite != itemSprite)
            {
                m_ItemIconImageField.sprite = itemSprite;
                m_ItemIconImageField.preserveAspect = true;
#if UNITY_EDITOR
                // Setting dirty here for the case where the TransactionItemView is being driven from a parent
                // component, instead of it's own inspector
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        ///     Inits the <see cref="purchaseButton"/> with the <see cref="transactionKey"/>,
        ///     <see cref="priceIconSpritePropertyKey"/>, and <see cref="noPriceString"/>.
        /// </summary>
        protected void InitPurchaseButton()
        {
            if (!(m_PurchaseButton is null))
            {
                m_PurchaseButton.Init(m_TransactionKey, m_PriceIconSpritePropertyKey, m_NoPriceString);
            }
            else
            {
                k_GFLogger.LogWarning("Purchase Button is not defined.");
            }
        }

        /// <summary>
        ///     Updates the button's interactable state to the state specified in <see cref="m_Interactable"/>.
        /// </summary>
        void UpdateInteractable()
        {
            if (!(m_PurchaseButton is null))
            {
                m_PurchaseButton.interactable = m_Interactable;
            }
        }
        
        /// <summary>
        ///     Callback for if there is an error while trying to load a sprite from its Property.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message returned by <see cref="PrefabTools.LoadSprite"/>.
        /// </param>
        protected void OnSpriteLoadFailed(string errorMessage)
        {
            k_GFLogger.LogWarning(errorMessage);
        }

        /// <summary>
        ///     Gets triggered when any item in the store is successfully purchased. Triggers the
        ///     user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            if (m_TransactionKey == transaction.key)
            {
                onTransactionSucceeded?.Invoke(transaction);
            }
        }

        /// <summary>
        ///     Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        ///     user-specified onTransactionFailed callback.
        /// </summary>
        void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (m_TransactionKey == transaction.key)
            {
                onTransactionFailed?.Invoke(transaction, exception);
            }
        }

        /// <summary>
        ///     Listener for the <see cref="PurchaseButton.onPurchasableStatusChanged"/> callback in
        ///     <see cref="PurchaseButton"/>. Invokes this class' <see cref="onPurchasableStatusChanged"/> callback.
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
