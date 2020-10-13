using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class GameParameterAssetEditor : BaseCatalogAssetEditor<GameParameterAsset>
    {
        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.GameParameters;

        public GameParameterAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawCreateInputFields()
        {
            string displayName = null;
            m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_NewItemKey, ref displayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("key");
            }

            // display name is unused, but base class requires it not be empty to enable 'create' button so we simply use the key.
            m_NewItemDisplayName = m_NewItemKey;
        }

        protected override void DrawGeneralDetail(GameParameterAsset gameParameter)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = null;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentItemKey, ref displayName);
            }
        }
    }
}
