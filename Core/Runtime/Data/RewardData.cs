using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Contains a serializable state of a <see cref="Reward"/>.
    /// </summary>
    [Serializable]
    public class RewardData
    {
        /// <summary>
        ///     The unique key of the reward.
        /// </summary>
        public string key;

        /// <summary>
        ///     A collection of reward item definition keys that have been claimed during the current reward cycle.
        ///     This list should have a value for every value in claimedRewardItemTimestamps.
        ///     If this reward is reset, this list will be cleared.
        /// </summary>
        public List<string> claimedRewardItemKeys;

        /// <summary>
        ///     A collection of timestamps (UTC ticks) for reward items that have been claimed.
        ///     This list should have a value for every value in claimedRewardItemKeys.
        ///     If this reward is reset, this list will be cleared.
        /// </summary>
        public List<long> claimedRewardItemTimestamps;
    }
}
