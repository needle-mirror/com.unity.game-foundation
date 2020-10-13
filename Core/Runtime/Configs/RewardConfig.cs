using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="RewardDefinition"/> instance.
    /// </summary>
    public sealed class RewardConfig : CatalogItemConfig<RewardDefinition>
    {
        /// <summary>
        ///     The amount of time, in seconds, between when a RewardItem is claimed
        ///     and when the next one will become available.
        /// </summary>
        public int cooldownSeconds;

        /// <summary>
        ///     The amount of time, in seconds, that a RewardItem will be claimable before either
        ///     moving on to the next RewardItem, or resetting back to the first RewardItem.
        /// </summary>
        public int expirationSeconds;

        /// <summary>
        ///     If a RewardItem expires, this determines whether the next RewardItem will become available,
        ///     or if the Reward is reset back to the first RewardItem.
        /// </summary>
        public bool resetIfExpired;

        /// <summary>
        ///     The reward items to add to the resulting <see cref="RewardDefinition"/>.
        /// </summary>
        public readonly List<RewardItemConfig> rewardItemConfigs = new List<RewardItemConfig>();

        /// <inheritdoc/>
        protected internal override RewardDefinition CompileItem()
        {
            var rewardDefinition = new RewardDefinition
            {
                cooldownSeconds = cooldownSeconds,
                expirationSeconds = expirationSeconds,
                resetIfExpired = resetIfExpired,
                m_Items = new RewardItemDefinition[rewardItemConfigs.Count]
            };

            return rewardDefinition;
        }

        /// <inheritdoc/>
        protected internal override void LinkItem(CatalogBuilder builder)
        {
            for (var i = 0; i < runtimeItem.m_Items.Length; i++)
            {
                runtimeItem.m_Items[i] = rewardItemConfigs[i].Compile();

                // make sure this reward item knows about its parent reward
                runtimeItem.m_Items[i].rewardDefinition = runtimeItem;

                // now run a linker to link up and currencies and inventory items
                rewardItemConfigs[i].Link(builder, runtimeItem.m_Items[i].payout);
            }
        }
    }
}
