# Store

## Overview

Each __Store__ maintains a list of available transactions (IAP, Virtual or both) that are returned when the store is queried.  A Store is a [catalog item] providing a subset of the [transaction catalog]. 

If your game uses multiple stores each can contain a different list of products, and the [catalog item] is the one you'll use to create those lists.

You can define multiple stores to display a different set of products, depending on the types of purchase. For example, you can create a 'in-game shop' with virtual purchases, or an 'IAP store' with in-app purchase items. 

If you want to personalize the content in the store, you can also use multiple store setup for different situation of the player, e.g. according their level, their location in the game, their character, etc.

A __Store__ contains a list of transaction objects ([Virtual Transactions] and [IAP Transactions]).

## Editor Overview

Open the __Store window__ by going to __Window → Game Foundation → Store__.
The Store window will let you configure Stores.

![An overview of the Store Window](../images/store-editor.png)

The interface is similar to the other [catalog items editor].

(1) The Store Transactions section list all transactions available in the store.
  The `Visible` checkbox next to the product is a way to avoid the transaction items from being added to the list of products at runtime, without having to remove it explicitly from the list.
  In other words, it is a soft way to remove a product.\

(2) Use the __Other Available Transactions__ section to populate the Store Transactions list with the transactions defined in the [Virtual Transactions] and [IAP Transactions].


[catalog item]: ../Catalog.md#Catalog-Items

[catalog items editor]: ../Catalog.md#Editor-Overview

[transaction catalog]:  ../Catalog.md

[virtual transactions]: VirtualTransaction.md

[iap transactions]: IAPTransaction.md
