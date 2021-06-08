using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="TransactionItemView"/> component.
    /// </summary>
    [CustomEditor(typeof(TransactionItemView))]
    class TransactionItemViewEditor : Editor
    {
        protected TransactionItemView m_TransactionItemView;

        int m_SelectedTransactionIndex = -1;
        int m_SelectedItemIconPropertyIndex = -1;
        int m_SelectedPriceIconPropertyIndex = -1;
        int m_SelectedBadgeTextPropertyIndex = -1;

        readonly DropdownCatalogItemHelper<BaseTransactionAsset> m_TransactionDropdownHelper = new DropdownCatalogItemHelper<BaseTransactionAsset>();
        readonly DropdownStaticPropertyHelper m_ItemIconPropertyDropdownHelper = new DropdownStaticPropertyHelper();
        readonly DropdownCostItemPropertyHelper m_PriceIconPropertyDropdownHelper = new DropdownCostItemPropertyHelper();
        readonly DropdownStaticPropertyHelper m_BadgeTextPropertyDropdownHelper = new DropdownStaticPropertyHelper();

        protected SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_ItemIconAssetPropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_BadgeTextPropertyKey_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_ItemIconImageField_SerializedProperty;
        SerializedProperty m_ItemNameTextField_SerializedProperty;
        SerializedProperty m_BadgeField_SerializedProperty;
        SerializedProperty m_PurchaseButton_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ShowButtonEditorFields_SerializedProperty;
        SerializedProperty m_ShowBadgeEditorFields_SerializedProperty;


        protected readonly List<string> kExcludedFields = new List<string>();

        void OnEnable()
        {
            m_TransactionItemView = target as TransactionItemView;

            Setup();
            PopulateTransactions();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            m_TransactionItemView.UpdateContent();
        }

        protected virtual void Setup()
        {
            kExcludedFields.Add("m_Script");
            kExcludedFields.Add(nameof(TransactionItemView.m_TransactionKey));
            kExcludedFields.Add(nameof(TransactionItemView.m_ItemIconSpritePropertyKey));
            kExcludedFields.Add(nameof(TransactionItemView.m_PriceIconSpritePropertyKey));
            kExcludedFields.Add(nameof(TransactionItemView.m_BadgeTextPropertyKey));
            kExcludedFields.Add(nameof(TransactionItemView.m_NoPriceString));
            kExcludedFields.Add(nameof(TransactionItemView.m_ItemIconImageField));
            kExcludedFields.Add(nameof(TransactionItemView.m_ItemNameTextField));
            kExcludedFields.Add(nameof(TransactionItemView.m_BadgeField));
            kExcludedFields.Add(nameof(TransactionItemView.m_PurchaseButton));
            kExcludedFields.Add(nameof(TransactionItemView.m_Interactable));
            kExcludedFields.Add(nameof(TransactionItemView.showBadgeEditorFields));
            kExcludedFields.Add(nameof(TransactionItemView.showButtonEditorFields));


            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_TransactionKey));
            m_ItemIconAssetPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconSpritePropertyKey));
            m_PriceIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PriceIconSpritePropertyKey));
            m_BadgeTextPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_BadgeTextPropertyKey));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_NoPriceString));
            m_ItemIconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconImageField));
            m_ItemNameTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemNameTextField));
            m_BadgeField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_BadgeField));
            m_PurchaseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PurchaseButton));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_Interactable));
            m_ShowBadgeEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.showBadgeEditorFields));
            m_ShowButtonEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.showButtonEditorFields));
        }

        void PopulateTransactions()
        {
            m_SelectedTransactionIndex = m_TransactionDropdownHelper.Populate(m_TransactionKey_SerializedProperty.stringValue);
        }

        protected virtual void PopulateStaticPropertyKeys()
        {
            var transactionItem = PrefabTools.GetLookUpCatalogAsset().FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset;

            m_SelectedItemIconPropertyIndex = m_ItemIconPropertyDropdownHelper.Populate(transactionItem,
                m_ItemIconAssetPropertyKey_SerializedProperty.stringValue, new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });

            m_SelectedPriceIconPropertyIndex = m_PriceIconPropertyDropdownHelper.Populate(transactionItem,
                m_PriceIconSpritePropertyKey_SerializedProperty.stringValue, new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });

            m_SelectedBadgeTextPropertyIndex = m_BadgeTextPropertyDropdownHelper.Populate(transactionItem,
                m_BadgeTextPropertyKey_SerializedProperty.stringValue, PropertyType.String);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (m_TransactionItemView.m_IsDrivenByOtherComponent)
            {
                EditorGUILayout.HelpBox("Some settings are driven by Store View", MessageType.None);
            }

            if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
            {
                DrawTransactionSection();
                EditorGUILayout.Space();

                DrawRuntimeSection();
                EditorGUILayout.Space();
            }

            DrawTransactionDetailSections();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields.ToArray());

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawTransactionSection()
        {
            var itemDisplayContent = new GUIContent("Transaction Item", 
                "The Transaction Item to display in this button");

            PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedTransactionIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedTransactionIndex,
                    m_TransactionDropdownHelper.displayNames);
                m_TransactionKey_SerializedProperty.stringValue = m_TransactionDropdownHelper
                    .GetKey(m_SelectedTransactionIndex);

                if (check.changed)
                {
                    PopulateStaticPropertyKeys();
                }
            }
        }

        void DrawRuntimeSection()
        {
            var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
            EditorGUILayout.PropertyField(m_Interactable_SerializedProperty, interactableContent);
        }

        protected virtual void DrawTransactionDetailSections()
        {
            DrawIconAndNameSection();
            EditorGUILayout.Space();

            DrawBadgeSection();
            EditorGUILayout.Space();

            DrawPurchaseButtonSection();
            EditorGUILayout.Space();
        }

        protected void DrawIconAndNameSection()
        {
            var itemIconPropertyKeyContent = new GUIContent("Icon Asset Property Key",
                "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                "If none is specified no image will be displayed.");
            var imageIconFieldContent = new GUIContent("Icon Image Field",
                "The Image component in which to display item icon sprite.");
            var itemNameFieldContent = new GUIContent("Name Text Field",
                "Text component in which to display Store Item price.");

            if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
            {
                m_SelectedItemIconPropertyIndex = EditorGUILayout.Popup(itemIconPropertyKeyContent,
                    m_SelectedItemIconPropertyIndex, m_ItemIconPropertyDropdownHelper.displayNames);
                m_ItemIconAssetPropertyKey_SerializedProperty.stringValue = m_ItemIconPropertyDropdownHelper
                    .GetKey(m_SelectedItemIconPropertyIndex);
            }
            
            EditorGUILayout.PropertyField(m_ItemIconImageField_SerializedProperty, imageIconFieldContent);
            EditorGUILayout.PropertyField(m_ItemNameTextField_SerializedProperty, itemNameFieldContent);
        }
        
        protected void DrawBadgeSection()
        {
            m_ShowBadgeEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowBadgeEditorFields_SerializedProperty.boolValue, "Badge", true);
            if (m_ShowBadgeEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var badgeTextPropertyKeyContent = new GUIContent("Badge Text Property Key",
                            "The key for the badge text that is defined in the Static Property of Transaction Item.");
                        m_SelectedBadgeTextPropertyIndex = EditorGUILayout.Popup(badgeTextPropertyKeyContent,
                            m_SelectedBadgeTextPropertyIndex, m_BadgeTextPropertyDropdownHelper.displayNames);

                        if (check.changed)
                        {
                            var badgeTextPropertyKey = m_BadgeTextPropertyDropdownHelper.GetKey(m_SelectedBadgeTextPropertyIndex);
                            m_TransactionItemView.SetBadgeTextPropertyKey(badgeTextPropertyKey);
                            m_BadgeTextPropertyKey_SerializedProperty.stringValue = badgeTextPropertyKey;
                        }
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var badgeFieldContent = new GUIContent("Badge Field",
                        "GameObject in which to display the transaction's badge.");
                    var badgeField = (ImageInfoView)EditorGUILayout.ObjectField(badgeFieldContent,
                        m_TransactionItemView.m_BadgeField, typeof(ImageInfoView), true);

                    if (check.changed)
                    {
                        m_TransactionItemView.SetBadgeField(badgeField);
                        m_BadgeField_SerializedProperty.objectReferenceValue = badgeField;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        protected void DrawPurchaseButtonSection()
        {
            var priceIconPropertyKeyContent = new GUIContent("Price Icon Asset Property",
                "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                "If none is specified no image will be displayed.");
            var noPriceStringContent = new GUIContent("No Price String",
                "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
            var purchaseButtonContent = new GUIContent("Purchase Button",
                "PurchaseButton component to use when generating a button for purchasing item in this view.");

            m_ShowButtonEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(
                m_ShowButtonEditorFields_SerializedProperty.boolValue, "Purchase Button", true);

            if (m_ShowButtonEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
                {
                    m_SelectedPriceIconPropertyIndex = EditorGUILayout.Popup(priceIconPropertyKeyContent,
                        m_SelectedPriceIconPropertyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
                    m_PriceIconSpritePropertyKey_SerializedProperty.stringValue =
                        m_PriceIconPropertyDropdownHelper.GetKey(m_SelectedPriceIconPropertyIndex);
                    
                    EditorGUILayout.PropertyField(m_NoPriceString_SerializedProperty, noPriceStringContent);

                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(m_PurchaseButton_SerializedProperty, purchaseButtonContent);

                EditorGUI.indentLevel--;
            }
        }
    }
}
