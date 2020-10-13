using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure that contains the state of the currency balance.
    /// </summary>
    [Serializable]
    public struct BalanceData : IEquatable<BalanceData>
    {
        /// <summary>
        ///     The Key of the currency
        /// </summary>
        public string currencyKey;

        /// <summary>
        ///     The balance of the currency
        /// </summary>
        public long balance;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(BalanceData other) => currencyKey == other.currencyKey && balance == other.balance;

        /// <summary>
        ///     Tells whether this <see cref="BalanceData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is BalanceData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="BalanceData"/> instance.
        ///     Returns the hash code of its <see cref="currencyKey"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="BalanceData"/> instance.
        /// </returns>
        public override int GetHashCode() => currencyKey?.GetHashCode() ?? 0;
    }
}
