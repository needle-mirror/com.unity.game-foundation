using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory basics.
    /// </summary>
    public class InventoryBasics : MonoBehaviour
    {
        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Reference to a list of InventoryItems of a certain definition.
        /// </summary>
        private readonly List<InventoryItem> m_ItemsByDefinition = new List<InventoryItem>();
        
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
        public Button addAppleButton;
        public Button addOrangeButton;
        public Button removeAppleButton;
        public Button removeOrangeButton;
        public Button removeAllApplesButton;
        public Button removeAllOrangesButton;

        /// <summary>
        /// All buttons in array for easy access.
        /// </summary>
        private Button[] m_AllButtons;

        /// <summary>
        /// Flag for inventory item changed callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        private bool m_SubscribedFlag = false;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private void Start()
        {
            // Put all buttons into array for easy access to enable/disable as a group
            m_AllButtons = new Button[] {
                addAppleButton,
                addOrangeButton,
                removeAppleButton,
                removeOrangeButton,
                removeAllApplesButton,
                removeAllOrangesButton
            };

            // Game Foundation Initialization is being managed by GameFoundationInit Game Object
            if (!GameFoundationSdk.IsInitialized)
            {
                // Disable all buttons while initializing
                EnableAllButtons(false);
            }
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        { 
            // The Inventory Manager starts with initial allocation of 2 apples and 1 orange, but we 
            // can add 2 additional oranges here to get us started.
            var orange = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("orange");
            GameFoundationSdk.inventory.CreateItem(orange);
            GameFoundationSdk.inventory.CreateItem(orange);

            // Enable all buttons.  Note: some may later be disabled in RefreshUI,
            // but this ensures static buttons are all enabled.
            EnableAllButtons();

            // Show list of inventory items and update the button interactability.
            RefreshUI();

            // Ensure that the inventory item changed callback is connected
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
        /// Enable InventoryManager callbacks for our UI refresh method.
        /// These callbacks will automatically be invoked anytime an inventory item is added, or removed.
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
            // These callbacks will automatically be invoked anytime an inventory item is added, or removed.
            // This allows us to refresh the UI as soon as the changes are applied.
            // Note: this ignores repeated requests to add callback if already properly set up.
            if (!m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded += OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted += OnInventoryItemChanged;

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

            // If callbacks have been added then remove them.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded -= OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted -= OnInventoryItemChanged;

                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Adds a single fruit to the main inventory.
        /// </summary>
        public void AddFruit(string fruitItemDefinitionKey)
        {
            try
            {
                // This will create a new item inside the GameFoundationSdk.inventory.
                // Because this method will throw an exception if the definition is not found in the inventory catalog,
                // we'll surround in a try catch and log any exceptions thrown.
                var fruitDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(fruitItemDefinitionKey);
                GameFoundationSdk.inventory.CreateItem(fruitDefinition);
            }
            catch (Exception exception)
            {
                OnGameFoundationException(exception);
            }
        }

        /// <summary>
        /// Removes a single fruit from the main inventory.
        /// </summary>
        public void RemoveFruit(string fruitItemDefinitionId)
        {
            // Find the definition for the specified key (i.e. "orange", "apple", etc).
            var fruitDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(fruitItemDefinitionId);

            // To remove a single item from the InventoryManager, you need all instances of that item. 
            // Since we know the InventoryItemDefinition of the items we want to remove, we'll first 
            // look for all items with that definition.  We'll use the version of FindItems that lets us pass
            // in a collection to be filled to reduce allocations.
            var count = GameFoundationSdk.inventory.FindItems(fruitDefinition, m_ItemsByDefinition);
            
            // If there is at least 1 item to remove
            if (count > 0)
            {
                // We'll remove the first instance in the list of items
                GameFoundationSdk.inventory.Delete(m_ItemsByDefinition[0]);

                // Once we remove the item from the InventoryManager, update our internal list to match.
                m_ItemsByDefinition.RemoveAt(0);
            }
        }

        /// <summary>
        /// Removes all instances of specified fruit from the Inventory.
        /// </summary>
        public void RemoveAllFruitType(string fruitDefinitionKey)
        {
            // Find the definition for the specified key (i.e. "orange", "apple", etc).
            var fruitDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(fruitDefinitionKey);

            // This method can be called whether or not there are any items with this definition id in the GameFoundationSdk.inventory.
            // If there are no items with that definition, the method will take no action and return a count of 0;
            int itemsRemovedCount = GameFoundationSdk.inventory.Delete(fruitDefinition);
            
            // Display count deleted as returned by the inventory.Delete call (above).
            Debug.Log($"{itemsRemovedCount} {fruitDefinitionKey} item(s) removed from inventory.");
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            // Generate new inventory list.
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");
            
            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string displayName = inventoryItem.definition.displayName;

                m_DisplayText.AppendLine(displayName);
            }

            // Show the newly-generated list
            mainText.text = m_DisplayText.ToString();

            // Update the buttons based on current inventory state.
            RefreshButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The AddButtons will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshButtons()
        {
            var apple = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("apple");
            var orange = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("orange");

            // FindItems will return a count of the number of items found no matter if you pass in a list or null
            // Since we don't actually care about the list, only the account, we'll save the allocation and just pass null
            var appleCount = GameFoundationSdk.inventory.FindItems(apple);
            var orangeCount = GameFoundationSdk.inventory.FindItems(orange);

            removeAppleButton.interactable = removeAllApplesButton.interactable = appleCount > 0;
            removeOrangeButton.interactable = removeAllOrangesButton.interactable = orangeCount > 0;
        }

        /// <summary>
        /// Listener for changes in GameFoundationSdk.inventory. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            RefreshUI();
        }

        /// <summary>
        /// Enable/disable all buttons.
        /// </summary>
        /// <param name="enableFlag">
        /// Flag (defaults to 'true') used to determine if all buttons should be enabled or disabled.
        /// </param>
        private void EnableAllButtons(bool enableFlag = true)
        {
            if (m_AllButtons is null)
                return;

            foreach (var button in m_AllButtons)
            {
                button.interactable = enableFlag;
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
