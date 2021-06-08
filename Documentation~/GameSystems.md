# Game Systems

The following `Game Systems`, accessible through [GameFoundationSdk](GameFoundationSdk.md), can be used to enhance your game, permit [IAP Transactions](CatalogItems/IAPTransaction.md) and save time as you make your game.

## Inventory

The [Inventory Manager](GameSystems/InventoryManager.md) tracks all items in player's inventory (items could have been acquired through an [In App Purchase](CatalogItems/IAPTransaction.md), a [Virtual Transaction](CatalogItems/VirtualTransaction.md), or granted through script in response to a game event) and can be used by your [Game Economy](GameSystems/GameEconomy.md) to provide objectives and track accomplisments (i.e. game play) in your game.

## Wallet

The [Wallet Manager](GameSystems/WalletManager.md), similar to the Inventory Manager (above), tracks [Currencies](CatalogItems/Currency.md) acquired by the player in your game to maintain your [Game Economy](GameSystems/GameEconomy.md).

## Reward

The [Reward Manager](GameSystems/RewardManager.md) provides access to in-game rewards such as Daily Bonuses, Promotions, etc.  These rewards can promote retention and generally improve your player's experience in your game world.

## Transaction

The [Transaction Manager](GameSystems/TransactionManager.md) process both [Virtual Transaction](CatalogItems/VirtualTransaction.md) and [IAP Transaction](CatalogItems/IAPTransaction.md) within your game.


## Data Access Layer

The data access layer is responsible for providing Game Foundation-related game state data to the Game Foundation systems.
This makes it easy for you to switch between no persistence, local persistence, and your own persistence, without changing how you use GameFoundation.

We provide ready-to-use implementations that match the most common cases: 
* Use a [MemoryDataLayer] if you want to play from a clean slate each session (especially useful for testing).
You can also get Game Foundation's raw data from it to serialize them however you want.
* Use a [PersistenceDataLayer] to save and load your progression on a local file.

You can also implement your own `IDataAccessLayer` if you have more specific requirements.

More info [in this page].


[inventory manager]: GameSystems/InventoryManager.md

[wallet manager]: GameSystems/WalletManager.md

[reward manager]: GameSystems/RewardManager.md

[transaction manager]: GameSystems/TransactionManager.md

[memorydatalayer]: DataLayers.md#memory-data-layer

[persistencedatalayer]: DataLayers.md#persistence-data-layer

[in this page]: DataLayers.md
