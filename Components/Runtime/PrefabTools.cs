using System;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation.Components
{
    internal static class PrefabTools
    {
        /// <summary>
        ///     Logs an error to the console if GameFoundation has not been initialized before the given method is called.
        /// </summary>
        /// <param name="logger">
        ///     A <see cref="GameFoundationDebug"/> constructed with the particular class type that contains the given method.
        /// </param>
        /// <param name="methodName">
        ///     The name of the method that cannot be called before initialization is complete.
        /// </param>
        /// <returns>
        ///     Returns true if Game Foundation is not initialized, otherwise false.
        /// </returns>
        public static bool FailIfNotInitialized(GameFoundationDebug logger, string methodName)
        {
            if (GameFoundationSdk.IsInitialized)
            {
                return false;
            }

            logger.LogError($"You must call {nameof(GameFoundationSdk)}.{nameof(GameFoundationSdk.Initialize)} "
                            + "and wait for "
                            + $"{nameof(GameFoundationSdk)}.{nameof(GameFoundationSdk.IsInitialized)} "
                            + $"to be true before {methodName} can be called.");

            return true;
        }

        /// <summary>
        ///     Gets the catalog to use for looking up items and properties during editor time.
        ///     By default it will return CatalogSettings.catalogAsset, but if there is a GameFoundationInit instance in the
        ///     scene with overrideCatalog enabled, it will return the catalog set there instead.
        /// </summary>
        public static CatalogAsset GetLookUpCatalogAsset()
        {
            if (GameFoundationInit.instance == null
                || !GameFoundationInit.instance.m_OverrideCatalogAsset
                || GameFoundationInit.instance.catalogAsset == null)
            {
                return CatalogSettings.catalogAsset;
            }

            return GameFoundationInit.instance.catalogAsset;
        }

        /// <summary>
        ///     Displays a message on EditorGUILayouts to indicate that an override catalog is in use instead of the
        ///     default one.
        /// </summary>
        public static void DisplayCatalogOverrideAlertIfNecessary()
        {
#if UNITY_EDITOR
            if (GetLookUpCatalogAsset() != CatalogSettings.catalogAsset)
            {
                EditorGUILayout.HelpBox(new GUIContent("Catalog override in use"));
            }
#endif
        }
        
        /// <summary>
        ///     Triggers one of the two callbacks specified in the parameters, either onLoadSucceeded or on LoadFailed,
        ///     depending on whether a sprite is found in the specified spriteProperty.
        /// </summary>
        /// <param name="spriteProperty">
        ///     The <see cref="Property"/> that maps to the desired Sprite.
        /// </param>
        /// <param name="onLoadSucceeded">
        ///     Callback for if a sprite is successfully found from the given Property.
        /// </param>
        /// <param name="onLoadFailed">
        ///     Callback for if a sprite could not be found in the given Property.
        /// </param>
        public static void LoadSprite(Property spriteProperty, Action<Sprite> onLoadSucceeded, Action<string> onLoadFailed)
        {
            if (Application.isPlaying)
            {
                switch (spriteProperty.type)
                {
                    case PropertyType.ResourcesAsset:
                    {
                        if (spriteProperty.AsAsset<Sprite>() is Sprite sprite)
                        {
                            onLoadSucceeded?.Invoke(sprite);
                        }
                        else
                        {
                            onLoadFailed?.Invoke($"No sprite was found in the {spriteProperty.type} " +
                                                 $"{nameof(Property)}.");
                        }

                        return;
                    }
                    case PropertyType.Addressables:
                    {
                        var spriteHandle = spriteProperty.AsAddressable<Sprite>();
                        spriteHandle.Completed += handle =>
                        {
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                onLoadSucceeded?.Invoke(handle.Result);
                            }
                            else
                            {
                                onLoadFailed?.Invoke($"A sprite could not be found in the {nameof(Property)} " +
                                                     $"({spriteProperty.type}). Addressables exception: " +
                                                     $"{handle.OperationException}");
                            }
                        };
                        return;
                    }
                }
            }
#if UNITY_EDITOR
            else
            {
                switch (spriteProperty.type)
                {
                    case PropertyType.ResourcesAsset:
                    {
                        if (spriteProperty.AsAsset<Sprite>() is Sprite sprite)
                        {
                            onLoadSucceeded?.Invoke(sprite);
                        }
                        else
                        {
                            onLoadFailed?.Invoke($"No sprite was found in the {nameof(PropertyType.ResourcesAsset)} " +
                                                 $"{nameof(Property)}.");
                        }
                        return;
                    }
                    case PropertyType.Addressables:
                        onLoadFailed?.Invoke($"Sprites associated with Properties of type {spriteProperty.type} " +
                                             $"cannot be loaded at editor time.");
                        return;
                }
            }
#endif
            onLoadFailed?.Invoke($"PropertyType {spriteProperty.type} is unsupported for displaying sprites.");
        }

        /// <summary>
        ///     Extension method to check if a MonoBehaviour should destroy and instantiate any of its children automatically.
        ///     This prevents errors when editing a prefab in Prefab Mode or when it is selected in the Project window.
        /// </summary>
        /// <param name="monoBehaviour">
        ///     The MonoBehaviour to validate.
        /// </param>
        /// <returns>
        ///     Returns <c>true</c> if it is safe to destroy and recreate child GameObjects, otherwise <c>false</c>.
        /// </returns>
        public static bool ShouldRegenerateGameObjects(this MonoBehaviour monoBehaviour)
        {
            var isPrefabInstance = true;

#if UNITY_EDITOR
            isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(monoBehaviour.gameObject);
#endif

            return Application.isPlaying || isPrefabInstance;
        }
    }
}
