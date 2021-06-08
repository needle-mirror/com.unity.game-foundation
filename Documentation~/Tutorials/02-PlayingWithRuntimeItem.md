# Game Foundation Tutorials

## Playing with items at runtime

We've created an [inventory item definition] in the [previous tutorial].
Let's see what we can do with it.

### Initialization of Game Foundation at runtime

#### Using Game Foundation Init Prefab
You can use the Game Foundation Init prefab to handle Game Foundation Initialization for you.
It supports initializing via the [Memory Data Layer] or the [Local Persistence Data Layer]. (You can also read more about the Data Layer [further down in this tutorial].)

- Create a new scene
- Go to `Window -> Game Foundation` and click `Create Game Foundation Init Game Object`
- A new game object called `Game Foundation` will appear in your Hierarchy.
Note that you can only have one of these objects in your scene at any given time.
Should duplicates somehow end up in the scene, only one will be kept, the others will be destroyed.
- With the Game Foundation game object selected, in the Inspector choose which Data Layer Type you want to use.
- If you select Local Persistence, enter the name of your Local Persistence Filename on the next line.

That is all you have to do!
Should you want to though, you can also override the Catalog Asset set in the GameFoundationCatalogSettings, or add listeners for Game Foundation initialization status changes.

Alternatively, you can initialize Game Foundation manually.

