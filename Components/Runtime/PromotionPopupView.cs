using System;
using System.Collections.Generic;
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
    [AddComponentMenu("Game Foundation/Promotion Popup", 2)]
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
        internal string m_PayoutItemIconSpritePropertyKey = "promotion_icon";

        /// <summary>
        ///     The Static Property key for the promotion image that will be displayed on this view, as specified in the
        ///     Transaction's Static Property.
        ///     Used only when Auto Generate Promo Image is disabled.
        /// </summary>
        public string promoImageSpritePropertyKey => m_PromoImageSpritePropertyKey;

        [SerializeField]
        internal string m_PromoImageSpritePropertyKey = "promotion_image";

        /// <summary>
        ///     The Static Property key for the promotion description to be displayed.
        /// </summary>
        public string descriptionPropertyKey => m_descriptionPropertyKey;

        [SerializeField]
        [Space]
        internal string m_descriptionPropertyKey = "promotion_description";

        /// <summary>
        ///     The Static Property key for the badge to be displayed in callout.
        /// </summary>
        public string badgeTextPropertyKey => m_BadgeTextPropertyKey;

        [SerializeField]
        internal string m_BadgeTextPropertyKey = "promotion_badge";

        /// <summary>
        ///     The Static Property key  for price icon that will be displayed on the <see cref="PurchaseButton"/>, as specified
        ///     in the Static Property for the Inventory Item or Currency listed in the price.
        /// </summary>
        public string priceIconSpritePropertyKey => m_PriceIconSpritePropertyKey;

        [SerializeField]
        internal string m_PriceIconSpritePropertyKey = "purchase_button_icon";

        /// <summary>
        ///     The Payout Item prefab to use when auto generating promotion image.
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
        ///     The Text component to assign the Transaction's display name to.
        /// </summary>
        public Text titleTextField => m_TitleTextField;

        [SerializeField]
        [Space]
        internal Text m_TitleTextField;

        /// <summary>
        ///     The Text component to assign the description to.
        /// </summary>
        public Text descriptionTextField => m_DescriptionTextField;

        [SerializeField]
        internal Text m_DescriptionTextField;

        /// <summary>
        ///     The Transform in which to auto generate promotion image.
        ///     Used only when Auto Generate Promo Image is enabled.
        /// </summary>
        public Transform autoGeneratedImageContainer => m_AutoGeneratedImageContainer;

        [SerializeField]
        internal Transform m_AutoGeneratedImageContainer;

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

            if (!ReferenceEquals(m_Transaction, null))
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
        ///     Initializes PromotionPopupView before the first frame update.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>) if the prefab is active in hierarchy at start.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ThrowIfNotInitialized();

            m_Transaction = GetTransaction(m_TransactionKey);

            UpdateContent();
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
            ThrowIfNotInitialized();

            if (gameObject.activeInHierarchy)
            {
                return;
            }
            
            if (transaction == null)
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - Transaction is null.");
            }

            m_Transaction = transaction;
            m_TransactionKey = transaction?.key;
            m_AutoGeneratePromoImage = autoGeneratePromoImage;

            gameObject.SetActive(true);

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
            UpdateContent();
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
                Debug.Log($"{nameof(PromotionPopupView)} - Auto-Generated Image is enabled");
            }

            m_AutoGeneratePromoImage = true;
            m_PayoutItemIconSpritePropertyKey = itemIconSpritePropertyKey;
            m_ItemPayoutCountPrefix = itemCountPrefix;
            m_CurrencyPayoutCountPrefix = currencyCountPrefix;

            UpdateContent();
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
                Debug.Log($"{nameof(PromotionPopupView)} - Auto-Generated Image is disabled");
            }

            m_AutoGeneratePromoImage = false;
            m_PromoImageSpritePropertyKey = propertyKey;

            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
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
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Text component to display the Transaction's display name on this view.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the transaction's name.
        /// </param>
        public void SetTitleTextField(Text text)
        {
            if (m_TitleTextField == text)
            {
                return;
            }

            m_TitleTextField = text;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Text component to display the promotion's description on this view.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the promotion's description.
        /// </param>
        public void SetDescriptionTextField(Text text)
        {
            if (m_DescriptionTextField == text)
            {
                return;
            }

            m_DescriptionTextField = text;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Transform in which to display the promotion's image on this view.
        /// </summary>
        /// <param name="container">
        ///     The Transform in which to display the promotion's image.
        /// </param>
        public void SetAutoGeneratedImageContainer(Transform container)
        {
            if (m_AutoGeneratedImageContainer == container)
            {
                return;
            }

            m_AutoGeneratedImageContainer = container;
            UpdateContent();
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
            UpdateContent();
        }

        /// <summary>
        ///     Sets the Text component to display the promotion's badge on this view.
        /// </summary>
        /// <param name="badge">
        ///     The Text component to display the promotion's badge.
        /// </param>
        public void SetBadgeField(ImageInfoView badge)
        {
            if (m_BadgeField == badge)
            {
                return;
            }

            m_BadgeField = badge;
            UpdateContent();
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
            UpdateContent();
        }

        /// <summary>
        ///     The prefab to use for each when auto generating the promotion image.
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
            UpdateContent();
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
            UpdateContent();
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

            if (!ReferenceEquals(m_Transaction, null))
            {
                displayName = m_Transaction.displayName;

                if (m_Transaction.TryGetStaticProperty(m_descriptionPropertyKey, out var descriptionProperty))
                {
                    descriptionText = descriptionProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    Debug.LogWarning(
                        $"{nameof(PromotionPopupView)} - \"{m_Transaction.displayName}\" transaction doesn't have Static Property called \"{m_descriptionPropertyKey}\"");
                }

                if (m_Transaction.TryGetStaticProperty(m_BadgeTextPropertyKey, out var badgeProperty))
                {
                    badgeText = badgeProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    Debug.LogWarning(
                        $"{nameof(PromotionPopupView)} - \"{m_Transaction.displayName}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                }
            }

            if (autoGeneratePromoImage)
            {
                SetContent(m_TransactionKey, displayName, descriptionText, badgeText, GetPayoutIcons(m_Transaction));
            }
            else
            {
                Sprite promotionImage = null;
                if (!ReferenceEquals(m_Transaction, null))
                {
                    if (m_Transaction.TryGetStaticProperty(m_PromoImageSpritePropertyKey, out var promotionProperty))
                    {
                        promotionImage = promotionProperty.AsAsset<Sprite>();
                    }

                    if (promotionImage == null)
                    {
                        Debug.LogWarning(
                            $"{nameof(PromotionPopupView)} - \"{m_Transaction.displayName}\" doesn't have Promotion Image sprite called \"{m_PromoImageSpritePropertyKey}\"");
                    }
                }

                SetContent(m_TransactionKey, displayName, descriptionText, badgeText, promotionImage);
            }
        }

        /// <summary>
        ///     Gets a list of sprites, one for each Inventory or Currency item listed in the Transaction's Payouts list, at
        ///     runtime.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="BaseTransaction"/> used for this promotion.
        /// </param>
        List<Tuple<Sprite, string>> GetPayoutIcons(BaseTransaction transaction)
        {
            if (ReferenceEquals(transaction, null))
            {
                return null;
            }

            var payouts = new List<Tuple<Sprite, string>>();
            var exchanges = new List<ExchangeDefinition>();

            transaction.payout.GetExchanges(exchanges);
            foreach (var exchange in exchanges)
            {
                Sprite icon = null;
                if (exchange.tradableDefinition.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey, out var iconProperty))
                {
                    icon = iconProperty.AsAsset<Sprite>();
                }

                if (icon != null)
                {
                    var quantity = "";
                    switch (exchange.tradableDefinition)
                    {
                        case Currency _:
                            quantity = (m_CurrencyPayoutCountPrefix ?? "") + exchange.amount;
                            break;
                        case InventoryItemDefinition _:
                            quantity = (m_ItemPayoutCountPrefix ?? "") + exchange.amount;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    payouts.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The \"{transaction.displayName}\" transaction's \"{exchange.tradableDefinition.displayName}\" payout does not have an asset with the name \"{m_PayoutItemIconSpritePropertyKey}\" so it will not be showed in the promotion");
                }
            }

            return payouts;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Updates the Transaction's Display Name, Description, Promotion image, Badge, and PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // Known Issue: It's temporary protection for editor time to avoid generating GameObjects(PayoutItem) in Prefab Asset.
            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                return;
            }

            // Get the values for transaction's text fields
            string displayName = null;
            string descriptionText = null;
            string badgeText = null;

            var transaction = CatalogSettings.catalogAsset.FindItem(m_TransactionKey) as BaseTransactionAsset;
            if (!ReferenceEquals(transaction, null))
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
                        Debug.LogWarning(
                            $"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_descriptionPropertyKey}\"");
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
                        Debug.LogWarning(
                            $"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                    }
                }
            }

            if (m_AutoGeneratePromoImage)
            {
                SetContent(m_TransactionKey, displayName, descriptionText, badgeText, GetPayoutIcons(transaction));
            }
            else
            {
                Sprite promotionImage = null;
                if (!ReferenceEquals(transaction, null))
                {
                    if (transaction.TryGetStaticProperty(m_PromoImageSpritePropertyKey, out var imageProperty))
                    {
                        promotionImage = imageProperty.AsAsset<Sprite>();
                    }

                    if (promotionImage == null)
                    {
                        Debug.LogWarning(
                            $"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" doesn't have sprite called \"{m_PromoImageSpritePropertyKey}\"");
                    }
                }

                SetContent(m_TransactionKey, displayName, descriptionText, badgeText, promotionImage);
            }
        }

        /// <summary>
        ///     Gets a list of sprites, one for each Inventory or Currency Item listed in the Transaction's Payouts list, at editor
        ///     time.
        /// </summary>
        /// <param name="transactionAsset">
        ///     The Transaction used for this promotion.
        /// </param>
        List<Tuple<Sprite, string>> GetPayoutIcons(BaseTransactionAsset transactionAsset)
        {
            if (ReferenceEquals(transactionAsset, null))
            {
                return null;
            }

            var payouts = new List<Tuple<Sprite, string>>();

            var exchangeObjects = new List<ExchangeDefinitionObject>();
            transactionAsset.payout.GetItems(exchangeObjects);

            foreach (var exchangeObject in exchangeObjects)
            {
                Sprite icon = null;
                if (exchangeObject.catalogItem.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey, out var iconProperty))
                {
                    icon = iconProperty.AsAsset<Sprite>();
                }

                if (icon != null)
                {
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

                    payouts.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The \"{transactionAsset.displayName}\" transaction's \"{exchangeObject.catalogItem.displayName}\" payout does not have an asset with the name \"{m_PayoutItemIconSpritePropertyKey}\" so it will not be showed in the promotion");
                }
            }

            return payouts;
        }
