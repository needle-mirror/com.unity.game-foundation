namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="ExchangeDefinition"/>
    /// </summary>
    public sealed partial class ExchangeDefinitionConfig
    {
        /// <summary>
        ///     The identifier of the tradable definition.
        /// </summary>
        public string tradableKey { get; internal set; }

        /// <summary>
        ///     The amount of tradable to exchange.
        /// </summary>
        public long amount { get; internal set; }

        /// <summary>
        ///     Create an empty <see cref="ExchangeDefinitionConfig"/>.
        /// </summary>
        public ExchangeDefinitionConfig()
            : this(null, 0) { }

        /// <summary>
        ///     Create a <see cref="ExchangeDefinitionConfig"/> with the given data.
        /// </summary>
        /// <param name="tradableKey">
        ///     The key of the <see cref="TradableDefinition"/> traded by the exchanges issued from this config.
        /// </param>
        /// <param name="amount">
        ///     The number of <see cref="TradableDefinition"/> traded by the exchanges issued from this config.
        /// </param>
        public ExchangeDefinitionConfig(string tradableKey, long amount)
        {
            this.tradableKey = tradableKey;
            this.amount = amount;
        }
    }
}
