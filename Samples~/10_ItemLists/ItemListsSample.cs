using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for item lists.
    /// </summary>
    public class ItemListsSample : MonoBehaviour
    {
        /// <summary>
        /// Number of collections used in sample.
        /// </summary>
        private const int k_NumCollections = 3;

        /// <summary>
        /// Number of starting items in inventory.
        /// </summary>
        private const int k_NumInitialItems = 2;

        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// List of all inventory item definitions from catalog.
        /// </summary>
        private readonly List<InventoryItemDefinition> m_AllInventoryItemDefinitions 
            = new List<InventoryItemDefinition>();

        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// List of all <see cref="ItemList"/>s used by this sample.
        /// </summary>
        private readonly List<ItemList> m_ItemLists = new List<ItemList>();
        
        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Text box containing initial instructions which are hidden to show items.
        /// </summary>
        public Text instructionsText;

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Text field for each <see cref="ItemList"/>'s contents.
        /// </summary>
        public Text[] listItemsText;

        /// <summary>
        /// References to the remove buttons to enable/disable when the action is not possible.
        /// </summary>
        public Button addItemButton;
        public Button removeItemButton;
        public Button removeAllButton;
        public Button[] addExistingToListButton;
        public Button[] addNewToListButton;
        public Button[] removeItemFromListButton;

        /// <summary>
        /// Flag for item-changed callback events to ensure they are added exactly once when
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
                // Disable all buttons while initializing.  Once done, RefreshUI will reenable them.
                addItemButton.interactable = false;
                removeItemButton.interactable = false;
                removeAllButton.interactable = false;
                DisableAllButtons(addExistingToListButton);
                DisableAllButtons(addNewToListButton);
                DisableAllButtons(removeItemFromListButton);
            }
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
            // Remove the auto-instantiated items
            GameFoundationSdk.inventory.DeleteAllItems();

            // Generate list of all available InventoryItemDefintions
            GameFoundationSdk.catalog.GetItems<InventoryItemDefinition>(m_AllInventoryItemDefinitions);

            // Add a couple of items to get us started
            for (int i = 0; i < k_NumInitialItems; ++i)
            {
                GameFoundationSdk.inventory.CreateItem(m_AllInventoryItemDefinitions[i]);
            }

            // Create 3 item lists to use in sample
            for (int i = 0; i < k_NumCollections; ++i)
            {
                m_ItemLists.Add(GameFoundationSdk.inventory.CreateList());
            }

            // Throw the first item into our first list to get us started
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);
            m_ItemLists[0].Add(m_InventoryItems[0]);

            // Hide the initial instructions text.
            instructionsText.gameObject.SetActive(false);

            // Show the 4 columns of lists of items (main inventory and 3 collections).
            mainText.gameObject.SetActive(true);
            foreach (var item in listItemsText)
            {
                item.gameObject.SetActive(true);
            }

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
                GameFoundationSdk.inventory.itemAddedToCollection += OnCollectionChanged;
                GameFoundationSdk.inventory.itemRemovedFromCollection += OnCollectionChanged;

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
                GameFoundationSdk.inventory.itemAddedToCollection -= OnCollectionChanged;
                GameFoundationSdk.inventory.itemRemovedFromCollection -= OnCollectionChanged;

                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Listener for changes in GameFoundationSdk.inventory. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">
        /// This parameter will not be used, but must exist so the signature is compatible.
        /// </param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_InventoryChanged = true;
        }

        /// <summary>
        /// Listener for changes to ItemCollections.  Called when item is added/removed from collection so
        /// we can update our UI accordingly.
        /// </summary>
        /// <param name="collectionChanged">
        /// This parameter will not be used, but must exist so the signature is compatible.
        /// </param>
        /// <param name="itemChanged">
        /// This parameter will not be used, but must exist so the signature is compatible.
        /// </param>
        private void OnCollectionChanged(IItemCollection collectionChanged, InventoryItem itemChanged)
        {
            m_InventoryChanged = true;
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUI();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Adds a single unique item to the main inventory.
        /// </summary>
        public void AddItem()
        {
            CreateUniqueItem();
        }

        /// <summary>
        /// Helper method to create an item not already in inventory.
        /// </summary>
        /// <returns>
        /// Newly created item or null if no unique definitions available.
        /// </returns>
        private InventoryItem CreateUniqueItem()
        {
            try
            {
                foreach (var item in m_AllInventoryItemDefinitions)
                {
                    if (GameFoundationSdk.inventory.FindItems(item) == 0)
                    {
                        return GameFoundationSdk.inventory.CreateItem(item);
                    }
                }
                Debug.Log("No more unique items available to add");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        /// <summary>
        /// Removes a single item from the main inventory.
        /// </summary>
        public void RemoveItem()
        {
            try
            {
                if (m_InventoryItems.Count > 0)
                {
                    GameFoundationSdk.inventory.Delete(m_InventoryItems[m_InventoryItems.Count - 1]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Removes all items from the Inventory.
        /// </summary>
        public void RemoveAll()
        {
            GameFoundationSdk.inventory.DeleteAllItems();
        }

        /// <summary>
        /// Adds newly-created item to specified collection.
        /// </summary>
        /// <param name="listIndex">
        /// List index (0-2) to add item to.
        /// </param>
        public void AddNewItemToList(int listIndex)
        {
            var itemList = m_ItemLists[listIndex];
            var newItem = CreateUniqueItem();
            itemList.Add(newItem);
        }

        /// <summary>
        /// Adds existing item to specified list.
        /// </summary>
        /// <param name="listIndex">
        /// List index (0-2) to add item to.
        /// </param>
        public void AddExistingItemToList(int listIndex)
        {
            // Add next item to item list that 
            var itemList = m_ItemLists[listIndex];
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                if (!itemList.Contains(inventoryItem))
                {
                    itemList.Add(inventoryItem);

                    return;
                }
            }

            Debug.LogError("Unable to find existing item to add to list.");
        }

        /// <summary>
        /// Remove First item from specified collection.
        /// </summary>
        /// <param name="listIndex">
        /// List index (0-2) to remove item from.
        /// </param>
        public void RemoveItemFromList(int listIndex)
        {
            var itemList = m_ItemLists[listIndex];
            itemList.Remove(0);
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>All Items:</i></b>");
            m_DisplayText.AppendLine($"<b><i>---------------</i></b>");
            
            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Update list of all items in GameFoundation inventory.
            if (m_InventoryItems.Count == 0)
            {
                m_DisplayText.AppendLine("<i>empty</i>");
            }
            else
            {
                // Loop through every type of item within the inventory and display its name and quantity.
                foreach (InventoryItem inventoryItem in m_InventoryItems)
                {
                    // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                    string itemName = inventoryItem.definition.displayName;

                    m_DisplayText.AppendLine(itemName);
                }
            }

            mainText.text = m_DisplayText.ToString();

            // Add text for contents of each ItemList
            for (var i = 0; i < k_NumCollections; ++i)
            {
                m_DisplayText.Clear();
                m_DisplayText.AppendLine($"<b><i>ItemList{i}:</i></b>");
                m_DisplayText.AppendLine($"<b><i>-------------</i></b>");

                var itemList = m_ItemLists[i];
                if (itemList.Count == 0)
                {
                    m_DisplayText.AppendLine("<i>This ItemList is currently empty.</i>");
                    m_DisplayText.AppendLine();
                    m_DisplayText.AppendLine("<i>Use the buttons below to create new items in the list or add existing ones.</i>");
                    m_DisplayText.AppendLine();
                }
                else
                {
                    foreach (var item in itemList)
                    {
                        m_DisplayText.AppendLine(item.definition.displayName);
                    }
                }

                listItemsText[i].text = m_DisplayText.ToString();
            }

            // Update the general inventory buttons
            var canAddNew = m_InventoryItems.Count < m_AllInventoryItemDefinitions.Count;

            addItemButton.interactable = canAddNew;
            removeItemButton.interactable = m_InventoryItems.Count > 0;
            removeAllButton.interactable = m_InventoryItems.Count > 0;

            // Update the item list buttons
            for (int i = 0; i < k_NumCollections; ++i)
            {
                addExistingToListButton[i].interactable = m_InventoryItems.Count > m_ItemLists[i].Count;
                addNewToListButton[i].interactable = canAddNew;
                removeItemFromListButton[i].interactable = m_ItemLists[i].Count > 0;
            }
        }

        /// <summary>
        /// Helper method to disable all buttons in array.
        /// </summary>
        /// <param name="buttons">
        /// Array of buttons to disable.
        /// </param>
        private void DisableAllButtons(Button[] buttons)
        {
            if (buttons is null)
                return;

            foreach (var button in buttons)
            {
                button.interactable = false;
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
