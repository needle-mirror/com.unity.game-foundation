namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Store system-specific editor window.
    /// </summary>
    class RewardEditorWindow : CollectionEditorWindowBase
    {
        static RewardEditorWindow m_RewardWindow;

        /// <summary>
        ///     Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static RewardEditorWindow ShowWindow()
        {
            if (m_RewardWindow == null)
            {
                m_RewardWindow = GetWindow<RewardEditorWindow>(false, "Reward", true);
            }

            return m_RewardWindow;
        }

        /// <summary>
        ///     Adds the editors for the store system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new RewardCatalogAssetEditor("Rewards", this));
        }
    }
}
