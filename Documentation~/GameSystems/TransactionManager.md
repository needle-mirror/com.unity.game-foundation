# The Transaction Manager

## Overview

The __Transaction Manager__, accessible through [GameFoundationSdk](../GameFoundationSdk.md), is responsible for processing both [Virtual Transactions](../CatalogItems/VirtualTransaction.md) and [IAP Transaction](../CatalogItems/IAPTransaction.md) within your game. A transaction describes the ability to trade something for something else. You could have a Virtual Transaction, such as spending gems to get coins, or using a banana and an orange to create a smoothie. Or you can have IAP Transactions, in which you turn real money into in-game items or currencies.

This can be an instant process, as in the case of a local-only virtual transaction, or it could be an asynchronous process, as in the case of an IAP purchase that also needs to be synced with a cloud backend. The TransactionManager handles both cases the same way. When you start processing a transaction, you can use the returned Deferred object to monitor the status of the transaction until it's done.

Currently, virtual transactions must finish in the same session they are started. However, if an IAP transaction succeeds in the native side, but then the app crashes, the system will still try to complete the transaction when the app is restarted (this is a built-in feature of the Unity Purchasing SDK).

## Getting transactions

Transactions are not created, modified, or destroyed at runtime. You can find all transaction definitions in the transaction [catalog]. Both [Virtual Transactions] and [IAP Transactions] found there can be processed by the __Transaction Manager__ in the same way.

## Processing transactions

Processing a transaction is as easy as sending the transaction into the __Transaction Manager__ and then monitoring the progress from there, and dealing with any successes or failures as necessary.


[catalog]: ../Catalog.md

[Virtual Transactions]: ../CatalogItems/VirtualTransaction.md

[IAP Transactions]: ../CatalogItems/IAPTransaction.md
