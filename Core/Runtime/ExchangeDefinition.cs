using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describe the <see cref="ITradable"/> change of a <see cref="Payout"/>.
    /// </summary>
    public struct ExchangeDefinition : IEquatable<ExchangeDefinition>
    {
        /// <summary>
        ///     The tradable of the exchange.
        /// </summary>
        public TradableDefinition tradableDefinition { get; internal set; }

        /// <summary>
        ///     The amount of the <see cref="tradableDefinition"/>.
        ///     As a payout, the amount is added to its respective container
        ///     (inventory for <see cref="InventoryItem"/> or wallet for <see cref="Currency"/>).
        ///     As a cost, the amount is removed from its respective container.
        /// </summary>
        public long amount { get; internal set; }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ExchangeDefinition other)
            => tradableDefinition == other.tradableDefinition && amount == other.amount;

        /// <summary>
        ///     Tells whether this <see cref="ExchangeDefinition"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is ExchangeDefinition other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="ExchangeDefinition"/> instance.
        ///     Returns the hash code of its <see cref="tradableDefinition"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="ExchangeDefinition"/> instance.
        /// </returns>
        public override int GetHashCode() => tradableDefinition?.GetHashCode() ?? 0;
    }
}
