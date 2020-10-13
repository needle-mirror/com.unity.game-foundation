using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

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
        int m_SelectedPriceIconPropertyIndex = -1;

        DropdownCatalogItemHelper<BaseTransactionAsset> m_TransactionDropdownHelper = new DropdownCatalogItemHelper<BaseTransactionAsset>();
        DropdownPayoutItemPropertyHelper m_PriceIconPropertyDropdownHelper = new DropdownPayoutItemPropertyHelper();

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
            m_SelectedPriceIconPropertyIndex = m_PriceIconPropertyDropdownHelper.Populate(
                CatalogSettings.catalogAsset.FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset,
                m_PriceIconSpritePropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true, true);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (!m_PurchaseButton.m_IsDrivenByOtherComponent)
            {
                var itemDisplayContent = new GUIContent("Transaction Item", "The Transaction Item to display in this button");
                if (m_TransactionDropdownHelper.displayNames != null && m_TransactionDropdownHelper.displayNames.Length > 0)
                {
                    m_SelectedTransactionIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedTransactionIndex, m_TransactionDropdownHelper.displayNames);
                    var transactionKey = m_TransactionDropdownHelper.GetKey(m_SelectedTransactionIndex);
                    if (m_PurchaseButton.transactionKey != transactionKey)
                    {
                        SetSelectedItemKey(transactionKey);
                    }
                }
                else
                {
                    EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
                }

                EditorGUILayout.Space();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
                    var interactable = EditorGUILayout.Toggle(interactableContent, m_PurchaseButton.interactable);

                    if (check.changed)
                    {
                        m_PurchaseButton.interactable = m_Interactable_SerializedProperty.boolValue = interactable;
                    }
                }

                EditorGUILayout.Space();

                var iconPropertyKeyContent = new GUIContent("Price Icon Asset Property", "The key for the sprite that is defined in the Static Property of Transaction Item. " +
                    "If none is specified no image will be displayed.");
                m_SelectedPriceIconPropertyIndex = EditorGUILayout.Popup(iconPropertyKeyContent, m_SelectedPriceIconPropertyIndex, m_PriceIconPropertyDropdownHelper.displayNames);
                var priceIconPropertyKey = m_PriceIconPropertyDropdownHelper.GetKey(m_SelectedPriceIconPropertyIndex);
                if (m_PurchaseButton.priceIconSpritePropertyKey != priceIconPropertyKey)
                {
                    m_PurchaseButton.SetPriceIconSpritePropertyKey(priceIconPropertyKey);
                    m_PriceIconSpritePropertyKey_SerializedProperty.stringValue = priceIconPropertyKey;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var noPriceStringContent = new GUIContent("No Price String", "String to display when there is no cost defined in the Transaction Item.");
                    var noPriceString = EditorGUILayout.TextField(noPriceStringContent, m_PurchaseButton.noPriceString);
                    if (check.changed)
                    {
                        m_PurchaseButton.SetNoPriceString(noPriceString);
                        m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Some settings are driven by Transaction Item View", MessageType.None);
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var priceIconImageContent = new GUIContent("Price Image Field", "The Image component in which to display price icon sprite.");
                var priceIconImage = (Image)EditorGUILayout.ObjectField(priceIconImageContent,
                    m_PurchaseButton.priceIconImageField, typeof(Image), true);

                if (check.changed)
                {
                    m_PurchaseButton.SetPriceIconImageField(priceIconImage);
                    m_PriceIconImageField_SerializedProperty.objectReferenceValue = priceIconImage;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var priceTextContent = new GUIContent("Price Text Field", "The Text component in which to display Transaction Item price.");
                var priceText = (Text)EditorGUILayout.ObjectField(priceTextContent,
                    m_PurchaseButton.priceTextField, typeof(Text), true);

                if (check.changed)
                {
                    m_PurchaseButton.SetPriceTextField(priceText);
                    m_PriceText_SerializedProperty.objectReferenceValue = priceText;
                }
            }

            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void SetSelectedItemKey(string key)
        {
            // Update the serialized value
            m_TransactionKey_SerializedProperty.stringValue = key;
            m_PurchaseButton.SetTransaction(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateStaticPropertyKeys();
        }
    }
}
