using System;
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

        [SerializeField]
        internal string m_TransactionKey;

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
        ///     The string to display on <see cref="PurchaseButton"/> if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;

        [SerializeField]
        internal string m_NoPriceString = PurchaseButton.kDefaultNoPriceString;

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
        ///     The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image itemIconImageField => m_ItemIconImageField;

        [SerializeField]
        internal Image m_ItemIconImageField;

        /// <summary>
        ///     The Text component to assign the item's display name to.
        /// </summary>
        public Text itemNameTextField => m_ItemNameTextField;

        [SerializeField]
        internal Text m_ItemNameTextField;

        /// <summary>
        ///     The PurchaseButton to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;

        [SerializeField]
        internal PurchaseButton m_PurchaseButton;

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
        ///     The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        public BaseTransaction transaction => m_Transaction;

        BaseTransaction m_Transaction;

        /// <summary>
        ///     Specifies whether this view is driven by other component
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;

        /// <summary>
        ///     Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showButtonEditorFields = true;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;
        
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
            GameFoundationSdk.uninitialized += UnregisterEvents;

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
            GameFoundationSdk.transactions.transactionSucceeded += OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed += OnTransactionFailed;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            GameFoundationSdk.transactions.transactionSucceeded -= OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed -= OnTransactionFailed;
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
        internal void Init(string transactionKey, string itemIconSpritePropertyKey, string priceIconSpritePropertyKey)
        {
            Init(transactionKey, itemIconSpritePropertyKey, priceIconSpritePropertyKey, PurchaseButton.kDefaultNoPriceString);
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
        internal void Init(string transactionKey, string itemIconSpritePropertyKey, string priceIconSpritePropertyKey, string noPriceString)
        {
            m_IsDrivenByOtherComponent = true;

            m_ItemIconSpritePropertyKey = itemIconSpritePropertyKey;
            m_PriceIconSpritePropertyKey = priceIconSpritePropertyKey;
            m_NoPriceString = noPriceString;

            SetTransaction(transactionKey);
        }

        /// <summary>
        ///     Initializes TransactionItemView before the first frame update.
        ///     If the it's already initialized by StoreView no action will be taken.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ThrowIfNotInitialized(nameof(Start));

            if (!m_IsDrivenByOtherComponent)
            {
                m_Transaction = GetTransaction(m_TransactionKey);
                UpdateContent();
            }
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
            ThrowIfNotInitialized(nameof(SetTransaction));

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

            k_GFLogger.LogWarning($"TransactionItem \"{definitionKey}\" doesn't exist in Transaction catalog.");
            return null;
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

                if (m_PurchaseButton != null)
                {
                    m_PurchaseButton.interactable = interactable;
                }
            }
        }

        /// <summary>
        ///     Sets the Text component to display item name on this view.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the item name.
        /// </param>
        public void SetItemNameTextField(Text text)
        {
            if (m_ItemNameTextField == text)
            {
                return;
            }

            m_ItemNameTextField = text;

            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
        }

        /// <summary>
        ///     Updates the item name, item icon, and PurchaseButton.
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
        void UpdateContentAtRuntime()
        {
            Sprite itemSprite = null;
            string displayName = null;

            // Icon and Display name
            if (m_Transaction != null)
            {
                displayName = m_Transaction.displayName;
                if (m_Transaction.TryGetStaticProperty(m_ItemIconSpritePropertyKey, out var spriteProperty))
                {
                    itemSprite = spriteProperty.AsAsset<Sprite>();
                }
            }

            SetItemContent(itemSprite, displayName);

            // Purchase Item Button
            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(m_TransactionKey, m_PriceIconSpritePropertyKey, m_NoPriceString);
            }
            else
            {
                k_GFLogger.LogWarning("Purchase Button is not defined.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     To update the item name, item icon, and PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // To avoid updating the content the prefab selected in the Project window
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            Sprite itemSprite = null;
            string displayName = null;

            var transactionAsset = CatalogSettings.catalogAsset.FindItem(m_TransactionKey) as BaseTransactionAsset;
            if (transactionAsset != null)
            {
                displayName = transactionAsset.displayName;
                if (transactionAsset.TryGetStaticProperty(m_ItemIconSpritePropertyKey, out var spriteProperty))
                {
                    itemSprite = spriteProperty.AsAsset<Sprite>();
                }
            }

            SetItemContent(itemSprite, displayName);

            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(m_TransactionKey, m_PriceIconSpritePropertyKey, m_NoPriceString);
            }
            else
            {
                k_GFLogger.LogWarning("Purchase Button is not defined.");
            }
        }
#endif

        void SetItemContent(Sprite itemSprite, string itemDisplayName)
        {
            if (m_ItemIconImageField == null)
            {
                k_GFLogger.LogWarning("Item Icon Image Field is not defined.");
            }
            else if (m_ItemIconImageField.sprite != itemSprite)
            {
                m_ItemIconImageField.sprite = itemSprite;
                m_ItemIconImageField.SetNativeSize();
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_ItemIconImageField);
#endif
            }

            if (m_ItemNameTextField == null)
            {
                k_GFLogger.LogWarning("Item Name Text Field is not defined.");
            }
            else if (m_ItemNameTextField.text != itemDisplayName)
            {
                m_ItemNameTextField.text = itemDisplayName;
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_ItemNameTextField);
#endif
            }
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
    }
}
