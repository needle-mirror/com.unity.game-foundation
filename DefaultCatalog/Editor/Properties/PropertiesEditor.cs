using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class PropertiesEditor
    {
        public static readonly GUIContent staticPropertiesLabel = new GUIContent(
            "Static Properties",
            "Store a set of fixed data inside your catalog item you can read at runtime.");

        protected static readonly GUIContent k_KeyLabel = new GUIContent(
            "Key",
            "This property's identifier. May be visible in the UI as well as code and logs. Must be alphanumeric, with at least one letter. Dashes (-) and underscores (_) allowed.");

        protected static readonly GUIContent k_TypeLabel = new GUIContent(
            "Type",
            "This property's value type. It cannot be changed at runtime.");

        protected static readonly GUIContent k_ValueLabel = new GUIContent(
            "Value",
            "This property's value. It cannot be changed at runtime.");

        protected static readonly GUIContent k_AddButton = new GUIContent(
            "Add",
            "A property can be added only if the new key isn't already used and is alphanumeric, with at least one letter, dashes (-) and underscores (_) allowed.");

        protected static readonly GUIContent k_EmptyStaticCollectionLabel
            = new GUIContent("No static properties configured");

        protected const float k_KeyWidth = 140;

        protected const float k_TypeWidth = 120;

        protected const float k_ActionButtonWidth = 30;

        protected static string[] s_PropertyTypeNames =
        {
            k_LongPropertyDisplayName,
            k_DoublePropertyDisplayName,
            nameof(PropertyType.Bool),
            nameof(PropertyType.String),
            k_ResourcesAssetPropertyDisplayName,
            k_AddressablesAssetPropertyDisplayName,
        };

        static readonly GUIContent k_LongPropertyLabel = new GUIContent(
            k_LongPropertyDisplayName,
            "This property can store integer values in the range of a long.\nIt can be cast into a long or an int if the value is small enough.");

        static readonly GUIContent k_DoublePropertyLabel = new GUIContent(
            k_DoublePropertyDisplayName,
            "This property can store floating-point values in the range of a double.\nIt can be cast into a float or a double.");

        static readonly GUIContent k_BoolPropertyLabel = new GUIContent(
            nameof(PropertyType.Bool),
            "This property stores a boolean value.\nIt can be cast into a bool.");

        static readonly GUIContent k_StringPropertyLabel = new GUIContent(
            nameof(PropertyType.String),
            "This property stores a string value.\nIt can be cast into a string.");

        static readonly GUIContent k_AssetPropertyLabel = new GUIContent(
            k_ResourcesAssetPropertyDisplayName,
            "This property stores an asset from a Resources folder.\nUse AsAsset<>() to get the asset reference.");

        static readonly GUIContent k_AddressablesPropertyLabel = new GUIContent(
            k_AddressablesAssetPropertyDisplayName,
            "This property stores an Addressables address.\nIt can be cast into a AsyncOperationHandle used to load an Addressables asset asynchronously.");

        const string k_LongPropertyDisplayName = "Integer number";

        const string k_DoublePropertyDisplayName = "Double";

        const string k_ResourcesAssetPropertyDisplayName = "Resources Asset";

        const string k_AddressablesAssetPropertyDisplayName = "Addressables";


        protected static GUIContent GetLabelFor(PropertyType type)
        {
            switch (type)
            {
                case PropertyType.Long:
                    return k_LongPropertyLabel;

                case PropertyType.Double:
                    return k_DoublePropertyLabel;

                case PropertyType.Bool:
                    return k_BoolPropertyLabel;

                case PropertyType.String:
                    return k_StringPropertyLabel;

                case PropertyType.ResourcesAsset:
                    return k_AssetPropertyLabel;

                case PropertyType.Addressables:
                    return k_AddressablesPropertyLabel;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    class PropertiesEditor<TCatalogItemAsset> : PropertiesEditor
        where TCatalogItemAsset : CatalogItemAsset
    {
        protected TCatalogItemAsset m_Asset;

        /// <summary>
        ///     One drawer for each property.
        /// </summary>
        /// <remarks>
        ///     Using only one drawer for all properties doesn't work because it will only edit the 1st property's field.
        ///     TODO: Fix the current drawer implementation so we can use one drawer for all properties.
        /// </remarks>
        readonly Dictionary<string, ResourcesAssetDrawer> m_ResourcesAssetDrawers
            = new Dictionary<string, ResourcesAssetDrawer>();
        readonly Dictionary<string, AddressablesDropdownDrawer> m_AddressablesDropdownDrawers
            = new Dictionary<string, AddressablesDropdownDrawer>();

        string m_NewPropertyName = "";

        PropertyType m_SelectedPropertyType;

        public void SelectItem(TCatalogItemAsset asset)
        {
            m_Asset = asset;

            m_NewPropertyName = "";

            m_SelectedPropertyType = default;
        }

        public void Draw()
        {
            var properties = GetAssetProperties();

            DrawExistingProperties(properties);

            //Draw horizontal separator.
            var separator = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(separator, EditorGUIUtility.isProSkin ? Color.black : Color.gray);

            DrawPropertyCreation(properties);
        }

        void DrawExistingProperties(Dictionary<string, ExternalizableValue<Property>> properties)
        {
            //Draw list header.
            using (new GUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
            {
                GUILayout.Label(
                    k_KeyLabel,
                    GameFoundationEditorStyles.tableViewToolbarTextStyle,
                    GUILayout.Width(k_KeyWidth));
                GUILayout.Label(
                    k_TypeLabel,
                    GameFoundationEditorStyles.tableViewToolbarTextStyle,
                    GUILayout.Width(k_TypeWidth));
                GUILayout.Label(GetValueLabel(), GameFoundationEditorStyles.tableViewToolbarTextStyle);

                //"X" column.
                GUILayout.Space(k_ActionButtonWidth);
            }

            if (properties.Count <= 0)
            {
                EditorGUILayout.Space();

                GUILayout.Label(GetEmptyCollectionLabel(), GameFoundationEditorStyles.centeredGrayLabel);

                EditorGUILayout.Space();

                return;
            }

            string additionalInfoString = null;

            //Duplicate keys to be able to edit the dictionary in the foreach loop.
            var propertyKeys = new List<string>(properties.Keys);
            string toDelete = null;

            foreach (var propertyKey in propertyKeys)
            {
                //Draw row: Property key | value type | default value | delete button
                using (new GUILayout.HorizontalScope())
                {
                    // show the key for this property
                    // detect clicks on key or property so we can select Addressables asset later
                    var selectPropertyClicked = GUILayout.Button(new GUIContent(propertyKey),
                        EditorStyles.label, GUILayout.Width(k_KeyWidth));

                    // show property type (i.e. "Integer number", "Addressables", etc)
                    Property property = properties[propertyKey];
                    var propertyLabel = GetLabelFor(property.type);
                    if (GUILayout.Button(new GUIContent(propertyLabel),
                        EditorStyles.label, GUILayout.Width(k_TypeWidth)))
                    {
                        selectPropertyClicked = true;
                    }

                    var controlName = $"valueField{propertyKey}";
                    GUI.SetNextControlName(controlName);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        switch (property.type)
                        {
                            case PropertyType.Long:
                            {
                                property.longValue = EditorGUILayout.LongField(property.longValue);

                                break;
                            }

                            case PropertyType.Double:
                            {
                                property.doubleValue = EditorGUILayout.DoubleField(property.doubleValue);

                                break;
                            }

                            case PropertyType.Bool:
                            {
                                property.boolValue = EditorGUILayout.Toggle(property.boolValue);

                                break;
                            }

                            case PropertyType.String:
                            {
                                property.m_StringValue = EditorGUILayout.TextField(property.m_StringValue);

                                break;
                            }

                            case PropertyType.ResourcesAsset:
                            {
                                if (!m_ResourcesAssetDrawers.TryGetValue(propertyKey, out var drawer))
                                {
                                    drawer = new ResourcesAssetDrawer();
                                    m_ResourcesAssetDrawers[propertyKey] = drawer;
                                }

                                var position = EditorGUILayout.GetControlRect();
                                property.m_StringValue = drawer.Draw(position, property.m_StringValue, m_Asset);

                                break;
                            }

                            case PropertyType.Addressables:
                            {                                
                                if (!m_AddressablesDropdownDrawers.TryGetValue(propertyKey, out var drawer))
                                {
                                    drawer = new AddressablesDropdownDrawer();
                                    m_AddressablesDropdownDrawers[propertyKey] = drawer;
                                }
                                
                                var addressablesPropertyResult = drawer.Draw(property.m_StringValue, selectPropertyClicked);
                                property.m_StringValue = addressablesPropertyResult.updatedAddressablesAddress;
                                additionalInfoString = addressablesPropertyResult.additionalInfo;

                                break;
                            }

                            default:
                                throw new NotSupportedException(
                                    $"{nameof(PropertiesEditor)}: Cannot draw field for {nameof(property)} " +
                                    $"because it is of unsupported type: {property.type}");
                        }

                        if (check.changed)
                        {
                            //Update asset.
                            UpdateProperty(propertyKey, property);
                        }
                    }

                    // add the delete button
                    if (GUILayout.Button(
                        "X",
                        GameFoundationEditorStyles.tableViewButtonStyle,
                        GUILayout.Width(k_ActionButtonWidth)))
                    {
                        toDelete = propertyKey;
                    }
                }
            }

            // if needed, add the additional info (i.e. "* Warning: ...") at the bottom of the window
            if (!(additionalInfoString is null))
            {
                GUILayout.Label(additionalInfoString, GameFoundationEditorStyles.addressablesWarningTextStyle);
            }

            //Do any actual deletion outside the rendering loop to prevent sync issues.
            if (toDelete != null)
            {
                //Update asset.
                RemoveProperty(toDelete);
                m_ResourcesAssetDrawers.Remove(toDelete);
            }
        }

        void DrawPropertyCreation(IDictionary<string, ExternalizableValue<Property>> properties)
        {
            using (new GUILayout.HorizontalScope())
            {
                m_NewPropertyName = EditorGUILayout.TextField(
                    m_NewPropertyName,
                    GUILayout.Width(k_KeyWidth));

                m_SelectedPropertyType = (PropertyType)EditorGUILayout.Popup(
                    (int)m_SelectedPropertyType,
                    s_PropertyTypeNames,
                    GUILayout.Width(k_TypeWidth));

                GUILayout.FlexibleSpace();

                var disabled = !UnityEngine.GameFoundation.Tools.IsValidId(m_NewPropertyName)
                    || properties.ContainsKey(m_NewPropertyName);
                using (new EditorGUI.DisabledScope(disabled))
                {
                    var doAddProperty = GUILayout.Button(
                        k_AddButton,
                        GameFoundationEditorStyles.tableViewButtonStyle,
                        GUILayout.Width(k_ActionButtonWidth));

                    if (doAddProperty)
                    {
                        var property = new Property
                        {
                            type = m_SelectedPropertyType
                        };

                        //Update asset.
                        AddProperty(m_NewPropertyName, property);

                        m_SelectedPropertyType = default;
                        m_NewPropertyName = "";

                        //Lose focus to avoid a weird issue where the new property's
                        //default value will display its key. Even if it is a number.
                        GUI.FocusControl(null);
                    }
                }
            }
        }

        protected virtual Dictionary<string, ExternalizableValue<Property>> GetAssetProperties()
            => m_Asset.staticProperties;

        protected virtual GUIContent GetValueLabel()
            => k_ValueLabel;

        protected virtual GUIContent GetEmptyCollectionLabel()
            => k_EmptyStaticCollectionLabel;

        protected virtual void AddProperty(string propertyKey, Property value)
            => m_Asset.Editor_AddStaticProperty(propertyKey, value);

        protected virtual void RemoveProperty(string propertyKey)
            => m_Asset.Editor_RemoveStaticProperty(propertyKey);

        protected virtual void UpdateProperty(string propertyKey, Property value)
            => m_Asset.Editor_UpdateStaticProperty(propertyKey, value);
    }
}
