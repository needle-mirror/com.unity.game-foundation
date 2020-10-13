using System;
using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Manages <see cref="Reward"/>s at runtime.
    /// </summary>
    public interface IRewardManager
    {
        /// <summary>
        ///     Invoked as soon as a valid reward item claim is initiated.
        ///     Parameters: reward key, reward item key
        /// </summary>
        event Action<string, string> rewardItemClaimInitiated;

        /// <summary>
        ///     Invoked when the progress is incremented on a reward item's <see cref="Deferred{TResult}"/>.
        ///     Parameters: Reward, reward item key, current step (zero-based), total steps
        /// </summary>
        event Action<Reward, string, int, int> rewardItemClaimProgressed;

        /// <summary>
        ///     Invoked after a reward item claim has succeeded at all levels.
        ///     Parameters: Reward, reward item key, currencies and items paid out
        /// </summary>
        event Action<Reward, string, Payout> rewardItemClaimSucceeded;

        /// <summary>
        ///     Invoked after a reward item claim fails.
        ///     Parameters: reward key, reward item key, exception
        /// </summary>
        event Action<string, string, Exception> rewardItemClaimFailed;

        /// <summary>
        ///     Checks each reward to see if it's time for a state change.
        ///     If yes, then recalculate the state of that reward.
        /// </summary>
        void Update();

        /// <summary>
        ///     Find a reward instance based on the reward definition key.
        /// </summary>
        /// <param name="rewardKey">
        ///     The reward definition key to search for.
        /// </param>
        /// <returns>
        ///     An instance of a <see cref="Reward"/>.
        /// </returns>
        Reward FindReward(string rewardKey);

        /// <summary>
        ///     Get all <see cref="Reward"/> instances.
        /// </summary>
        /// <param name="target">
        ///     A List to clear and populate with the reward instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="Reward"/> instances found.
        /// </returns>
        int GetRewards(ICollection<Reward> target = null, bool clearTarget = true);

        /// <summary>
        ///     Get the key of the reward item that is currently claimable.
        ///     If multiple items are claimable, then this returns the one
        ///     with the highest position (index) in the reward item collection.
        /// </summary>
        /// <param name="rewardDefinition">
        ///     The reward to search for a claimable item.
        /// </param>
        /// <returns>
        ///     Returns the key of a claimable reward item, or an empty string if none are claimable.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown if the reward key does not match any known RewardDefinition in the catalog.
        /// </exception>
        string GetLastClaimableRewardItemKey(RewardDefinition rewardDefinition);

        /// <summary>
        ///     Claim a reward item's payout.
        /// </summary>
        /// <param name="rewardDefinition">
        ///     The reward definition that contains the reward item you wish to claim.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the reward item you wish to claim.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="Deferred{TPayout}"/> for tracking the asynchronous <see cref="Promise{TPayout}"/>.
        /// </returns>
        Deferred<Payout> Claim(RewardDefinition rewardDefinition, string rewardItemKey);
    }
}
