using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     The Editor of a <see cref="CurrencyHudView"/> instance.
    /// </summary>
    [CustomEditor(typeof(CurrencyHudView))]
    class CurrencyHudViewEditor : Editor
    {
        CurrencyHudView m_CurrencyHudView;

        int m_SelectedCurrencyIndex = -1;
        int m_SelectedIconPropertyIndex = -1;

        DropdownCatalogItemHelper<CurrencyAsset> m_CurrencyDropdownHelper = new DropdownCatalogItemHelper<CurrencyAsset>();
        DropdownStaticPropertyHelper m_IconPropertyDropdownHelper = new DropdownStaticPropertyHelper();

        SerializedProperty m_CurrencyKey_SerializedProperty;
        SerializedProperty m_IconImageField_SerializedProperty;
        SerializedProperty m_QuantityText_SerializedProperty;
        SerializedProperty m_IconAssetPropertyKey_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(CurrencyHudView.m_CurrencyKey),
            nameof(CurrencyHudView.m_IconImageField),
            nameof(CurrencyHudView.m_QuantityTextField),
            nameof(CurrencyHudView.m_IconSpritePropertyKey)
        };

        void OnEnable()
        {
            m_CurrencyHudView = target as CurrencyHudView;

            m_CurrencyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_CurrencyKey));
            m_IconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_IconImageField));
            m_QuantityText_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_QuantityTextField));
            m_IconAssetPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_IconSpritePropertyKey));

            PopulateCurrencies();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            m_CurrencyHudView.UpdateContent();
        }

        void PopulateCurrencies()
        {
            m_SelectedCurrencyIndex = m_CurrencyDropdownHelper.Populate(m_CurrencyKey_SerializedProperty.stringValue);
        }

        void PopulateStaticPropertyKeys()
        {
            m_SelectedIconPropertyIndex = m_IconPropertyDropdownHelper.Populate(
                CatalogSettings.catalogAsset.FindItem(m_CurrencyKey_SerializedProperty.stringValue) as CurrencyAsset,
                m_IconAssetPropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);
        }

        /// <summary>
        ///     Draws the inspector for the <see cref="CurrencyHudView"/>.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var itemDisplayContent = new GUIContent("Currency", "The Currency to display.");
            if (m_CurrencyDropdownHelper.displayNames != null && m_CurrencyDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedCurrencyIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedCurrencyIndex, m_CurrencyDropdownHelper.displayNames);
                var currencyKey = m_CurrencyDropdownHelper.GetKey(m_SelectedCurrencyIndex);
                if (m_CurrencyHudView.currencyKey != currencyKey)
                {
                    SetSelectedCurrencyKey(currencyKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
            }

            EditorGUILayout.Space();

            var iconPropertyKeyContent = new GUIContent("Icon Asset Property Key", "The key for the sprite that is defined in the Static Property of Currency. " +
                "If none is specified no image will be displayed.");
            m_SelectedIconPropertyIndex = EditorGUILayout.Popup(iconPropertyKeyContent, m_SelectedIconPropertyIndex, m_IconPropertyDropdownHelper.displayNames);
            var iconAssetPropertyKey = m_IconPropertyDropdownHelper.GetKey(m_SelectedIconPropertyIndex);
            if (m_CurrencyHudView.iconSpritePropertyKey != iconAssetPropertyKey)
            {
                m_CurrencyHudView.SetIconSpritePropertyKey(iconAssetPropertyKey);
                m_IconAssetPropertyKey_SerializedProperty.stringValue = iconAssetPropertyKey;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemImageFieldContent = new GUIContent("Item Image Field", "The Image component in which to display Currency icon sprite.");
                var itemImageField = (Image)EditorGUILayout.ObjectField(itemImageFieldContent, m_CurrencyHudView.iconImageField, typeof(Image), true);

                if (check.changed)
                {
                    m_CurrencyHudView.SetIconImageField(itemImageField);
                    if (m_IconImageField_SerializedProperty != null)
                    {
                        m_IconImageField_SerializedProperty.objectReferenceValue = itemImageField;
                    }
                }
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemQuantityFieldContent = new GUIContent("Item Quantity Text Field", "Text component in which to display Currency quantity.");
                var itemQuantityField = (Text)EditorGUILayout.ObjectField(itemQuantityFieldContent, m_CurrencyHudView.quantityTextField, typeof(Text), true);

                if (check.changed)
                {
                    m_CurrencyHudView.SetQuantityTextField(itemQuantityField);
                    if (m_QuantityText_SerializedProperty != null)
                    {
                        m_QuantityText_SerializedProperty.objectReferenceValue = itemQuantityField;
                    }
                }
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void SetSelectedCurrencyKey(string key)
        {
            // Update the serialized value
            m_CurrencyKey_SerializedProperty.stringValue = key;

            // Update Component's state
            m_CurrencyHudView.SetCurrency(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateStaticPropertyKeys();
        }
    }
}
