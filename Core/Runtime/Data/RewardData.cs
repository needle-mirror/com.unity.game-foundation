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
        ///     A collection of reward item claimed during the current reward cycle.
        ///     If this reward is reset, this list will be cleared.
        /// </summary>
        public List<ClaimedRewardData> claimedRewards;
    }
}
