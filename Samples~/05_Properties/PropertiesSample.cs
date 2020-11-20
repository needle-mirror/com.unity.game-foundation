using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the Properties system.
    /// </summary>
    public class PropertiesSample : MonoBehaviour
    {
        /// <summary>
        /// Flag to refresh the UI at the next frame.
        /// </summary>
        private bool m_MustRefreshUI;

        /// <summary>
        /// References for easy access.
        /// </summary>
        private InventoryItem m_Sword;
        private InventoryItem m_HealthPotion;

        /// <summary>
        /// Variable to track player health value.
        /// </summary>
        private float m_PlayerHealth = 100;

        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// References to the specific buy/sell buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button takeDamageButton;
        public Button healButton;

        /// <summary>
        /// Flag for item-changed callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        private bool m_SubscribedFlag = false;

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
            // For this sample, we're focusing on swords and health, so let's remove all others from the Inventory.
            // Note: this is helpful since we have an initial allocation of 2 apples and 1 orange.
            GameFoundationSdk.inventory.DeleteAllItems();

            var sword = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("sword");
            var healthPotion = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("healthPotion");

            // We will create the sword and health potion inventoryItems in the InventoryManager and
            // store their references to get us started.
            m_Sword = GameFoundationSdk.inventory.CreateItem(sword);
            m_HealthPotion = GameFoundationSdk.inventory.CreateItem(healthPotion);

            // Show list of inventory items and update the button interactability.
            RefreshUI();

            // Ensure that the stackable-inventory-item-changed callback is connected.
            SubscribeToGameFoundationEvents();
        }

        /// <summary>
        /// Called when this object becomes enabled to ensure our callbacks are active, if Game Foundation
        /// has already completed initialization (otherwise they will be enabled at end of initialization).
        /// </summary>
        private void OnEnable()
        {
            SubscribeToGameFoundationEvents();
        }

        /// <summary>
        /// Disable InventoryManager callbacks if Game Foundation has been initialized and callbacks were added.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeFromGameFoundationEvents();
        }

        /// <summary>
        /// Bind a listener to item-changed callbacks on the Inventory Manager.
        /// These callbacks will automatically be invoked anytime a stackable inventory item quantity changes.
        /// This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
        /// </summary>
        private void SubscribeToGameFoundationEvents()
        {
            // If inventory has not yet been initialized the ignore request.
            if (GameFoundationSdk.inventory is null || m_Sword is null || m_HealthPotion is null)
            {
                return;
            }

            // If app has been disabled then ignore the request
            if (!enabled)
            {
                return;
            }

            // Here we bind our UI refresh method to callbacks on the inventory manager and mutable property changed.
            // These callbacks will automatically be invoked anytime an inventory item is added, or removed.
            // This allows us to refresh the UI as soon as the changes are applied.
            if (!m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded += OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted += OnInventoryItemChanged;

                // These events will automatically be invoked when sword's or potion's properties are changed.
                m_Sword.mutablePropertyChanged += OnItemPropertyChanged;
                m_HealthPotion.mutablePropertyChanged += OnItemPropertyChanged;

                m_SubscribedFlag = true;
            }
        }

        /// <summary>
        /// Removes the above-added callbacks for when app is disabled or destroyed.
        /// </summary>
        private void UnsubscribeFromGameFoundationEvents()
        {
            // If inventory has not yet been initialized the ignore request.
            if (GameFoundationSdk.inventory is null || m_Sword is null || m_HealthPotion is null)
            {
                return;
            }

            // If callbacks have been added then remove them.
            // Note: this will ignore repeated requests to remove callbacks when they have not been added.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded -= OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted -= OnInventoryItemChanged;
                m_Sword.mutablePropertyChanged -= OnItemPropertyChanged;
                m_HealthPotion.mutablePropertyChanged -= OnItemPropertyChanged;

                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_MustRefreshUI)
            {
                RefreshUI();
                m_MustRefreshUI = false;
            }
        }

        /// <summary>
        /// Apply the sword's damage value to the player's health.
        /// </summary>
        public void TakeDamage()
        {
            // Get the damage value of the sword by checking the damage mutable property attached to the item.
            // Even though we're pretty sure our sword has a damage property attached,
            // we use TryGetMutableProperty to prevent crashes in case we made a typo or
            // if the catalogs are not set up as expected.
            float damage = 0;
            if (m_Sword.TryGetMutableProperty("damage", out var damageProperty))
            {
                // We explicitly use AsFloat here to get the float stored in the mutable property
                // but we would have the same result if we directly assigned the property to the float variable. 
                damage = damageProperty.AsFloat();
            }

            // Get the quantity of swords available by checking the quantity mutable property attached to the item.
            // We could use HasMutableProperty and GetMutableProperty to check the property's existence then retrieve its value
            // but it is faster to use TryGetMutableProperty since it does both in a single call.
            var quantity = 0;
            if (m_Sword.TryGetMutableProperty("quantity", out var quantityProperty))
            {
                quantity = quantityProperty;
            }

            if (quantity <= 0 || damage <= 0 || m_PlayerHealth < damage)
            {
                return;
            }

            // Apply the damage to playerHealth
            m_PlayerHealth -= damage;

            // If the sword doesn't have a durability mutable property, no action needs to be taken.
            // If it does have a durability mutable property, we will lower the sword's durability,
            // if it drops to 0, a single sword has been used.
            if (m_Sword.TryGetMutableProperty("durability", out var durabilityProperty))
            {
                var durability = durabilityProperty.AsInt();
                if (durability == 1)
                {
                    // If there is only one quantity of sword left, remove the sword item from the Inventory,
                    // otherwise, reduce the quantity in the property by one and reset the durability property.
                    if (quantity == 1)
                    {
                        GameFoundationSdk.inventory.Delete(m_Sword);

                        // Once we remove the m_Sword item from the InventoryManager, the reference is no longer useful.
                        m_Sword = null;
                    }
                    else
                    {
                        // We use AdjustProperty to apply the change to the current property's value.
                        m_Sword.AdjustMutableProperty("quantity", -1);

                        m_Sword.SetMutableProperty("durability", 3);
                    }
                }
                else
                {
                    m_Sword.AdjustMutableProperty("durability", -1);
                }
            }

            RefreshUI();
        }

        /// <summary>
        /// Increases the player's health by the health restore property of a health potion, then removes it.
        /// This only happens if there is at least one health potion in the inventory, and if the player's health is not maxed out.
        /// </summary>
        public void Heal()
        {
            // Get the quantity of health potions available by checking the quantity mutable property attached to the item.
            var quantity = 0;
            if (m_HealthPotion.TryGetMutableProperty("quantity", out var quantityProperty))
            {
                quantity = quantityProperty;
            }

            if (quantity <= 0 || m_PlayerHealth >= 100)
            {
                return;
            }

            // We'll confirm that Health Potion has the healthRestore property on it before accessing to prevent exceptions
            // and only reduce the quantity of the health potion if it can in fact restore health to the player.
            if (m_HealthPotion.TryGetMutableProperty("healthRestore", out var healthRestoreProperty))
            {
                // Mathf.Min expects a float value but "healthRestore" is an integer property.
                // This isn't a problem, we can cast it to a floating value by either doing
                // an explicit cast or using the AsFloat method.
                var health = Mathf.Min(healthRestoreProperty.AsFloat() + m_PlayerHealth, 100f);

                m_PlayerHealth = health;

                // If there is only one quantity of health potion left,
                // remove the health potion item from the Inventory;
                // otherwise, reduce the quantity in the property by one.
                if (quantity == 1)
                {
                    GameFoundationSdk.inventory.Delete(m_HealthPotion);

                    // Once we remove the m_HealthPotion item from the InventoryManager, the reference is no longer useful.
                    m_HealthPotion = null;
                }
                else
                {
                    m_HealthPotion.AdjustMutableProperty("quantity", -1);
                }

                // Remember, we don't need to set the refresh UI flag here since it will
                // be set by OnInventoryItemChanged as soon as the changes are applied.
            }
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();

            // To save allocations we will reuse our m_InventoryItems list each time.
            // The GetItems method will clear the list passed in.
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (var inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                var itemName = inventoryItem.definition.displayName;

                // We'll initialize our quantity value to 1,
                // that way if an item doesn't have a quantity mutable property,
                // we still represent the 1 item in the GameFoundationSdk.inventory.
                // Depending on your implementation, you may instead want to initialize to 0.
                var quantity = 1;
                if (inventoryItem.TryGetMutableProperty("quantity", out var quantityProperty))
                {
                    quantity = quantityProperty;
                }

                m_DisplayText.AppendLine($"<b>{itemName}</b>: {quantity}");

                // For items with health restore, durability, or damage mutable properties, we want to display their values here.
                if (inventoryItem.TryGetMutableProperty("healthRestore", out var healthRestoreProperty))
                {
                    m_DisplayText.AppendLine($"- Health Restore: {healthRestoreProperty}");
                }

                if (inventoryItem.TryGetMutableProperty("damage", out var damageProperty))
                {
                    m_DisplayText.AppendLine($"- Damage: {damageProperty}");
                }

                if (inventoryItem.TryGetMutableProperty("durability", out var durabilityProperty) && quantity > 0)
                {
                    m_DisplayText.AppendLine($"- Durability: {durabilityProperty}");
                }

                m_DisplayText.AppendLine();
            }

            // Show the player's health
            m_DisplayText.AppendLine();
            m_DisplayText.AppendLine($"<b>Health:</b> {m_PlayerHealth}");


            mainText.text = m_DisplayText.ToString();

            RefreshDamageAndHealButtons();
        }

        /// <summary>
        /// This method will turn the heal/damage buttons on/off if the conditions for their functionality are met. 
        /// </summary>
        private void RefreshDamageAndHealButtons()
        {
            // When initializing variables that will store Properties of type float using var,
            // the initial value needs to specify that it is a float.
            var swordDamage = 0f;
            var swordQuantity = 0;

            // First we'll safely get our items' mutable properties that we need to complete our condition checks.
            if (m_Sword != null)
            {
                if (m_Sword.TryGetMutableProperty("damage", out var damageProperty))
                {
                    swordDamage = damageProperty;
                }

                if (m_Sword.TryGetMutableProperty("quantity", out var swordQuantityProperty))
                {
                    swordQuantity = swordQuantityProperty;
                }
            }

            var potionQuantity = 0;
            if (m_HealthPotion != null && m_HealthPotion.TryGetMutableProperty("quantity", out var potionQuantityProperty))
            {
                potionQuantity = potionQuantityProperty;
            }

            takeDamageButton.interactable = swordQuantity > 0 && swordDamage > 0 && m_PlayerHealth >= swordDamage;
            healButton.interactable = potionQuantity > 0 && m_PlayerHealth < 100;
        }

        /// <summary>
        /// Listener for changes in GameFoundationSdk.inventory.
        /// Will get called whenever an item is added or removed.
        /// </summary>
        /// <param name="itemChanged">
        /// This parameter will not be used, but must exist to be bound to
        /// <see cref="GameFoundationSdk.inventory.itemAdded"/> and <see cref="GameFoundationSdk.inventory.itemDeleted"/>.
        /// </param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_MustRefreshUI = true;
        }

        /// <summary>
        /// Listener for changes on an item's properties.
        /// Will get called whenever Sword's properties or Potion's properties are changed.
        /// </summary>
        /// <param name="args">
        /// This parameter will not be used, but must exist to be bound to
        /// <see cref="InventoryItem.propertyChanged"/>.
        /// </param>
        private void OnItemPropertyChanged(PropertyChangedEventArgs args)
        {
            m_MustRefreshUI = true;
        }

        /// <summary>
        /// If GameFoundation throws exception, log the error to console.
        /// </summary>
        /// <param name="exception">
        /// Exception thrown by GameFoundation.
        /// </param>
        public void OnGameFoundationException(Exception exception)
        {
            Debug.LogError($"GameFoundation exception: {exception}");
        }
    }
}
