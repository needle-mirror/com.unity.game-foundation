using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for an <see cref="InventoryItemHudView"/> component.
    /// </summary>
    [CustomEditor(typeof(InventoryItemHudView))]
    class InventoryItemHudViewEditor : Editor
    {
        InventoryItemHudView m_InventoryItemHudView;

        int m_SelectedItemIndex = -1;
        int m_SelectedIconPropertyIndex = -1;

        DropdownCatalogItemHelper<InventoryItemDefinitionAsset> m_ItemDropdownHelper = new DropdownCatalogItemHelper<InventoryItemDefinitionAsset>();
        DropdownStaticPropertyHelper m_IconPropertyDropdownHelper = new DropdownStaticPropertyHelper();

        SerializedProperty m_ItemDefinitionKey_SerializedProperty;
        SerializedProperty m_IconImageField_SerializedProperty;
        SerializedProperty m_QuantityText_SerializedProperty;
        SerializedProperty m_IconAssetPropertyKey_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(InventoryItemHudView.m_ItemDefinitionKey),
            nameof(InventoryItemHudView.m_IconImageField),
            nameof(InventoryItemHudView.m_QuantityTextField),
            nameof(InventoryItemHudView.m_IconSpritePropertyKey)
        };

        void OnEnable()
        {
            m_InventoryItemHudView = target as InventoryItemHudView;

            m_ItemDefinitionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_ItemDefinitionKey));
            m_IconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_IconImageField));
            m_QuantityText_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_QuantityTextField));
            m_IconAssetPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_IconSpritePropertyKey));

            PopulateItems();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            if (!Application.isPlaying)
            {
                m_InventoryItemHudView.UpdateContent();
            }
        }

        void PopulateItems()
        {
            m_SelectedItemIndex = m_ItemDropdownHelper.Populate(m_ItemDefinitionKey_SerializedProperty.stringValue);
        }

        void PopulateStaticPropertyKeys()
        {
            m_SelectedIconPropertyIndex = m_IconPropertyDropdownHelper.Populate(
                CatalogSettings.catalogAsset.FindItem(m_ItemDefinitionKey_SerializedProperty.stringValue) as InventoryItemDefinitionAsset,
                m_IconAssetPropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var itemDisplayContent = new GUIContent("Item", "The Inventory Item to display.");
            if (m_ItemDropdownHelper.displayNames != null && m_ItemDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedItemIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedItemIndex, m_ItemDropdownHelper.displayNames);
                var itemKey = m_ItemDropdownHelper.GetKey(m_SelectedItemIndex);
                if (m_InventoryItemHudView.itemDefinitionKey != itemKey)
                {
                    SetSelectedInventoryItemKey(itemKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
            }

            EditorGUILayout.Space();

            var iconPropertyKeyContent = new GUIContent("Icon Asset Property Key", "The key for the sprite that is defined in the Static Property of Inventory Item. " +
                "If none is specified no image will be displayed.");
            m_SelectedIconPropertyIndex = EditorGUILayout.Popup(iconPropertyKeyContent, m_SelectedIconPropertyIndex, m_IconPropertyDropdownHelper.displayNames);
            var iconAssetPropertyKey = m_IconPropertyDropdownHelper.GetKey(m_SelectedIconPropertyIndex);
            if (m_InventoryItemHudView.iconSpritePropertyKey != iconAssetPropertyKey)
            {
                m_InventoryItemHudView.SetIconSpritePropertyKey(iconAssetPropertyKey);
                m_IconAssetPropertyKey_SerializedProperty.stringValue = iconAssetPropertyKey;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemImageFieldContent = new GUIContent("Item Image Field", "The Image component in which to display Currency icon sprite.");
                var itemImageField = (Image)EditorGUILayout.ObjectField(itemImageFieldContent,
                    m_InventoryItemHudView.iconImageField, typeof(Image), true);

                if (check.changed)
                {
                    m_InventoryItemHudView.SetIconImageField(itemImageField);
                    if (m_IconImageField_SerializedProperty != null)
                    {
                        m_IconImageField_SerializedProperty.objectReferenceValue = itemImageField;
                    }
                }
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemQuantityFieldContent = new GUIContent("Item Quantity Text Field",
                    "Text component in which to display Inventory item quantity.");
                var itemQuantityField = (Text)EditorGUILayout.ObjectField(itemQuantityFieldContent,
                    m_InventoryItemHudView.quantityTextField, typeof(Text), true);

                if (check.changed)
                {
                    m_InventoryItemHudView.SetQuantityTextField(itemQuantityField);
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

        void SetSelectedInventoryItemKey(string key)
        {
            // Update the serialized value
            m_ItemDefinitionKey_SerializedProperty.stringValue = key;

            // Update Component's state
            m_InventoryItemHudView.SetItemDefinition(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateStaticPropertyKeys();
        }
    }
}
