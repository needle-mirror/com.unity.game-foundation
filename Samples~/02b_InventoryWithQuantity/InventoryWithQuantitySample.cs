using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory basics.
    /// </summary>
    public class InventoryWithQuantitySample : MonoBehaviour
    {
        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// We will keep an aggregated list of Inventory Items that are in InventoryManager
        /// based on their InventoryItemDefinition ids so that we can display each item with a quantity
        /// rather than each item individually.
        /// </summary>
        private readonly Dictionary<string, List<InventoryItem>> m_UniqueInventoryItems = new Dictionary<string, List<InventoryItem>>();

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
        /// References to the remove buttons to enable/disable when the action is not possible.
        /// </summary>
        public Button addAppleButton;
        public Button removeAppleButton;
        public Button removeAllApplesButton;

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
                removeAppleButton,
                removeAllApplesButton
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
            // We'll call this to get our initial list of items to know the correct quantities for each aggregated item
            RefreshUniqueItems();

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
            // Note: this will ignore repeated requests to remove callbacks when they have not been added.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded -= OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted -= OnInventoryItemChanged;

                m_SubscribedFlag = false;
            }
        }
        
        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUniqueItems();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Adds a single apple to the GameFoundationSdk.inventory.
        /// </summary>
        public void AddApple()
        {
            try
            {
                // This will create a new item inside the InventoryManager, if the definition exists in the inventory catalog.
                // Because this method will throw an exception if the definition is not found in the inventory catalog, we'll surround
                // in a try catch and log any exceptions thrown.
                var apple = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("apple");
                GameFoundationSdk.inventory.CreateItem(apple);
            }
            catch (Exception e)
            {
                OnGameFoundationException(e);
            }
        }

        /// <summary>
        /// Removes a single apple from the main inventory.
        /// </summary>
        public void RemoveApple()
        {
            // First we'll check that our list of unique items contains any items with apple as their inventoryItemDefinition id
            // If it doesn't, we'll take no action since there is nothing to remove.
            if (m_UniqueInventoryItems.TryGetValue("apple", out var itemGroup))
            {
                // Because we only care about the items in aggregate, and not their specific instances,
                // we can remove any item with the correct inventoryItemDefinition id. In this case we'll remove
                // the last one in our list.
                GameFoundationSdk.inventory.Delete(itemGroup[itemGroup.Count - 1]);

                // Once we remove the item from the InventoryManager, the reference to it will be broken.
                itemGroup[itemGroup.Count - 1] = null;
            }
        }

        /// <summary>
        /// Removes all instances of apple InventoryItemDefinition from the Inventory.
        /// </summary>
        public void RemoveAllApples()
        {
            // This method can be called whether or not there are any items with this definition id in the GameFoundationSdk.inventory.
            // If there are no items with that definition id, the method will take no action and return a count of 0;
            var apple = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("apple");
            int itemsRemovedCount = GameFoundationSdk.inventory.Delete(apple);
            
            Debug.Log(itemsRemovedCount + " apple item(s) removed from inventory.");
        }
        
        /// <summary>
        /// Updates the aggregated list of inventory items to make sure that all counts are accurate.
        /// This example shows the more complicated way of getting an aggregated list of all items in the GameFoundationSdk.inventory.
        /// If you want to only track one item (like we're doing here with apple) you can also use the
        /// GameFoundationSdk.inventory.FindItemsByDefinition() group of methods. 
        /// </summary>
        private void RefreshUniqueItems()
        {
            // Gets the list of all items in the Inventory Manager. This list will contain one item for each inventory item
            // created, which means it may include multiple of the same inventoryItemDefinition.
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Clear out our unique items dictionary to start.
            m_UniqueInventoryItems.Clear();

            // Loop through the list of all items, adding them to our list of unique items based on the key
            // of their InventoryItemDefinition
            foreach (var item in m_InventoryItems)
            {
                if (m_UniqueInventoryItems.TryGetValue(item.definition.key, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    m_UniqueInventoryItems.Add(item.definition.key, new List<InventoryItem>{ item });
                }
            }

            // Update the UI with the new list information
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (var itemGroup in m_UniqueInventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                // We'll use that to display the name of this aggregated group of Items.
                string itemName = itemGroup.Value[0].definition.displayName;

                // The quantity in this case will the count of items in the itemGroup list.
                var quantity = itemGroup.Value.Count;

                m_DisplayText.AppendLine(itemName + ": " + quantity);
            }

            mainText.text = m_DisplayText.ToString();

            RefreshRemoveButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The addButton will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshRemoveButtons()
        {
            if (m_UniqueInventoryItems.TryGetValue("apple", out var list))
            {
                removeAppleButton.interactable = removeAllApplesButton.interactable = list.Count > 0;
            }
            else
            {
                removeAppleButton.interactable = removeAllApplesButton.interactable = false;
            }
        }

        /// <summary>
        /// Listener for changes in GameFoundationSdk.inventory. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">
        /// This parameter will not be used, but must exist so the signature is compatible with the inventory 
        /// callbacks so we can bind it.
        /// </param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_InventoryChanged = true;
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
