using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying a Inventory Item's icon and quantity.
    ///     When attached to a game object, it will display the Inventory Item's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Inventory Item HUD View", 5)]
    [ExecuteInEditMode]
    public class InventoryItemHudView : MonoBehaviour
    {
        /// <summary>
        ///     The identifier of the <see cref="InventoryItem"/> to display.
        /// </summary>
        public string itemDefinitionKey => m_ItemDefinitionKey;

        [SerializeField]
        internal string m_ItemDefinitionKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the sprite of the Inventory Item
        ///     for displaying in the hud view.
        /// </summary>
        public string iconSpritePropertyKey => m_IconSpritePropertyKey;

        [SerializeField]
        internal string m_IconSpritePropertyKey;

        /// <summary>
        ///     The Image component to assign the <see cref="InventoryItem"/> icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        ///     The TextMeshProUGUI component to assign the <see cref="InventoryItem"/> quantity to.
        /// </summary>
        public TextMeshProUGUI quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal TextMeshProUGUI m_QuantityTextField;

        /// <summary>
        ///     A reference to the <see cref="InventoryItemDefinition"/> definition.
        /// </summary>
        public InventoryItemDefinition itemDefinition => m_ItemDefinition;

        InventoryItemDefinition m_ItemDefinition;

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
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<InventoryItemHudView>();

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

            if (!(m_ItemDefinition is null))
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
            if (GameFoundationSdk.inventory == null)
                return;

            GameFoundationSdk.inventory.itemAdded += OnItemChanged;
            GameFoundationSdk.inventory.itemDeleted += OnItemChanged;
            GameFoundationSdk.inventory.itemQuantityChanged += OnItemQuantityChanged;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            if (GameFoundationSdk.inventory == null)
                return;

            GameFoundationSdk.inventory.itemAdded -= OnItemChanged;
            GameFoundationSdk.inventory.itemDeleted -= OnItemChanged;
            GameFoundationSdk.inventory.itemQuantityChanged -= OnItemQuantityChanged;
        }

        /// <summary>
        ///     Initializes InventoryItemHudView before the first frame update if Game Foundation Sdk was already
        ///     initialized before CurrencyHudView was enabled, otherwise sets content to a blank state in order to
        ///     wait for Game Foundation Sdk to initialize.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
                return;

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_ItemDefinition is null)
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
        ///     Initializes InventoryItemHudView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            m_ItemDefinition = GetItemDefinition(m_ItemDefinitionKey);
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets Inventory Item should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinitionKey">
        ///     The InventoryItem key that should be displayed.
        /// </param>
        void SetItemDefinition(string itemDefinitionKey)
        {
            m_ItemDefinitionKey = itemDefinitionKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Finds Inventory Item definition in the Inventory catalog.
        /// </summary>
        /// <param name="itemDefinitionKey">
        ///     The definition key of Inventory.
        /// </param>
        /// <returns>
        ///     A reference to Inventory definition.
        /// </returns>
        InventoryItemDefinition GetItemDefinition(string itemDefinitionKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(itemDefinitionKey))
                return null;

            var inventoryItemDefinition = GameFoundationSdk.catalog?.Find<InventoryItemDefinition>(itemDefinitionKey);
            if (inventoryItemDefinition != null || !m_ShowDebugLogs) return inventoryItemDefinition;

            k_GFLogger.LogWarning($"InventoryItemDefinition \"{itemDefinitionKey}\" doesn't exist in Inventory catalog.");
            return null;
        }

        /// <summary>
        ///     Sets Inventory Item should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinition">
        ///     The Inventory Item definition that should be displayed.
        /// </param>
        public void SetItemDefinition(InventoryItemDefinition itemDefinition)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetItemDefinition)))
            {
                return;
            }

            m_ItemDefinition = itemDefinition;
            m_ItemDefinitionKey = itemDefinition?.key;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the property key to use when getting the sprite of the Inventory Item icon.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up sprite by.
        ///     Inventory Item doesn't have this key in its Static Properties will not display an icon.
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
        ///     Sets the Image component to display Inventory item icon sprite on this view.
        /// </summary>
        /// <param name="image">
        ///     The Image component to display Inventory item icon sprite.
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
        ///     Sets the TextMeshProUGUI component to display the Inventory item quantity on this view.
        /// </summary>
        /// <param name="text">
        ///     The TextMeshProUGUI component to display the Inventory item quantity
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
        ///     At runtime, also assigns the appropriate value for <see cref="m_ItemDefinition"/> from the Catalog if needed.
        ///     If m_ItemDefinition and m_ItemDefinitionKey don't currently match, this replaces m_ItemDefinition with the
        ///     correct transaction by searching the Catalog for m_ItemDefinitionKey.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;
                if (GameFoundationSdk.IsInitialized &&
                    (m_ItemDefinition is null && !string.IsNullOrEmpty(m_ItemDefinitionKey) ||
                     !(m_ItemDefinition is null) && m_ItemDefinition.key != m_ItemDefinitionKey))
                {
                    m_ItemDefinition = GetItemDefinition(m_ItemDefinitionKey);
                }

                UpdateContent();
            }
        }

        /// <summary>
        ///     Updates the Inventory item icon and quantity on this view.
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
        ///     Updates the Inventory item icon on this view.
        /// </summary>
        void LoadAndSetIconSprite()
        {
            if (!string.IsNullOrEmpty(m_ItemDefinitionKey))
            {
                if (Application.isPlaying)
                {
                    if (m_ItemDefinition != null && m_ItemDefinition.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
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
                    var itemAsset = PrefabTools.GetLookUpCatalogAsset().FindItem(m_ItemDefinitionKey) as InventoryItemDefinitionAsset;
                    if (!(itemAsset is null) && itemAsset.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
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
            if (m_IconImageField is null)
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
        ///     Updates the Inventory item quantity on this view.
        /// </summary>
        void UpdateQuantity()
        {
            if (!Application.isPlaying || m_ItemDefinition == null) return;

            SetQuantity(GetQuantity().ToString());
        }

        /// <summary>
        ///     Find all items that have the same definition key to calculate the quantity.
        /// </summary>
        /// <returns>
        ///     The quantity of the item
        /// </returns>
        long GetQuantity()
        {
            if (!Application.isPlaying || m_ItemDefinition == null) return 0;

            return GameFoundationSdk.inventory.GetTotalQuantity(itemDefinition);
        }

        /// <summary>
        ///     Updates quantity of Inventory item in label.
        /// </summary>
        /// <param name="quantity">
        ///     The new quantity to display.
        /// </param>
        void SetQuantity(string quantity)
        {
            if (m_QuantityTextField is null)
            {
                k_GFLogger.LogWarning("Item Quantity Text field is not defined.");
                return;
            }

            if (m_QuantityTextField.text == quantity)
                return;

            m_QuantityTextField.text = quantity;
        }

        /// <summary>
        ///     Listens to updates from the inventory that contains the item being displayed.
        ///     If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        void OnItemChanged(InventoryItem inventoryItem)
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionKey) || inventoryItem == null)
            {
                return;
            }
            
            if (m_ItemDefinitionKey == inventoryItem.definition.key)
            {
                m_IsDirty = true;
            }
        }
        
        /// <summary>
        ///     Listens to updates from the inventory that contains the item being displayed.
        ///     If the item's quantity that has changed is the one being displayed it updates the quantity.
        /// </summary>
        void OnItemQuantityChanged(IQuantifiable quantifiable, long quantity)
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionKey))
            {
                return;
            }
            
            if (quantifiable is StackableInventoryItem item)
            {
                if (item.definition.key == m_ItemDefinitionKey)
                {
                    m_IsDirty = true;
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
        ///     When changes are made in the Inspector, set <see cref="m_IsDirty"/> to true
        ///     in order to trigger <see cref="UpdateContent"/>
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
