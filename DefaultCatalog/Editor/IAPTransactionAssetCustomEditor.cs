#if UNITY_EDITOR

using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    [CustomEditor(typeof(IAPTransactionAsset))]
    public class IAPTransactionAssetCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                DrawPropertiesExcluding(serializedObject, "m_ProductType", "m_Payout");

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ProductType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Payout"));
                }

                if (check.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}

#endif
