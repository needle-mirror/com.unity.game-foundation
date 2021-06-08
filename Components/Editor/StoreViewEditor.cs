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

        int m_SelectedStoreIndex = -1;
        int m_SelectedTagIndex = -1;
        int m_SelectedItemIconPropertyIndex = -1;
        int m_SelectedPriceIconPropertyIndex = -1;
        int m_SelectedPayoutItemIconPropertyIndex = -1;
        int m_SelectedBadgeTextPropertyIndex = -1;

        List<DropdownItem> m_TagNames = new List<DropdownItem>();

        DropdownCatalogItemHelper<StoreAsset> m_StoreDropdownHelper = new DropdownCatalogItemHelper<StoreAsset>();
        DropdownPopulateHelper m_TagDropdownHelper = new DropdownPopulateHelper();
        DropdownMultipleCatalogItemsHelper m_ItemIconPropertyDropdownHelper = new DropdownMultipleCatalogItemsHelper();
        DropdownCostItemPropertyHelper m_PriceIconPropertyDropdownHelper = new DropdownCostItemPropertyHelper();
        DropdownPayoutItemPropertyHelper m_PayoutItemIconPropertyDropdownHelper = new DropdownPayoutItemPropertyHelper();
        DropdownMultipleCatalogItemsHelper m_BadgeTextPropertyDropdownHelper = new DropdownMultipleCatalogItemsHelper();

        SerializedProperty m_StoreKey_SerializedProperty;
        SerializedProperty m_TagKey_SerializedProperty;

        SerializedProperty m_ItemIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ItemContainer_SerializedProperty;
        SerializedProperty m_ItemPrefab_SerializedProperty;
        SerializedProperty m_UseRevealHiddenItemsButton_SerializedProperty;
        SerializedProperty m_RevealHiddenItemsButton_SerializedProperty;

        SerializedProperty m_ShowBundleTransactionItemEditorFields_SerializedProperty;
        SerializedProperty m_UseBundleTransactionItem_SerializedProperty;
        SerializedProperty m_PayoutItemIconPropertyKey_SerializedProperty;
        SerializedProperty m_ItemPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_CurrencyPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_BadgeTextPropertyKey_SerializedProperty;
        SerializedProperty m_BundleItemPrefab_SerializedProperty;

        SerializedProperty m_ShowFeaturedItemsEditorFieldsSerializedProperty;
        SerializedProperty m_UseFeaturedTransactionItem_SerializedProperty;
        SerializedProperty m_FeaturedItemPrefab_SerializedProperty;
        SerializedProperty m_FeaturedBundleItemPrefab_SerializedProperty;


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
            nameof(StoreView.m_ShowHiddenItems),
            nameof(StoreView.m_UseRevealHiddenItemsButton),
            nameof(StoreView.m_RevealHiddenItemsButton),
            nameof(StoreView.showBundleTransactionItemEditorFields),
            nameof(StoreView.m_UseBundleTransactionItem),
            nameof(StoreView.m_BundleItemPrefab),
            nameof(StoreView.m_PayoutItemIconPropertyKey),
            nameof(StoreView.m_ItemPayoutCountPrefix),
            nameof(StoreView.m_CurrencyPayoutCountPrefix),
            nameof(StoreView.m_BadgeTextPropertyKey),
            nameof(StoreView.showFeaturedItemsEditorFields),
            nameof(StoreView.m_UseFeaturedTransactionItem),
            nameof(StoreView.m_FeaturedItemPrefab),
            nameof(StoreView.m_FeaturedBundleItemPrefab)
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
            m_UseRevealHiddenItemsButton_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_UseRevealHiddenItemsButton));
            m_RevealHiddenItemsButton_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_RevealHiddenItemsButton));

            m_ShowBundleTransactionItemEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.showBundleTransactionItemEditorFields));
            m_UseBundleTransactionItem_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_UseBundleTransactionItem));
            m_PayoutItemIconPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_PayoutItemIconPropertyKey));
            m_ItemPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_ItemPayoutCountPrefix));
            m_CurrencyPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_CurrencyPayoutCountPrefix));
            m_BadgeTextPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_BadgeTextPropertyKey));
            m_BundleItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_BundleItemPrefab));

            m_ShowFeaturedItemsEditorFieldsSerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.showFeaturedItemsEditorFields));
            m_UseFeaturedTransactionItem_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_UseFeaturedTransactionItem));
            m_FeaturedItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_FeaturedItemPrefab));
            m_FeaturedBundleItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_FeaturedBundleItemPrefab));

            PopulateStores();
            PopulateItemImagesPropertyKeys();
            PopulateTags();
            
            // To update the content when the GameObject is selected
            m_StoreView.UpdateContent();
        }

        void PopulateStores()
        {
            m_SelectedStoreIndex = m_StoreDropdownHelper.Populate(m_StoreKey_SerializedProperty.stringValue, true);
        }

        void PopulateItemImagesPropertyKeys()
        {
            var selectedStoreKey = m_StoreKey_SerializedProperty.stringValue;
            var selectedTagKey = m_TagKey_SerializedProperty.stringValue;
            var baseTransactionAssets = new List<BaseTransactionAsset>();

            var catalog = PrefabTools.GetLookUpCatalogAsset();

            var store = !string.IsNullOrEmpty(selectedStoreKey)
                ? catalog.FindItem(selectedStoreKey) as StoreAsset
                : null;

            if (store != null)
            {
                var tag = !string.IsNullOrEmpty(selectedTagKey)
                    ? catalog.tagCatalog.FindTag(selectedTagKey)
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

                    baseTransactionAssets.Add(transactionAsset);
                }
            }

            m_SelectedItemIconPropertyIndex = m_ItemIconPropertyDropdownHelper.Populate(
                baseTransactionAssets, m_ItemIconSpritePropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });
            m_SelectedPriceIconPropertyIndex = m_PriceIconPropertyDropdownHelper.Populate(
                baseTransactionAssets, m_PriceIconSpritePropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });
            m_SelectedPayoutItemIconPropertyIndex = m_PayoutItemIconPropertyDropdownHelper.Populate(
                baseTransactionAssets, m_PayoutItemIconPropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables }, false);
            m_SelectedBadgeTextPropertyIndex = m_BadgeTextPropertyDropdownHelper.Populate(baseTransactionAssets,
                m_BadgeTextPropertyKey_SerializedProperty.stringValue, PropertyType.String);
        }

        void PopulateTags()
        {
            m_TagNames.Clear();
            m_TagNames.Add(new DropdownItem("<All>", null));

            var tagAssets = new List<TagAsset>();
            PrefabTools.GetLookUpCatalogAsset().tagCatalog.GetTags(tagAssets);

            for (int i = 0; i < tagAssets.Count; i++)
            {
                var key = tagAssets[i].key;
                m_TagNames.Add(new DropdownItem(key, key));
            }

            m_SelectedTagIndex = m_TagDropdownHelper.Populate(m_TagNames, m_TagKey_SerializedProperty.stringValue, false);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            DrawStoreSection();
            EditorGUILayout.Space();

            DrawRuntimeSection();
            EditorGUILayout.Space();

            DrawDefaultTransactionItemsSection();
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawStoreSection()
        {
            var itemDisplayContent = new GUIContent("Store", "The Store to display in this view.");
            var tagDisplayContent = new GUIContent("Tag", "If a specific tag is selected, list of " +
                                    "Transaction Items rendered will be filtered to only ones in that tag.");

            PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedStoreIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedStoreIndex, 
                    m_StoreDropdownHelper.displayNames);
                m_StoreKey_SerializedProperty.stringValue = m_StoreDropdownHelper.GetKey(m_SelectedStoreIndex);

                m_SelectedTagIndex = EditorGUILayout.Popup(tagDisplayContent, m_SelectedTagIndex, 
                    m_TagDropdownHelper.displayNames);
                m_TagKey_SerializedProperty.stringValue = m_TagDropdownHelper.GetKey(m_SelectedTagIndex);

                if (check.changed)
                {
                    PopulateItemImagesPropertyKeys();

                    // Uncheck Use Reveal Hidden Items Button toggle if key is null
                    // (feature is only available if a tag is selected)
                    if (string.IsNullOrEmpty(m_TagKey_SerializedProperty.stringValue))
                    {
                        m_UseRevealHiddenItemsButton_SerializedProperty.boolValue = false;
                    }
                }
            }
        }

        void DrawRuntimeSection()
        {
            var interactableContent = new GUIContent("Interactable", 
                "Sets the Store View's interactable state.");
            EditorGUILayout.PropertyField(m_Interactable_SerializedProperty, interactableContent);
        }

        void DrawDefaultTransactionItemsSection()
        {
            var itemIconPropertyContent = new GUIContent("Item Icon Asset Property Key",
                "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                "If none is specified no image will be displayed.");
            var priceIconPropertyContent = new GUIContent("Price Icon Asset Property Key",
                "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                "If none is specified no image will be displayed.");
            var noPriceStringContent = new GUIContent("No Price String", 
                "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
            var useRevealItemsContent = new GUIContent("Use Reveal Hidden Items Button",
                "Optional, and only available as a valid selection if a Tag has been specified. " +
                "The button to use for revealing the transaction items in the Store list that don't have " +
                "the tag specified by the Tag dropdown. If a Button Game Object is set here, it will " +
                "automatically be moved to the bottom of the Scroll Area.");
            var populatedItemContainerContent = new GUIContent("Populated Item Container",
                "Optionally allows specifying an alternate parent container for automatically rendered " +
                "Transaction Item Prefabs. If not defined, StoreView's Transform will be the parent by default.");
            var itemPrefabContent = new GUIContent("Item Prefab",
                "The prefab to use when generating Transaction Items in the Store View.");

            m_SelectedItemIconPropertyIndex = EditorGUILayout.Popup(itemIconPropertyContent,
                m_SelectedItemIconPropertyIndex, m_ItemIconPropertyDropdownHelper.displayNames);
            m_ItemIconSpritePropertyKey_SerializedProperty.stringValue = m_ItemIconPropertyDropdownHelper
                .GetKey(m_SelectedItemIconPropertyIndex);

            m_SelectedPriceIconPropertyIndex = EditorGUILayout.Popup(priceIconPropertyContent, 
                m_SelectedPriceIconPropertyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
            m_PriceIconSpritePropertyKey_SerializedProperty.stringValue = m_PriceIconPropertyDropdownHelper
                .GetKey(m_SelectedPriceIconPropertyIndex);

            EditorGUILayout.PropertyField(m_NoPriceString_SerializedProperty, noPriceStringContent);
            EditorGUILayout.PropertyField(m_ItemContainer_SerializedProperty, populatedItemContainerContent);
            EditorGUILayout.PropertyField(m_ItemPrefab_SerializedProperty, itemPrefabContent);

            using (new EditorGUI.DisabledScope(m_SelectedTagIndex <= 0))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(m_UseRevealHiddenItemsButton_SerializedProperty, useRevealItemsContent);

                    using (new EditorGUI.DisabledScope(!m_UseRevealHiddenItemsButton_SerializedProperty.boolValue))
                    {
                        EditorGUILayout.PropertyField(m_RevealHiddenItemsButton_SerializedProperty, new GUIContent());
                    }
                }
            }

            EditorGUILayout.Space();
            DrawBundleTransactionItemSection();
            EditorGUILayout.Space();
            DrawFeaturedItemSection();
        }

        void DrawBundleTransactionItemSection()
        {
            var useBundleContent = new GUIContent("Use Bundle Item Prefab", 
                "Sets whether to use a Bundle Transaction Item prefab for transaction items with" +
                " more than one payout.");
            var payoutItemIconPropertyKey = new GUIContent("Payout Items Icon Property Key",
                "The key for the payout items' icons, defined in the Static Properties of each of the " +
                "Transaction's Payout Items. If none is specified no payouts will be displayed.");
            var itemPayoutPrefixContent = new GUIContent("Item Count Prefix",
                "The string to add as a prefix to each item's payout count.");
            var currencyPayoutPrefixContent = new GUIContent("Currency Count Prefix",
                "The string to add as a prefix to each currency's payout count.");
            var badgeTextPropertyKeyContent = new GUIContent("Badge Text Property Key",
                "The key for the badge text that is defined in the Static Property of Transaction Item.");

            m_ShowBundleTransactionItemEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(
                m_ShowBundleTransactionItemEditorFields_SerializedProperty.boolValue,
                "Bundle Transaction Items", true);

            if (!m_ShowBundleTransactionItemEditorFields_SerializedProperty.boolValue)
            {
                return;
            }

            EditorGUI.indentLevel++;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_UseBundleTransactionItem_SerializedProperty, useBundleContent);

                using (new EditorGUI.DisabledScope(!m_UseBundleTransactionItem_SerializedProperty.boolValue))
                {
                    EditorGUILayout.PropertyField(m_BundleItemPrefab_SerializedProperty, new GUIContent());
                }
            }

            m_SelectedPayoutItemIconPropertyIndex = EditorGUILayout.Popup(payoutItemIconPropertyKey,
                m_SelectedPayoutItemIconPropertyIndex, m_PayoutItemIconPropertyDropdownHelper.displayNames);
            m_PayoutItemIconPropertyKey_SerializedProperty.stringValue = m_PayoutItemIconPropertyDropdownHelper
                .GetKey(m_SelectedPayoutItemIconPropertyIndex);

            EditorGUILayout.PropertyField(m_ItemPayoutCountPrefix_SerializedProperty, itemPayoutPrefixContent);
            EditorGUILayout.PropertyField(m_CurrencyPayoutCountPrefix_SerializedProperty, currencyPayoutPrefixContent);

            m_SelectedBadgeTextPropertyIndex = EditorGUILayout.Popup(badgeTextPropertyKeyContent,
                m_SelectedBadgeTextPropertyIndex, m_BadgeTextPropertyDropdownHelper.displayNames);
            m_BadgeTextPropertyKey_SerializedProperty.stringValue = m_BadgeTextPropertyDropdownHelper
                .GetKey(m_SelectedBadgeTextPropertyIndex);

            EditorGUI.indentLevel--;
        }

        void DrawFeaturedItemSection()
        {
            var useFeaturedItemContent = new GUIContent("Show First Item as Featured",
                "If enabled, the first transaction item in the list will be displayed using one of " +
                "the TransactionItemView prefabs specified below.");
            var featuredItemPrefabContent = new GUIContent("Featured Item Prefab",
                "If the first transaction item in the list has only one payout, or there is no " +
                "prefab assigned in Featured Bundle Item, this prefab will be used to display the first " +
                "Transaction Item in the Store list");
            var featBundlePrefabContent = new GUIContent("Featured Bundle Item Prefab",
                "If the first transaction item in the list has more than one payout this prefab " +
                "will be used to display the first Transaction Item in the Store list.");

            m_ShowFeaturedItemsEditorFieldsSerializedProperty.boolValue = EditorGUILayout.Foldout(
                m_StoreView.showFeaturedItemsEditorFields, "Featured Transaction Items", true);

            if (m_StoreView.showFeaturedItemsEditorFields)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_UseFeaturedTransactionItem_SerializedProperty, useFeaturedItemContent);

                using (new EditorGUI.DisabledScope(!m_UseFeaturedTransactionItem_SerializedProperty.boolValue))
                {
                    EditorGUILayout.PropertyField(m_FeaturedItemPrefab_SerializedProperty,
                        featuredItemPrefabContent);

                    EditorGUILayout.PropertyField(m_FeaturedBundleItemPrefab_SerializedProperty,
                        featBundlePrefabContent);
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
