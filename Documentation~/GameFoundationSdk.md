# GameFoundationSdk

## Overview

The GameFoundationSdk provides access to all the Managers, Catalogs and events that make up the Game Foundation Package, as well as methods to initialize and unitializae Game Foundation when needed.

## Details

The managers available in Game Foundation include:

    /// <summary>
    ///     The inventory manager.
    /// </summary>
    public static IInventoryManager inventory;

    /// <summary>
    ///     The wallet manager.
    /// </summary>
    public static IWalletManager wallet;

    /// <summary>
    ///     The transaction manager.
    /// </summary>
    public static ITransactionManager transactions;

    /// <summary>
    ///     The reward manager.
    /// </summary>
    public static IRewardManager rewards;


GameFoundationSdk also holds the Catalog of all items available for to your application (these items are created using the various Game Foundation windows accessiable through Unity menu `Window` > `Game Foundation`):

    /// <summary>
    ///     The complete collection of all items you created in the GameFoundation editor.
    /// </summary>
    public static Catalog catalog;

As well as the TagCatalog:

    /// <summary>
    ///     The collection of all tags created within Game Foundation.
    ///     Find <see cref="Tag"/>s here and keep references of them
    ///     around to improve performance when finding other things by tag.
    /// </summary>
    public static TagCatalog tags;

## Initialization

To use Game Foundation, it must first be initialized in your game, either through code using `GameFoundationSdk.Initialize` (below) or by utilizing the `GameFoundationInit.cs` script which automatically initializes Game Foundation and can even trigger methods in Game Objects within your Hierarchy when initialization completes to enable your game to begin using the package once it's ready.  To initialize manually, use:

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
    public static Deferred Initialize(IDataAccessLayer dataLayer);

You can also unintialize Game Foundation if needed to free resources or reload from Persisted Data using:

    /// <summary>
    ///     Frees the resources of the <see cref="GameFoundationSdk"/>.
    /// </summary>
    public static void Uninitialize();

To monitor the progress of initialization, you can subscribe to the following events:

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
    ///     Event raised when GameFoundation is uninitialized.
    /// </summary>
    public static event Action uninitialized;

Or you can check the current initialization status using:

    /// <summary>
    ///     Check if the Game Foundation is initialized.
    /// </summary>
    /// <returns>
    ///     Whether the Game Foundation is initialized or not.
    /// </returns>
    public static bool IsInitialized;

You can also check the current verions of GameFoundationSdk through:

    /// <summary>
    ///     The current version of the Game Foundation SDK
    /// </summary>
    public static string currentVersion;

If needed, you can also retrieve the current DataAccessLayer through:

    /// <summary>
    ///     The current Data Access Layer used by GameFoundation.
    /// </summary>
    public static IDataAccessLayer dataLayer;
    
Game Foundation also lets you override fixed catalog data at initialization using the External Value Provider. For more detail, please visit the [IExternalValueProvider](ExternalValueProvider.md) page.
