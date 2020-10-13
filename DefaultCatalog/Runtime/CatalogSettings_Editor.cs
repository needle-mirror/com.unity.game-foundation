#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CatalogSettings
    {
        /// <summary>
        ///     The directory name where Unity project assets will be created/stored.
        /// </summary>
        static readonly string kAssetsFolder = "GameFoundation";

        /// <summary>
        ///     Creates the catalog settings asset file if necessary.
        /// </summary>
        static void Editor_TryCreateCatalogSettings()
        {
            if (s_Instance == null && !Application.isPlaying)
            {
                k_GFLogger.Log("No Game Foundation catalog settings file has been found. Game Foundation code " +
                    "will automatically create one. Catalog settings file is critical to Game Foundation, " +
                    "if you wish to remove it you will need to remove the entire Game Foundation package.");

                s_Instance = CreateInstance<CatalogSettings>();

                if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                {
                    AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                }

                if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}/Resources"))
                {
                    AssetDatabase.CreateFolder($"Assets/{kAssetsFolder}", "Resources");
                }

                AssetDatabase.CreateAsset(s_Instance, $"Assets/{kAssetsFolder}/Resources/GameFoundationCatalogSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                s_Instance = Resources.Load<CatalogSettings>("GameFoundationCatalogSettings");
            }
        }

        /// <summary>
        ///     Creates the catalog asset if necessary.
        /// </summary>
        static void Editor_TryCreateCatalogAsset()
        {
            if (s_Instance.m_CatalogAsset == null)
            {
                string catalogAssetPath = $"Assets/{kAssetsFolder}/GameFoundationCatalog.asset";

                // try to load a catalog asset by hardcoded path
                s_Instance.m_CatalogAsset = AssetDatabase.LoadAssetAtPath<CatalogAsset>(catalogAssetPath);

                // if that doesn't work, then create one
                if (s_Instance.m_CatalogAsset == null)
                {
                    s_Instance.m_CatalogAsset = CreateInstance<CatalogAsset>();

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                    {
                        AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                    }

                    s_Instance.m_CatalogAsset.Editor_Save(catalogAssetPath);
                    EditorUtility.SetDirty(s_Instance);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}

#endif
