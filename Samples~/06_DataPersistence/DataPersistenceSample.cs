using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
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
        /// We need to keep a reference to the persistence data layer if we want to save data.
        /// </summary>
        private PersistenceDataLayer m_DataLayer;
        
        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

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
        /// Standard starting point for Unity scripts.
        /// </summary>
        private IEnumerator Start()
        {
            // The database has been properly setup.
            if (!SamplesHelper.VerifyDatabase())
            {
                wrongDatabasePanel.SetActive(true);
                yield break;
            }

            // Initialize Game Foundation.
            yield return InitializeGameFoundation();
        }

        /// <summary>
        /// Initialize Game Foundation.  
        /// Called at startup as well as when reinitializing.
        /// </summary>
        private IEnumerator InitializeGameFoundation()
        {
            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we will persist GameFoundation's data using a PersistenceDataLayer.
            //   We create it with a LocalPersistence setup to save/load these data in a JSON file
            //   named "DataPersistenceSampleV2" stored on the device.  Note: 'V2' appended
            //   to the filename to ensure old persistence from previous version of Game Foundation
            //   isn't used, causing Sample to throw at initialization.  This is only needed during
            //   the 'preview' phase of Game Foundation while the structure of persistent data is
            //   changing.
            var persistence = new LocalPersistence("DataPersistenceSampleV2", new JsonDataSerializer());
            m_DataLayer = new PersistenceDataLayer(persistence);

            // Initialize Game Foundation.
            var initDeferred = GameFoundationSdk.Initialize(m_DataLayer);

            return WaitForInitialization(initDeferred);
        }

        /// <summary>
        /// This private method will wait on the deferred Game Foundation initialization object until
        /// initialization completes, then trigger OnGameFoundationInitialized method to finish setup.
        /// </summary>
        /// <param name="deferred">
        /// Deferred object returned from Game Foundation Sdk Initialize method.
        /// </param>
        private IEnumerator WaitForInitialization(Deferred deferred)
        {
            yield return deferred.Wait();

            if (deferred.isFulfilled)
            {
                OnGameFoundationInitialized();
            }
            else
            {
                OnGameFoundationException(deferred.error);
            }

            deferred.Release();
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        private void OnGameFoundationInitialized()
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
            // Deferred is a struct that helps you track the progress of an asynchronous operation of Game Foundation.
            Deferred saveOperation = m_DataLayer.Save();

            // Check if the operation is already done.
            if (saveOperation.isDone)
            {
                LogSaveOperationCompletion(saveOperation);
            }
            else
            {
                StartCoroutine(WaitForSaveCompletion(saveOperation));
            }

            // Release deferred promise handler.
            saveOperation.Release();
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
            GameFoundationSdk.Uninitialize();

            // Begin Game Foundation initialization process
            var deferred = GameFoundationSdk.Initialize(m_DataLayer);

            // Wait for initialization 
            StartCoroutine(WaitForInitialization(deferred));
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
        private void OnGameFoundationException(Exception exception)
        {
            Debug.LogError($"GameFoundation exception: {exception}");
        }
    }
}
