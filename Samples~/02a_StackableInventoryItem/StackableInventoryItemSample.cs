using System;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for stackable inventory items.
    /// </summary>
    public class StackableInventoryItemSample : MonoBehaviour
    {
        /// <summary>
        /// Key used for stackable inventory item definition.
        /// </summary>
        private const string k_HealthPotionKey = "healthPotion";

        /// <summary>
        /// Reference to the health potion stackable inventory item.
        /// </summary>
        private StackableInventoryItem m_HealthPotion;

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// References to the remove buttons to enable/disable when the action is not possible.
        /// </summary>
        public Button increaseQuantityButton;
        public Button decreaseQuantityButton;
        public Button zeroQuantityButton;
	
        /// <summary>
        /// Flag for itemQuantityChanged callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        private bool m_SubscribedFlag = false;
        
        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private void Start()
        {
            // Game Foundation Initialization is being managed by GameFoundationInit Game Object
            if (!GameFoundationSdk.IsInitialized)
            {
                // Disable all buttons while initializing
                increaseQuantityButton.interactable = false;
                decreaseQuantityButton.interactable = false;
                zeroQuantityButton.interactable = false;
            }
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        { 
            // Create a single, stackable Health Potion item.
            var healthPotionDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(k_HealthPotionKey);
            m_HealthPotion = GameFoundationSdk.inventory.CreateItem(healthPotionDefinition) as StackableInventoryItem;

            // Enable add button.  Others will be dynamically enabled/disabled based on quantity.
            increaseQuantityButton.interactable = true;

            // Show list of inventory items and update the button interactability.
            RefreshUI();

            // Ensure that the stackable-inventory-item-changed callback is connected
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
        /// Bind a listener to itemQuantityChanged callbacks on the Inventory Manager.
        /// These callbacks will automatically be invoked anytime a stackable inventory item quantity changes.
        /// This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
        /// </summary>
        private void SubscribeToGameFoundationEvents()
        {
            // If inventory has not yet been initialized the ignore request.
            if (!GameFoundationSdk.IsInitialized)
            {
                return;
            }

            // If app has been disabled then ignore the request
            if (!enabled)
            {
                return;
            }

            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an item quantity is changed.
            // This allows us to refresh the UI as soon as the changes are applied.
            // Note: this ignores repeated requests to add callback if already properly set up.
            if (!m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemQuantityChanged += OnStackableQuantityChanged;
                m_SubscribedFlag = true;
            }
        }

        /// <summary>
        /// Disable InventoryManager callbacks.
        /// </summary>
        private void UnsubscribeFromGameFoundationEvents()
        {
            // If inventory has not yet been initialized the ignore request.
            if (!GameFoundationSdk.IsInitialized)
            {
                return;
            }

            // If callback has been added then remove them.
            // Note: this will ignore repeated requests to remove callbacks when they have not been added.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemQuantityChanged -= OnStackableQuantityChanged;
                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Increases stack quantity of health potions.
        /// </summary>
        public void IncreaseQuantity()
        {
            m_HealthPotion.SetQuantity(m_HealthPotion.quantity + 1);
        }

        /// <summary>
        /// Decreases quantity of stackable health potions.
        /// </summary>
        public void DecreaseQuantity()
        {
            m_HealthPotion.SetQuantity(m_HealthPotion.quantity - 1);
        }

        /// <summary>
        /// Resets quantity of stackable health potions to 0.
        /// </summary>
        public void ZeroQuantity()
        {
            m_HealthPotion.SetQuantity(0);
        }

        /// <summary>
        /// This will fill out the main text box with information about the inventory.
        /// </summary>
        private void RefreshUI()
        {
            // Display title
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");

            // Display health potion stack quantity
            m_DisplayText.AppendLine($"{m_HealthPotion.definition.displayName} quantity: {m_HealthPotion.quantity}");

            mainText.text = m_DisplayText.ToString();

            // Enable/disable remove buttons' interactable flag so all remove buttons disable when quantity is 0
            decreaseQuantityButton.interactable = zeroQuantityButton.interactable = m_HealthPotion.quantity > 0;
        }

        /// <summary>
        /// This will be called every time a stackable inventory item quantity is changed.
        /// </summary>
        /// <param name="quantifiable">
        /// Quantifiable Game Foundation object that changed quantity.
        /// </param>
        private void OnStackableQuantityChanged(IQuantifiable quantifiable, long _)
        {
            if (quantifiable is StackableInventoryItem item &&
                item.definition.key == k_HealthPotionKey)
            {
                RefreshUI();
            }
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