#### Initializing manually
- Create a new scene
- Create a new `game object` and name it `Game Foundation Init` (the name doesn't really matter).
- Add a new `MonoBehaviour` to your project and name it `GFInit`.
- Copy the following code into this script file:

```cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.Promise;

public class GFInit : MonoBehaviour
{
    IEnumerator Start()
    {
        // Creates a new data layer for Game Foundation,
        // with the default parameters.
        MemoryDataLayer dataLayer = new MemoryDataLayer();

        // - Initializes Game Foundation with the data layer.
        // - We use a using block to automatically release the deferred promise handler.
        using (Deferred initDeferred = GameFoundationSdk.Initialize(dataLayer))
        {
            yield return initDeferred.Wait();

            if (initDeferred.isFulfilled)
                OnInitSucceeded();
            else
                OnInitFailed(initDeferred.error);
        }
    }

    // Called when Game Foundation is successfully initialized.
    void OnInitSucceeded()
    {
        Debug.Log("Game Foundation is successfully initialized");
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```

Come back to your editor, and start your game by pushing the `Play` button.
In the console, you should see two log entries:

```
! `Successfully initialized Game Foundation version x.y.z`
! `Game Foundation is successfully initialized`
```

The second is the one you've written, while the first is logged by Game Foundation automatically.

### The Data Layer

While Game Foundation provides general APIs to manipulate your items, it needs a source to load the data, and a destination to push the states of its items. 

Given the different needs in game development in terms of loading / saving data, for example storing it locally on the device vs. remotely on a cloud storage, we want to provide a good abstraction so that our implementation can be built without too much assumptions on the underlying data storage and protocal. 

The [data layer] fulfills this role.
It gives Game Foundation a [catalog] of the static data, and Game Foundation notifies it of all the instance data it creates, modifies, or destroys.

The default constructor of the [MemoryDataLayer] loads the database of the static data you've manipulated in the [previous tutorial].
You can find this database in the `Assets/GameFoundation` folder, with the name `GameFoundationDatabase.asset`.

### Getting the inventory item definition at runtime

Go back to the `OnInitSucceeded` method, and add the following code, right under the `Debug.Log` statement:

```cs
void OnInitSucceeded()
{
    Debug.Log("Game Foundation is successfully initialized");

    // Use the key you've used in the previous tutorial.
    const string definitionKey = "myFirstItem";

    // Finding a definition takes a non-null string parameter.
    InventoryItemDefinition definition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(definitionKey);

    // Make sure you retrieved a valid definition.
    if (definition is null)
    {
        Debug.Log($"Definition {definitionKey} not found");
        return;
    }

    // You should be able to get information from your definition now.
    Debug.Log($"Definition {definition.key} '{definition.displayName}' found.");
}
```

Compile and start your scene.
You should see the following log in your console:

```
! Definition myFirstItem 'My First Item' found.
```

### Creating an item instance

Now that we have an [inventory item definition], we can create an item instance.
Go back to your `OnInitSucceeded` method, then append the following lines:

```cs
InventoryItem item = GameFoundationSdk.inventory.CreateItem(definition);

Debug.Log($"Item {item.id} of definition '{item.definition.key}' created");
```

Compile and start your scene.
You should see a new log entry:

```
Item xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx of definition 'myFirstItem' created
```

When created, an item instance is assigned a unique id, as a string version of a GUID.
This ID ensure that the item will remain unique.

### Removing the item instance

Now that we have an item instance, why not trying to remove it.
Go back to your `OnInitSucceeded` method, then append the following lines:

```cs
bool removed = GameFoundationSdk.inventory.Delete(item);

if (!removed)
{
    Debug.LogError($"Unable to remove item {item.id}");
    return;
}

Debug.Log($"Item {item.id} successfully removed. Its discarded value is {item.hasBeenDiscarded.ToString()}");
```

Compile and start your scene.
You should see a new log entry:

```
Item xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx successfully removed. Its discarded value is True
```

Removing an item from Game Foundation doesn't destroy the object.
It is a managed object, so it cannot be really removed as long as it is not garbage collected.
It just tells the [data layer] that the item has to be removed from the persistence, and it sets the `item.discarded property` to `true`.  
A lot of properties are still accessible, but you cannot modify your object anymore, nor use it in the other APIs of Game Foundation.
This is pretty convenient, as you'd need to keep an access to the properties of a removed item in case you need to propagate its removal in your gameplay code.

### Conclusion

Item instance can represent a lot of different aspects of your game.
As soon as you want to manipulate an object with static data and properties, being able to create it, but also destroying on demand, and if you want to persist it in the player profile, then item instance should be the feature you'd need.

It can be the player inventory: the character equipment, consumable items like potions or buffs.
It can be the character itself.  
The way you'll use [Inventory Item Definition] and their instance is up to you.

But what if what you want now, is to play with [currencies]?
Let's switch to the [next tutorial] then.

### Final source code of this tutorial

```cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.Promise;

public class GFInit : MonoBehaviour
{
    IEnumerator Start()
    {
        // Creates a new data layer for Game Foundation,
        // with the default parameters.
        MemoryDataLayer dataLayer = new MemoryDataLayer();

        // - Initializes Game Foundation with the data layer.
        // - We use a using block to automatically release the deferred promise handler.
        using (Deferred initDeferred = GameFoundationSdk.Initialize(dataLayer))
        {
            yield return initDeferred.Wait();

            if (initDeferred.isFulfilled)
                OnInitSucceeded();
            else
                OnInitFailed(initDeferred.error);
        }
    }

    // Called when Game Foundation is successfully initialized.
    void OnInitSucceeded()
    {
        Debug.Log("Game Foundation is successfully initialized");

        // Use the key you've used in the previous tutorial.
        const string definitionKey = "myFirstItem";

        // Finding a definition takes a non-null string parameter.
        InventoryItemDefinition definition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(definitionKey);

        // Make sure you retrieved a valid definition.
        if (definition is null)
        {
            Debug.Log($"Definition {definitionKey} not found");
            return;
        }

        // You should be able to get information from your definition now.
        Debug.Log($"Definition {definition.key} '{definition.displayName}' found.");

        InventoryItem item = GameFoundationSdk.inventory.CreateItem(definition);

        Debug.Log($"Item {item.id} of definition '{item.definition.key}' created");

        bool removed = GameFoundationSdk.inventory.Delete(item);

        if (!removed)
        {
            Debug.LogError($"Unable to remove item {item.id}");
            return;
        }

        Debug.Log($"Item {item.id} successfully removed. Its discarded value is {item.hasBeenDiscarded.ToString()}");
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```

[inventory item definition]: ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[definition]:                ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[definitions]:               ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[item definition]:           ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[item definitions]:          ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"

[tag]:   ../CatalogItems/Tag.md "Go to Tag"
[tags]:  ../CatalogItems/Tag.md "Go to Tag"

[previous tutorial]: 01-CreatingAnItemDefinition.md "Creating an Inventory Item Definition"

[memory data layer]: ../DataLayers.md#memory-data-layer "Go to Memory Data Layer"

[local persistence data layer]: ../DataLayers.md#persistence-data-layer "Go to Persistence Data Layer"

[further down in this tutorial]: 02-PlayingWithRuntimeItem.md#the-data-layer "Go to Data Layer section"

[catalog]: ../Catalog.md "Go to Catalog"

[data layer]: ../DataLayers.md "Go to Data Layers"

[memorydatalayer]: ../DataLayers.md#memory-data-layer "Go to Memory Data Layer"

[currencies]: ../CatalogItems/Currency.md "To to Currency"

[stores]: ../CatalogItems/Store.md "Go to Store"

[transactions]: ../CatalogItems/VirtualTransaction.md "Go to Virtual Transaction"

[game parameters]: ../CatalogItems/GameParameters.md "Go to Game Parameters"

[next tutorial]: 03-CreatingCurrency.md
