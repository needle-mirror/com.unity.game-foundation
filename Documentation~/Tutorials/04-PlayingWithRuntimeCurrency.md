# Game Foundation Tutorials

## Playing with currencies at runtime

We've created a [currency] in the [previous tutorial].
Let's see how to play with this new item.

For this tutorial, please follow the [initialization steps you can find in the inventory tutorial].

### Getting the currency at runtime

With your script created, go to the `OnInitSucceeded` method, and replace it with the following code

```cs
void OnInitSucceeded()
{
    Debug.Log("Game Foundation is successfully initialized");

    // Use the key you've used in the previous tutorial.
    const string definitionKey = "myFirstCurrency";

    // Finding a currency takes a non-null string parameter.
    Currency definition = GameFoundationSdk.catalog.Find<Currency>(definitionKey);

    // Make sure you retrieved a valid currency.
    if (definition is null)
    {
        Debug.Log($"Definition {definitionKey} not found");
        return;
    }

    Debug.Log($"Definition {definition.key} ({definition.type}) '{definition.displayName}' found.");
}
```

Compile and start your scene.
You should see the following log entry in your console:

```
! Definition myFirstCurrency (Soft) 'My First Currency' found.
```

The API is very similar to the one described in the [inventory item tutorial].

### Getting the balance of a currency

We now have a valid definition of a [currency].
What I want to know now is the amount of this [currency] the player owns.
Go back to your `OnInitSucceeded` method and append the following lines:

```cs
// You can get the balance of a currency with the WalletManager.
long balance = GameFoundationSdk.wallet.Get(definition);

Debug.Log($"The balance of '{definition.displayName}' is {balance.ToString()}");
```

Compile and start your scene.
You should see the following log entry in your console:

```
! The balance of 'My First Currency' is xxx
```

`xxx` is the value you've set for the `initial allocation`.

### Changing the balance

We just learned how to get the balance amount, now let's see how we can make changes to it as well. 

The [WalletManager] exposes a simple API to change the balance of a currency.
We'll see here the most straightforward method of its API.  
Go back to your `OnInitSucceeded` method and append the following lines:

```cs
// Set replaces the current balance by a new one.
bool success = GameFoundationSdk.wallet.Set(definition, 50);

if (!success)
{
    Debug.LogError($"Failed in setting a new value for '{definition.displayName}'");
    return;
}

var newBalance = GameFoundationSdk.wallet.Get(definition);

Debug.Log($"The new balance of '{definition.displayName}' is {newBalance.ToString()}");
```

Compile and start your scene.
You should see a new log entry:

```
The new balance of 'My First Currency' is 50
```

The [WalletManager] also exposes methods to add and subtract an amount from the balance.

### Conclusion

The [WalletManager] is a very simple API, but a lot of games, especially on mobile, can find usage in this one.

### Going forward

`Debug.Log`-ging your balance and items may be a bit intrusive in your code.
You may need a better way to monitor the player profile, and that's what we'll see in the [next tutorial].


[currency]: ../CatalogItems/Currency.md

[previous tutorial]: 03-CreatingCurrency.md

[initialization steps you can find in the inventory tutorial]: 02-PlayingWithRuntimeItem.md#initialization-of-game-foundation-at-runtime

[inventory item tutorial]: 02-PlayingWithRuntimeItem.md#getting-the-inventory-item-definition-at-runtime

[WalletManager]: ../GameSystems/WalletManager.md

[next tutorial]: 05-Debugger.md
