using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for item map.
    /// </summary>
    public class ItemMapsSample : MonoBehaviour
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
        /// Array of slots used for items in ItemMaps.
        /// </summary>
        private readonly string[] k_MapSlots = new string[] {"1", "two", "C"};

        /// <summary>
        /// Number of slots used in each ItemMap for this sample project.
        /// Note: you can have unlimited slots in practice.
        /// </summary>
        private int numSlots => k_MapSlots.Length;

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
        /// List of all <see cref="ItemMap"/>s used by this sample.
        /// </summary>
        private readonly List<ItemMap> m_ItemMaps = new List<ItemMap>();
        
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
        /// Text field for each <see cref="ItemMap"/>'s contents.
        /// </summary>
        public Text[] itemsText;

        /// <summary>
        /// References to the remove buttons to enable/disable when the action is not possible.
        /// </summary>
        public Button addItemButton;
        public Button removeItemButton;
        public Button removeAllButton;
        public Button[] toggleItemMap1Buttons;
        public Button[] toggleItemMap2Buttons;
        public Button[] toggleItemMap3Buttons;

        /// <summary>
        /// Array of all toggle buttons by map index, slot index.
        /// </summary>
        private Button[,] m_AllToggleButtons;

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
            // Copy the buttons into easy-to-access array
            m_AllToggleButtons = new Button[k_NumCollections, numSlots];
            CopyToggleButtons(toggleItemMap1Buttons, 0);
            CopyToggleButtons(toggleItemMap2Buttons, 1);
            CopyToggleButtons(toggleItemMap3Buttons, 2);

            // Game Foundation Initialization is being managed by GameFoundationInit Game Object
            if (!GameFoundationSdk.IsInitialized)
            {
                // Disable all buttons while initializing.  Once done, RefreshUI will reenable them.
                addItemButton.interactable = false;
                removeItemButton.interactable = false;
                removeAllButton.interactable = false;
                foreach (var button in m_AllToggleButtons)
                {
                    button.interactable = false;
                }
            }
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
            // Remove the auto-instantiated items.
            GameFoundationSdk.inventory.DeleteAllItems();

            // Generate list of all available InventoryItemDefintions.
            GameFoundationSdk.catalog.GetItems<InventoryItemDefinition>(m_AllInventoryItemDefinitions);

            // Add a couple of items to get us started.
            for (int i = 0; i < k_NumInitialItems; ++i)
            {
                GameFoundationSdk.inventory.CreateItem(m_AllInventoryItemDefinitions[i]);
            }

            // Create 3 item maps to use in sample and add 1 item to get us started.
            for (int i = 0; i < k_NumCollections; ++i)
            {
                m_ItemMaps.Add(GameFoundationSdk.inventory.CreateMap());
            }
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);
            m_ItemMaps[0].Set(k_MapSlots[0], m_InventoryItems[0]);

            // Hide the initial instructions text.
            instructionsText.gameObject.SetActive(false);

            // Show the 4 columns of lists of items (main inventory and 3 collections).
            mainText.gameObject.SetActive(true);
            foreach (var item in itemsText)
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
        /// Removes the above-added callbacks for when app is disabled or destroyed.
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
            // This flag will be set to true when something has changed in the InventoryManager.
            if (m_InventoryChanged)
            {
                RefreshUI();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Creates a single unique item in main inventory.
        /// </summary>
        public void CreateItem()
        {
            try
            {
                foreach (var item in m_AllInventoryItemDefinitions)
                {
                    if (GameFoundationSdk.inventory.FindItems(item) == 0)
                    {
                        GameFoundationSdk.inventory.CreateItem(item);
                        return;
                    }
                }
                Debug.Log("No more unique items available to add");
            }
            catch (Exception e)
            {
                OnGameFoundationException(e);
            }
        }

        /// <summary>
        /// Deletes a single item from the main inventory.
        /// </summary>
        public void DeleteItem()
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
                OnGameFoundationException(e);
            }
        }

        /// <summary>
        /// Deletes all items from the Inventory.
        /// </summary>
        public void DeleteAll()
        {
            try
            { 
                GameFoundationSdk.inventory.DeleteAllItems();
            }
            catch (Exception e)
            {
                OnGameFoundationException(e);
            }
        }

        /// <summary>
        /// Button callback to toggle an item in map 1.
        /// </summary>
        /// <param name="slotIndex">
        /// Slot index to toggle.
        /// </param>
        public void ToggleItemInMap1(int slotIndex)
        {
            ToggleItemInMap(0, slotIndex);
        }

        /// <summary>
        /// Button callback to toggle an item in map 2.
        /// </summary>
        /// <param name="slotIndex">
        /// Slot index to toggle.
        /// </param>
        public void ToggleItemInMap2(int slotIndex)
        {
            ToggleItemInMap(1, slotIndex);
        }

        /// <summary>
        /// Button callback to toggle an item in map 3.
        /// </summary>
        /// <param name="slotIndex">
        /// Slot index to toggle.
        /// </param>
        public void ToggleItemInMap3(int slotIndex)
        {
            ToggleItemInMap(2, slotIndex);
        }

        /// <summary>
        /// Toggles specified slot in specified ItemMap.
        /// </summary>
        /// <param name="mapIndex">
        /// Map index (0-2) to toggle item within.
        /// </param>
        /// <param name="slotIndex">
        /// Slot index (0-2) to modify.
        /// </param>
        public void ToggleItemInMap(int mapIndex, int slotIndex)
        {
            // Get ItemMap requested
            var itemMap = m_ItemMaps[mapIndex];

            // Get Slot requested
            var slot = k_MapSlots[slotIndex];

            // If slot is already set
            if (itemMap.IsSlotSet(slot))
            {
                // Toggle value by 'unsetting' it
                itemMap.Unset(slot);
            }

            // If slot is NOT already set
            else
            {
                // Find next available item to add to the ItemMap
                foreach (var item in m_InventoryItems)
                {
                    if (!itemMap.Contains(item))
                    {
                        // Add this item to the ItemMap
                        itemMap[slot] = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Update list of all items in GameFoundation inventory.
            AddInventoryList();

            // Add text for contents of each ItemMap
            for (var i = 0; i < k_NumCollections; ++i)
            {
                AddCollectionItemsList(i);
            }

            // Update app buttons
            UpdateButtons();
        }

        /// <summary>
        /// Update list of all items in GameFoundation inventory.
        /// </summary>
        private void AddInventoryList()
        {
            m_DisplayText.Clear();

            m_DisplayText.AppendLine("<b><i>All Items:</i></b>");
            m_DisplayText.AppendLine($"<b><i>---------------</i></b>");

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

            // Show text in app.
            mainText.text = m_DisplayText.ToString();
        }

        /// <summary>
        /// Add text for items in specified collection.
        /// </summary>
        /// <param name="index">
        /// Index (0-2) of collection to display.
        /// </param>
        private void AddCollectionItemsList(int index)
        {
            // Show title for this collection.
            m_DisplayText.Clear();

            m_DisplayText.AppendLine($"<b><i>ItemMap{index + 1}:</i></b>");
            m_DisplayText.AppendLine($"<b><i>-------------</i></b>");

            // Show text for collection empty.
            var itemMap = m_ItemMaps[index];
            if (itemMap.Count == 0)
            {
                m_DisplayText.AppendLine("<i>This ItemMap is currently empty.");
                m_DisplayText.AppendLine();
                m_DisplayText.AppendLine("Use the buttons below to add items to the map.</i>");
            }

            // If collection is not empty, list the items it contains.
            else
            {
                foreach (var slot in k_MapSlots)
                {
                    var item = itemMap.Get(slot);
                    if (item is null)
                    {
                        m_DisplayText.AppendLine($"{slot}: <i>empty</i>");
                    }
                    else
                    {
                        m_DisplayText.AppendLine($"{slot}: {item.definition.displayName}");
                    }
                }
            }

            // Update text field in app.
            itemsText[index].text = m_DisplayText.ToString();
        }

        /// <summary>
        /// Update the buttons based on Game Foundation contents and status.
        /// </summary>
        private void UpdateButtons()
        { 
            // Update the general inventory buttons.
            var canAddNew = m_InventoryItems.Count < m_AllInventoryItemDefinitions.Count;

            // Update inventory items add/remove/remove-all buttons.
            addItemButton.interactable = canAddNew;
            removeItemButton.interactable = m_InventoryItems.Count > 0;
            removeAllButton.interactable = m_InventoryItems.Count > 0;

            if (m_AllToggleButtons is null)
                return;

            // Update the ItemMap toggle buttons for each ItemMap
            for (int mapOn = 0; mapOn < k_NumCollections; ++mapOn)
            {
                var itemMap = m_ItemMaps[mapOn];
                var canAdd = itemMap.Count < m_InventoryItems.Count;

                // Update all available slots within this map
                for (int slotOn = 0; slotOn < numSlots; ++slotOn)
                {
                    // If slot is full then you can always toggle (ie remove the item)
                    var slot = k_MapSlots[slotOn];
                    if (itemMap.IsSlotSet(slot))
                    {
                        m_AllToggleButtons[mapOn, slotOn].interactable = true;
                    }

                    // if slot is empty then enable toggle button ONLY if there are items in the inventory to add
                    else
                    {
                        m_AllToggleButtons[mapOn, slotOn].interactable = canAdd;
                    }
                }
            }
        }


        /// <summary>
        /// Helper method to copy buttons in array into AllToggleButtons array for easy access.
        /// </summary>
        /// <param name="buttons">
        /// Array of buttons for 1 ItemMap.
        /// </param>
        /// <param name="mapIndex">
        /// Map index to copy toggle-buttons into for AllToggleButtons.
        /// </param>
        private void CopyToggleButtons(Button[] buttons, int mapIndex)
        {
            for (int i = 0; i < numSlots; ++i)
            {
                m_AllToggleButtons[mapIndex, i] = buttons[i];
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
