using System;
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
    [AddComponentMenu("Game Foundation/Currency Hud", 4)]
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
        internal string m_IconSpritePropertyKey = "hud_icon";

        /// <summary>
        ///     The Image component to assign the <see cref="Currency"/> icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        ///     The Text component to assign the <see cref="Currency"/> quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal Text m_QuantityTextField;

        /// <summary>
        ///     A reference to the <see cref="Currency"/> definition.
        /// </summary>
        public Currency currency => m_Currency;

        Currency m_Currency;

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
            GameFoundationSdk.uninitialized += UnregisterEvents;

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
            GameFoundationSdk.wallet.balanceChanged += OnCurrencyChanged;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            GameFoundationSdk.wallet.balanceChanged -= OnCurrencyChanged;
        }

        /// <summary>
        ///     Initializes CurrencyHudView before the first frame update.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
                return;

            ThrowIfNotInitialized(nameof(Start));

            m_Currency = GetCurrency(m_CurrencyKey);
            UpdateContent();
        }

        /// <summary>
        ///     Sets Currency should be displayed by this view.
        /// </summary>
        /// <param name="currency">
        ///     A reference to the Currency that should be displayed.
        /// </param>
        public void SetCurrency(Currency currency)
        {
            ThrowIfNotInitialized(nameof(SetCurrency));

            m_Currency = currency;
            m_CurrencyKey = currency?.key;

            UpdateContent();
        }

        /// <summary>
        ///     Sets Currency should be displayed by this view.
        /// </summary>
        /// <param name="currencyKey">
        ///     The Currency identifier that should be displayed.
        /// </param>
        internal void SetCurrency(string currencyKey)
        {
            m_CurrencyKey = currencyKey;
            m_Currency = GetCurrency(currencyKey);

            UpdateContent();
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
            UpdateIconSprite();
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
            UpdateIconSprite();
        }

        /// <summary>
        ///     Sets the Text component to display the Currency quantity on this view.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the Currency quantity
        /// </param>
        public void SetQuantityTextField(Text text)
        {
            if (m_QuantityTextField == text)
            {
                return;
            }

            m_QuantityTextField = text;
            UpdateQuantity();
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

            UpdateIconSprite();
            UpdateQuantity();
        }

        /// <summary>
        ///     Updates the Currency icon on this view.
        /// </summary>
        void UpdateIconSprite()
        {
            Sprite sprite = null;

            if (!string.IsNullOrEmpty(m_CurrencyKey))
            {
                if (Application.isPlaying)
                {
                    if (m_Currency != null && m_Currency.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
                    {
                        sprite = spriteProperty.AsAsset<Sprite>();
                    }
                }
#if UNITY_EDITOR
                else
                {
                    var currencyAsset = CatalogSettings.catalogAsset.FindItem(m_CurrencyKey) as CurrencyAsset;
                    if (currencyAsset != null && currencyAsset.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
                    {
                        sprite = spriteProperty.AsAsset<Sprite>();
                    }
                }
#endif
            }

            SetIconSprite(sprite);
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
                if (!(icon is null)) m_IconImageField.SetNativeSize();
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_IconImageField);
#endif
            }
        }

        /// <summary>
        ///     Updates the Currency quantity on this view.
        /// </summary>
        void UpdateQuantity()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var currency = GameFoundationSdk.catalog?.Find<Currency>(m_CurrencyKey);
            SetQuantity(currency != null ? GameFoundationSdk.wallet.Get(currency) : 0);
        }

        /// <summary>
        ///     Updates quantity of Currency item in label.
        /// </summary>
        /// <param name="quantity">
        ///     The new quantity to display.
        /// </param>
        void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                k_GFLogger.LogWarning("Item Quantity Text field is not defined.");
                return;
            }

            var quantityText = quantity.ToString();
            if (m_QuantityTextField.text != quantityText)
            {
                m_QuantityTextField.text = quantity.ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_QuantityTextField);
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
                if (currency?.key == m_CurrencyKey)
                {
                    UpdateQuantity();
                }
            }
        }
    }
}
