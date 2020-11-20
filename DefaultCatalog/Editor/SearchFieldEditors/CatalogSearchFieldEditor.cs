using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class CatalogSearchFieldEditor
    {
        readonly SearchField m_ItemSearchField = new SearchField();
        string m_ItemSearchString = string.Empty;

        public void OnGUI()
        {
            CollectionEditorTools.SetGUIEnabledAtRunTime(true);

            EditorGUILayout.Space();
            m_ItemSearchString = m_ItemSearchField.OnGUI(m_ItemSearchString, GUILayout.Width(GameFoundationEditorStyles.sideBarWidth - 25));
            EditorGUILayout.Space();

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);
        }

        public List<T> GetFoundItems<T>(List<T> fullList) where T : CatalogItemAsset
        {
            if (fullList == null)
            {
                return new List<T>();
            }

            if (string.IsNullOrEmpty(m_ItemSearchString))
            {
                return fullList;
            }

            return fullList.FindAll(item =>
                item.key.ToLowerInvariant().Contains(m_ItemSearchString.ToLowerInvariant()) ||
                item.displayName.currentValue.ToLowerInvariant().Contains(m_ItemSearchString.ToLowerInvariant())
            );
        }
    }
}
