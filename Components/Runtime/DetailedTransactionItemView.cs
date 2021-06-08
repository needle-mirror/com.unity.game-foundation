#if UNITY_EDITOR
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying a <see cref="BaseTransaction"/>'s icon, display name and
    ///     <see cref="PurchaseButton"/>.
    ///     When attached to a game object, it will display the Transaction Item's icon and displayName and create and display
    ///     a PurchaseButton (<see cref="PurchaseButton"/>) to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Detailed Transaction Item View", 2)]
    [ExecuteInEditMode]
    public class DetailedTransactionItemView : TransactionItemView
    {
        /// <summary>
        ///     The string to prefix the payout <see cref="InventoryItem"/> counts when displaying the transaction's payouts.
        /// </summary>
        public string itemPayoutCountPrefix => m_ItemPayoutCountPrefix;

        [SerializeField]
        internal string m_ItemPayoutCountPrefix = kDefaultCountPrefix;

        /// <summary>
        ///     The string to prefix the payout <see cref="Currency"/> counts when displaying the transaction's payouts.
        /// </summary>
        public string currencyPayoutCountPrefix => m_CurrencyPayoutCountPrefix;

        [SerializeField]
        internal string m_CurrencyPayoutCountPrefix = kDefaultCountPrefix;

        /// <summary>
        ///     The Static Property key for the icon of the Inventory or Currency payout items, as
        ///     specified in their Static Properties.
        /// </summary>
        public string payoutItemIconPropertyKey => m_PayoutItemIconPropertyKey;

        [SerializeField]
        internal string m_PayoutItemIconPropertyKey;

        /// <summary>
        ///     The Transform in which to display the payout items.
        /// </summary>
        public PayoutView payoutItemsContainer => m_PayoutItemsContainer;

        [SerializeField]
        internal PayoutView m_PayoutItemsContainer;

        /// <summary>
        ///     Default count prefix for inventory item and currency.
        /// </summary>
        internal static readonly string kDefaultCountPrefix = "x";

        /// <summary>
        ///     A name to use when generating PayoutItem GameObjects under Payout Items Image Container.
        /// </summary>
        const string kPayoutItemGameObjectName = "PayoutItem";

        /// <summary>
        ///     Specifies whether the payout related fields in the custom editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showPayoutEditorFields = true;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<DetailedTransactionItemView>();

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
        /// <param name="payoutItemIconPropertyKey">
        ///     The Static Property key for the icon that will be displayed on the Payout Items
        /// </param>
        /// <param name="badgeTextPropertyKey">
        ///     The Static Property key for the string that, if defined, will be displayed on the Transaction Item.
        /// </param>
        /// <param name="currencyCountPrefix">
        ///     The string to be prefixed in front of the the count for any Currency payout items on the transaction.
        /// </param>
        /// <param name="inventoryItemCountPrefix">
        ///     The string to be prefixed in front of the the count for any Inventory Item payout items on the transaction.
        /// </param>
        internal void Init(string transactionKey, string itemIconSpritePropertyKey, string priceIconSpritePropertyKey,
            string noPriceString, string payoutItemIconPropertyKey, string badgeTextPropertyKey,
            string currencyCountPrefix, string inventoryItemCountPrefix)
        {
            m_PayoutItemIconPropertyKey = payoutItemIconPropertyKey;
            m_CurrencyPayoutCountPrefix = currencyCountPrefix;
            m_ItemPayoutCountPrefix = inventoryItemCountPrefix;

            base.Init(transactionKey, itemIconSpritePropertyKey, badgeTextPropertyKey, priceIconSpritePropertyKey, noPriceString);
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
        ///     Sets the Transform in which to display the payout items' images on this view.
        /// </summary>
        /// <param name="container">
        ///     The Transform in which to display the payout items' images.
        /// </param>
        public void SetPayoutItemsContainer(PayoutView container)
        {
            if (m_PayoutItemsContainer == container)
            {
                return;
            }

            m_PayoutItemsContainer = container;
            m_IsDirty = true;
        }

        /// <summary>
        ///     To update the item name, item icon, badge text, payout images and PurchaseButton at runtime.
        /// </summary>
        protected override void UpdateContentAtRuntime()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                SetTextContent(string.Empty, string.Empty);
                SetIconSprite(null);
                if (m_PayoutItemsContainer)
                {
                    m_PayoutItemsContainer.gameObject.SetActive(false);
                    m_PayoutItemsContainer.ClearContent();
                }

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
                    k_GFLogger.LogWarning($"\"{m_Transaction.displayName}\" transaction doesn't have Static " +
                                          $"Property called \"{m_BadgeTextPropertyKey}\"");
                }
            }

            SetTextContent(displayName, badgeText);
            if (m_PayoutItemsContainer)
            {
                m_PayoutItemsContainer.Init(m_Transaction, m_PayoutItemIconPropertyKey,
                    m_ItemPayoutCountPrefix, m_CurrencyPayoutCountPrefix);
                m_PayoutItemsContainer.gameObject.SetActive(true);
            }
            InitPurchaseButton();

        }

#if UNITY_EDITOR
        /// <summary>
        ///     To update the item name, item icon, badge text, payout images and PurchaseButton at editor time.
        /// </summary>
        protected override void UpdateContentAtEditor()
        {
            string displayName = null;
            string badgeText = null;
            BaseTransactionAsset transactionAsset = null;

            if (!(m_TransactionKey is null))
            {
                transactionAsset = PrefabTools.GetLookUpCatalogAsset().FindItem(m_TransactionKey) as BaseTransactionAsset;
            }

            if (!(transactionAsset is null))
            {
                displayName = transactionAsset.displayName;
                
                if (!string.IsNullOrEmpty(m_ItemIconSpritePropertyKey))
                {
                    if (transactionAsset.TryGetStaticProperty(m_ItemIconSpritePropertyKey, out var spriteProperty))
                    {
                        PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                    }
                    else if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogWarning($"\"{transactionAsset.displayName.currentValue}\" " +
                                              $"transaction doesn't have Static Property called \"{m_ItemIconSpritePropertyKey}\"");
                    }   
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
                        k_GFLogger.LogWarning($"\"{transactionAsset.displayName.currentValue}\" " +
                                  $"transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                    }
                }
            }

            SetTextContent(displayName, badgeText);
            if (m_PayoutItemsContainer)
            {
                m_PayoutItemsContainer.Init(transactionAsset, m_PayoutItemIconPropertyKey,
                    m_ItemPayoutCountPrefix, m_CurrencyPayoutCountPrefix);
            }

            InitPurchaseButton();
        }
#endif
    }
}
