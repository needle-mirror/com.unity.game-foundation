namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Store system-specific editor window.
    /// </summary>
    class StoreEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        ///     Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<StoreEditorWindow>(false, "Store", true);
        }

        /// <summary>
        ///     Adds the editors for the store system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new StoreCatalogAssetEditor("Stores", this));
        }
    }
}
