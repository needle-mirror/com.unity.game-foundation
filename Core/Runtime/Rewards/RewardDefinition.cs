using System.Collections.Generic;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     A plan for rewarding a player based on a schedule.
    ///     Different Rewards will have different schedules and different <see cref="RewardItemDefinition"/>s.
    /// </summary>
    public class RewardDefinition : CatalogItem
    {
        /// <summary>
        ///     The amount of time, in seconds, between when a RewardItem is claimed
        ///     and when the next one will become available.
        /// </summary>
        public int cooldownSeconds { get; internal set; }

        /// <summary>
        ///     The amount of time, in seconds, that a RewardItem will be claimable before either
        ///     moving on to the next RewardItem, or resetting back to the first RewardItem.
        /// </summary>
        public int expirationSeconds { get; internal set; }

        /// <summary>
        ///     If a RewardItem expires, this determines whether the next RewardItem will become available,
        ///     or if the Reward is reset back to the first RewardItem.
        /// </summary>
        public bool resetIfExpired { get; internal set; }

        /// <summary>
        ///     The collection of <see cref="RewardItemDefinition"/>s presented by this Reward.
        /// </summary>
        internal RewardItemDefinition[] m_Items;

        /// <summary>
        ///     Looks for a <see cref="RewardItemDefinition"/> based on its key.
        /// </summary>
        /// <param name="rewardItemKey">
        ///     The unique key string of the reward item to find.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="RewardItemDefinition"/> if one is found, otherwise null.
        /// </returns>
        public RewardItemDefinition FindRewardItem(string rewardItemKey)
        {
            GFTools.ThrowIfArgNull(rewardItemKey, nameof(rewardItemKey));

            foreach (var rewardItem in m_Items)
            {
                if (rewardItem.key.Equals(rewardItemKey))
                {
                    return rewardItem;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds all the <see cref="RewardItemDefinition"/>s to the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">
        ///     The target collection where the <see cref="RewardItemDefinition"/>s are added.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="RewardItemDefinition"/> added.
        /// </returns>
        public int GetRewardItems(ICollection<RewardItemDefinition> target = null, bool clearTarget = true)
            => GFTools.Copy(m_Items, target, clearTarget);

        /// <summary>
        ///     Tells whether or not the reward contains the given <paramref name="rewardItem"/>.
        /// </summary>
        /// <param name="rewardItem">
        ///     The <see cref="RewardItemDefinition"/> to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> of this <see cref="RewardDefinition"/> instance contains the <paramref name="rewardItem"/>.
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Contains(RewardItemDefinition rewardItem)
        {
            GFTools.ThrowIfArgNull(rewardItem, nameof(rewardItem));

            return IndexOf(rewardItem) >= 0;
        }

        /// <summary>
        ///     Get the index of a <see cref="RewardItemDefinition"/> in this Reward.
        /// </summary>
        /// <param name="rewardItem">
        ///     The reward item to find.
        /// </param>
        /// <returns>
        ///     The index of the reward item.
        /// </returns>
        public int IndexOf(RewardItemDefinition rewardItem)
        {
            if (rewardItem == null || m_Items == null)
            {
                return -1;
            }

            for (var i = 0; i < m_Items.Length; i++)
            {
                if (ReferenceEquals(m_Items[i], rewardItem))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Get the index of a <see cref="RewardItemDefinition"/> in this Reward.
        /// </summary>
        /// <param name="rewardItemKey">
        ///     The key of the reward item to find.
        /// </param>
        /// <returns>
        ///     The index of the reward item.
        /// </returns>
        public int IndexOf(string rewardItemKey)
        {
            return IndexOf(FindRewardItem(rewardItemKey));
        }
    }
}
