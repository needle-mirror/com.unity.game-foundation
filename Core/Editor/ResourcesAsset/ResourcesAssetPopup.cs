using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    class ResourcesAssetPopup : PopupWindowContent
    {
        ResourcesAssetTreeView m_Tree;

        TreeViewState m_TreeState;

        bool m_ShouldClose;

        string m_CurrentName = string.Empty;

        Rect m_Position;

        readonly ResourcesAssetDrawer m_Drawer;

        readonly SearchField m_SearchField;

        public ResourcesAssetPopup(Rect position, ResourcesAssetDrawer drawer)
        {
            m_Position = position;
            m_Drawer = drawer;
            m_SearchField = new SearchField();
            m_ShouldClose = false;
        }

        public void ForceClose()
        {
            m_ShouldClose = true;
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public override Vector2 GetWindowSize()
        {
            var result = base.GetWindowSize();
            result.x = m_Position.width;
            return result;
        }

        public override void OnGUI(Rect rect)
        {
            var border = 4;
            var topPadding = 12;
            var searchHeight = 20;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainTop = topPadding + searchHeight + border;
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2, rect.height - remainTop - border);
            m_CurrentName = m_SearchField.OnGUI(searchRect, m_CurrentName);

            if (m_Tree == null)
            {
                if (m_TreeState == null)
                    m_TreeState = new TreeViewState();

                m_Tree = new ResourcesAssetTreeView(m_TreeState, m_Drawer, this);
                m_Tree.Reload();
            }

            m_Tree.searchString = m_CurrentName;
            m_Tree.OnGUI(remainingRect);

            if (m_ShouldClose)
            {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
            }
        }
    }
}
