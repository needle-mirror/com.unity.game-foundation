using UnityEngine;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector of a <see cref="GameFoundationInit"/> component.
    /// </summary>
    [CustomEditor(typeof(GameFoundationInit))]
    class GameFoundationInitEditor : Editor
    {
        GameFoundationInit m_GameFoundationInit;

        SerializedProperty m_DataLayerType_SerializedProperty;
        SerializedProperty m_LocalPersistenceFilename_SerializedProperty;
        SerializedProperty m_CatalogAsset_SerializedProperty;
        SerializedProperty m_OverrideCatalogAsset_SerializedProperty;

        readonly GUIContent m_OverrideCatalogContent = new GUIContent("Override Catalog Asset",
            "Overrides the catalog asset that is defined in GameFoundationSettings.");

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(GameFoundationInit.m_DataLayerType),
            nameof(GameFoundationInit.m_LocalPersistenceFilename),
            nameof(GameFoundationInit.m_CatalogAsset),
            nameof(GameFoundationInit.m_OverrideCatalogAsset),
        };

        void OnEnable()
        {
            m_GameFoundationInit = target as GameFoundationInit;

            m_DataLayerType_SerializedProperty = serializedObject.FindProperty(nameof(m_GameFoundationInit.m_DataLayerType));
            m_LocalPersistenceFilename_SerializedProperty = serializedObject.FindProperty(nameof(m_GameFoundationInit.m_LocalPersistenceFilename));
            m_CatalogAsset_SerializedProperty = serializedObject.FindProperty(nameof(m_GameFoundationInit.m_CatalogAsset));
            m_OverrideCatalogAsset_SerializedProperty = serializedObject.FindProperty(nameof(m_GameFoundationInit.m_OverrideCatalogAsset));
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_DataLayerType_SerializedProperty);

            if ((GameFoundationInit.DataLayerType) m_DataLayerType_SerializedProperty.intValue ==
                GameFoundationInit.DataLayerType.LocalPersistence)
            {
                EditorGUILayout.PropertyField(m_LocalPersistenceFilename_SerializedProperty);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_OverrideCatalogAsset_SerializedProperty, m_OverrideCatalogContent);

                if (m_GameFoundationInit.m_OverrideCatalogAsset)
                {
                    EditorGUILayout.PropertyField(m_CatalogAsset_SerializedProperty, new GUIContent());
                }
            }


            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
