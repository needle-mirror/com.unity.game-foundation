using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Abstract class which sets up an editor window with a tab bar at the top.
    /// </summary>
    abstract class CollectionEditorWindowBase : EditorWindow
    {
        int m_TabBarIndex { get; set; }
        int m_OldTabBarIndex { get; set; }

        protected List<ICollectionEditor> m_Editors { get; } = new List<ICollectionEditor>();

        string[] editorNames
        {
            get
            {
                var items = new string[m_Editors.Count];
                for (int i = 0; i < m_Editors.Count; i++)
                {
                    items[i] = m_Editors[i].name;
                }

                return items;
            }
        }

        protected virtual void OnEnable()
        {
            minSize = new Vector2(850f, 400f);

            m_TabBarIndex = 0;
            m_OldTabBarIndex = -1;

            CreateEditors();
        }

        protected virtual void OnFocus()
        {
            foreach (var collectionEditor in m_Editors)
            {
                collectionEditor.ValidateSelection();
            }

            if (m_Editors != null && m_Editors.Count > 0 && m_TabBarIndex < m_Editors.Count)
            {
                m_Editors[m_TabBarIndex].RefreshItems();
            }
        }

        void OnGUI()
        {
            // TAB BAR

            if (m_Editors.Count > 1)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    m_TabBarIndex = GUILayout.Toolbar(m_TabBarIndex, editorNames, GameFoundationEditorStyles.topToolbarStyle);
                }
            }

            if (m_TabBarIndex < 0 || m_TabBarIndex >= m_Editors.Count || m_Editors.Count == 0)
            {
                m_TabBarIndex = 0;
            }

            EditorGUILayout.Space();

            if (m_TabBarIndex != m_OldTabBarIndex)
            {
                // if trying to switch tabs while in the middle of creating a new item, show a confirmation dialog
                if (m_OldTabBarIndex >= 0
                    && m_Editors[m_OldTabBarIndex].isCreating
                    && !CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    // clicked "Stay" - prevent switching tabs - change the index back
                    m_TabBarIndex = m_OldTabBarIndex;
                    return;
                }

                if (m_OldTabBarIndex >= 0 && m_OldTabBarIndex < m_Editors.Count)
                {
                    m_Editors[m_OldTabBarIndex].OnWillExit();
                }

                m_Editors[m_TabBarIndex].OnWillEnter();

                m_OldTabBarIndex = m_TabBarIndex;
            }

            m_Editors[m_TabBarIndex].Draw();

            // if you click anywhere that isn't a GUI control, then clear the focus
            // do not use a big button here - it will block the context click no matter what
            // if (GUI.Button(new Rect(0f, 0f, position.width, position.height), "", GUIStyle.none))
            if (new Rect(0f, 0f, position.width, position.height).Contains(Event.current.mousePosition)
                && Event.current.type == EventType.MouseUp)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        /// <summary>
        ///     Abstract method where editors for the implementing system's tabs should be added to the window.
        /// </summary>
        protected abstract void CreateEditors();
    }
}
