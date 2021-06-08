using System;
using System.Collections;
using System.Reflection;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Manage the initialization and the persistence of Game Foundation's systems.
    /// </summary>
    public static class GameFoundationSdk
    {
        enum InitializationStatus
        {
            NotInitialized,
            Initializing,
            Initialized,
            Failed
        }

        /// <summary>
        ///     Event raised when GameFoundation is successfully initialized.
        /// </summary>
        public static event Action initialized;

        /// <summary>
        ///     Event raised when GameFoundation failed its initialization.
        ///     The provided exception is the reason of the failure.
        /// </summary>
        public static event Action<Exception> initializationFailed;

        /// <summary>
        ///     Event raised immediately before GameFoundation is uninitialized.
        /// </summary>
        public static event Action willUninitialize;

        /// <summary>
        ///     Event raised immediately after GameFoundation is uninitialized.
        /// </summary>
        public static event Action uninitialized;

        /// <summary>
        ///     Initializes some static values of the <see cref="GameFoundationSdk"/>.
        /// </summary>
        static GameFoundationSdk()
        {
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        ///     The current version of the Game Foundation SDK
        /// </summary>
        public static string currentVersion { get; private set; }

        static InitializationStatus s_InitializationStatus = InitializationStatus.NotInitialized;

        /// <summary>
        ///     Check if the Game Foundation is initialized.
        /// </summary>
        /// <returns>
        ///     Whether the Game Foundation is initialized or not.
        /// </returns>
        public static bool IsInitialized => s_InitializationStatus == InitializationStatus.Initialized;

        /// <summary>
        ///     The inventory manager.
        /// </summary>
        public static IInventoryManager inventory { get; private set; }

        /// <summary>
        ///     The wallet manager.
        /// </summary>
        public static IWalletManager wallet { get; private set; }

        /// <summary>
        ///     The transaction manager.
        /// </summary>
        public static ITransactionManager transactions { get; private set; }

        /// <summary>
        ///     The reward manager.
        /// </summary>
        public static IRewardManager rewards { get; private set; }

        /// <summary>
        ///     The collection of all tags created within Game Foundation.
        ///     Find <see cref="Tag"/>s here and keep references of them
        ///     around to improve performance when finding other things by tag.
        /// </summary>
        public static TagCatalog tags { get; private set; }

        /// <summary>
        ///     The complete collection of all items you created in the GameFoundation editor.
        ///     This contains:
        ///     GameParameters, Currencies, InventoryItemDefinitions, VirtualTransactions, IAPTransactions, Stores, and Rewards.
        /// </summary>
        public static Catalog catalog { get; private set; }

        /// <summary>
        ///     The current Data Access Layer used by GameFoundation.
        /// </summary>
        public static IDataAccessLayer dataLayer { get; private set; }

        /// <summary>
        ///     A dummy MonoBehaviour to run updates functions and coroutines.
        ///     Used since <see cref="GameFoundation"/> and its services are static.
        /// </summary>
        internal static GameFoundationUpdater updater;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(GameFoundationSdk));

        /// <summary>
        ///     Initialize GameFoundation systems.
        /// </summary>
        /// <param name="dataLayer">
        ///     The data provider for the inventory manager.
        /// </param>
        /// <returns>
        ///     A promise handle which can be used to get the result of the initialization.
        ///     <seealso cref="Deferred"/>
        /// </returns>
        public static Deferred Initialize(IDataAccessLayer dataLayer)
        {
            return Initialize(dataLayer, null);
        }

        internal static Deferred Initialize(IDataAccessLayer dataLayer, GameFoundationInitOptions initOptions)
        {
            Promises.GetHandles(out var deferred, out var completer);

            if (Tools.RejectIfArgNull(dataLayer, nameof(dataLayer), completer)) return deferred;

            if (s_InitializationStatus == InitializationStatus.Initializing ||
                s_InitializationStatus == InitializationStatus.Initialized)
            {
                const string message = nameof(GameFoundationSdk) + " is already initialized and cannot be initialized again.";
                k_GFLogger.LogWarning(message);
                completer.Reject(new GameFoundationException(message));

                return deferred;
            }

#if UNITY_EDITOR

            // This might happen if Game Foundation is initialized in editor while not running the game.
            // In this case, initializing Game Foundation is disallowed.
            if (!Application.isPlaying)
            {
                const string message = nameof(GameFoundationSdk) + " was attempted to be initialized while in Edit Mode, which is unsupported.";
                k_GFLogger.LogError(message);
                completer.Reject(new GameFoundationException(message));
                return deferred;
            }
#endif

            s_InitializationStatus = InitializationStatus.Initializing;

            updater = new GameObject(nameof(GameFoundationUpdater))
                .AddComponent<GameFoundationUpdater>();

            GameFoundationSdk.dataLayer = dataLayer;

            var routine = InitializeRoutine(completer, initOptions);

            // Works on both Editor and binary
            if (Application.isPlaying)
            {
                updater.StartCoroutine(routine);
            }

            return deferred;
        }

        /// <summary>
        ///     Routine to initialize Game Foundation asynchronously.
        /// </summary>
        /// <param name="completer">
        ///     Collects the status of the initialization.
        /// </param>
        /// <param name="initOptions">
        ///     An instance of <see cref="GameFoundationInitOptions"/>
        ///     containing any dependencies you need to provide or wish to override.
        /// </param>
        static IEnumerator InitializeRoutine(Completer completer, GameFoundationInitOptions initOptions = null)
        {
            void FailInitialization(Exception reason)
            {
                Uninitialize();

                k_GFLogger.LogWarning($"Failed to initialize the SDK: {reason}");

                s_InitializationStatus = InitializationStatus.Failed;

                completer.Reject(reason);

                // Raise event.
                initializationFailed?.Invoke(reason);
            }

            //DataLayer initialization.
            Promises.GetHandles(out var dalInitDeferred, out var dalInitCompleter);
            using (dalInitDeferred)
            {
                try
                {
                    dataLayer.Initialize(dalInitCompleter);
                }
                catch (Exception e)
                {
                    dalInitCompleter.Reject(e);
                }

                if (!dalInitDeferred.isDone)
                {
                    yield return dalInitDeferred.Wait();
                }

                if (!dalInitDeferred.isFulfilled)
                {
                    FailInitialization(dalInitDeferred.error);

                    yield break;
                }
            }

            //Catalog building.
            try
            {
                var catalogBuilder = new CatalogBuilder();
                dataLayer.Configure(catalogBuilder);
                using (var catalogBuilding = catalogBuilder.Build())
                {
                    if (catalogBuilding.isFulfilled)
                    {
                        (catalog, tags) = catalogBuilding.result;
                    }
                    else
                    {
                        FailInitialization(catalogBuilding.error);

                        yield break;
                    }
                }
            }
            catch (Exception e)
            {
                const string message = nameof(GameFoundationSdk) + " failed to initialize runtime catalog.";
                var customException = new GameFoundationException(message, e);

                FailInitialization(customException);

                yield break;
            }

            //Managers initialization
            {
                var inventoryManager = new InventoryManagerImpl();
                using (var initialization = inventoryManager.Initialize())
                {
                    if (!initialization.isDone)
                    {
                        yield return initialization.Wait();
                    }

                    if (!initialization.isFulfilled)
                    {
                        FailInitialization(initialization.error);

                        yield break;
                    }

                    inventory = inventoryManager;
                }

                var walletManager = new WalletManagerImpl();
                using (var initialization = walletManager.Initialize())
                {
                    if (!initialization.isDone)
                    {
                        yield return initialization.Wait();
                    }

                    if (!initialization.isFulfilled)
                    {
                        FailInitialization(initialization.error);

                        yield break;
                    }

                    wallet = walletManager;
                }

                var rewardManager = new RewardManagerImpl();
                using (var initialization = rewardManager.Initialize())
                {
                    if (!initialization.isDone)
                    {
                        yield return initialization.Wait();
                    }

                    if (!initialization.isFulfilled)
                    {
                        FailInitialization(initialization.error);

                        yield break;
                    }

                    rewards = rewardManager;
                }

                var transactionManager = new TransactionManagerImpl();

                // transactions needs to be set before calling Initialize because
                // the IPurchasingAdapter uses this reference in its initialization.
                transactions = transactionManager;
                using (var initialization = transactionManager.Initialize(initOptions))
                {
                    if (!initialization.isDone)
                    {
                        yield return initialization.Wait();
                    }

                    if (!initialization.isFulfilled)
                    {
                        FailInitialization(initialization.error);

                        yield break;
                    }
                }
            }

            s_InitializationStatus = InitializationStatus.Initialized;

            k_GFLogger.Log($"Successfully initialized Game Foundation version {currentVersion}");

            completer.Resolve();

            // Raise event.
            initialized?.Invoke();
        }

        /// <summary>
        ///     Frees the resources of the <see cref="GameFoundationSdk"/>.
        /// </summary>
        public static void Uninitialize()
        {
            try
            {
                willUninitialize?.Invoke();
            }
            catch (Exception ex)
            {
                k_GFLogger.LogException("An event handler subscribed to willUninitialize threw an exception.", ex);
            }

            (inventory as InventoryManagerImpl)?.Uninitialize();
            inventory = default;

            (wallet as WalletManagerImpl)?.Uninitialize();
            wallet = default;

            (transactions as TransactionManagerImpl)?.Uninitialize();
            transactions = default;

            (rewards as RewardManagerImpl)?.Uninitialize();
            rewards = default;

            currentVersion = null;
            catalog = null;
            dataLayer = null;

            if (updater != null && Application.isPlaying)
            {
                Object.Destroy(updater.gameObject);
                updater = null;
            }

            s_InitializationStatus = InitializationStatus.NotInitialized;

            uninitialized?.Invoke();
        }
    }
}
