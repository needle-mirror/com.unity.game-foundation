namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Currency system-specific editor window.
    /// </summary>
    class CurrencyEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        ///     Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<CurrencyEditorWindow>(false, "Currency", true);
        }

        /// <summary>
        ///     Adds the editors for the currency system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();
            m_Editors.Add(new CurrencyCatalogAssetEditor("Currency", this));
        }
    }
}
