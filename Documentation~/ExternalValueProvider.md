# Using IExternalValueProvider at Initialization

When initializing Game Foundation using a `MemoryDataLayer` class or `PersistenceDataLayer` class, you typically set the `CatalogAsset` class with fixed data. While having fixed data for your catalog at initialization works in most situations, there may be cases where it's useful to have catalog data with different values available under specific conditions, or determined for a select group of players. Some examples of this being useful is when:

* [A/B testing](https://en.wikipedia.org/wiki/A/B_testing)
* Randomizing the catalog
* Handling different catalog configurations

Game Foundation lets you override fixed catalog data at initialization using the External Value Provider, in particular, implementing your own `IExternalValueProvider` interface. In other words, by binding to a `CatalogAsset` class, Game Foundation requests the `IExternalValueProvider` implementation for overriding a specified [list](#list-of-externalizable-values-to-override-catalog-data) of catalog data by providing a `catalogItemFieldName`/`catalogItemKey` key pair.

  - `catalogItemFieldName` is an identifier of the catalog item field to get a value for. Refer to `ExternalValueProviderNames` to see exact set of existing identifiers.
  - `catalogItemKey` is the key of the catalog item for which the override is requested.

Note that external value providers don't require a value for all given identifier pairs. If no value is provided for an identifier pair, the value from the catalog will be used.

**Important**: The `IExternalValueProvider.TryGetValue` method must be synchronous! If you need to initialize it and fetch data from a server, do it before binding it to the `CatalogAsset` class.


## List of externalizable values to override catalog data

All fields of a `CatalogItemAsset` and its inherited classes, wrapped into an `ExternalizableValue` are overridable. Here is a description of each class and the respective variables that can be overriden using `IExternalValueProvider`.

For the `CatalogItemAsset` class:
  - `displayName`: The name of a catalog item.
  - `staticProperties`: A set of user-defined properties for catalog items. Only the values are overridable, the structure and the types cannot be overriden.

For the `CurrencyAsset` class:
  - `initialBalance`: The balance provided to a new player.
  - `maximumBalance`: The maximum balance a player can have.
  - `currencyType`: The type of currency (Soft or Hard).

For the `InventoryItemDefinitionAsset` class:
  - `initialAllocation`: The amount of items made from this definition provided to a new player.
  - `mutableProperties`: A set of user-defined properties for items made from this definition. Only the default values are overridable, the structure and the types cannot be overriden.
  - `initialQuantityPerStack`: Only for stackable items - the quantity of items for each stack provided to a new player.

For the `IAPTransactionAsset` class:
  - `googleId`: The product ID for the Google Play Store.
  - `appleId`: The product ID for the Apple App Store.

For the `RewardAsset` class:
  - `cooldownSeconds`: The amount of time (in seconds) a player needs to wait after claiming a reward item before they can claim the next reward item in the sequence. 
  - `expirationSeconds`: The amount of time (in seconds) to claim a reward item before it becomes no longer available to the player. When a reward item is not claimed in time, it expires.
  - `resetIfExpired`: A flag to determine the behaviour if the current reward item expires. If true, the whole reward is reset to the first valid item in sequence, otherwise it passes to the next valid item.


## Code sample implementation of IExternalValueProvider

There are many applications of customizing the External Value Provider to create your own implementations of setting catalog data with different values. 

The following code sample is a simple implementation of the `IExternalValueProvider` interface that allows you to randomize the initial balance for currencies. This is just one example that you can easily use and adapt to implement your own value provider for another scenario.

```cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.Promise;

[Serializable]
public class CatalogItemRandomRange
{ 
    public string CatalogItemKey;

    [Min(0)]
    public long MinimumValue;

    [Min(0)]
    public long MaximumValue;
}

[Serializable]
public class CurrencyInitialBalanceRandomizer : IExternalValueProvider
{
    public List<CatalogItemRandomRange> InitialBalanceRanges;

    public bool TryGetValue(string catalogItemFieldName, string catalogItemKey, out Property value)
    {
        // Let's start by checking the given catalogItemFieldName to make sure it is handled by our value provider.
        // Remember to use the static class ExternalValueProviderNames to identify
        // what kind of catalogItemFieldName you are currently dealing with.
        // This example handles only one type of field but you can handle multiple field types in
        // the same value provider by using a more advanced matchup table than InitialBalanceRanges.
        if (catalogItemFieldName != ExternalValueProviderNames.initialBalance)
        {
            value = default;

            return default;
        }

        // Now that the kind of catalog field is confirmed, let's verify if the given catalogItemKey is handled.
        foreach (CatalogItemRandomRange range in InitialBalanceRanges)
        {
            if (range.CatalogItemKey != catalogItemKey)
            {
                continue;
            }

            // Randomize the value.
            value = UnityEngine.Random.Range(range.MinimumValue, range.MaximumValue);

            // Make sure the value is positive.
            value = Mathf.Max(0, value);

            return true;
        }

        // No match found.
        value = default;

        return default;
    }
}

public class Initializer : MonoBehaviour
{
    public CurrencyInitialBalanceRandomizer ValueProvider;

    IEnumerator Start()
    {
        // Get the catalog to give to Game Foundation.
        // Here we use CatalogSettings.catalogAsset for simplicity but you can use any other CatalogAsset.
        CatalogAsset catalog = CatalogSettings.catalogAsset;

        // Bind the value provider to the catalog.
        catalog.SetValueProvider(ValueProvider);

        // Create the data layer with the setup catalog.
        MemoryDataLayer dataLayer = new MemoryDataLayer(catalog);

        // Initialize Game Foundation.
        using (Deferred initialization = GameFoundationSdk.Initialize(dataLayer))
        {
            // Wait for the initialization to be over.
            if (!initialization.isDone)
            {
                yield return initialization.Wait();
            }

            // Make sure the initialization succeeded.
            if (!initialization.isFulfilled)
            {
                Debug.LogError(initialization.error);

                yield break;
            }

            // Get the randomized currency and log its current balance.
            const string key = "coins";
            var coins = GameFoundationSdk.catalog.Find<Currency>(key);
            Debug.Log($"{coins.displayName}'s initial balance is {coins.quantity.ToString()}.");
        }
    }
}
```

