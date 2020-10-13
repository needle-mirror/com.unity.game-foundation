using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="TransactionItemView"/> component.
    /// </summary>
    [CustomEditor(typeof(TransactionItemView))]
    class TransactionItemViewEditor : Editor
    {
        TransactionItemView m_TransactionItemView;

        int m_SelectedTransactionIndex = -1;
        int m_SelectedItemIconPropertyIndex = -1;
        int m_SelectedPriceIconPropertyIndex = -1;

        DropdownCatalogItemHelper<BaseTransactionAsset> m_TransactionDropdownHelper = new DropdownCatalogItemHelper<BaseTransactionAsset>();
        DropdownStaticPropertyHelper m_ItemIconPropertyDropdownHelper = new DropdownStaticPropertyHelper();
        DropdownPayoutItemPropertyHelper m_PriceIconPropertyDropdownHelper = new DropdownPayoutItemPropertyHelper();

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_ItemIconAssetPropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_ItemIconImageField_SerializedProperty;
        SerializedProperty m_ItemNameTextField_SerializedProperty;
        SerializedProperty m_PurchaseButton_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ShowButtonEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(TransactionItemView.m_TransactionKey),
            nameof(TransactionItemView.m_ItemIconSpritePropertyKey),
            nameof(TransactionItemView.m_PriceIconSpritePropertyKey),
            nameof(TransactionItemView.m_NoPriceString),
            nameof(TransactionItemView.m_ItemIconImageField),
            nameof(TransactionItemView.m_ItemNameTextField),
            nameof(TransactionItemView.m_PurchaseButton),
            nameof(TransactionItemView.m_Interactable),
            nameof(TransactionItemView.showButtonEditorFields)
        };

        void OnEnable()
        {
            m_TransactionItemView = target as TransactionItemView;

            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_TransactionKey));
            m_ItemIconAssetPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconSpritePropertyKey));
            m_PriceIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PriceIconSpritePropertyKey));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_NoPriceString));
            m_ItemIconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconImageField));
            m_ItemNameTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemNameTextField));
            m_PurchaseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PurchaseButton));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_Interactable));
            m_ShowButtonEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.showButtonEditorFields));

            PopulateTransactions();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            m_TransactionItemView.UpdateContent();
        }

        void PopulateTransactions()
        {
            m_SelectedTransactionIndex = m_TransactionDropdownHelper.Populate(m_TransactionKey_SerializedProperty.stringValue);
        }

        void PopulateStaticPropertyKeys()
        {
            m_SelectedItemIconPropertyIndex = m_ItemIconPropertyDropdownHelper.Populate(
                CatalogSettings.catalogAsset.FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset,
                m_ItemIconAssetPropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);

            m_SelectedPriceIconPropertyIndex = m_PriceIconPropertyDropdownHelper.Populate(
                CatalogSettings.catalogAsset.FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset,
                m_PriceIconSpritePropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true, true);
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

            DrawIconAndNameSection();
            EditorGUILayout.Space();

            DrawButtonSection();
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawTransactionSection()
        {
            var itemDisplayContent = new GUIContent("Transaction Item", "The Transaction Item to display in this button");
            if (m_TransactionDropdownHelper.displayNames != null && m_TransactionDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedTransactionIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedTransactionIndex, m_TransactionDropdownHelper.displayNames);
                var transactionKey = m_TransactionDropdownHelper.GetKey(m_SelectedTransactionIndex);
                if (m_TransactionItemView.transactionKey != transactionKey)
                {
                    SetSelectedItemKey(transactionKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
            }
        }

        void DrawRuntimeSection()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
                var interactable = EditorGUILayout.Toggle(interactableContent, m_TransactionItemView.interactable);

                if (check.changed)
                {
                    m_TransactionItemView.interactable = m_Interactable_SerializedProperty.boolValue = interactable;
                }
            }
        }

        void DrawIconAndNameSection()
        {
            if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
            {
                var itemIconPropertyKeyContent = new GUIContent("Icon Asset Property Key",
                    "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                    "If none is specified no image will be displayed.");
                m_SelectedItemIconPropertyIndex = EditorGUILayout.Popup(itemIconPropertyKeyContent,
                    m_SelectedItemIconPropertyIndex, m_ItemIconPropertyDropdownHelper.displayNames);
                var iconAssetPropertyKey = m_ItemIconPropertyDropdownHelper.GetKey(m_SelectedItemIconPropertyIndex);
                if (m_TransactionItemView.itemIconSpritePropertyKey != iconAssetPropertyKey)
                {
                    m_TransactionItemView.SetItemIconSpritePropertyKey(iconAssetPropertyKey);
                    m_ItemIconAssetPropertyKey_SerializedProperty.stringValue = iconAssetPropertyKey;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var imageIconFieldContent = new GUIContent("Icon Image Field", "The Image component in which to display item icon sprite.");
                var itemIconField = (Image)EditorGUILayout.ObjectField(imageIconFieldContent, m_TransactionItemView.itemIconImageField, typeof(Image), true);

                if (check.changed)
                {
                    m_TransactionItemView.SetItemIconImageField(itemIconField);
                    m_ItemIconImageField_SerializedProperty.objectReferenceValue = itemIconField;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemNameFieldContent = new GUIContent("Name Text Field", "Text component in which to display Store Item price.");
                var itemNameField = (Text)EditorGUILayout.ObjectField(itemNameFieldContent, m_TransactionItemView.itemNameTextField, typeof(Text), true);

                if (check.changed)
                {
                    m_TransactionItemView.SetItemNameTextField(itemNameField);
                    m_ItemNameTextField_SerializedProperty.objectReferenceValue = itemNameField;
                }
            }
        }

        void DrawButtonSection()
        {
            m_ShowButtonEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowButtonEditorFields_SerializedProperty.boolValue, "Buttons");
            if (m_ShowButtonEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
                {
                    var priceIconPropertyKeyContent = new GUIContent("Price Icon Asset Property",
                        "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                        "If none is specified no image will be displayed.");
                    m_SelectedPriceIconPropertyIndex = EditorGUILayout.Popup(priceIconPropertyKeyContent,
                        m_SelectedPriceIconPropertyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
                    var priceIconAssetPropertyKey =
                        m_PriceIconPropertyDropdownHelper.GetKey(m_SelectedPriceIconPropertyIndex);
                    if (m_TransactionItemView.priceIconSpritePropertyKey != priceIconAssetPropertyKey)
                    {
                        m_TransactionItemView.SetPriceIconSpritePropertyKey(priceIconAssetPropertyKey);
                        m_PriceIconSpritePropertyKey_SerializedProperty.stringValue = priceIconAssetPropertyKey;
                    }

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var noPriceStringContent = new GUIContent("No Price String",
                            "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
                        var noPriceString =
                            EditorGUILayout.TextField(noPriceStringContent, m_TransactionItemView.noPriceString);
                        if (check.changed)
                        {
                            m_TransactionItemView.SetNoPriceString(noPriceString);
                            m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                        }
                    }

                    EditorGUILayout.Space();
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var purchaseButtonContent = new GUIContent("Purchase Button", "PurchaseButton component to use when generating a button for purchasing item in this view.");
                    var purchaseButton = (PurchaseButton)EditorGUILayout.ObjectField(purchaseButtonContent, m_TransactionItemView.purchaseButton, typeof(PurchaseButton), true);

                    if (check.changed)
                    {
                        m_TransactionItemView.SetPurchaseButton(purchaseButton);
                        m_PurchaseButton_SerializedProperty.objectReferenceValue = purchaseButton;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        void SetSelectedItemKey(string key)
        {
            // Update the serialized value
            m_TransactionKey_SerializedProperty.stringValue = key;
            m_TransactionItemView.SetTransaction(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateStaticPropertyKeys();
        }
    }
}
