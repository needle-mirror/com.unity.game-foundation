# The Wallet Manager

## Overview

The __Wallet Manager__, accessible through [GameFoundationSdk](../GameFoundationSdk.md), is dedicated to the management of the [currencies] and their balance.  
Contrary to the [Inventory Manager], which manages item instances, the __Wallet Manager__ doesn't create or destroy any object when adding or removing amounts. It just changes the balance of the related [currency].

## Wallet System vs Inventory System

### What's the difference 

The __Wallet Manager__ is designed for items that the player can collect plenty of.

> You have two swords in your inventory, each having a different `damage` property value.

Swords are item instances in the __Inventory Manager__.

> I have `1500` gold coins, and each coin doesn't have any individual property.

Gold coin is a [currency] of the __Wallet Manager__, with a balance of `1500`.

### Which one to choose? 

While both the Wallet system and [inventory manager] can help you manage concepts of in-game resources and track the quantities for them, if you want to be able to track each instance of a given item at runtime (for example if all instances of an item can be re-arranged in the player bag), then define it as an [inventory item] so you can track the attributes of each instance separately at runtime.

On the other hand, if you only need to track the aggregated amount of a given resource, then defining them as [currency] will allow Game Foundation to handle these with more performance efficiency. 

We will revise the user-facing name of these two systems in the near future to better distinguish the runtime behavior difference while removing the opinionate-ness of the use case of certain in-game resources (as money vs. generic items). In the meanwhile, use this to guide your implementatino decisions on which one to choose. 

## Initialization

The __Wallet Manager__ is initialized with all the available [currency] types defined in the [Catalog].  

Only new players can benefit from the `Initial Allocation` of a [currency].
If a new [currency] is created between two sessions of an existing player, their balance will be `0` for this new currency.

## API

The __Wallet Manager__ comes with a basic set of expected methods to:

- Get the balance of a [currency].
- Set the balance of a [currency].
- Adjust the balance by adding or removing an amount.
















[currencies]: ../CatalogItems/Currency.md
[currency]:   ../CatalogItems/Currency.md
[inventory item]: ../CatalogItems/InventoryItemDefinition.md  
[inventory manager]: ../GameSystems/InventoryManager.md
[catalog]: ../Catalog.md
