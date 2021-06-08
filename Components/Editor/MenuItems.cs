using System.IO;
using UnityEngine;
using UnityEngine.GameFoundation.Components;

namespace UnityEditor.GameFoundation.Components
{
    static class MenuItems
    {
        [MenuItem("GameObject/Game Foundation/Initializer", false, 10)]
        public static void GameObjectCreateGameFoundationInitializer()
        {
            if (Object.FindObjectOfType(typeof(GameFoundationInit)) == null)
            {
                var go = new GameObject("Game Foundation");
                go.AddComponent<GameFoundationInit>();
                Selection.activeObject = go;
                Undo.RegisterCreatedObjectUndo(go, "Created Game Foundation Initializer");
            }
            else
            {
                Debug.LogWarning("A Game Foundation initializer already exists in this scene.");
            }
        }

        [MenuItem("GameObject/Game Foundation/Initializer", true)]
        public static bool GameObjectCreateGameFoundationInitializerValidate()
        {
            return Object.FindObjectOfType(typeof(GameFoundationInit)) == null;
        }

        [MenuItem("Window/Game Foundation/Import Prefabs and Samples", false, 1900)]
        public static void ImportPrefabsAndComponents()
        {
            var packagePath = Path.GetFullPath("Packages/com.unity.game-foundation");
            AssetDatabase.ImportPackage(packagePath + "/Package Resources/Game Foundation Prefabs.unitypackage", true);
        }
    }
}
