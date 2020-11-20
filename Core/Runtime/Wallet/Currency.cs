using System;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes a currency.
    /// </summary>
    public class Currency : TradableDefinition, IQuantifiable, IEquatable<Currency>
    {
        /// <summary>
        ///     Tells whether the currency is <see cref="CurrencyType.Soft"/> or <see cref="CurrencyType.Hard"/>.
        /// </summary>
        public CurrencyType type { get; internal set; }

        /// <summary>
        ///     The maximum balance the player can have. 0 means no limit.
        /// </summary>
        public long maximumBalance { get; internal set; }

        /// <inheritdoc/>
        public long quantity => GameFoundationSdk.wallet.Get(this);

        internal override bool VerifyCost(long cost, out Exception failReason)
        {
            var actualBalance = GameFoundationSdk.wallet.Get(this);

            var canPay = actualBalance >= cost;

            failReason = canPay ? null : new NotEnoughBalanceException(key, cost, actualBalance);

            return canPay;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        bool IEquatable<Currency>.Equals(Currency other) => key == other.key;

        /// <summary>
        ///     Tells whether this <see cref="Currency"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is Currency other && key == other.key;

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        ///     Gets the string representation of this <see cref="Currency"/> instance.
        /// </summary>
        /// <returns>
        ///     The string representation of this <see cref="Currency"/> instance.
        /// </returns>
        public override string ToString() => $"[{nameof(Currency)} {key} ({type})]";
    }
}
