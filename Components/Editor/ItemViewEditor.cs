using UnityEditor;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for an <see cref="ItemView"/> component.
    /// </summary>
    [CustomEditor(typeof(ItemView), true)]
    class ItemViewEditor : Editor
    {
        readonly string[] kExcludedFields =
        {
            "m_Script"
        };

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
