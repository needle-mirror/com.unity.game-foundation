using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying a Currency's icon and quantity.
    ///     When attached to a game object, it will display the Currency's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Currency HUD View", 4)]
    [ExecuteInEditMode]
    public class CurrencyHudView : MonoBehaviour
    {
        /// <summary>
        ///     The identifier of the <see cref="Currency"/> to display.
        /// </summary>
        public string currencyKey => m_CurrencyKey;

        [SerializeField]
        internal string m_CurrencyKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the sprite of the <see cref="Currency"/>
        ///     for displaying in the hud view.
        /// </summary>
        public string iconSpritePropertyKey => m_IconSpritePropertyKey;

        [SerializeField]
        internal string m_IconSpritePropertyKey;

        /// <summary>
        ///     The Image component to assign the <see cref="Currency"/> icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        ///     The TextMeshProUGUI component to assign the <see cref="Currency"/> quantity to.
        /// </summary>
        public TextMeshProUGUI quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal TextMeshProUGUI m_QuantityTextField;

        /// <summary>
        ///     A reference to the <see cref="Currency"/> definition.
        /// </summary>
        public Currency currency => m_Currency;

        Currency m_Currency;

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
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<CurrencyHudView>();

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

            if (!(m_Currency is null))
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
            if (GameFoundationSdk.wallet == null)
                return;

            GameFoundationSdk.wallet.balanceChanged += OnCurrencyChanged;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            if (GameFoundationSdk.wallet == null)
                return;

            GameFoundationSdk.wallet.balanceChanged -= OnCurrencyChanged;
        }

        /// <summary>
        ///     Initializes CurrencyHudView before the first frame update if Game Foundation Sdk was already initialized
        ///     before CurrencyHudView was enabled, otherwise sets content to a blank state in order to wait for
        ///     Game Foundation Sdk to initialize.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
                return;

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Currency is null)
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
        ///     Initializes CurrencyHudView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Currency that should be displayed by this view.
        /// </summary>
        /// <param name="currencyKey">
        ///     The Currency identifier that should be displayed.
        /// </param>
        void SetCurrency(string currencyKey)
        {
            m_CurrencyKey = currencyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Finds Currency definition in the Currency catalog.
        /// </summary>
        /// <param name="currencyKey">
        ///     The definition key of Currency.
        /// </param>
        /// <returns>
        ///     A reference to Currency definition.
        /// </returns>
        Currency GetCurrency(string currencyKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(currencyKey))
                return null;

            var currency = GameFoundationSdk.catalog?.Find<Currency>(currencyKey);
            if (currency != null || !m_ShowDebugLogs) return currency;

            k_GFLogger.LogWarning($"Currency \"{currencyKey}\" doesn't exist in Currency catalog.");
            return null;
        }

        /// <summary>
        ///     Sets the Currency that should be displayed by this view. Runtime only.
        /// </summary>
        /// <param name="currency">
        ///     A reference to the Currency that should be displayed.
        /// </param>
        public void SetCurrency(Currency currency)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetCurrency)))
            {
                return;
            }

            m_Currency = currency;
            m_CurrencyKey = currency?.key;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets sprite name for Currency icon that will be displayed on this view.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up sprite by.
        ///     Currency doesn't have this key in its Static Properties will not display an icon.
        /// </param>
        public void SetIconSpritePropertyKey(string propertyKey)
        {
            if (m_IconSpritePropertyKey == propertyKey)
            {
                return;
            }

            m_IconSpritePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the Image component to display Currency icon sprite on this view.
        /// </summary>
        /// <param name="image">
        ///     The Image component to display Currency icon sprite.
        /// </param>
        public void SetIconImageField(Image image)
        {
            if (m_IconImageField == image)
            {
                return;
            }

            m_IconImageField = image;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the TextMeshProUGUI component to display the Currency quantity on this view.
        /// </summary>
        /// <param name="text">
        ///     The TextMeshProUGUI component to display the Currency quantity
        /// </param>
        public void SetQuantityTextField(TextMeshProUGUI text)
        {
            if (m_QuantityTextField == text)
            {
                return;
            }

            m_QuantityTextField = text;
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
                if (GameFoundationSdk.IsInitialized &&
                    (m_Currency is null && !string.IsNullOrEmpty(m_CurrencyKey) || 
                    !(m_Currency is null) && m_Currency.key != m_CurrencyKey))
                {
                    m_Currency = GetCurrency(m_CurrencyKey);
                }

                UpdateContent();
            }
        }

        /// <summary>
        ///     Updates the Currency icon and quantity on this view.
        /// </summary>
        internal void UpdateContent()
        {
#if UNITY_EDITOR
            // To avoid updating the content the prefab selected in the Project window
            if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }
#endif
            
            if (Application.isPlaying && !GameFoundationSdk.IsInitialized)
            {
                SetIconSprite(null);
                SetQuantity(null);
                return;
            }

            LoadAndSetIconSprite();
            UpdateQuantity();
        }

        /// <summary>
        ///     Updates the Currency icon on this view.
        /// </summary>
        void LoadAndSetIconSprite()
        {
            if (!string.IsNullOrEmpty(m_CurrencyKey))
            {
                if (Application.isPlaying)
                {
                    if (m_Currency != null && m_Currency.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
                    {
                        PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                    }
                    else
                    {
                        SetIconSprite(null);
                    }
                }
#if UNITY_EDITOR
                else
                {
                    var currencyAsset = PrefabTools.GetLookUpCatalogAsset().FindItem(m_CurrencyKey) as CurrencyAsset;
                    if (currencyAsset != null && currencyAsset.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
                    {
                        PrefabTools.LoadSprite(spriteProperty, SetIconSprite, OnSpriteLoadFailed);
                    }
                    else
                    {
                        SetIconSprite(null);
                    }
                }
#endif
            }
        }

        /// <summary>
        ///     Sets sprite of item in display.
        /// </summary>
        /// <param name="icon">
        ///     The new sprite to display.
        /// </param>
        void SetIconSprite(Sprite icon)
        {
            if (m_IconImageField == null)
            {
                k_GFLogger.LogWarning("Icon Image field is not defined.");
                return;
            }

            if (m_IconImageField.sprite != icon)
            {
                m_IconImageField.sprite = icon;
                m_IconImageField.preserveAspect = true;
            }
        }

        /// <summary>
        ///     Updates the Currency quantity on this view. Runtime only.
        /// </summary>
        void UpdateQuantity()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var quantity = m_Currency != null ? GameFoundationSdk.wallet.Get(m_Currency) : 0;
            SetQuantity(quantity.ToString());
        }

        /// <summary>
        ///     Updates quantity of Currency item in label.
        /// </summary>
        /// <param name="quantity">
        ///     The new quantity to display.
        /// </param>
        void SetQuantity(string quantity)
        {
            if (m_QuantityTextField == null)
            {
                k_GFLogger.LogWarning("Item Quantity Text field is not defined.");
                return;
            }

            if (m_QuantityTextField.text != quantity)
            {
                m_QuantityTextField.text = quantity;
            }
        }

        /// <summary>
        ///     Listens to updates from the Wallet that contains the item being displayed.
        ///     If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        /// <param name="quantifiable">
        ///     Quantifiable object (<see cref="Currency"/>) effected.
        /// </param>
        void OnCurrencyChanged(IQuantifiable quantifiable, long _)
        {
            if (quantifiable is Currency currency)
            {
                if (currency.key == m_CurrencyKey)
                {
                    UpdateQuantity();
                }
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
        ///     When changes are made via the Inspector, trigger <see cref="UpdateContent"/>
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
