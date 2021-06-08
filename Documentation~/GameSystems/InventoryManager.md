# The Inventory Manager

## Overview

The __Inventory Manager__, accessible through [GameFoundationSdk](../GameFoundationSdk.md), is a central piece of the Game Foundation architecture.  It tracks all Inventory Items in a player's inventory acquired through an [In-App Purchase](../CatalogItems/IAPTransaction.md), a [Virtual Transaction](../CatalogItems/VirtualTransaction.md), or a script in response to a game event. Inventory Items can track player progress, purchases, rewards, etc. and can also be used by your [Game Economy](../GameSystems/GameEconomy.md) to provide objectives and track game play accomplishments.

![Main inventory example](../images/image16.png)  
*Example: the main inventory contains characters, hats, bonuses, and themes.*

## Creating/Removing items

In order to create an item, the __Inventory Manager__ requires an [Inventory Item Definition].
This [definition] can be found in the [Catalog] and passed to the `CreateItem()` method, or specified with its `Key`.

When created, an item is assigned a unique identifier (`ID`), and its properties are initialized with their default values.

## Getting items

The __Inventory Manager__ provides some methods to retrieve items, or find specific items by their `ID`, their [definition] or their [tags]

> Some of the methods allocate an array to return the collection of items found.
> You'll also find a non-allocating version of those methods, accepting a target collection.

## Removing items

To remove items, the __Inventory Manager__ exposes various methods:

- Removing one item by passing its reference.
- Removing one item by passing its `ID`.
- Removing items by their [definition].
- Removing all the items.

Removing items from the __Inventory Manager__ doesn't destroy the item object in memory.
It wouldn't be an expected behaviour for a managed language like C#.
Instead, the items are _discarded_: they are removed from the [data layer] and they cannot be part of any process, but their `ID`, `display name` and `definition` remain accessible.


[inventory item definition]: ../CatalogItems/InventoryItemDefinition.md
[definition]:                ../CatalogItems/InventoryItemDefinition.md

[catalog]: ../Catalog.md

[tags]: ../CatalogItems/Tag.md

[data layer]: ../DataLayers.md
