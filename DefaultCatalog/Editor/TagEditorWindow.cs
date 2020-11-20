namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Tag system-specific editor window.
    /// </summary>
    class TagEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        ///     Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<TagEditorWindow>(false, "Tag Management", true);
        }

        /// <summary>
        ///     Adds the editors for the Tag system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new TagAssetEditor("Tags", this));
        }
    }
}
