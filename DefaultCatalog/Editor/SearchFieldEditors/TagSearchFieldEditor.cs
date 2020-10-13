using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class TagSearchFieldEditor
    {
        readonly SearchField m_TagSearchField = new SearchField();
        string m_TagSearchString = string.Empty;

        public void OnGUI()
        {
            CollectionEditorTools.SetGUIEnabledAtRunTime(true);

            EditorGUILayout.Space();
            m_TagSearchString = m_TagSearchField.OnGUI(m_TagSearchString, GUILayout.Width(GameFoundationEditorStyles.sideBarWidth - 25));
            EditorGUILayout.Space();

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);
        }

        public List<TagAsset> GetFoundItems(List<TagAsset> fullList)
        {
            if (fullList == null)
            {
                return new List<TagAsset>();
            }

            if (string.IsNullOrEmpty(m_TagSearchString))
            {
                return fullList;
            }

            return fullList.FindAll(item =>
                item.key.ToLowerInvariant().Contains(m_TagSearchString.ToLowerInvariant())
            );
        }
    }
}
