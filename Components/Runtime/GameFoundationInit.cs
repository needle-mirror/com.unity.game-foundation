using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that initialize Game Foundation SDK.
    /// </summary>
    public class GameFoundationInit : MonoBehaviour
    {
        /// <summary>
        ///     Data layer type to set the <see cref="IDataAccessLayer"/> when initialization of Game Foundation.
        /// </summary>
        public DataLayerType dataLayerType => m_DataLayerType;

        [SerializeField]
        internal DataLayerType m_DataLayerType;

        /// <summary>
        ///     Local persistence filename for save file.
        ///     If <see cref="DataLayerType"/> is <see cref="DataLayerType.LocalPersistence"/>, this filename should be defined.
        /// </summary>
        public string localPersistenceFilename => m_LocalPersistenceFilename;

        [SerializeField]
        internal string m_LocalPersistenceFilename = "GameFoundationSave";

        /// <summary>
        ///     A reference to <see cref="CatalogAsset"/> to override the catalog asset that is defined in
        ///     <see cref="GameFoundationSettings"/> when initialization of Game Foundation.
        ///     If it's null, the <see cref="catalogAsset"/> that is defined in <see cref="GameFoundationSettings"/> will be
        ///     used.
        /// </summary>
        public CatalogAsset catalogAsset => m_CatalogAsset;

        [SerializeField]
        internal CatalogAsset m_CatalogAsset;

        /// <summary>
        ///     Whether <see cref="CatalogAsset"/> will be overridden or not.
        /// </summary>
        internal bool overrideCatalogAsset
        {
            get => m_OverrideCatalogAsset;
            set
            {
                if (!value)
                {
                    m_CatalogAsset = null;
                }

                m_OverrideCatalogAsset = value;
            }
        }

        [SerializeField]
        internal bool m_OverrideCatalogAsset;

        /// <summary>
        ///     Event raised when GameFoundation is successfully initialized.
        /// </summary>
        [Space]
        public GameFoundationInitializationEvent onGameFoundationInitialized;

        /// <summary>
        ///     Event raised when GameFoundation failed its initialization.
        ///     The provided exception is the reason of the failure.
        /// </summary>
        public GameFoundationInitializationFailedEvent onGameFoundationInitializationFailed;

        /// <summary>
        ///     Event raised when GameFoundation is uninitialized.
        /// </summary>
        public GameFoundationInitializationEvent onGameFoundationUninitialized;

        /// <summary>
        ///     A <see cref="UnityEvent"/> for Game Foundation initialization and uninitialization is successful.
        /// </summary>
        [Serializable]
        public class GameFoundationInitializationEvent : UnityEvent { }

        /// <summary>
        ///     A <see cref="UnityEvent"/> for when Game Foundation initialization is failed.
        /// </summary>
        [Serializable]
        public class GameFoundationInitializationFailedEvent : UnityEvent<Exception> { }

        /// <summary>
        ///     Whether events are registered yet or not.
        /// </summary>
        bool m_EventsRegistered;

        /// <summary>
        ///     Whether this component already exists or not.
        /// </summary>
        static bool s_Initialized;

        /// <summary>
        ///     Data layer type to define <see cref="IDataAccessLayer"/>.
        /// </summary>
        public enum DataLayerType
        {
            /// <summary>
            ///     For the MemoryDataLayer
            /// </summary>
            Memory,

            /// <summary>
            ///     For the PersistenceDataLayer
            /// </summary>
            LocalPersistence
        }

        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<GameFoundationSettings>();

        void Awake()
        {
            if (!s_Initialized)
            {
                s_Initialized = true;
                DontDestroyOnLoad(gameObject);

                RegisterEvents();
                Initialize();
            }
            else
            {
                var allComponents = new List<Component>();
                gameObject.GetComponents(allComponents);
                if (allComponents.Count == 1)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        void OnEnable()
        {
            RegisterEvents();
        }

        void OnDisable()
        {
            UnregisterEvents();
        }

        void RegisterEvents()
        {
            if (m_EventsRegistered)
                return;

            GameFoundationSdk.initialized += OnGameFoundationInitialized;
            GameFoundationSdk.initializationFailed += OnGameFoundationInitializationFailed;
            GameFoundationSdk.uninitialized += OnGameFoundationUninitialized;

            m_EventsRegistered = true;
        }

        void UnregisterEvents()
        {
            if (!m_EventsRegistered)
                return;

            GameFoundationSdk.initialized -= OnGameFoundationInitialized;
            GameFoundationSdk.initializationFailed -= OnGameFoundationInitializationFailed;
            GameFoundationSdk.uninitialized -= OnGameFoundationUninitialized;

            m_EventsRegistered = false;
        }

        /// <summary>
        ///     Initialize GameFoundation SDK.
        /// </summary>
        public void Initialize()
        {
            if (!s_Initialized)
            {
                k_GFLogger.LogWarning($"This instance of {nameof(GameFoundationInit)} not in use.");
                return;
            }

            if (GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.LogWarning("Game Foundation is already initialized.");
                return;
            }

            StartCoroutine(ExecuteInitialization());
        }

        IEnumerator ExecuteInitialization()
        {
            IDataAccessLayer dataLayer = null;

            if (dataLayerType == DataLayerType.Memory)
            {
                // this data layer will not save any data, it is usually used for examples or tests
                dataLayer = new MemoryDataLayer(m_CatalogAsset);
            }
            else if (dataLayerType == DataLayerType.LocalPersistence)
            {
                if (string.IsNullOrEmpty(m_LocalPersistenceFilename))
                {
                    onGameFoundationInitializationFailed.Invoke(new ArgumentException("Local Persistence filename is not defined."));
                    yield return null;
                }

                dataLayer = new PersistenceDataLayer(new LocalPersistence(m_LocalPersistenceFilename, new JsonDataSerializer()), m_CatalogAsset);
            }

            // initialize Game Foundation for runtime access
            var initDeferred = GameFoundationSdk.Initialize(dataLayer);
            yield return initDeferred.Wait();

            // It optimizes the memory as it allows the object to be reused.
            initDeferred.Release();
        }

        /// <summary>
        ///     Uninitialize GameFoundation SDK.
        /// </summary>
        public void Uninitialize()
        {
            if (!s_Initialized)
            {
                k_GFLogger.LogWarning($"This instance of {nameof(GameFoundationInit)} not in use.");
                return;
            }

            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.LogWarning("Game Foundation is not initialized.");
                return;
            }

            GameFoundationSdk.Uninitialize();
        }

        void OnGameFoundationInitialized()
        {
            onGameFoundationInitialized.Invoke();
        }

        void OnGameFoundationInitializationFailed(Exception exception)
        {
            onGameFoundationInitializationFailed.Invoke(exception);
        }

        void OnGameFoundationUninitialized()
        {
            onGameFoundationUninitialized.Invoke();
        }
    }
}
