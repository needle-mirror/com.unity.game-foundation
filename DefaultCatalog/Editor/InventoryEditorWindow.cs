namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Inventory system-specific editor window.
    /// </summary>
    class InventoryEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        ///     Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<InventoryEditorWindow>(false, "Inventory Item", true);
        }

        /// <summary>
        ///     Adds the editors for the inventory system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new InventoryCatalogAssetEditor("Inventory Items", this));
        }
    }
}
