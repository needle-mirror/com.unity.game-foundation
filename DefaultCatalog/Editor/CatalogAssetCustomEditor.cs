#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    [CustomEditor(typeof(CatalogAsset))]
    public class CatalogAssetCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate Constants"))
            {
                if (target is CatalogAsset catalogAsset)
                {
                    ConstantGenerator.GenerateConstants(catalogAsset);
                }
            }

            EditorGUILayout.Space();

            // show all fields with script suppressed.
            serializedObject.Update();

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
