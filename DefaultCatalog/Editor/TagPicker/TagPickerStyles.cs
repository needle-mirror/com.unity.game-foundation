using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    static class TagPickerStyles
    {
        public const int tagAddButtonWidth = 75;
        public const int tagItemMargin = 6;
        public const int tagRemoveButtonSpaceWidth = 15;

        public static GUIStyle searchSuggestAreaStyle { get; } = new GUIStyle(GUI.skin.textArea);

        public static GUIStyle tagListItemStyle { get; } =
            new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(8, 7, 6, 6),
                wordWrap = false,
                clipping = TextClipping.Overflow,
                alignment = TextAnchor.MiddleLeft
            };

        public static GUIStyle tagSuggestItemStyle { get; } =
            new GUIStyle
            {
                padding = new RectOffset(5, 4, 3, 3),
                normal =
                {
                    textColor = GUI.skin.label.normal.textColor
                }
            };

        public static GUIStyle tagSuggestItemStyleSelected { get; } =
            new GUIStyle(tagSuggestItemStyle)
            {
                normal =
                {
                    background = EditorGUIUtility.IconContent("selected").image as Texture2D,
                    textColor = Color.white
                }
            };
    }
}
