using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.Promise;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the data persistence sample.
    /// </summary>
    public class DataPersistenceSample : MonoBehaviour
    {
        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

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
        /// Flag for inventory item changed callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        bool m_SubscribedFlag = false;

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
            // Show list of inventory items.
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
        /// Adds a new sword item to the GameFoundationSdk.inventory.
        /// The sword Inventory Item first needs to be set up in the Inventory window.
        /// </summary>
        public void AddNewSword()
        {
            var sword = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("sword");
            GameFoundationSdk.inventory.CreateItem(sword);
        }

        /// <summary>
        /// Adds a health potion item to the inventory manager.
        /// The health potion Inventory Item first needs to be set up in the Inventory window.
        /// </summary>
        public void AddHealthPotion()
        {
            var healthPotion = GameFoundationSdk.catalog.Find<InventoryItemDefinition>("healthPotion");
            GameFoundationSdk.inventory.CreateItem(healthPotion);
        }

        /// <summary>
        /// Removes all Inventory Items from GameFoundationSdk.inventory.
        /// </summary>
        public void RemoveAllItems()
        {
            var deletedCount = GameFoundationSdk.inventory.DeleteAllItems();
            Debug.Log(deletedCount + " inventory items removed");
        }

        /// <summary>
        /// This will save game foundation's data as a JSON file on your machine.
        /// This data will persist between play sessions.
        /// This sample only showcases inventories, but this method saves their items, and properties too. 
        /// </summary>
        public void Save()
        {
            // Get the persistence data layer used during Game Foundation initialization.
            if (!(GameFoundationSdk.dataLayer is PersistenceDataLayer dataLayer))
                return;

            // - Deferred is a struct that helps you track the progress of an asynchronous operation of Game Foundation.
            // - We use a using block to automatically release the deferred promise handler.
            using (Deferred saveOperation = dataLayer.Save())
            {
                // Check if the operation is already done.
                if (saveOperation.isDone)
                {
                    LogSaveOperationCompletion(saveOperation);
                }
                else
                {
                    StartCoroutine(WaitForSaveCompletion(saveOperation));
                }
            }
        }

        /// <summary>
        /// This will un-initialize game foundation and re-initialize it with data from the save file.
        /// This will set the current state of inventories and properties to be what's within the save file.
        /// </summary>
        public void Load()
        {
            // Don't forget to stop listening to events before un-initializing.
            UnsubscribeFromGameFoundationEvents();

            // Reset and reinitialize Game Foundation
            gameFoundationInit.Uninitialize();

            // Begin Game Foundation initialization process
            gameFoundationInit.Initialize();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            // Display the main inventory's display name
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Inventory:</i></b>");

            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string itemName = inventoryItem.definition.key;

                m_DisplayText.AppendLine(itemName);
            }

            mainText.text = m_DisplayText.ToString();
        }

        private static IEnumerator WaitForSaveCompletion(Deferred saveOperation)
        {
            // Wait for the operation to complete.
            yield return saveOperation.Wait();

            LogSaveOperationCompletion(saveOperation);
        }

        private static void LogSaveOperationCompletion(Deferred saveOperation)
        {
            // Check if the operation was successful.
            if (saveOperation.isFulfilled)
            {
                Debug.Log("Saved!");
            }
            else
            {
                Debug.LogError($"Save failed! Error: {saveOperation.error}");
            }
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
            m_InventoryChanged = true;
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
