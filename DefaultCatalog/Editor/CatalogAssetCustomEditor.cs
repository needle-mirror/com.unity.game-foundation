#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    [CustomEditor(typeof(CatalogAsset))]
    public class CatalogAssetCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Generate Constants"))
            {
                if (target is CatalogAsset catalogAsset)
                {
                    ConstantGenerator.GenerateConstants(catalogAsset);
                }
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
#endif
