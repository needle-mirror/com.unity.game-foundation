# Changelog

## [0.9.0] - 2021-06-08

### Added

* Added compatibility with Unity In App Purchasing package version 3.x.
* Menu option for CatalogAssets (Right click in Project window or in Assets dropdown when selected) to set selected CatalogAsset as the GameFoundationCatalogSettings Catalog Asset.
* Added a new Bundle Transaction Item prefab and DetailedTransactionItemView component for displaying a more detailed view of Transaction items with multiple payouts.
* Added a new Horizontal Payout Item to provide a different orientation of layout for a payout item, used by default by the Bundle Transaction Item.
* Added concept of a Featured Transaction Item or Featured Bundle Transaction Item to Store View component. This allows a special prefab to be designated for use with the first item in the store.
* Added a new `Reveal Hidden Items Button` feature to the StoreView component that can be used in combination with the StoreView.ShowHiddenItems method to display the full transaction items list when the store is actively being filtered by a given tag.
* New event `GameFoundationSdk.willUninitialize`. Use this if you're having problems due to nullified Game Foundation managers when handling `GameFoundationSdk.uninitialized`.
* Badge field has been added to `TransactionItemView` and a badge game object to the Transaction Item prefab.
* `StoreView` now generates its content at editor time.
* Added Addressables support to Static Properties and prefab components and included Addressables package as a dependency. These properties allow accessing Addressables assets by linking 'Keys' in the Game Foundation Inventory Item, Currency, Transaction, Game Parameter, Store and/or Reward windows to addresses in the Addressables package. These addresses can be changed in the editor using a drop down list and you can click on the descriptive 'label' to the right of the address to cycle between associated Addressables. Once set, the associated assets can be accessed using the desired ``CatalogItemAsset``.

### Changed

* Minimum Unity Editor version has increased to 2019.4 LTS from 2018.4 LTS
* ``RewardData`` has been updated to regroup all claimed reward data into a single object: ``ClaimedRewardData``.
* IAP Transactions can no longer be assigned payouts if the given product ID is not found in the IAP Catalog.
* IAP Transactions can no longer be assigned payouts if the given product ID found in the IAP Catalog is of the Non-Consumable type. To grant inventory items or currency based on a non-consumable product, you should use the ``GameFoundationSdk.transactions.IsIapProductOwned`` method in your custom logic. Note that subscriptions are still not yet explicitly supported by Game Foundation, but they would work similarly to non-consumable products.
* Purchase Button component no longer auto disables the purchase button under any circumstances.
  The property `itemPurchasableStatus` and callback `onPurchasableStatusChanged` have been added for getting more detailed info about the purchasable status of the transaction for greater customization in the button's behavior under various circumstances. 
* Renamed the folder containing transaction items, from `Items` to `Transaction Items`.
* `Payout Item` prefab (previously located in Promotion folder) has been renamed to `Vertical Payout Item` and moved to a new folder, `Store/Payout Items`.
* `Horizontal Transaction Item` prefab filename has been changed to `Transaction Item`.
* All MonoBehaviours, prefabs, and scenes in `Game Foundation Prefabs.unitypackage` are now using TextMeshProUGUI components instead of Text components.
* `StoreView` content generation is optimized to reuse existing GameObjects when possible.

### Removed

