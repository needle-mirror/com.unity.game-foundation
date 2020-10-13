using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="ExchangeDefinition"/>
    /// </summary>
    public struct ExchangeDefinitionConfig : IEquatable<ExchangeDefinitionConfig>
    {
        /// <summary>
        ///     The identifier of the tradable definition.
        /// </summary>
        public string tradableKey;

        /// <summary>
        ///     The amount of tradable to exchange.
        /// </summary>
        public long amount;

        /// <summary>
        ///     Checks the configuration and builds the <see cref="ExchangeDefinition"/>.
        /// </summary>
        /// <returns>
        ///     The <see cref="ExchangeDefinition"/> data.
        /// </returns>
        internal ExchangeDefinition Compile()
        {
            Tools.ThrowIfArgNullOrEmpty(tradableKey, nameof(tradableKey));
            Tools.ThrowIfArgNegative(amount, nameof(amount));

            var exchange = new ExchangeDefinition
            {
                amount = amount
            };

            return exchange;
        }

        /// <summary>
        ///     Resolves the possible links the compiled <see cref="ExchangeDefinition"/> may contain.
        /// </summary>
        /// <param name="builder">
        ///     The builder where the references can be found.
        /// </param>
        /// <param name="exchange">
        ///     The <see cref="ExchangeDefinition"/> to link.
        /// </param>
        internal void Link(CatalogBuilder builder, ref ExchangeDefinition exchange)
        {
            var config = builder.GetConfigOrDie(tradableKey);

            if (!(config is CurrencyConfig)
                && !(config is InventoryItemDefinitionConfig))
            {
                throw new Exception(
                    $"{nameof(CatalogItemConfig)} {tradableKey} is not a valid {nameof(TradableDefinition)}.");
            }

            exchange.tradableDefinition = (TradableDefinition)config.runtimeItem;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ExchangeDefinitionConfig other)
            => tradableKey == other.tradableKey && amount == other.amount;

        /// <summary>
        ///     Tells whether this <see cref="ExchangeDefinitionConfig"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is ExchangeDefinitionConfig other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="ExchangeDefinitionConfig"/> instance.
        ///     Returns the hash code of its <see cref="tradableKey"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="ExchangeDefinitionConfig"/> instance.
        /// </returns>
        public override int GetHashCode() => tradableKey?.GetHashCode() ?? 0;
    }
}
