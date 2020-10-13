using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Description of a currency update.
    /// </summary>
    public struct CurrencyExchangeData : IEquatable<CurrencyExchangeData>
    {
        /// <summary>
        ///     The identifier of the updated currency.
        /// </summary>
        public string currencyKey;

        /// <summary>
        ///     The amount.
        /// </summary>
        public long amount;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(CurrencyExchangeData other) => currencyKey == other.currencyKey && amount == other.amount;

        /// <summary>
        ///     Tells whether this <see cref="CurrencyExchangeData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is CurrencyExchangeData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="CurrencyExchangeData"/> instance.
        ///     Returns the hash code of its <see cref="currencyKey"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="CurrencyExchangeData"/> instance.
        /// </returns>
        public override int GetHashCode() => currencyKey?.GetHashCode() ?? 0;
    }
}
