using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     UI Module for displayName and Key fields.
    ///     Contains logic for converting displayName to a Key
    ///     suggestion when Key field is empty.
    /// </summary>
    class ReadableNameKeyEditor
    {
        bool m_AutomaticKeyGenerationMode;
        readonly bool m_KeyEditingAllowedMode;
        readonly HashSet<string> m_OldKeys;

        static readonly GUIContent s_DisplayNameLabel = new GUIContent("Display Name", "This definition's name, as displayed in the UI and logs. Must be alphanumeric, with at least one letter. Spaces, special characters, and underscores are allowed.");
        static readonly GUIContent s_KeyLabel = new GUIContent("Key", "This definition's identifier within the application. May be visible in the UI as well as code and logs. Must be alphanumeric, with at least one letter.  Dashes (-) and underscores (_) allowed.");

        public ReadableNameKeyEditor(bool createNewMode, HashSet<string> oldKeys)
        {
            m_AutomaticKeyGenerationMode = createNewMode;
            m_KeyEditingAllowedMode = createNewMode;

            m_OldKeys = oldKeys;
        }

        /// <summary>
        ///     Draws UI input fields for displayName and Key.
        ///     Will create a suggested Key based on input displayName based
        ///     on the following conditions: displayName has a value, Key has
        ///     not been edited manually or is blank, displayName field loses focus by
        ///     tab or mouse click event, and item is being created new (vs editing existing item).
        ///     ref parameters may change what has been passed in.
        /// </summary>
        /// <param name="itemKey">Text to display for Key text field.</param>
        /// <param name="displayName">Text to display for display name text field.</param>
        public void DrawReadableNameKeyFields(ref string itemKey, ref string displayName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // handle display name (disabled for tags that do not have one)
                if (displayName != null)
                {
                    GUI.SetNextControlName("displayName");
                    displayName = EditorGUILayout.TextField(s_DisplayNameLabel, displayName);
                }

                if (m_KeyEditingAllowedMode)
                {
                    ConvertKeyIfNecessary(ref itemKey, ref displayName);

                    GUI.changed = false;
                    GUI.SetNextControlName("key");
                    itemKey = EditorGUILayout.TextField(s_KeyLabel, itemKey);
                    if (GUI.changed)
                    {
                        m_AutomaticKeyGenerationMode = false;
                    }

                    if (HasRegisteredKey(itemKey))
                    {
                        EditorGUILayout.HelpBox("The current Key conflicts with existing Keys.", MessageType.Error);
                    }
                    else if (!string.IsNullOrWhiteSpace(itemKey) && !CollectionEditorTools.IsValidId(itemKey))
                    {
                        EditorGUILayout.HelpBox("The current Key is not valid. A valid Key must begin with an " +
                            "alphabetic character, and may be followed by any number of alphanumeric characters, " +
                            "dashes (-) and/or underscores (_)", MessageType.Error);
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(s_KeyLabel, GUILayout.Width(145));
                        EditorGUILayout.SelectableLabel(itemKey, GUILayout.Height(15), GUILayout.ExpandWidth(true));
                    }
                }
            }
        }

        /// <summary>
        ///     Determines if the proposed Key will conflict with those registered in the system that this ReadableNameIdEditor
        ///     object exists in.
        /// </summary>
        /// <param name="itemKey">Key to check</param>
        /// <returns>Whether this Key is problematic</returns>
        public bool HasRegisteredKey(string itemKey)
        {
            return m_OldKeys.Contains(itemKey);
        }

        void ConvertKeyIfNecessary(ref string itemKey, ref string displayName)
        {
            var e = Event.current;

            // abort updating key for various reasons (incorrect mode, wrong field, key manually changed, etc.
            if (!e.Equals(Event.KeyboardEvent("tab")) && !e.type.Equals(EventType.MouseDown))
            {
                return;
            }
            if (GUI.GetNameOfFocusedControl() != "displayName" && GUI.GetNameOfFocusedControl() != "key")
            {
                return;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                return;
            }
            if (!m_AutomaticKeyGenerationMode && !string.IsNullOrEmpty(itemKey) &&
                CollectionEditorTools.IsValidId(itemKey))
            {
                return;
            }

            // turn back on auto-gen if key is empty
            if (string.IsNullOrEmpty(itemKey))
            {
                m_AutomaticKeyGenerationMode = true;
            }

            // auto-gen the key based on display name
            itemKey = CollectionEditorTools.CraftUniqueKey(displayName, m_OldKeys);
        }
    }
}
