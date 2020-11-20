namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Class creates Game Parameter system-specific editor window.
    /// </summary>
    class GameParameterEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        ///     Opens the Game Parameter window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<GameParameterEditorWindow>(false, "Game Parameter", true);
        }

        /// <summary>
        ///     Adds the editors for the game parameter system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();
            m_Editors.Add(new GameParameterAssetEditor("Game Parameters", this));
        }
    }
}
