# Game systems

## Data access layer

The data access layer is responsible for providing Game Foundation-related game state data to the Game Foundation systems.
This makes it easy for you to switch between no persistence, local persistence, and your own persistence, without changing how you use GameFoundation.

We provide ready-to-use implementations that match the most common cases: 
* Use a [MemoryDataLayer] if you want to play from a clean slate each session (especially useful for testing).
You can also get Game Foundation's raw data from it to serialize them however you want.
* Use a [PersistenceDataLayer] to save and load your progression on a local file.

You can also implement your own `IDataAccessLayer` if you have more specific requirements.

More info [in this page].

## Inventory

See the [Inventory Manager] page.

## Wallet

See the [Wallet Manager] page.

## Rewards

See the [Reward Manager] page.

## Transaction

See the [Transaction Manager] page.









[inventory manager]: GameSystems/InventoryManager.md

[wallet manager]: GameSystems/WalletManager.md

[reward manager]: GameSystems/RewardManager.md

[transaction manager]: GameSystems/TransactionManager.md

[memorydatalayer]: DataLayers.md#memory-data-layer

[persistencedatalayer]: DataLayers.md#persistence-data-layer

[in this page]: DataLayers.md
