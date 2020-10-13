# The Game Foundation Database

## Overview

The __Game Foundation Database__ contains all the static data.
This data is grouped into different catalogs, each of them dealing with a kind of [catalog item].

A default database is created automatically when you open one of the Game Foundation windows, but you can also create your own using __Assets → Create → Game Foundation → Database__.

## Catalog Items

__[Catalog Items]__ are where data about the items are defined.
They are stored in their respective Game Foundation catalogs.

The current version of Game Foundation provides the following catalog item types:

- [Inventory Item Definition]
- [Currency]
- [Reward]
- [Virtual Transaction]
- [IAP Transaction]
- [Store]
- [Tags]
- [Game Parameters]

__Catalog Items__ can be assigned [tags] and properties.

## Editor Overview

Each catalog share the same editor layout.

![Catalog item generic editor](images/catalog-item-generic-editor.png)

(1) All catalog's items are listed on the left.
  Use the `+` button at the bottom to create a new one.

(2) The General section allows you to edit the Display Name of the item definition.
  The key is read-only. It can be set only at creation time.

(3) The [Tags] section is a widget which provides an intuitive way to assign [tags] to the edited item.  
  Start typing your tag, then select the proper one in the dynamically created list, or create a new one.

(4) Here are the sections specific to the type of edited catalog item. 

(5) In the **Static Properties** section you can define a list of fields for the catalog item to read at runtime.\
  A static property must define:
  - A value type. Supported types are `integer number` (e.g. int & long), `real number` (e.g. float & double), `bool`, `string` and `resources asset` (e.g. any asset stored in a Resources folder).
  - A unique key for you to access the property at runtime.
  - A value.

  Static Properties belong only to the definition they are declared into.
  This means you can use the same property key in different definitions with a different type if you want to.










[catalog item]:  #Catalog-Items
[catalog items]: #Catalog-Items

[inventory item definition]: CatalogItems/InventoryItemDefinition.md

[currency]: CatalogItems/Currency.md

[reward]: CatalogItems/Reward.md

[virtual transaction]: CatalogItems/VirtualTransaction.md

[iap transaction]: CatalogItems/IAPTransaction.md

[store]: CatalogItems/Store.md

[tags]: CatalogItems/Tag.md

[game parameters]: CatalogItems/GameParameter.md
