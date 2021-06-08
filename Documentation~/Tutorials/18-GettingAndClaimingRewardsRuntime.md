# Game Foundation Tutorials

## Getting and claiming a reward at runtime

So far you have set up [Reward Definitions] by [creating a simple daily reward or progressive daily reward in your game], and seen how to use them with the Progressive Reward Popup prefab in the [previous tutorial].
However there may be times where you don't want to use the Reward prefab, and want to work directly with the Reward system.
This tutorial will show you how to do that.

### Finding a reward item to claim

To claim a reward at runtime, you'll need the key of the reward and the key of the reward item. 

The key of the reward can be found by going to __Unity__ > __Game Foundation__ > __Reward__, then selecting the __Reward__ from the list of reward items, and looking at the __Key__ field under the __General__ section (see the [first reward tutorial]).

Once you have the __Reward__ key, you can use it to find a __Reward__ object.

```cs
var reward = GameFoundationSdk.rewards.FindReward("dailyCoinGift");
```

This __Reward__ object can then be used to get the key of the currently claimable __Reward Item__, if any.
Once you have the key of the claimable Reward Item, you can initiate the claim on it by passing in both the __Reward__ and the __Reward Item__ key. Claiming a reward is an asynchronous process, so you'll need a coroutine to do so. You might notice a similarity between this and [processing a Virtual Transaction]. Here's an example of claiming a reward:

```cs
IEnumerator ClaimReward()
{
    var reward = GameFoundationSdk.rewards.FindReward("sevenDayProgressive");

    string claimableKey = reward.GetLastClaimableRewardItemKey();

    // We use a using block to automatically release the deferred promise handler.
    using (var deferredResult = GameFoundationSdk.rewards.Claim(reward.rewardDefinition, claimableKey))
    {
        // Wait for the process to finish
        while (!deferredResult.isDone)
        {
            yield return null;
        }

        // The process failed
        if (!deferredResult.isFulfilled)
        {
            Debug.LogException(deferredResult.error);
        }

        // The process succeeded
        else
        {
            var result = deferredResult.result;

            foreach (var tradable in deferred.result.products)
            {
                if (tradable is CurrencyExchange currencyExchange)
                {
                    Debug.Log($"A reward item granted you "
                    + currencyExchange.currency.displayName
                    + " x " + currencyExchange.amount);
                }
            }
        }
    }
}
```

### Displaying the states of rewards

At runtime, the [Reward system] will automatically create instances of Reward objects to track the state of every reward defined in your catalog. The states are based on timestamps from when the player has made past claims.

Reward states aren't refreshed automatically. Call `GameFoundationSdk.rewards.Update()` from inside the `Update` or `LateUpdate` method in one of your MonoBehaviours to keep the reward system up-to-date.

As we already saw, you can get an individual reward from the Reward system like this:

```cs
var dailyCoinsReward = GameFoundationSdk.rewards.FindReward("sevenDayProgressive");
```

Similarly, to check the state of all Rewards in the Reward system:

```cs
var myRewards = new List<Reward>();
GameFoundationSdk.rewards.GetRewards(myRewards);
```

For any reward state object, you can check several aspects of its state, such as cooldown and claimability. Examples:

- __IsInCooldown__: Tells whether this item is locked because it was claimed recently
- __countdownSeconds__: How many seconds until a new item becomes claimable
- __countdownTicks__: If you prefer a more precise interval than seconds
- __rewardItemStates__: A dictionary where the key is a reward item key, and the value is one of the values in a RewardItemState

> Possible RewardItemState values:
> - __Locked__: The reward item cannot be claimed right now.
> - __Claimable__: The reward item can be claimed right now.
> - __Claimed__: The reward item has already been claimed. 
> - __Missed__: The reward item was not claimed within the valid window, so it can no longer be claimed.

### Conclusion
We hope the Game Foundation Reward System helps you to provide the most enjoyable game experience to your players, thereby helping your retention!


[Reward definitions]: ../CatalogItems/RewardDefinition.md
[creating a simple daily reward or progressive daily reward in your game]: 17-CreatingRewards.md
[previous tutorial]: 18-WorkingWithRewardPrefabs.md
[first reward tutorial]: 17-CreatingRewards.md
[runtime transaction tutorial]: 10-PlayingWithRuntimeVirtualTransaction.md
[reward system]: ../GameSystems/RewardManager.md
[processing a Virtual Transaction]: 10-PlayingWithRuntimeVirtualTransaction.md
