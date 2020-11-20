#if UNITY_EDITOR

using System;
using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Game Foundation settings for runtime implementation and serialization.
    /// </summary>
    public partial class GameFoundationSettings
    {
        /// <summary>
        ///     The directory name where Unity project assets will be created/stored.
        /// </summary>
        static readonly string kAssetsFolder = "GameFoundation";

        internal static void Editor_CreateGameFoundationSettingsIfNecessary()
        {
            if (s_Instance == null)
            {
                s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");

                if (s_Instance == null && !Application.isPlaying)
                {
                    const string errorMessage = "No Game Foundation settings file has been found. " +
                        "Game Foundation code will automatically create one. " +
                        "The Settings file is critical to Game Foundation, " +
                        "if you wish to remove it you will need to " +
                        "remove the entire Game Foundation package.";
                    k_GFLogger.Log(errorMessage);

                    s_Instance = CreateInstance<GameFoundationSettings>();

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                    {
                        AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                    }

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}/Resources"))
                    {
                        AssetDatabase.CreateFolder($"Assets/{kAssetsFolder}", "Resources");
                    }

                    AssetDatabase.CreateAsset(s_Instance, $"Assets/{kAssetsFolder}/Resources/GameFoundationSettings.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");
                }

                if (s_Instance == null)
                {
                    throw new InvalidOperationException("Unable to find or create a GameFoundationSettings resource!");
                }
            }
        }
    }
}

#endif
