using System;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Game Foundation catalog settings for Game Foundation editor catalog serialization.
    /// </summary>
    public partial class CatalogSettings : ScriptableObject
    {
        internal static event Action onCatalogChanged;
        static CatalogSettings s_Instance;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<CatalogSettings>();

        internal static CatalogSettings singleton
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<CatalogSettings>("GameFoundationCatalogSettings");

#if UNITY_EDITOR
                    Editor_TryCreateCatalogSettings();
#endif
                    if (s_Instance == null)
                    {
                        throw new InvalidOperationException($"{nameof(CatalogSettings)}: Unable to find or " +
                            "create a GameFoundationCatalogSettings resource!");
                    }
                }

#if UNITY_EDITOR
                Editor_TryCreateCatalogAsset();
#endif

                if (s_Instance.m_CatalogAsset == null)
                {
                    throw new NullReferenceException($"{nameof(CatalogSettings)}: Game Foundation catalog asset reference " +
                        "cannot be null. Open one of the Game Foundation windows in the Unity Editor " +
                        "while not in Play Mode to have a catalog asset created for you automatically.");
                }

                return s_Instance;
            }
        }

        /// <inheritdoc cref="catalogAsset"/>
        [SerializeField]
        [FormerlySerializedAs("m_Database")]
        CatalogAsset m_CatalogAsset;

        /// <summary>
        ///     The CatalogAsset in use.
        /// </summary>
        public static CatalogAsset catalogAsset
        {
            get => singleton.m_CatalogAsset;
            set => SetCatalogAsset(value);
        }

        /// <summary>
        ///     Sets the catalogAsset and invokes the onCatalogChanged event.
        /// </summary>
        /// <param name="asset">New catalog asset to set.</param>
        private static void SetCatalogAsset(CatalogAsset asset)
        {
            singleton.m_CatalogAsset = asset;
            onCatalogChanged?.Invoke();
        }

        /// <summary>
        ///     Invokes catalog changed event.
        /// </summary>
        private void OnValidate()
        {
            onCatalogChanged?.Invoke();
        }
    }
}
