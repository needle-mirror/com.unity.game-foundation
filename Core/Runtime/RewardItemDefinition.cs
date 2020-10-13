namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     A definition of a reward item usable at runtime.
    /// </summary>
    public sealed class RewardItemDefinition : CatalogItem
    {
        /// <summary>
        ///     A reference to the parent <see cref="RewardDefinition"/>.
        /// </summary>
        public RewardDefinition rewardDefinition { get; internal set; }

        /// <summary>
        ///     Configures the items and currencies which are granted when this reward item is redeemed.
        /// </summary>
        public TransactionExchangeDefinition payout { get; internal set; }
    }
}