#endif

        /// <summary>
        ///     Sets popup content with user-provided promotion image.
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
        /// <param name="promotionImage">
        ///     A image for the promotion.
        /// </param>
        void SetContent(string transactionKey, string title, string description, string badgeText, Sprite promotionImage)
        {
            ClearPromotionImages();

            SetTextContent(title, description, badgeText);
            SetPurchaseButton(transactionKey, m_PriceIconSpritePropertyKey);

            m_AutoGeneratedImageContainer?.gameObject.SetActive(false);

            if (!ReferenceEquals(m_PromoImageField, null))
            {
                if (promotionImage != null)
                {
                    m_PromoImageField.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - Promotion Image Field is not defined.");
                }

                if (m_PromoImageField.sprite != promotionImage)
                {
                    m_PromoImageField.sprite = promotionImage;
                    m_PromoImageField.SetNativeSize();
#if UNITY_EDITOR
                    EditorUtility.SetDirty(m_PromoImageField);
#endif
                }
            }

            // To force rebuilt Layouts at Editor and Run time
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_PromoImageField.transform);
        }

        /// <summary>
        ///     Sets popup content with Transaction Item's payouts
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
        /// <param name="payouts">
        ///     Sprite and quantity sets of payouts.
        /// </param>
        void SetContent(string transactionKey, string title, string description, string badgeText, IList<Tuple<Sprite, string>> payouts)
        {
            ClearPromotionImages();

            SetTextContent(title, description, badgeText);
            SetPurchaseButton(transactionKey, m_PriceIconSpritePropertyKey);

            if (m_PromoImageField != null)
            {
                m_PromoImageField.gameObject.SetActive(false);
            }

            if (m_AutoGeneratedImageContainer != null)
            {
                m_AutoGeneratedImageContainer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - Auto-Generated Image Container should be defined to generate Promo Image");
            }

            if (payoutItemPrefab == null)
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - PayoutItem Prefab should be defined to generate Promo Image, ");
            }
            else if (m_AutoGeneratedImageContainer != null && payouts != null)
            {
                var count = payouts.Count;
                for (var i = 0; i < count; i++)
                {
                    var payout = payouts[i];

                    if (i > 0 && count > 1 && separatorPrefab != null)
                    {
                        var separator = Instantiate(separatorPrefab, m_AutoGeneratedImageContainer);
                        separator.transform.localScale = Vector3.one;
                        separator.name = kSeparatorGameObjectName;
                    }

                    var payoutItem = Instantiate(payoutItemPrefab, m_AutoGeneratedImageContainer)
                        .GetComponent<ImageInfoView>();
                    payoutItem.transform.localScale = Vector3.one;
                    payoutItem.name = kPayoutItemGameObjectName;
                    payoutItem.SetView(payout.Item1, payout.Item2);
                }

                // To force rebuilt Layouts at Editor and Run time    
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_AutoGeneratedImageContainer.parent);
            }
        }

        /// <summary>
        ///     Clears auto-generated Container and user-provided image
        /// </summary>
        void ClearPromotionImages()
        {
            if (m_AutoGeneratedImageContainer != null)
            {
                var toRemove = new List<Transform>();

                foreach (Transform child in m_AutoGeneratedImageContainer)
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

            if (m_PromoImageField != null)
            {
                m_PromoImageField.sprite = null;
            }
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_TitleTextField);
#endif
            }

            if (m_DescriptionTextField != null && m_DescriptionTextField.text != description)
            {
                m_DescriptionTextField.text = description;
                m_DescriptionTextField.gameObject.SetActive(!string.IsNullOrEmpty(description));
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_DescriptionTextField);
#endif
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
        void SetPurchaseButton(string transactionKey, string priceIconSpritePropertyKey)
        {
            // Purchase Item Button
            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(transactionKey, priceIconSpritePropertyKey);
            }
            else
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - {nameof(PurchaseButton)} is not defined.");
            }
        }

        /// <summary>
        ///     Throws an Invalid Operation Exception if GameFoundation has not been initialized before this view is used.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void ThrowIfNotInitialized()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                throw new InvalidOperationException($"Error: GameFoundation.Initialize() must be called before the {nameof(PromotionPopupView)} is used.");
            }
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
    }
}
