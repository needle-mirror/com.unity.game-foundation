using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Contains serializable data for a claimed reward item.
    /// </summary>
    [Serializable]
    public class ClaimedRewardData
    {
        /// <summary>
        ///     The key of the claimed <see cref="RewardItemDefinition"/>.
        /// </summary>
        public string rewardItemKey;

        /// <summary>
        ///     The timestamp at which the reward have been claimed.
        /// </summary>
        public long timestamp;
    }
}
