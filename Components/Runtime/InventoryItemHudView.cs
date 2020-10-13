using System;
using System.Collections.Generic;
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
    [AddComponentMenu("Game Foundation/Inventory Item Hud", 5)]
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
        internal string m_IconSpritePropertyKey = "hud_icon";

        /// <summary>
        ///     The Image component to assign the <see cref="InventoryItem"/> icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        ///     The Text component to assign the <see cref="InventoryItem"/> quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal Text m_QuantityTextField;

        /// <summary>
        ///     The List of Inventory Items <see cref="InventoryItem"/> that has the same definition key to cache them.
        /// </summary>
        readonly List<InventoryItem> m_TempItemList = new List<InventoryItem>();

        /// <summary>
        ///     The Quantity of the <see cref="InventoryItem"/>.
        /// </summary>
        long m_Quantity;

        /// <summary>
        ///     A reference to the <see cref="InventoryItemDefinition"/> definition.
        /// </summary>
        public InventoryItemDefinition itemDefinition => m_ItemDefinition;

        InventoryItemDefinition m_ItemDefinition;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

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

            if (!ReferenceEquals(m_ItemDefinition, null))
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
            GameFoundationSdk.inventory.itemAdded += OnItemChanged;
            GameFoundationSdk.inventory.itemDeleted += OnItemChanged;
            GameFoundationSdk.inventory.itemQuantityChanged += OnItemQuantityChanged;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            GameFoundationSdk.inventory.itemAdded -= OnItemChanged;
            GameFoundationSdk.inventory.itemDeleted -= OnItemChanged;
            GameFoundationSdk.inventory.itemQuantityChanged -= OnItemQuantityChanged;
        }

        /// <summary>
        ///     Initializes InventoryItemHudView before the first frame update.
        /// </summary>
        void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();

                m_ItemDefinition = GetItemDefinition(m_ItemDefinitionKey);
                UpdateContent();
            }
        }

        /// <summary>
        ///     Sets Inventory Item should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinition">
        ///     The Inventory Item definition that should be displayed.
        /// </param>
        public void SetItemDefinition(InventoryItemDefinition itemDefinition)
        {
            ThrowIfNotInitialized();

            m_ItemDefinition = itemDefinition;
            m_ItemDefinitionKey = itemDefinition?.key;

            UpdateContent();
        }

        /// <summary>
        ///     Sets Inventory Item should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinitionKey">
        ///     The InventoryItem key that should be displayed.
        /// </param>
        internal void SetItemDefinition(string itemDefinitionKey)
        {
            m_ItemDefinition = GetItemDefinition(itemDefinitionKey);
            m_ItemDefinitionKey = itemDefinitionKey;

            UpdateContent();
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

            Debug.LogWarning($"{nameof(InventoryItemHudView)} - InventoryItemDefinition \"{itemDefinitionKey}\" doesn't exist in Inventory catalog.");
            return null;
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
            UpdateIconSprite();
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
            UpdateIconSprite();
        }

        /// <summary>
        ///     Sets the Text component to display the Inventory item quantity on this view.
        /// </summary>
        /// <param name="text">
        ///     The Text component to display the Inventory item quantity
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

            UpdateIconSprite();
            UpdateQuantity();
        }

        /// <summary>
        ///     Updates the Inventory item icon on this view.
        /// </summary>
        void UpdateIconSprite()
        {
            Sprite sprite = null;

            if (!string.IsNullOrEmpty(m_ItemDefinitionKey))
            {
                if (Application.isPlaying)
                {
                    if (m_ItemDefinition != null && m_ItemDefinition.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
                    {
                        sprite = spriteProperty.AsAsset<Sprite>();
                    }
                }
#if UNITY_EDITOR
                else
                {
                    var itemAsset = CatalogSettings.catalogAsset.FindItem(m_ItemDefinitionKey) as InventoryItemDefinitionAsset;
                    if (!ReferenceEquals(itemAsset, null) && itemAsset.TryGetStaticProperty(m_IconSpritePropertyKey, out var spriteProperty))
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
                Debug.LogWarning($"{nameof(CurrencyHudView)} - Icon Image field is not defined.");
                return;
            }

            if (m_IconImageField.sprite != icon)
            {
                m_IconImageField.sprite = icon;
                if (!ReferenceEquals(icon, null)) m_IconImageField.SetNativeSize();
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_IconImageField);
#endif
            }
        }

        /// <summary>
        ///     Updates the Inventory item quantity on this view.
        /// </summary>
        void UpdateQuantity()
        {
            SetQuantity(GetQuantity());
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
        void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                Debug.LogWarning($"{nameof(InventoryItemHudView)} - Item Quantity Text field is not defined.");
                return;
            }

            if (m_Quantity == quantity) return;

            m_Quantity = quantity;
            m_QuantityTextField.text = quantity.ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_QuantityTextField);
#endif
        }

        /// <summary>
        ///     Throws an Invalid Operation Exception if GameFoundation has not been initialized before this view is used.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void ThrowIfNotInitialized()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                throw new InvalidOperationException($"Error: GameFoundation.Initialize() must be called before the {nameof(InventoryItemHudView)} is used.");
            }
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
                UpdateContent();
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
                    UpdateContent();
                }
            }
        }
    }
}
