namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator for a <see cref="RewardItemDefinition"/> instance.
    /// </summary>
    public sealed partial class RewardItemConfig
    {
        /// <summary>
        ///     A globally-unique identifier of this reward item.
        /// </summary>
        public string key { get; internal set; }

        /// <summary>
        ///     The currencies and inventory items to be granted by this reward item.
        /// </summary>
        public TransactionExchangeDefinitionConfig payout { get; internal set; }
    }
}
