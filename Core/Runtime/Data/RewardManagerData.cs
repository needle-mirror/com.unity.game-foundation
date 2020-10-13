using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable structure that contains the runtime state of the <see cref="IRewardManager"/>.
    /// </summary>
    [Serializable]
    public class RewardManagerData
    {
        /// <summary>
        ///     Get an empty instance of this class.
        /// </summary>
        public static RewardManagerData Empty => new RewardManagerData
        {
            rewards = new RewardData[0]
        };

        /// <summary>
        ///     The reward states that are part of this state.
        /// </summary>
        public RewardData[] rewards;
    }
}
