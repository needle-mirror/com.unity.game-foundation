using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="PurchaseButton"/> component.
    /// </summary>
    [CustomEditor(typeof(PurchaseButton))]
    class PurchaseButtonEditor : Editor
    {
        PurchaseButton m_PurchaseButton;

        int m_SelectedTransactionIndex = -1;
        int m_SelectedPriceIconKeyIndex = -1;

        DropdownCatalogItemHelper<BaseTransactionAsset> m_TransactionDropdownHelper = new DropdownCatalogItemHelper<BaseTransactionAsset>();
        DropdownCostItemPropertyHelper m_PriceIconPropertyDropdownHelper = new DropdownCostItemPropertyHelper();

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_PriceIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconImageField_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_PriceText_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(PurchaseButton.m_TransactionKey),
            nameof(PurchaseButton.m_PriceIconImageField),
            nameof(PurchaseButton.m_PriceTextField),
            nameof(PurchaseButton.m_NoPriceString),
            nameof(PurchaseButton.m_PriceIconSpritePropertyKey),
            nameof(PurchaseButton.m_Interactable)
        };

        void OnEnable()
        {
            m_PurchaseButton = target as PurchaseButton;

            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_TransactionKey));
            m_PriceIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceIconSpritePropertyKey));
            m_PriceIconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceIconImageField));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_NoPriceString));
            m_PriceText_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceTextField));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_Interactable));

            PopulateTransactions();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            m_PurchaseButton.UpdateContent();
        }

        void PopulateTransactions()
        {
            m_SelectedTransactionIndex = m_TransactionDropdownHelper.Populate(m_TransactionKey_SerializedProperty.stringValue);
        }

        void PopulateStaticPropertyKeys()
        {
            m_SelectedPriceIconKeyIndex = m_PriceIconPropertyDropdownHelper.Populate(PrefabTools.GetLookUpCatalogAsset()
                    .FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset, 
                m_PriceIconSpritePropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });
        }

        public override void OnInspectorGUI()
        {
            var itemDisplayContent = new GUIContent("Transaction Item",
                "The Transaction Item to display in this button");
            var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
            var iconPropertyKeyContent = new GUIContent("Price Icon Asset Property",
                "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                "If none is specified no image will be displayed.");
            var noPriceStringContent = new GUIContent("No Price String",
                "String to display when there is no cost defined in the Transaction Item.");
            var priceIconImageContent = new GUIContent("Price Image Field",
                "The Image component in which to display price icon sprite.");
            var priceTextContent = new GUIContent("Price Text Field",
                "The Text component in which to display Transaction Item price.");

            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (m_PurchaseButton.m_IsDrivenByOtherComponent)
            {
                EditorGUILayout.HelpBox("Some settings are driven by Transaction Item View", MessageType.None);
            }
            else
            {
                PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_SelectedTransactionIndex = EditorGUILayout.Popup(
                        itemDisplayContent, m_SelectedTransactionIndex, m_TransactionDropdownHelper.displayNames);
                    m_TransactionKey_SerializedProperty.stringValue = m_TransactionDropdownHelper
                        .GetKey(m_SelectedTransactionIndex);

                    if (check.changed)
                    {
                        PopulateStaticPropertyKeys();
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_Interactable_SerializedProperty, interactableContent);

                EditorGUILayout.Space();

                m_SelectedPriceIconKeyIndex = EditorGUILayout.Popup(
                    iconPropertyKeyContent, m_SelectedPriceIconKeyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
                m_PriceIconSpritePropertyKey_SerializedProperty.stringValue = m_PriceIconPropertyDropdownHelper
                    .GetKey(m_SelectedPriceIconKeyIndex);

                EditorGUILayout.PropertyField(m_NoPriceString_SerializedProperty, noPriceStringContent);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_PriceIconImageField_SerializedProperty, priceIconImageContent);
            EditorGUILayout.PropertyField(m_PriceText_SerializedProperty, priceTextContent);

            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
