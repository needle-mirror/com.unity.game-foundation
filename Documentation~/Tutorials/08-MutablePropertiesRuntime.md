# Game Foundation Tutorials

## Playing with properties at runtime

In [the previous tutorial], we learned how to configure mutable properties in the [item definition].

In this one, we'll take a look at the code to get and modify the property of an item instance.

### Prerequisites

We'll start from the end of [the inventory tutorial].
If you didn't follow this tutorial, please copy/paste [the final source code] in to a class file named `GFInit.cs` and associate it to a new `GameObject` of the new scene.

### Checking the property existence for an item

Open your `GFInit.cs` file and replace `OnInitSucceeded` with the following code:

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

    InventoryItem item = GameFoundationSdk.inventory.CreateItem(definition);

    Debug.Log($"Item {item.id} of definition '{item.definition.key}' created");

    // This is the key of the property we've created
    const string propertyKey = "durability";

    // Check if the item has the property. Note that you can check
    // the property's existence on both the item and its definition.
    //bool hasProperty = definition.HasProperty(propertyKey);
    bool hasProperty = item.HasProperty(propertyKey);

    if (!hasProperty)
    {
        Debug.LogError($"Cannot find the property {propertyKey}");
        return;
    }

    Debug.Log($"Property '{propertyKey}' found");
}
```

Compile and start the scene.
You should see the following log entry:

```
! Property 'durability' found
```

### Getting the property's default value

You can retrieve the default value of a property within an [item definition] as following.
Back to your code, append the follow code:

```cs
// Get the default property from the item definition.
Property defaultValue = definition.GetDefaultProperty(propertyKey);

Debug.Log($"The default value of '{propertyKey}' is {defaultValue.AsString()}");
```

Compile and start the scene.
You should see the following log entry:

```
! The default value of 'durability' is 20
```

### Getting the current value of the property of an item instance

While the default value of a property cannot be changed in the definition, the item instance can overwrite it.

Append the following code:

```cs
// Tries to get the value of the property for the created item
bool found = item.TryGetProperty(propertyKey, out var propertyValue);
if (!found)
{
    Debug.LogError($"item '{definition.displayName}' doesn't have a property '{propertyKey}'");
}
else
{
    Debug.Log($"item '{definition.displayName}' value for '{propertyKey}' is {propertyValue.AsString()}");
}
```

This new code should log the following message:

```
! item 'My First Item' value for 'durability' is 20
```

As expected, the current value is the same as the default one.
That's because we haven't played with it yet.
That's what we are about to do, with the following code:

```cs
item.SetProperty(propertyKey, 18);

propertyValue = item.GetProperty(propertyKey);

Debug.Log($"item '{definition.displayName}' value for '{propertyKey}' is {propertyValue.AsString()}");
```

Start your scene.
Your console should have the following log:

```
! item 'My First Item' value for 'durability' is 18
```

As you can see, setting a property is pretty straightforward.

### Conclusion

Playing with [static properties] and [mutable properties] of your [catalog items] will help you build a better economy for your game.

### Going forward

Speaking about economy, now is the time to turn your game into a successful business.

In [the next tutorial], we'll talk about transactions.
But we'll start smooth with the virtual transactions.

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

        // <---- Insert the following code here

        // This is the key of the property we've created
        const string propertyKey = "durability";

        // Check if the item has the property. Note that you can check
        // the property's existence on both the item and its definition.
        //bool hasProperty = definition.HasProperty(propertyKey);
        bool hasProperty = item.HasProperty(propertyKey);

        if (!hasProperty)
        {
            Debug.LogError($"Cannot find the property {propertyKey}");
            return;
        }

        Debug.Log($"Property '{propertyKey}' found");

        // Get the default property from the item definition.
        Property defaultValue = definition.GetDefaultProperty(propertyKey);

        Debug.Log($"The default value of '{propertyKey}' is {defaultValue.AsString()}");

        // Tries to get the value of the property for the created item
        bool found = item.TryGetProperty(propertyKey, out var propertyValue);
        if (!found)
        {
            Debug.LogError($"item '{definition.displayName}' doesn't have a property '{propertyKey}'");
        }
        else
        {
            Debug.Log($"item '{definition.displayName}' value for '{propertyKey}' is {propertyValue.AsString()}");
        }

        item.SetProperty(propertyKey, 18);

        propertyValue = item.GetProperty(propertyKey);

        Debug.Log($"item '{definition.displayName}' value for '{propertyKey}' is {propertyValue.AsString()}");
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```


[the previous tutorial]: 07-MutablePropertiesEditor.md

[the inventory tutorial]:   02-PlayingWithRuntimeItem.md

[the final source code]:    02-PlayingWithRuntimeItem.md#Final-source-code-of-this-tutorial

[item definition]: ../CatalogItems/InventoryItemDefinition.md

[static properties]: 06-StaticProperties.md

[catalog items]: ../Catalog.md#Catalog-Items

[the next tutorial]: 09-CreatingAVirtualTransaction.md
