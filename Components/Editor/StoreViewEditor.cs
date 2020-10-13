using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="StoreView"/> component.
    /// </summary>
    [CustomEditor(typeof(StoreView))]
    class StoreViewEditor : Editor
    {
        StoreView m_StoreView;

        string m_SelectedStoreKey;
        string m_SelectedTagKey;
        int m_SelectedStoreIndex = -1;
        int m_SelectedTagIndex = -1;
        int m_SelectedItemIconPropertyIndex = -1;
        int m_SelectedPriceIconPropertyIndex = -1;

        List<Tuple<string, string>> m_TagNames = new List<Tuple<string, string>>();

        DropdownCatalogItemHelper<StoreAsset> m_StoreDropdownHelper = new DropdownCatalogItemHelper<StoreAsset>();
        DropdownPopulateHelper m_TagDropdownHelper = new DropdownPopulateHelper();
        DropdownMultipleCatalogItemsHelper m_ItemIconPropertyDropdownHelper = new DropdownMultipleCatalogItemsHelper();
        DropdownMultipleCatalogItemsHelper m_PriceIconPropertyDropdownHelper = new DropdownMultipleCatalogItemsHelper();

        SerializedProperty m_StoreKey_SerializedProperty;
        SerializedProperty m_TagKey_SerializedProperty;
        SerializedProperty m_ItemIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ItemContainer_SerializedProperty;
        SerializedProperty m_ItemPrefab_SerializedProperty;
        SerializedProperty m_ShowTransationEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(StoreView.m_StoreKey),
            nameof(StoreView.m_TagKey),
            nameof(StoreView.m_ItemIconSpritePropertyKey),
            nameof(StoreView.m_PriceIconSpritePropertyKey),
            nameof(StoreView.m_NoPriceString),
            nameof(StoreView.m_Interactable),
            nameof(StoreView.m_ItemContainer),
            nameof(StoreView.m_TransactionItemPrefab),
            nameof(StoreView.showTransactionItemEditorFields)
        };

        void OnEnable()
        {
            m_StoreView = target as StoreView;

            m_StoreKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_StoreKey));
            m_TagKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_TagKey));
            m_ItemIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_ItemIconSpritePropertyKey));
            m_PriceIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_PriceIconSpritePropertyKey));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_NoPriceString));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_Interactable));
            m_ItemContainer_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_ItemContainer));
            m_ItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_TransactionItemPrefab));
            m_ShowTransationEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.showTransactionItemEditorFields));

            m_SelectedStoreKey = m_StoreKey_SerializedProperty.stringValue;
            m_SelectedTagKey = m_TagKey_SerializedProperty.stringValue;

            PopulateStores();
            PopulateItemImagesPropertyKeys();
            PopulateTags();
        }

        void PopulateStores()
        {
            m_SelectedStoreIndex = m_StoreDropdownHelper.Populate(m_StoreKey_SerializedProperty.stringValue);
        }

        void PopulateItemImagesPropertyKeys()
        {
            var catalogItemAssets = new List<CatalogItemAsset>();
            var costCatalogItemAssets = new List<CatalogItemAsset>();

            var store = !string.IsNullOrEmpty(m_SelectedStoreKey)
                ? CatalogSettings.catalogAsset.FindItem(m_SelectedStoreKey) as StoreAsset
                : null;

            if (store != null)
            {
                var tag = !string.IsNullOrEmpty(m_SelectedTagKey)
                    ? CatalogSettings.catalogAsset.tagCatalog.FindTag(m_SelectedTagKey)
                    : null;

                var storeItems = new List<StoreItemObject>();
                store.GetStoreItems(storeItems);

                foreach (var storeItemObject in storeItems)
                {
                    var transactionAsset = storeItemObject.transaction;
                    if (tag != null && !transactionAsset.HasTag(tag))
                    {
                        continue;
                    }

                    catalogItemAssets.Add(transactionAsset);

                    // getting price icon property list for virtual transactions
                    if (transactionAsset is VirtualTransactionAsset vTransactionAsset)
                    {
                        var exchangeObjects = new List<ExchangeDefinitionObject>();
                        vTransactionAsset.costs.GetItems(exchangeObjects);

                        if (exchangeObjects.Count > 0)
                        {
                            // getting the only first cost since the UI can only show one price icon.
                            costCatalogItemAssets.Add(exchangeObjects[0].catalogItem);
                        }
                    }
                }
            }

            m_SelectedItemIconPropertyIndex = m_ItemIconPropertyDropdownHelper.Populate(catalogItemAssets, m_ItemIconSpritePropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);
            m_SelectedPriceIconPropertyIndex = m_PriceIconPropertyDropdownHelper.Populate(costCatalogItemAssets, m_PriceIconSpritePropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);
        }

        void PopulateTags()
        {
            m_TagNames.Clear();
            m_TagNames.Add(new Tuple<string, string>("<All>", null));

            var tagAssets = new List<TagAsset>();
            CatalogSettings.catalogAsset.tagCatalog.GetTags(tagAssets);

            for (int i = 0; i < tagAssets.Count; i++)
            {
                var key = tagAssets[i].key;
                m_TagNames.Add(new Tuple<string, string>(key, key));
            }

            m_SelectedTagIndex = m_TagDropdownHelper.Populate(m_TagNames, m_TagKey_SerializedProperty.stringValue);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            DrawStoreSection();
            EditorGUILayout.Space();

            DrawRuntimeSection();
            EditorGUILayout.Space();

            DrawAutoPopulatedItemSection();
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawStoreSection()
        {
            var itemDisplayContent = new GUIContent("Store", "The Store to display in this view.");
            if (m_StoreDropdownHelper.displayNames != null && m_StoreDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedStoreIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedStoreIndex, m_StoreDropdownHelper.displayNames);
                var storeKey = m_StoreDropdownHelper.GetKey(m_SelectedStoreIndex);
                if (m_StoreView.storeKey != storeKey)
                {
                    SetSelectedStoreKey(storeKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
            }

            var tagDisplayContent = new GUIContent("Tag", "If a specific tag is selected, list of Transaction Items rendered will be filtered to only ones in that tag.");
            if (m_TagDropdownHelper.displayNames != null && m_TagDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedTagIndex = EditorGUILayout.Popup(tagDisplayContent, m_SelectedTagIndex, m_TagDropdownHelper.displayNames);
                var tagKey = m_TagDropdownHelper.GetKey(m_SelectedTagIndex);
                if (m_StoreView.tagKey != tagKey)
                {
                    SetSelectedTagKey(tagKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(tagDisplayContent, 0, new[] { "None" });
            }
        }

        void DrawRuntimeSection()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var interactableContent = new GUIContent("Interactable", "Sets the Store View's interactable state.");
                var interactable = EditorGUILayout.Toggle(interactableContent, m_StoreView.interactable);

                if (check.changed)
                {
                    m_StoreView.interactable = m_Interactable_SerializedProperty.boolValue = interactable;
                }
            }
        }

        void DrawAutoPopulatedItemSection()
        {
            m_ShowTransationEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowTransationEditorFields_SerializedProperty.boolValue, "Auto Populated Transaction Items");
            if (m_ShowTransationEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                var itemIconPropertyContent = new GUIContent("Item Icon Asset Property Key",
                    "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                    "If none is specified no image will be displayed.");
                m_SelectedItemIconPropertyIndex = EditorGUILayout.Popup(itemIconPropertyContent, m_SelectedItemIconPropertyIndex, m_ItemIconPropertyDropdownHelper.displayNames);
                var itemIconPropertyKey = m_ItemIconPropertyDropdownHelper.GetKey(m_SelectedItemIconPropertyIndex);
                if (m_StoreView.itemIconSpritePropertyKey != itemIconPropertyKey)
                {
                    m_StoreView.SetItemIconSpritePropertyKey(itemIconPropertyKey);
                    m_ItemIconSpritePropertyKey_SerializedProperty.stringValue = itemIconPropertyKey;
                }

                EditorGUILayout.Space();

                var priceIconPropertyContent = new GUIContent("Price Icon Asset Property Key",
                    "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                    "If none is specified no image will be displayed.");
                m_SelectedPriceIconPropertyIndex = EditorGUILayout.Popup(priceIconPropertyContent, m_SelectedPriceIconPropertyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
                var priceIconPropertyKey = m_PriceIconPropertyDropdownHelper.GetKey(m_SelectedPriceIconPropertyIndex);
                if (m_StoreView.priceIconSpritePropertyKey != priceIconPropertyKey)
                {
                    m_StoreView.SetPriceIconSpritePropertyKey(priceIconPropertyKey);
                    m_PriceIconSpritePropertyKey_SerializedProperty.stringValue = priceIconPropertyKey;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var noPriceStringContent = new GUIContent("No Price String", "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
                    var noPriceString = EditorGUILayout.TextField(noPriceStringContent, m_StoreView.noPriceString);
                    if (check.changed)
                    {
                        m_StoreView.SetNoPriceString(noPriceString);
                        m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_ItemContainer_SerializedProperty,
                    new GUIContent("Populated Item Container",
                        "Optionally allows specifying an alternate parent container for automatically rendered Transaction Item Prefabs. If not defined, StoreView's Transform will be the parent by default."));

                EditorGUILayout.PropertyField(m_ItemPrefab_SerializedProperty,
                    new GUIContent("Item Prefab",
                        "The prefab to use when generating Transaction Items in the Store View."));

                EditorGUI.indentLevel--;
            }
        }

        void SetSelectedStoreKey(string key)
        {
            if (m_SelectedStoreKey == key) return;

            m_SelectedStoreKey = key;

            // Update the serialized value
            m_StoreKey_SerializedProperty.stringValue = key;
            m_StoreView.SetStore(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateItemImagesPropertyKeys();
        }

        void SetSelectedTagKey(string key)
        {
            if (m_SelectedTagKey == key) return;

            m_SelectedTagKey = key;

            // Update the serialized value
            m_TagKey_SerializedProperty.stringValue = key;
            m_StoreView.SetTagKey(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateItemImagesPropertyKeys();
        }
    }
}
