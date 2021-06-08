using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    static class MenuItems
    {
        const int GF_GameParameter = 1001;
        const int GF_Inventory = 1002;
        const int GF_Currency = 1003;
        const int GF_Transactions = 1004;
        const int GF_Store = 1005;
        const int GF_Reward = 1006;
        const int GF_Tags = 1007;

        /// <summary>
        ///     Creates menu item for game parameters system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Game Parameter", priority = GF_GameParameter)]
        public static void ShowGameParameterWindow()
        {
            GameParameterEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for currency system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Currency", priority = GF_Currency)]
        public static void ShowCurrencyWindow()
        {
            CurrencyEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for inventory system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Inventory Item", priority = GF_Inventory)]
        public static void ShowInventoriesWindow()
        {
            InventoryEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for Store system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Store", priority = GF_Store)]
        public static void ShowStoresWindow()
        {
            StoreEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for Store system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Transaction", priority = GF_Transactions)]
        public static void ShowPurchasesWindow()
        {
            TransactionEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for Reward system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Reward", priority = GF_Reward)]
        public static void ShowRewardWindow()
        {
            RewardEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Creates menu item for tag system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Tag Management", priority = GF_Tags)]
        public static void ShowTagWindow()
        {
            TagEditorWindow.ShowWindow();
        }

        /// <summary>
        ///     Selects the GameFoundationCatalogSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings/Catalog Settings", false, 2012)]
        public static void SelectGameFoundationCatalogSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(CatalogSettings.singleton, null);
        }

        /// <summary>
        ///     Sets selected CatalogAsset as the Catalog Asset in CatalogSettings.
        /// </summary>
        [MenuItem("Assets/Game Foundation/Use This Catalog In GameFoundationCatalogSettings", false)]
        public static void SetAsCatalogSettingsCatalogAsset()
        {
            CatalogSettings.catalogAsset = (CatalogAsset) Selection.activeObject;
        }

        /// <summary>
        ///     Validates whether active object is of type CatalogAsset in order to enable SetAsCatalogSettingsCatalogAsset MenuItem.
        /// </summary>
        [MenuItem("Assets/Game Foundation/Use This Catalog In GameFoundationCatalogSettings", true)]
        public static bool ValidateSetAsCatalogSettingsCatalogAsset()
        {
            return Selection.activeObject is CatalogAsset;
        }
    }
}
