using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

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
            m_SelectedIconPropertyIndex = m_IconPropertyDropdownHelper.Populate(PrefabTools.GetLookUpCatalogAsset()
                    .FindItem(m_ItemDefinitionKey_SerializedProperty.stringValue) as InventoryItemDefinitionAsset,
                m_IconAssetPropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });
        }

        public override void OnInspectorGUI()
        {
            var itemDisplayContent = new GUIContent("Item", "The Inventory Item to display.");
            var iconPropertyKeyContent = new GUIContent("Icon Asset Property Key", 
                "The key for the sprite that is defined in the Static Property of Inventory Item. " +
                "If none is specified no image will be displayed.");
            var itemImageFieldContent = new GUIContent("Item Image Field", 
                "The Image component in which to display Currency icon sprite.");
            var itemQuantityFieldContent = new GUIContent("Item Quantity Text Field",
                "TextMeshProUGUI component in which to display Inventory item quantity.");

            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedItemIndex = EditorGUILayout.Popup(
                    itemDisplayContent, m_SelectedItemIndex, m_ItemDropdownHelper.displayNames);
                m_ItemDefinitionKey_SerializedProperty.stringValue = m_ItemDropdownHelper.GetKey(m_SelectedItemIndex);

                if (check.changed)
                {
                    PopulateStaticPropertyKeys();
                }
            }

            EditorGUILayout.Space();

            m_SelectedIconPropertyIndex = EditorGUILayout.Popup(
                iconPropertyKeyContent, m_SelectedIconPropertyIndex, m_IconPropertyDropdownHelper.displayNames);
            m_IconAssetPropertyKey_SerializedProperty.stringValue = m_IconPropertyDropdownHelper
                .GetKey(m_SelectedIconPropertyIndex);

            EditorGUILayout.PropertyField(m_IconImageField_SerializedProperty, itemImageFieldContent);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_QuantityText_SerializedProperty, itemQuantityFieldContent);

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
