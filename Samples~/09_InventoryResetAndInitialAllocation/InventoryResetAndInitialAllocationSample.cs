using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory reset & inventory initial allocation feature.
    /// </summary>
    public class InventoryResetAndInitialAllocationSample : MonoBehaviour
    {
        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Reference in the scene to the Game Foundation Init component.
        /// </summary>
        public GameFoundationInit gameFoundationInit;

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
        public Button removeAllButton;
        public Button deleteAndReinitializeButton;

        /// <summary>
        /// All buttons in array for easy access.
        /// </summary>
        private Button[] m_AllButtons;

        /// <summary>
        /// Flag for inventory item changed callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        bool m_SubscribedFlag = false;

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
                removeAllButton,
                deleteAndReinitializeButton
            };
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
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

            // If callback has been added then remove them.
            // Note: this will ignore repeated requests to remove callbacks when they have not been added.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.inventory.itemAdded -= OnInventoryItemChanged;
                GameFoundationSdk.inventory.itemDeleted -= OnInventoryItemChanged;

                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Adds a single item to the main inventory.
        /// </summary>
        public void AddItem(string itemDefinitionKey)
        {
            try
            {
                // This will create a new item inside the InventoryManager.
                var itemDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(itemDefinitionKey);
                GameFoundationSdk.inventory.CreateItem(itemDefinition);
            }
            catch (Exception exception)
            {
                OnGameFoundationException(exception);
            }
        }

        /// <summary>
        /// Removes a single item from the main inventory.
        /// </summary>
        public void RemoveItem(string itemDefinitionKey)
        {
            try
            {
                // To remove a single item from the InventoryManager, you need a specific instance of that item. Since we only know the 
                // InventoryItemDefinition of the item we want to remove, we'll first look for all items with that definition.
                // We'll use the version of FindItems that lets us pass in a collection to be filled to reduce allocations.
                var itemDefinition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(itemDefinitionKey);
                var itemCount = GameFoundationSdk.inventory.FindItems(itemDefinition, m_InventoryItems);
            
                // Make sure there actually is an item available to return
                if (itemCount > 0)
                {
                    // We'll remove the first instance in the list of items
                    GameFoundationSdk.inventory.Delete(m_InventoryItems[0]);
                }
            }
            catch (Exception exception)
            {
                OnGameFoundationException(exception);
            }
        }

        /// <summary>
        /// Removes all items from the inventory WITHOUT reinitializing.
        /// </summary>
        public void RemoveAllInventoryItems()
        {
            try
            {
                GameFoundationSdk.inventory.DeleteAllItems();
            }
            catch (Exception exception)
            {
                OnGameFoundationException(exception);
            }
        }

        /// <summary>
        /// Uninitializes Game Foundation, deletes persistence data, then re-initializes Game Foundation.
        /// Note: Because Game Foundation is initialized again, all Initial Allocation items will be added again.
        /// </summary>
        public void DeleteAndReinitializeGameFoundation()
        {
            try
            {
                // Disable all buttons until initialization completes.
                EnableAllButtons(false);

                // Stop watching item events (events reconnected after initialization completes).
                UnsubscribeFromGameFoundationEvents();

                // Get the reference to the Data Layer used to initialize Game Foundation before GameFoundationSdk gets uninitialized.
                if (!(GameFoundationSdk.dataLayer is PersistenceDataLayer dataLayer))
                    return;

                // Using the Data Layer get the local persistence layer.
                if (!(dataLayer.persistence is LocalPersistence localPersistence))
                    return;

                // Uninitialize GameFoundation so we can delete the local persistence data file.
                gameFoundationInit.Uninitialize();

                // Delete local persistence data file.  Once finished, we will ReInitializeGameFoundation.
                // Note: if we fail to delete for any reason, exception will be sent to 
                //       OnGameFoundationInitializeException method for logging and buttons will remain disabled.
                localPersistence.Delete( () => gameFoundationInit.Initialize(),
                    OnGameFoundationException);
            }
            catch (Exception exception)
            {
                OnGameFoundationException(exception);
            }
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");
            
            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (var inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                var displayName = inventoryItem.definition.displayName;

                m_DisplayText.AppendLine(displayName);
            }

            mainText.text = m_DisplayText.ToString();

            RefreshButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The addButtons will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshButtons()
        {
            var apple = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("apple");
            var orange = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("orange");

            if (apple is null || orange is null)
            {
                return;
            }

            // FindItems will return a count of the number of items found no matter if you pass in a list or null
            // Since we don't actually care about the list, only the account, we'll save the allocation and just pass null
            var appleCount = GameFoundationSdk.inventory.FindItems(apple);
            var orangeCount = GameFoundationSdk.inventory.FindItems(orange);

            removeAppleButton.interactable = appleCount > 0;
            removeOrangeButton.interactable = orangeCount > 0;
            removeAllButton.interactable = appleCount + orangeCount > 0;
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
