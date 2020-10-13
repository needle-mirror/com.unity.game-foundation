using System.IO;
using UnityEngine;
using UnityEngine.GameFoundation.Components;

namespace UnityEditor.GameFoundation.Components
{
    static class MenuItems
    {
        [MenuItem("Window/Game Foundation/Create Game Foundation Init GameObject", false, 1800)]
        static void CreateGameFoundationInit()
        {
            if (Object.FindObjectOfType(typeof(GameFoundationInit)) == null)
            {
                GameObject go = new GameObject("Game Foundation");
                go.AddComponent<GameFoundationInit>();
                Selection.activeObject = go;
                Undo.RegisterCreatedObjectUndo(go, "Created Game Foundation Init GameObject");
            }
            else
            {
                Debug.LogWarning("A Game Foundation object already exists in this scene.");
            }
        }

        [MenuItem("Window/Game Foundation/Import Prefabs and Samples", false, 1900)]
        public static void ImportPrefabsAndComponents()
        {
            var packagePath = Path.GetFullPath("Packages/com.unity.game-foundation");
            AssetDatabase.ImportPackage(packagePath + "/Package Resources/Game Foundation Prefabs.unitypackage", true);
        }
    }
}
