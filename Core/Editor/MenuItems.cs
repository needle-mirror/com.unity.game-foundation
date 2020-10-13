using UnityEditor.GameFoundation.Debugging;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    static class MenuItems
    {
        /// <summary>
        ///     Selects the GameFoundationSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings/Runtime Settings", false, 2011)]
        public static void SelectGameFoundationSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(GameFoundationSettings.singleton, null);
        }

        [MenuItem("Window/Game Foundation/Tools/Debugger", false, 2013)]
        public static void ShowDebugWindow()
        {
            DebugEditorWindow.ShowWindow();
        }

        [MenuItem("Window/Game Foundation/Help/Documentation", false, 2016)]
        public static void OpenHelpURL()
        {
            Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.game-foundation@latest/");
        }

        [MenuItem("Window/Game Foundation/Help/Support Forum", false, 2017)]
        public static void OpenSupportForumURL()
        {
            Application.OpenURL("https://forum.unity.com/forums/game-foundation.416/");
        }
    }
}