* `Grid Store`, `Horizontal Store`, `Vertical Transaction Item`, and `Columns Transaction Item` prefabs has been removed.
* `Grid Store` and `Horizontal Store` sample scenes has been removed. 
* Support for In App Purchasing versions older than 3.0 are now deprecated and will be removed in a future release.
* Dropped support for ChilliConnect. This will be replaced soon by [Unity Gaming Services](https://unity.com/solutions/gaming-services).

### Fixed

* Fixed bug where ResetIfExpired for a Reward would still be true but display as false in Editor under certain circumstances.
* Fixed bug where if Reward expiration time is 0 and ResetIfExpired is true the reward would still reset, where instead it should not reset when expiration time is 0.
* Fixed case where prefabs in the same scene as a GameFoundationInit that has a catalog override selected would display items and properties from the catalog selected in GameFoundationCatalogSettings, instead of from the catalog override selection.
* Fixed issue in prefabs where the icon image size is reset to its native dimensions at runtime or if a different definition is selected. Now you can resize the image's rect transform to the size you like and the icon image's native size will no longer be a factor. This may cause existing projects to need to reset the transforms for the icon so that it is not shifted over.
* Fixed bug where Promotion Popup Prefab would not auto lay out promotion items correctly after calling the Open() method.
* Fixed bug where the inspectors for the PurchaseButton component or any components that pass info to a PurchaseButton (StoreView, PromotionPopupView, etc) were getting their list of possible keys from the Transaction's payout list instead of it's costs list.
* Fixed bug where static property icon key fields in component inspectors were showing keys for any type of Resources Asset, when they only support Sprite resources.
* Fixed bug where when creating a Catalog Asset from the right click -> Create menu in the Project window, the window navigates one folder up in the hierarchy once the catalog is created.

### Prefab and Sample Package Note
If the new prefab package is installed on top of a previous package, the `Vertical Payout Item` and `Transaction Item` prefabs, `01 - Store` and `02 - Manual Store` scenes will show up as newly created files.
The old `Payout Item`, `Horizontal Transaction Item`, and `Grid Store` prefabs and `Store - Horizotal`, `Store - Vertical`, and `Store Grid` sample scenes are no longer supported, and can be deleted.
All the current prefabs will change to point to the new prefabs.

## [0.8.0] - 2020-11-20

### Added

* Inventory now implements a new event `itemMutablePropertyChanged` which is fired any time any item's mutable property is changed in the entire `GameFoundationSdk.inventory` (specific item and property effected are passed as an argument).  This is in addition to individual InventoryItem's `mutablePropertyChanged` event which targets only mutable property changes to a specific `InventoryItem`.
* `GameFoundationSdk.inventory` now emits OnCollectionAddedCallback or OnCollectionDeletedCallback events when `IItemCollections` are created or deleted (respectively), as well as OnCollectionItemAddedCallback or OnCollectionItemRemovedCallback events whenever items are added to or removed from `IItemCollections`.
* Added a `[Generate Constants]` button to `CatalogAssets` in Unity Inspector window to permit exporting constants.  To utilize this feature, select a Catalog (i.e. 'SampleCatalog') and click the `[Generate Constants]` button at the top.  Specify an export filename (must be inside Assets folder and is of type 'cs') and GameFoundation will generate all constants to simplify catalog access and avoid typos.
* `GameFoundationSdk.inventory` now allows `StackableInventoryItems` to be created with quantity in 1 step using `CreateItem(StackableInventoryItemDefinition, quantity)`.
* `InventoryItemDefinitionAsset.initialQuantityPerStack` has been added to the list of overridable catalog data for `IExternalValueProvider`.
* Some fields from the different implementations of `CatalogItemAsset` are now wrapped in a `ExternalizableValue<T>`. They can be used as regular `T` properties.
  These fields are externalizable through `IExternalValueProvider` if you want to override them with an external source.
* Rewards now appear in the Game Foundation Debugger.

### Changed

* Game Foundation Init Prefab's default Data Layer has changed to Local Data Persistence from Memory Layer.
* PurchaseButton and RewardClaimButton prefabs now have a weak reference to the Purchase and Claim methods.
  They can be found in the button's onClick listener list in the inspector, and can be removed to change the functionality of what happens to the button when clicked.
* Renamed `Reward Popup` prefab to `Progressive Reward Popup`.
* Game Foundation initialization will no longer fail if an IAP Transaction's product id is missing for the platform being built to.
  The purchase of that IAP Transaction can be attempted but will fail because a product id is required, however it will no longer prevent all of Game Foundation from functioning.
* `Deferred` and `Deferred<T>` are now `IDisposable` so you can use them in an `using` block to release them automatically.
* All sample scenes using Memory Data Layer or Persistence Data Layer now use Game Foundation Init Prefab instead of code initialization.
* Changed GameFoundationSdk.dataLayer property to public from internal.

### Removed

* `IDictionaryConvertible` has been removed since the `JsonDetail` no longer exists.

### Fixed

* Fixed compilation errors when using IAP package version 2.1.0 or 2.1.1.
* Fixed an issue where the reward countdown would start going backwards if you missed an item and `Reset if Expired` is set to false.
* Fixed an issue where a reward with multiple items, `Reset if Expired` set to true, and a countdown of 0 would be reset immediately after the first claim.
* Fixed an issue where a missed reward item would show as Locked if it was right before the latest claimed item and during cooldown.
* GameFoundationInit component now serializes changes to the Override Catalog Asset bool and asset values, and the catalog asset value is no longer forgotten when unchecked.

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
