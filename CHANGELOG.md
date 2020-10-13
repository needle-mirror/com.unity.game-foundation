# Changelog

## [0.7.0] - 2020-10-13

### Added

* StackableInventoryItem now exists (derived from InventoryItem), along with corresponding StackableInventoryItemDefinition to expose a quantity member that can be used to store multiple items in one InventoryItem to improve performance and reduce Game Foundation memory usage.
* `Property` has a new type: `ResourcesAsset`. It stores the path of an asset.
* IItemCollection: an interface for grouping items created by the InventoryManager into either ItemLists or ItemMaps for easier management, searching, iteration, etc.
* ItemList: a list (very similar to List<InventoryItem>) which Game Foundation manages to ensure all items are valid and have not been removed from Game Foundation system.
* ItemMap: a map (very similar to Dictionary<string, InventoryItem>) which Game Foundation manages to ensure all items are valid and have not been removed from Game Foundation system.
* GameFoundation.inventory.CreateList, GameFoundation.inventory.CreateMap: methods to create Game-Foundation-managed lists/maps of items for later retrieval.
* The `IExternalValueProvider` to give you the possibility of overriding some data from your local catalog (i.e. `GameFoundationDatabase`) with any third party you want.
  This is particularly helpful if you want to conduct some A/B testing on your local catalog.
* Game Foundation Init Component: A component that allows developers to initialize Game Foundation. This component can be added to the scene by using the menu item "Game Foundation/Create Game Foundation Object". 

### Changed

* The package is renamed `com.unity.game-foundation`, instead of `com.unity.game.foundation`.
* The `GameFoundation` is renamed `GameFoundationSdk`.
  It removes the ambiguity with the namespace.
* `TransactionRewards` has been renamed to `Payout`.
* `BaseTransaction.rewards` has been renamed to `BaseTransaction.payout`. This also affects `VirtualTransaction` and `IAPTransaction`.
* `Currency` and `InventoryItemDefinition` now have a common parent `TradableDefinition` to ease working with them when handling transactions.
  For the same reason: `InventoryItem` and `CurrencyExchange` both extends `ITradable`;
  `CurrencyExchangeDefinition` and `ItemExchangeDefinition` have been merged into `ExchangeDefinition` (including there related config and serializable objects).
  This means that costs and payouts in existing database will be erased and need to be recreated.
* `GameFoundationSdk.Initialize` doesn't take callbacks anymore. It returns a `deferred` instead.
* `InventoryManager` class is obsolete. Replaced by `GameFoundationSdk.inventory`.
* `WalletManager` class is obsolete. Replaced by `GameFoundationSdk.wallet`.
* `RewardManager` class is obsolete. Replaced by `GameFoundationSdk.rewards`.
* `TransactionManager` class is obsolete. Replaced by `GameFoundationSdk.transactions`.
* `GameFoundation.catalogs.tagCatalog` is now `Gamefoundation.tags`.
* `GameFoundation.catalogs.tagCatalog.FindTag()` is now `Gamefoundation.tags.Find()`.
* `GameFoundation.catalogs.tagCatalog.ContainsTag()` is now `Gamefoundation.tags.Contains()`.
* All catalogs (besides tags) have been consolidated into one. For example, `GameFoundation.catalogs.inventoryCatalog.FindItem(someKey)` is now `Gamefoundation.catalog.Find<InventoryItemDefinition>(someKey)`.
* `GameFoundationDatabase` class is now `Catalog` and the file that was called `GameFoundationDatabase.asset` is now called `GameFoundationCatalog.asset`.
* `GameFoundationDatabaseSettings` class is now `CatalogSettings` and the file that was called `GameFoundationDatabaseSettings.asset` is now called `GameFoundationCatalogSettings.asset`.
* All static property text fields on component's custom inspector windows has been changed to dropdown menus.
* All the fields on custom inspector windows of components has been regrouped.
* Prefabs and their sample scenes have been moved into a `.unitypackage` file within this package. It can be installed into your project's Assets folder by using the menu item "Game Foundation/Import Prefabs and Sample Scenes".
* `CatalogBuilder.CreateTag(string)` is obsolete. Replaced by `CatalogBuilder.GetTag(string, bool)`.
* StoreView (and other prefabs) now update their content display when being re-enabled using SetActive(true).

### Removed

* `AssetsDetail` has been removed, use `Property` to store asset paths now.
* `AnalyticsDetail` and related classes have been removed, along with `GameFoundationSettings.enableEditorModeAnalytics` parameter, and `GameFoundationSettings.enablePlayModeAnalytics` parameter.
* `JsonDetail` has been removed.

## [0.6.0] - 2020-06-30

### Added

* New prefab and component created for creating popups advertising a special promotion.
* Static Properties have been added to all catalog items.\
  Use them to define fixed data that won't change at runtime.
* The new Game Parameters feature provide a solution to store your game configuration.
  Editor can be found at: __Unity→Window→Game Foundation→Game Parameters__.
* You now can define an initial allocation for any inventory item.
  Those items are instantiated when the player profile is created.
