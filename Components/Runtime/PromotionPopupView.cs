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
    ///     Component that manages displaying a Transaction Item's promotion popup, including promotion image, display name and
    ///     <see cref="PurchaseButton"/>.
    ///     When attached to a game object, it will display an image featuring the contents of the Transaction Item's payouts
    ///     (or a specified image), the Transaction Item's displayName and create and display a
    ///     <see cref="PurchaseButton"/> to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Promotion Popup View", 2)]
    [ExecuteInEditMode]
    public class PromotionPopupView : MonoBehaviour
    {
        /// <summary>
        ///     The key of the Transaction Item being displayed.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        [SerializeField]
        internal string m_TransactionKey;

        /// <summary>
        ///     Determines whether or not the promotion's image will be auto-generated.
        ///     If true (default), the image will be generated using the sprite asset name's for each Inventory or Currency Item
        ///     listed in the Transaction's Payout.
        ///     If false, the Promotion Sprite property key will be used to get the image from the transaction's static property.
        /// </summary>
        public bool autoGeneratePromoImage => m_AutoGeneratePromoImage;

        [SerializeField]
        [Space]
        internal bool m_AutoGeneratePromoImage = true;

        /// <summary>
        ///     The string to prefix the payout <see cref="InventoryItem"/> counts when auto generating the promotion image.
        /// </summary>
        public string itemPayoutCountPrefix => m_ItemPayoutCountPrefix;

        [SerializeField]
        internal string m_ItemPayoutCountPrefix = kDefaultCountPrefix;

        /// <summary>
        ///     The string to prefix the payout <see cref="Currency"/> counts when auto generating the promotion image.
        /// </summary>
        public string currencyPayoutCountPrefix => m_CurrencyPayoutCountPrefix;

        [SerializeField]
        internal string m_CurrencyPayoutCountPrefix = kDefaultCountPrefix;

        /// <summary>
        ///     The Static Property key for the icon of the Inventory or Currency Items listed in the Transaction's Payout, as
        ///     specified in their Static Properties.
        ///     Used only when Auto Generate Promo Image is enabled.
        /// </summary>
        public string payoutItemIconSpritePropertyKey => m_PayoutItemIconSpritePropertyKey;

        [SerializeField]
        internal string m_PayoutItemIconSpritePropertyKey;

        /// <summary>
        ///     The Static Property key for the promotion image that will be displayed on this view, as specified in the
        ///     Transaction's Static Property.
        ///     Used only when Auto Generate Promo Image is disabled.
        /// </summary>
        public string promoImageSpritePropertyKey => m_PromoImageSpritePropertyKey;

        [SerializeField]
        internal string m_PromoImageSpritePropertyKey;

        /// <summary>
        ///     The Static Property key for the promotion description to be displayed.
        /// </summary>
        public string descriptionPropertyKey => m_descriptionPropertyKey;

        [SerializeField]
        [Space]
        internal string m_descriptionPropertyKey;

        /// <summary>
        ///     The Static Property key for the badge to be displayed in callout.
        /// </summary>
        public string badgeTextPropertyKey => m_BadgeTextPropertyKey;

        [SerializeField]
        internal string m_BadgeTextPropertyKey;

        /// <summary>
        ///     The Static Property key  for price icon that will be displayed on the <see cref="PurchaseButton"/>, as specified
        ///     in the Static Property for the Inventory Item or Currency listed in the price.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey;

        /// <summary>
        ///     The TextMeshProUGUI component to assign the Transaction's display name to.
        /// </summary>
        public TextMeshProUGUI titleTextField => m_TitleTextField;

        [SerializeField]
        [Space]
        internal TextMeshProUGUI m_TitleTextField;

        /// <summary>
        ///     The TextMeshProUGUI component to assign the description to.
        /// </summary>
        public TextMeshProUGUI descriptionTextField => m_DescriptionTextField;

        [SerializeField]
        internal TextMeshProUGUI m_DescriptionTextField;

        /// <summary>
        ///     The Transform in which to auto generate promotion image.
        ///     Used only when Auto Generate Promo Image is enabled.
        /// </summary>
        public PayoutView autoGeneratedImageContainer => m_AutoGeneratedImageContainer;

        [SerializeField]
        internal PayoutView m_AutoGeneratedImageContainer;

        /// <summary>
        ///     The Image component to assign the Promotion Image to.
        ///     Used only when Auto Generate Promo Image is disabled.
        /// </summary>
        public Image promoImageField => m_PromoImageField;

        [SerializeField]
        internal Image m_PromoImageField;

        /// <summary>
        ///     The <see cref="ImageInfoView"/> to assign the badge to.
        /// </summary>
        public ImageInfoView badgeField => m_BadgeField;

        [SerializeField]
        internal ImageInfoView m_BadgeField;

        /// <summary>
        ///     The <see cref="PurchaseButton"/> to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;

        [SerializeField]
        internal PurchaseButton m_PurchaseButton;

        /// <summary>
        ///     The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        public BaseTransaction transaction => m_Transaction;

        BaseTransaction m_Transaction;

        /// <summary>
        ///     Callback that will get triggered when the Promotion Popup is opened.
        /// </summary>
        public PopupOpenedEvent onPopupOpened;

        /// <summary>
        ///     Callback that will get triggered when the Promotion Popup is closed.
        /// </summary>
        public PopupClosedEvent onPopupClosed;

        /// <summary>
        ///     A callback for when the Promotion Popup opens. Wraps UnityEvent
        /// </summary>
        [Serializable]
        public class PopupOpenedEvent : UnityEvent { }

        /// <summary>
        ///     A callback for when the Promotion Popup closes. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupClosedEvent : UnityEvent { }

        /// <summary>
        ///     A callback for when the Promotion Popup opens. Wraps UnityEvent
        /// </summary>
        [Serializable]
        public class PopupWillOpenEvent : UnityEvent { }

        /// <summary>
        ///     A callback for when the Promotion Popup closes. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupWillCloseEvent : UnityEvent { }

        /// <summary>
        ///     A callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        ///     A callback that will get triggered if a purchase for any item in the store fails.
        /// </summary>
        public TransactionFailureEvent onTransactionFailed;

        /// <summary>
        ///     A callback for when a transaction is completed. Wraps UnityEvent and accepts a BaseTransaction as a parameter.
        /// </summary>
        [Serializable]
        public class TransactionSuccessEvent : UnityEvent<BaseTransaction> { }

        /// <summary>
        ///     A callback for when a transaction is failed. Wraps UnityEvent and accepts a BaseTransaction and Exception as a
        ///     parameter.
        /// </summary>
        [Serializable]
        public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> { }

        /// <summary>
        ///     Default count prefix for inventory item and currency.
        /// </summary>
        const string kDefaultCountPrefix = "x";

        /// <summary>
        ///     A name to use when generating PayoutItem GameObjects under Auto Generated Image Container.
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
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

        /// <summary>
        ///     Specifies whether the Text fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showTextEditorFields = true;

        /// <summary>
        ///     Specifies whether the Button fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showButtonEditorFields = true;
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<PromotionPopupView>();

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

            if (!(m_Transaction is null))
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
        ///     Initializes PromotionPopupView before the first frame update if Game Foundation Sdk was already
        ///     initialized before CurrencyHudView was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Transaction is null)
            {
                InitializeComponentData();
                return;
            }

            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Waiting for initialization.");
                m_IsDirty = true;
            }
        }

        /// <summary>
        ///     Initializes PromotionPopupView data from Game Foundation Sdk.
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
        ///     Displays the promotion popup.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        public void Open()
        {
            Open(m_Transaction ?? GetTransaction(m_TransactionKey), m_AutoGeneratePromoImage);
        }

        /// <summary>
        ///     Displays the promotion popup.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        /// <param name="transaction">
        ///     A reference to <see cref="BaseTransaction"/> that should be displayed.
        /// </param>
        public void Open(BaseTransaction transaction)
        {
            Open(transaction, m_AutoGeneratePromoImage);
        }

        /// <summary>
        ///     Displays the promotion popup.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        /// <param name="transaction">
        ///     A reference to <see cref="BaseTransaction"/> that should be displayed.
        /// </param>
        /// <param name="autoGeneratePromoImage">
        ///     Specifies whether the promotion image should be auto generated using the Payout sprite property key or will be user
        ///     provided via the promotion sprite property key.
        /// </param>
        public void Open(BaseTransaction transaction, bool autoGeneratePromoImage)
        {
            if (gameObject.activeInHierarchy)
            {
                return;
            }
            
            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Promotion Popup has been opened when Game Foundation Sdk is not initialized. Content will be blank until Game Foundation initializes and no changes to state have been made.");
                m_Transaction = null;
            }
            else
            {
                if (transaction is null)
                {
                    k_GFLogger.LogWarning("Transaction is null.");
                }

                m_TransactionKey = transaction?.key;
                m_Transaction = transaction;
                m_AutoGeneratePromoImage = autoGeneratePromoImage;
            }

            gameObject.SetActive(true);
            // Need to trigger a layout rebuild after SetActive has fully completed it's call because the promotion items aren't able to be displayed properly until then.
            if (m_AutoGeneratedImageContainer)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    (RectTransform) m_AutoGeneratedImageContainer.transform.parent);
            }

            if (gameObject.activeInHierarchy)
            {
                onPopupOpened?.Invoke();   
            }
        }

        /// <summary>
        ///     Hides the promotion popup.
        ///     Will trigger a PopupClosedEvent (<see cref="PopupClosedEvent"/>) and a PopupWillCloseEvent (
        ///     <see cref="PopupWillCloseEvent"/>).
        /// </summary>
        public void Close()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            gameObject.SetActive(false);
            onPopupClosed?.Invoke();
        }

        /// <summary>
        ///     Sets whether the image used for the promotion should be auto generated or use an asset from the transaction's Asset
        ///     Detail.
        /// </summary>
        /// <param name="autoGenerateImage">
        ///     If true it will auto generate the Promotion Image, ignoring the <see cref="promoImageSpritePropertyKey"/> field.
        ///     Field is true by default.
        ///     If false, it will use the key specified in <see cref="promoImageSpritePropertyKey"/> to get the image from the
        ///     Transaction's Static Property.
        /// </param>
        public void SetAutoGenerateImage(bool autoGenerateImage)
        {
            m_AutoGeneratePromoImage = true;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Set the image used for the promotion should be auto generated.
        /// </summary>
        /// <param name="itemIconSpritePropertyKey">
        ///     The Static Property key for the icon of the Inventory or Currency Items listed in the Transaction's Payout, as
        ///     specified in their Static Properties.
        /// </param>
        /// <param name="itemCountPrefix">
        ///     The string to prefix the payout inventory item counts when auto generating the promotion image.
        /// </param>
        /// <param name="currencyCountPrefix">
        ///     The string to prefix the payout currency counts when auto generating the promotion image.
        /// </param>
        public void SetAutoGeneratePromotionImage(string itemIconSpritePropertyKey, string itemCountPrefix = kDefaultCountPrefix, string currencyCountPrefix = kDefaultCountPrefix)
        {
            if (!m_AutoGeneratePromoImage && m_ShowDebugLogs)
            {
                k_GFLogger.Log("Auto-Generated Image is enabled");
            }

            m_AutoGeneratePromoImage = true;
            m_PayoutItemIconSpritePropertyKey = itemIconSpritePropertyKey;
            m_ItemPayoutCountPrefix = itemCountPrefix;
            m_CurrencyPayoutCountPrefix = currencyCountPrefix;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets Static Property key for the promotion image asset that will be used when displaying the user-provided
        ///     promotion image and disable Auto Generate Promo Image (<see cref="autoGeneratePromoImage"/>).
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined on the Transaction item's Static Property for the promotion image.
        /// </param>
        public void SetPromotionImagePropertyKey(string propertyKey)
        {
            if (m_AutoGeneratePromoImage && m_ShowDebugLogs)
            {
                k_GFLogger.Log("Auto-Generated Image is disabled");
            }

            m_AutoGeneratePromoImage = false;
            m_PromoImageSpritePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the static property key that will be used when displaying the promotion's description.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined in the Transaction's Static Properties for the promotion's description.
        /// </param>
        public void SetDescriptionPropertyKey(string propertyKey)
        {
            if (m_descriptionPropertyKey == propertyKey)
            {
                return;
            }

            m_descriptionPropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Static Property key that will be used when displaying the promotion's badge.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key that is defined in the Transaction's static properties for the promotion's badge.
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
        ///     Sets the Static Property key for price icon that will be displayed on <see cref="PurchaseButton"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The key is defined in the Transaction's Static Properties.
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
        ///     Sets the TextMeshProUGUI component to display the Transaction's display name on this view.
        /// </summary>
        /// <param name="text">
        ///     The TextMeshProUGUI component to display the transaction's name.
        /// </param>
        public void SetTitleTextField(TextMeshProUGUI text)
        {
            if (m_TitleTextField == text)
            {
                return;
            }

            m_TitleTextField = text;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the TextMeshProUGUI component to display the promotion's description on this view.
        /// </summary>
        /// <param name="text">
        ///     The TextMeshProUGUI component to display the promotion's description.
        /// </param>
        public void SetDescriptionTextField(TextMeshProUGUI text)
        {
            if (m_DescriptionTextField == text)
            {
                return;
            }

            m_DescriptionTextField = text;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Transform in which to display the promotion's image on this view.
        /// </summary>
        /// <param name="container">
        ///     The Transform in which to display the promotion's image.
        /// </param>
        public void SetAutoGeneratedImageContainer(PayoutView container)
        {
            if (m_AutoGeneratedImageContainer == container)
            {
                return;
            }

            m_AutoGeneratedImageContainer = container;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Image component to display the promotion image on this view.
        /// </summary>
        /// <param name="image">
        ///     The Image component to display the promotion.
        /// </param>
        public void SetPromoImageField(Image image)
        {
            if (m_PromoImageField == image)
            {
                return;
            }

            m_PromoImageField = image;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the ImageInfoView component to display the promotion's badge on this view.
        /// </summary>
        /// <param name="badge">
        ///     The ImageInfoView component to display the promotion's badge.
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
        ///     Sets <see cref="PurchaseButton"/> to be able to purchase Transaction Item by UI.
        /// </summary>
        /// <param name="purchaseButton">
        ///     The PurchaseButton to display price and price icon and
        ///     to be able to purchase the TransactionItem by using UI.
        /// </param>
        public void SetPurchaseButton(PurchaseButton purchaseButton)
        {
            if (m_PurchaseButton == purchaseButton)
            {
                return;
            }

            m_PurchaseButton = purchaseButton;
            m_IsDirty = true;
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
                // UpdateContent doesn't update m_Currency, which is what is used to fetch icons at runtime.
                // If Game Foundation is initialized and m_Currency does not match m_CurrencyKey,
                // reset m_Currency based on m_CurrencyKey.
                if (GameFoundationSdk.IsInitialized &&
                    (m_Transaction is null && !string.IsNullOrEmpty(m_TransactionKey) || 
                     !(m_Transaction is null) && m_Transaction.key != m_TransactionKey))
                {
                    m_Transaction = GetTransaction(m_TransactionKey);
                }

                UpdateContent();
            }
        }

        /// <summary>
        ///     Updates the content displayed in the Promotion Popup.
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
        ///     Updates the transaction's display name, description, promotion image, badge, and PurchaseButton at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            // Get the values for transaction's text fields
            string displayName = null;
            string descriptionText = null;
            string badgeText = null;

            if (!(m_Transaction is null))
            {
                displayName = m_Transaction.displayName;

                if (m_Transaction.TryGetStaticProperty(m_descriptionPropertyKey, out var descriptionProperty))
                {
                    descriptionText = descriptionProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning($"\"{m_Transaction.displayName}\" transaction doesn't have Static " +
                                          $"Property called \"{m_descriptionPropertyKey}\"");
                }

                if (m_Transaction.TryGetStaticProperty(m_BadgeTextPropertyKey, out var badgeProperty))
                {
                    badgeText = badgeProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning($"\"{m_Transaction.displayName}\" transaction doesn't have Static " +
                                          $"Property called \"{m_BadgeTextPropertyKey}\"");
                }
            }

            if (autoGeneratePromoImage)
            {
                ClearPromotionImage();
                if (m_AutoGeneratedImageContainer)
                {
                    if (this.ShouldRegenerateGameObjects())
                    {
                        m_AutoGeneratedImageContainer.Init(m_Transaction, m_PayoutItemIconSpritePropertyKey,
                            m_ItemPayoutCountPrefix, m_CurrencyPayoutCountPrefix);
                    }
                    else
                    {
                        m_AutoGeneratedImageContainer.Init();
                    }
                }
            }
            else
            {
                if (m_AutoGeneratedImageContainer && this.ShouldRegenerateGameObjects())
                {
                    m_AutoGeneratedImageContainer.ClearContent();
                }

                LoadAndSetIconSprite();
            }

            ToggleIconContainerVisibility();
            SetContent(m_TransactionKey, displayName, descriptionText, badgeText);
        }

        /// <summary>
        ///     Sets popup content with user-provided promotion image.
        /// </summary>
        /// <param name="transactionAsset">
        ///     The transaction to use to fetch promotion image.
        /// </param>
        void LoadAndSetIconSprite()
        {
            ClearPromotionImage();

            if (!(m_Transaction is null) && !string.IsNullOrEmpty(m_PromoImageSpritePropertyKey))
            {
                if (m_Transaction.TryGetStaticProperty(m_PromoImageSpritePropertyKey, out var promotionProperty))
                {
                    PrefabTools.LoadSprite(promotionProperty, SetIconSpriteContent, OnSpriteLoadFailed);
                }
                else
                {
                    k_GFLogger.LogWarning($"\"{m_Transaction.displayName}\" doesn't have a static property " +
                                          $"with the key \"{m_PromoImageSpritePropertyKey}\"");
                }
            }
            else
            {
                SetIconSpriteContent(null);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Updates the Transaction's Display Name, Description, Promotion image, Badge, and PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // Get the values for transaction's text fields
            string displayName = null;
            string descriptionText = null;
            string badgeText = null;
            BaseTransactionAsset transaction = null;

            if (!(m_TransactionKey is null))
            {
                transaction = PrefabTools.GetLookUpCatalogAsset().FindItem(m_TransactionKey) as BaseTransactionAsset;
            }
            if (!(transaction is null))
            {
                displayName = transaction.displayName;
                var properties = transaction.GetStaticProperties();

                if (!string.IsNullOrEmpty(m_descriptionPropertyKey))
                {
                    var descriptionIndex = properties.FindIndex(x => x.key == m_descriptionPropertyKey);
                    if (descriptionIndex >= 0)
                    {
                        descriptionText = properties[descriptionIndex].value.AsString();
                    }
                    else if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogWarning($"\"{transaction.displayName.currentValue}\" transaction doesn't have Static Property called \"{m_descriptionPropertyKey}\"");
                    }
                }

                if (!string.IsNullOrEmpty(m_BadgeTextPropertyKey))
                {
                    var badgeIndex = properties.FindIndex(x => x.key == m_BadgeTextPropertyKey);
                    if (badgeIndex >= 0)
                    {
                        badgeText = properties[badgeIndex].value.AsString();
                    }
                    else if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogWarning($"\"{transaction.displayName.currentValue}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                    }
                }
            }

            if (m_AutoGeneratePromoImage)
            {
                ClearPromotionImage();
                if (m_AutoGeneratedImageContainer)
                {
                    if (this.ShouldRegenerateGameObjects())
                    {
                        m_AutoGeneratedImageContainer.Init(transaction, m_PayoutItemIconSpritePropertyKey,
                            m_ItemPayoutCountPrefix, m_CurrencyPayoutCountPrefix);
                    }
                    else
                    {
                        m_AutoGeneratedImageContainer.Init();
                    }
                }
            }
            else
            {
                if (m_AutoGeneratedImageContainer && this.ShouldRegenerateGameObjects())
                {
                    m_AutoGeneratedImageContainer.ClearContent();
                }

                LoadAndSetIconSprite(transaction);
            }

            ToggleIconContainerVisibility();
            SetContent(m_TransactionKey, displayName, descriptionText, badgeText);
        }

        /// <summary>
        ///     Sets popup content with user-provided promotion image.
        /// </summary>
        /// <param name="transactionAsset">
        ///     The transaction asset to use to fetch promotion image.
        /// </param>
        void LoadAndSetIconSprite(BaseTransactionAsset transactionAsset)
        {
            ClearPromotionImage();

            if (!(transactionAsset is null) && !string.IsNullOrEmpty(m_PromoImageSpritePropertyKey))
            {
                if (transactionAsset.TryGetStaticProperty(m_PromoImageSpritePropertyKey, out var imageProperty))
                {
                    PrefabTools.LoadSprite(imageProperty, SetIconSpriteContent, OnSpriteLoadFailed);
                }
                else
                {
                    k_GFLogger.LogWarning($"\"{transactionAsset.displayName.currentValue}\" doesn't have" +
                                          $"static property with key \"{m_PromoImageSpritePropertyKey}\"");
                }
            }
            else
            {
                SetIconSpriteContent(null);
            }
        }
#endif

        /// <summary>
        ///     Clears user-provided image
        /// </summary>
        void ClearPromotionImage()
        {
            if (m_PromoImageField != null)
            {
                m_PromoImageField.sprite = null;
            }
        }

        /// <summary>
        ///     Sets the promotion image on the <see cref="m_PromoImageField"/> if image exists, otherwise sets field
        ///     to inactive.
        /// </summary>
        /// <param name="promotionImage">
        ///     The promotion image sprite to set.
        /// </param>
        void SetIconSpriteContent(Sprite promotionImage)
        {
            if (!(m_PromoImageField is null))
            {
                if (promotionImage is null)
                {
                    m_PromoImageField.gameObject.SetActive(false);
                }

                if (m_PromoImageField.sprite != promotionImage)
                {
                    m_PromoImageField.sprite = promotionImage;
                    m_PromoImageField.preserveAspect = true;
                }

                // To force rebuilt Layouts at Editor and Run time
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_PromoImageField.transform);
            }
        }

        /// <summary>
        ///     Determines visibility of <see cref="m_AutoGeneratedImageContainer"/> and <see cref="m_PromoImageField"/>
        ///     based on <see cref="m_AutoGeneratePromoImage"/>. Only one of the above containers will be visible at any
        ///     given time.
        /// </summary>
        void ToggleIconContainerVisibility()
        {
            if (m_AutoGeneratePromoImage)
            {
                if (m_PromoImageField != null)
                {
                    m_PromoImageField.gameObject.SetActive(false);
                }

                if (m_AutoGeneratedImageContainer != null)
                {
                    m_AutoGeneratedImageContainer.gameObject.SetActive(true);
                }
                else if (Application.isPlaying)
                {
                    k_GFLogger.LogWarning("Auto-Generated Image Container should be defined to generate Promo Image");
                }
            }
            else
            {
                if (m_AutoGeneratedImageContainer != null)
                {
                    m_AutoGeneratedImageContainer.gameObject.SetActive(false);
                }

                if (m_PromoImageField != null)
                {
                    m_PromoImageField.gameObject.SetActive(true);
                }
                else if (Application.isPlaying)
                {
                    k_GFLogger.LogWarning("Promotion Image Field should be defined to show Promo Image.");
                }
            }
        }

        /// <summary>
        ///     Sets popup content text and Purchase Button fields.
        /// </summary>
        /// <param name="transactionKey">
        ///     The key of the Transaction Item being displayed.
        /// </param>
        /// <param name="title">
        ///     A title of the popup
        /// </param>
        /// <param name="description">
        ///     A description the popup.
        /// </param>
        /// <param name="badgeText">
        ///     A text for the badge. If it's empty or null, badge will be hidden.
        /// </param>
        void SetContent(string transactionKey, string title, string description, string badgeText)
        {
            SetTextContent(title, description, badgeText);
            SetPurchaseButtonContent(transactionKey, m_PriceIconSpritePropertyKey);
        }

        /// <summary>
        ///     Set text fields of popup content.
        /// </summary>
        /// <param name="title">
        ///     A title of the popup
        /// </param>
        /// <param name="description">
        ///     A description the popup.
        /// </param>
        /// <param name="badgeText">
        ///     A text for the badge. If it's empty or null, badge will be hidden.
        /// </param>
        void SetTextContent(string title, string description, string badgeText)
        {
            if (m_TitleTextField != null && m_TitleTextField.text != title)
            {
                m_TitleTextField.text = title;
            }

            if (m_DescriptionTextField != null)
            {
                if (m_DescriptionTextField.text != description)
                {
                    m_DescriptionTextField.text = description;
                }

                m_DescriptionTextField.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }

            if (m_BadgeField != null)
            {
                m_BadgeField.SetText(badgeText);
                m_BadgeField.gameObject.SetActive(!string.IsNullOrEmpty(badgeText));
            }
        }

        /// <summary>
        ///     Set Purchase Button
        /// </summary>
        /// <param name="transactionKey">
        ///     The key of the Transaction Item being displayed.
        /// </param>
        /// <param name="priceIconSpritePropertyKey">
        ///     The Static Property key for price icon on Purchase Button
        /// </param>
        void SetPurchaseButtonContent(string transactionKey, string priceIconSpritePropertyKey)
        {
            // Purchase Item Button
            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(transactionKey, priceIconSpritePropertyKey);
            }
            else
            {
                k_GFLogger.LogWarning($"{nameof(PurchaseButton)} is not defined.");
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
        ///     Gets triggered when the promotion popup transaction is successfully purchased. Triggers the
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
        ///     Gets triggered when the promotion popup transaction is attempted and fails to be purchased. Triggers the
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
        ///     When changes are made via the Inspector, set <see cref="m_IsDirty"/> to true.
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
