using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    static class GameFoundationEditorStyles
    {
        public const string noValueLabelColor = "#808080";
        public static float tablePadding = 5;
        public const float sideBarWidth = 270f;
        public const float contentDetailMaxWidth = 630f;

        public static GUIStyle topToolbarStyle { get; } = new GUIStyle(EditorStyles.toolbarButton) { fixedHeight = 40 };
        public static GUIStyle boxStyle { get; } = new GUIStyle("HelpBox") { padding = new RectOffset(10, 10, 10, 10) };
        public static GUIStyle deleteButtonStyle { get; } = new GUIStyle("OL Minus");
        public static GUIStyle tableViewToolbarStyle { get; } = new GUIStyle(EditorStyles.toolbarButton);
        public static GUIStyle tableViewToolbarTextStyle { get; } = new GUIStyle(EditorStyles.miniLabel);
        public static GUIStyle tableViewButtonStyle { get; } = new GUIStyle("toolbarbutton");
        public static GUIStyle boldTextStyle { get; } = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
        public static GUIStyle richTextLabelStyle { get; } = new GUIStyle(EditorStyles.label) { richText = true };

        public static GUIStyle sideBarStyle { get; } = new GUIStyle("HelpBox")
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(10, 5, 5, 5)
        };

        public static GUIStyle titleStyle { get; } = new GUIStyle("IN TitleText")
        {
            alignment = TextAnchor.MiddleLeft,
            padding = { left = 5 },
            margin = { top = 10 }
        };

        public static GUIStyle centeredGrayLabel { get; } = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.gray }
        };

        public static GUIStyle createButtonStyle { get; } = new GUIStyle(EditorStyles.miniButton)
        {
            fixedHeight = 20f,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
    }
}
