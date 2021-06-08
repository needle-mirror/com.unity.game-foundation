using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Used to indicate the state of a reward item.
    /// </summary>
    public enum RewardItemState
    {
        /// <summary>
        ///     The reward os locked
        /// </summary>
        Locked = 0, // Default

        /// <summary>
        ///     The reward is claimable
        /// </summary>
        Claimable = 10,

        /// <summary>
        ///     The reward is claimed
        /// </summary>
        Claimed = 20,

        /// <summary>
        ///     The reward is missed
        /// </summary>
        Missed = 30,
    }

    /// <summary>
    ///     The state of a reward at runtime, managed by the <see cref="IRewardManager"/>.
    /// </summary>
    public class Reward
    {
        /// <summary>
        ///     Invoked when any of a Reward's item states have changed
        ///     (including when a Reward is reset).
        /// </summary>
        public static event Action<Reward> rewardStateChanged;

        /// <summary>
        ///     Invoked when a Reward has been reset (all the claim timestamps were deleted).
        /// </summary>
        public static event Action<Reward> rewardReset;

        /// <summary>
        ///     The unique key of the reward.
        /// </summary>
        public string key;

        /// <summary>
        ///     The <see cref="rewardDefinition"/> on which this reward instance is based.
        /// </summary>
        public RewardDefinition rewardDefinition;

        /// <summary>
        ///     For tracking any reward items that have been claimed in this cycle.
        ///     The key is a reward item key, and the value is a timestamp of when it was successfully claimed.
        ///     When the reward is reset, this collection is cleared.
        /// </summary>
        public Dictionary<string, long> claimTimestamps = new Dictionary<string, long>();

        /// <summary>
        ///     For every reward item, this collection will contain its <see cref="RewardItemState"/>.
        ///     The only way to update this collection is by calling <see cref="Update"/>.
        /// </summary>
        public Dictionary<string, RewardItemState> rewardItemStates { get; }
            = new Dictionary<string, RewardItemState>();

        /// <summary>
        ///     This is used in Update for checking if a reward state has changed.
        /// </summary>
        Dictionary<string, RewardItemState> m_OldRewardItemStates = new Dictionary<string, RewardItemState>();

        /// <summary>
        ///     Tells whether this reward is in a cooldown state.
        ///     This means we're waiting for a countdown until the next claimable item unlocks.
        /// </summary>
        /// <returns>
        ///     Returns true if this reward is in cooldown, otherwise false;
        /// </returns>
        public bool IsInCooldown()
        {
            // if no item is claimable then this reward is in cooldown

            foreach (var kvp in rewardItemStates)
            {
                if (kvp.Value == RewardItemState.Claimable)
                {
                    return false;
                }
            }

            return true;
        }

        long m_NextUpdateTimestamp;

        /// <summary>
        ///     Gets the countdown in ticks.
        ///     <seealso cref="DateTime.Ticks"/>
        /// </summary>
        public long countdownTicks => m_NextUpdateTimestamp - DateTime.UtcNow.Ticks;

        /// <summary>
        ///     Get the number of seconds until the next state change (can be negative).
        ///     Either until the cooldown ends, or until the current claimable item expires.
        /// </summary>
        public float countdownSeconds
        {
            get
            {
                // anything over 100 years might as well be infinity
                // we do this so we can guarantee that the return value is constant if the timer isn't relevant
                if (countdownTicks > TimeSpan.FromDays(36500).Ticks) return float.PositiveInfinity;

                return (float)countdownTicks / TimeSpan.TicksPerSecond;
            }
        }

        /// <summary>
        ///     Get the key of the reward item that is currently claimable.
        ///     If multiple items are claimable, then this returns the one
        ///     with the highest position (index) in the reward item collection.
        /// </summary>
        /// <returns>
        ///     Returns the key of a claimable reward item, or an empty string if none are claimable.
        /// </returns>
        public string GetLastClaimableRewardItemKey()
        {
            Update();

            // loop backwards through the reward item states until finding one that's Claimable

            for (int i = rewardDefinition.m_Items.Length - 1; i >= 0; i--)
            {
                var rewardItemKey = rewardDefinition.m_Items[i].key;

                if (rewardItemStates[rewardItemKey] == RewardItemState.Claimable)
                {
                    return rewardItemKey;
                }
            }

            return "";
        }

        /// <summary>
        ///     Get a RewardItemState based on everything we know about the current reward state.
        /// </summary>
        RewardItemState GetState(
            string rewardItemKey,
            int rewardItemIndex,
            long ticksSinceClaim,
            int potentiallyClaimableIndex)
        {
            if (claimTimestamps.ContainsKey(rewardItemKey))
            {
                // if a key exists, it's claimed, period
                return RewardItemState.Claimed;
            }

            if (claimTimestamps.Count == 0)
            {
                // there have been no claims on this reward
                // therefore only the first item is claimable
                return rewardItemIndex == 0 ? RewardItemState.Claimable : RewardItemState.Locked;
            }

            if (rewardItemIndex > potentiallyClaimableIndex)
            {
                return RewardItemState.Locked;
            }

            if (rewardItemIndex == potentiallyClaimableIndex)
            {
                if (ticksSinceClaim < rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond)
                {
                    // nothing is claimable because it's in cooldown
                    return RewardItemState.Locked;
                }

                return RewardItemState.Claimable;
            }

            if (rewardItemIndex < potentiallyClaimableIndex)
            {
                // this item is older than the current claimable and has not been claimed
                return RewardItemState.Missed;
            }

            // this item must be in the future
            return RewardItemState.Locked;
        }

        /// <summary>
        ///     Checks to see if it's time to update state.
        ///     If yes, this recalculates the state of each reward item in this reward instance.
        ///     If the reward is reset, the <see cref="rewardReset"/> event is invoked.
        ///     If any reward item states change, the <see cref="rewardStateChanged"/> event is invoked.
        ///     Both events could be invoked in the same pass, but rewardReset would be invoked first.
        /// </summary>
        /// <param name="force">
        ///     If <see cref="countdownTicks"/> is more than 0, this method won't do anything.
        ///     You can bypass the countdown gate by passing in true here.
        /// </param>
        public void Update(bool force = false)
        {
            if (!force && countdownTicks > 0)
            {
                return;
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            var cooldownTicks = rewardDefinition.cooldownSeconds * TimeSpan.TicksPerSecond;
            var expirationTicks = rewardDefinition.expirationSeconds * TimeSpan.TicksPerSecond;
            var highestIndexClaimed = -1;
            var ticksSinceClaim = long.MaxValue;
            long highestIndexClaimedTimestamp = 0;

            for (var i = 0; i < rewardDefinition.m_Items.Length; i++)
            {
                var rewardItemDefinition = rewardDefinition.m_Items[i];

                if (claimTimestamps.ContainsKey(rewardItemDefinition.key)
                    && i > highestIndexClaimed)
                {
                    highestIndexClaimed = i;
                    highestIndexClaimedTimestamp = claimTimestamps[rewardItemDefinition.key];
                    ticksSinceClaim = nowTicks - claimTimestamps[rewardItemDefinition.key];
                }
            }

            // how many items have gone unclaimed since the last claim?
            // - this does not include older missed items, only the ones since the last claim 
            // - this is used to:
            //     - calculate the countdown timer
            //     - decide whether it's time to reset the reward or not
            var missedItemsCount = 0;

            // if we're in cooldown, then there can't be any missed items since the last claim
            if (ticksSinceClaim > cooldownTicks
                && rewardDefinition.expirationSeconds > 0
                && !rewardDefinition.resetIfExpired)
            {
                missedItemsCount = Mathf.FloorToInt((float)(ticksSinceClaim - cooldownTicks) / expirationTicks);
            }

            // update the countdown

            // account for one additional expiration period for each missed item
            expirationTicks += expirationTicks * missedItemsCount;

            if (highestIndexClaimedTimestamp > 0)
            {
                if (nowTicks < highestIndexClaimedTimestamp + cooldownTicks)
                {
                    // currently in cooldown
                    m_NextUpdateTimestamp = highestIndexClaimedTimestamp + cooldownTicks;
                }

                // not in cooldown, something is currently claimable

                else if (rewardDefinition.expirationSeconds > 0
                    && nowTicks < highestIndexClaimedTimestamp + cooldownTicks + expirationTicks)
                {
                    // currently available and heading toward expiration
                    m_NextUpdateTimestamp = highestIndexClaimedTimestamp + cooldownTicks + expirationTicks;
                }
            }
            else
            {
                // never going to expire
                // next state change is the end of time

                m_NextUpdateTimestamp = long.MaxValue;
            }

            var potentiallyClaimableIndex = highestIndexClaimed >= 0 ? highestIndexClaimed + 1 + missedItemsCount : -1;

            // when should we reset:
            var hasCooldownFinished = ticksSinceClaim >= cooldownTicks;
            var lastRewardItemIndex = rewardDefinition.m_Items.Length - 1;

            // the highest claimable index exceeds the size of the item collection
            // and the cooldown has finished
            var isClaimableOutOfRange = potentiallyClaimableIndex > lastRewardItemIndex
                && hasCooldownFinished;

            // when the last item in the reward has been claimed and the cooldown has finished
            var wasLastRewardClaimed = highestIndexClaimed == lastRewardItemIndex
                && hasCooldownFinished;

            // when resetIfExpires is true and any item expires
            var doResetAfterExpiration = rewardDefinition.expirationSeconds > 0
                                         && rewardDefinition.resetIfExpired
                                         && ticksSinceClaim > cooldownTicks + expirationTicks;
            if (isClaimableOutOfRange
                || wasLastRewardClaimed
                || doResetAfterExpiration)
            {
                claimTimestamps.Clear();
                highestIndexClaimed = -1;
                ticksSinceClaim = long.MaxValue;
                m_NextUpdateTimestamp = long.MaxValue;
                rewardReset?.Invoke(this);
            }

            // cache the old states for comparison
            m_OldRewardItemStates.Clear();
            foreach (var kvp in rewardItemStates)
            {
                m_OldRewardItemStates.Add(kvp.Key, kvp.Value);
            }

            var changeDetected = false;

            rewardItemStates.Clear();

            // now check each reward item to see what status it should have

            for (var i = 0; i < rewardDefinition.m_Items.Length; i++)
            {
                var rewardItemDefinition = rewardDefinition.m_Items[i];

                var rewardItemState = GetState(
                    rewardItemDefinition.key,
                    i,
                    ticksSinceClaim,
                    potentiallyClaimableIndex);

                rewardItemStates.Add(rewardItemDefinition.key, rewardItemState);

                if (rewardStateChanged != null
                    && m_OldRewardItemStates.ContainsKey(rewardItemDefinition.key)
                    && m_OldRewardItemStates[rewardItemDefinition.key] != rewardItemState)
                {
                    changeDetected = true;
                }
            }

            if (changeDetected)
            {
                rewardStateChanged?.Invoke(this);
            }
        }
    }
}