* Support for non-consumable IAP.
* Tools to let the dev process the "background" IAP queue (restoring purchases, delayed purchase successes, etc) on their own, instead of letting Game Foundation do it for them automatically.

### Changed

* `JsonDetail` has been flagged `Obsolete` and will be replaced by Static Properties as soon as they handle lists and dictionaries.
* Simplified initialization of TransactionManager (UnityPurchasingAdapter is now internal and no longer decoupled).

## [0.5.0] - 2020-06-03

### Added

* Properties can now store `long` and `double` values.
  Since `int` and `float` are covered by these types, they have been removed from the editor type dropdown but are still handled.
* Properties can now store `bool` and `string` values.
* Add a public API to manipulate the Default Catalog.

### Changed

* The code is now split into different assemblies.\
  The local `Catalog`, formerly in the `CatalogManager` namespace is now in the `DefaultCatalog` assembly and `DefaultCatalog` namespace.\
  The scripts of sample Prefabs also have their own assembly.
* `Category` has been renamed `Tag` and now exists in a separate `TagCatalog`.\
  All old `FindItemsByCategory` methods have been reimplemented as FindItemsByTag and behave as before.\
  Tags can be added or removed to/from all of GameFoundation in `Window\Game Foundation\Tag` window.\
  When tags are removed from Tag Catalog, they will be removed from all items throughout Game Foundation.
* Stats have been renamed to Properties.
* Properties have been directly integrated inside Inventory Items.\
  This means that `StatCatalog`, `StatDefinition`, `StatDetail`, `StatManager` and the Stat editor window no longer exist.\
  Properties are accessible directly through the items and definitions they are defined for.
  The `IStatDataLayer` has been merged into `IInventoryDataLayer` to match this change.
* `GameItem` has been merged into `InventoryItem` since they are the only instantiable objects in Game Foundation systems at the moment.
* We've removed IPurchasingAdapter, and UnityPurchasingAdapter is now internal. Use the TransactionManager class for anything related to IAP in Game Foundation.

## [0.4.0] - 2020-04-24

### Added

* Store System.
  Editor can be found at: __Unity->Window->Game Foundation->Store__.
  Manager/API can be found at `GameFoundation.catalogs.storeCatalog` to `GetCollectionDefinition` and/or `GetItem` as needed.
* Assets Detail.
  Attach assets to your item definitions and load them using `Resources.Load()` automatically.
* Json Detail.
  Provides the ability to add arbitrary typed fields to your item definition
* IAP Transaction with IAP SDK
* Item instances.
  Items are no longer quantities in inventories, but identifiable item instances with custom stat per item.
  Quantities, if needed, can be achieved using stats.

### Changed

* Inventories definition removed.
  All the items are instantiated in the player inventory.
  Item collections (list, map) will be introduced next release.
* Virtual Transactions is now a item.

## [0.3.0] - 2020-02-19

### Added

* Data Access Layer
* Transaction System
* Transaction System Sample Scene
* Purchasable Detail

### Changed

* GameFoundation's Initialization changed to take an IDataAccessLayer as an argument instead of a persistence object.
* GameFoundationSettings ScriptableObject is now split into GameFoundationDatabaseSettings, which holds the reference to the database for the editor, and GameFoundationSettings, which continues to hold the other settings, like analytics flags.
* CatalogManager now holds the reference to the catalogs at Runtime. Any runtime code that was previously written as GameFoundationSettings.database.xCatalog should now be written as CatalogManager.xCatalog.
* Persistence and Serializer interfaces changed to handle only GameFoundation's data.

## [0.2.0] - 2019-12-11

### Added

* Samples
* Debug window for visualizing data during Play Mode in the Editor
* Three new detail definition types:
  * Sprite Assets Detail
  * Prefab Assets Detail
  * Audio Clip Assets Detail
* Tools for creating custom detail definitions
* Ability to choose a Reference Definition while creating a new Inventory Item (which also pre-populates the Display Name and ID fields)
* Menu items that link to the documentation and the forums

### Changed

* Improved Stat Detail UI
* Icon Detail is now marked as obsolete and will be removed in a future version (please use Sprite Assets Detail instead)
* Currency Detail Type is now broken into Type and Sub-Type (with related UI change)
* Minor API performance optimizations
* Minor editor UI/UX improvements and optimizations

## [0.1.0-preview.6] - 2019-09-18

### Added

* Analytics system
* Support for serialization of runtime stats data
* "Auto-Create Instance" feature for the Inventory system

### Changed

* Improvements to local runtime persistence system
* Some classes and members have been renamed
* Details renamed to Detail
* ScriptableObject database format changed to one file instead of multiple

## [0.1.0-preview.5] - 2019-08-23

### Added

* Core Stats System

### Changed

* Local persistence implementation updated
* Internal renaming

## [0.1.0-preview.4] - 2019-08-09

_First external release_

### Added

* Inventory System
* Player Wallet

#### This is the initial release of *Unity Package \<com.unity.game-foundation\>*.

### Features

* Inventory System
* Player Wallet
* Stats System Core