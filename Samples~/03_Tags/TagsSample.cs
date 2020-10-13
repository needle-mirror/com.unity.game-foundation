using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory basics.
    /// </summary>
    public class TagsSample : MonoBehaviour
    {
        /// <summary>
        /// The id of the tag we want to filter items by.
        /// </summary>
        private string m_CurrentTag;

        /// <summary>
        /// Reference to the list of InventoryItemDefinitions in the Inventory Catalog.
        /// </summary>
        private readonly List<InventoryItemDefinition> m_ItemDefinitions = new List<InventoryItemDefinition>();

        /// <summary>
        /// Reference to the list of InventoryItems to show in the display.
        /// </summary>
        private readonly List<InventoryItem> m_FilteredItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// Reference to the buttons
        /// </summary>
        public Button[] buttons;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private IEnumerator Start()
        {
            // The database is NOT correct, show message and abort.
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
            // Disable all buttons while initializing
            foreach (var button in buttons)
            {
                button.interactable = false;
            }

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            var initDeferred = GameFoundationSdk.Initialize(new MemoryDataLayer());

            // Wait for initialization to complete, then continue.
            yield return initDeferred.Wait();

            // Continue initialization process or report error on failure.
            if (initDeferred.isFulfilled)
            {
                OnGameFoundationInitialized();
            }
            else
            {
                OnGameFoundationException(initDeferred.error);
            }

            // Release deferred promise handler.
            initDeferred.Release();
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        private void OnGameFoundationInitialized()
        { 
            // The Inventory Manager starts empty, so we will add a selection of items to the inventory.
            InitializeInventoryItems();

            // Select the 'no filter' option and show results.
            NoTagFilter();
        }

        /// <summary>
        /// Sets the current tag to empty string for no filtering.
        /// </summary>
        public void NoTagFilter()
        {
            m_CurrentTag = "";

            UnselectAllButtons();
            buttons[0].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// Set the current tag to be 'fruit.'
        /// </summary>
        public void FruitTag()
        {
            m_CurrentTag = "fruit";

            UnselectAllButtons();
            buttons[1].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// Set the current tag to be 'food.'
        /// </summary>
        public void FoodTag()
        {
            m_CurrentTag = "food";

            UnselectAllButtons();
            buttons[2].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// Set the current tag to be 'vegetable.'
        /// </summary>
        public void VegetableTag()
        {
            m_CurrentTag = "vegetable";

            UnselectAllButtons();
            buttons[3].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            if (m_CurrentTag == "")
            {
                m_DisplayText.AppendLine("<b><i>Unfiltered Inventory:</i></b>");
            }
            else
            {
                m_DisplayText.AppendLine($"<b><i>Inventory, filtered by \"{m_CurrentTag}\":</i></b>");
            }

            // We'll use the versions of GetItems and FindItemsByTag that let us pass in a collection to be filled to reduce allocations
            if (string.IsNullOrEmpty(m_CurrentTag))
            {
                GameFoundationSdk.inventory.GetItems(m_FilteredItems);
            }
            else
            {
                try
                {
                    // Similar to GetItems, FindItems will return a list of items, but only those that have the requested tag assigned to them.
                    // Because this method will throw an exception if the tagKey is not found in the inventory catalog, we'll surround
                    // in a try catch and log any exceptions thrown.
                    var tag = GameFoundationSdk.tags.Find(m_CurrentTag);
                    GameFoundationSdk.inventory.FindItems(tag, m_FilteredItems);
                }
                catch (Exception e)
                {
                    OnGameFoundationException(e);
                }
            }

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_FilteredItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string itemName = inventoryItem.definition.displayName;

                m_DisplayText.AppendLine(itemName);
            }

            mainText.text = m_DisplayText.ToString();
        }

        /// <summary>
        /// Initializes one InventoryItem for each InventoryItemDefinition in the InventoryCatalog.
        /// </summary>
        private void InitializeInventoryItems()
        {
            // Remove the items added from 'initial inventory allocation' 
            // (Please see Sample 10 for details).
            GameFoundationSdk.inventory.DeleteAllItems();

            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations.
            GameFoundationSdk.catalog.GetItems(m_ItemDefinitions);

            // Iterate all item definitions and create 1 item of each in our inventory.
            foreach (var itemDefinition in m_ItemDefinitions)
            {
                GameFoundationSdk.inventory.CreateItem(itemDefinition);
            }
        }

        /// <summary>
        /// Disables filter button selection (i.e. enables all buttons so none are dimmed)
        /// </summary>
        private void UnselectAllButtons()
        {
            foreach (var button in buttons)
            {
                button.interactable = true;
            }
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
