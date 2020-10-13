using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure that contains the state of the Wallet.
    /// </summary>
    [Serializable]
    public struct WalletData : IEquatable<WalletData>
    {
        /// <summary>
        ///     Get an empty instance of this class.
        /// </summary>
        public static WalletData Empty => new WalletData
        {
            balances = new BalanceData[0]
        };

        /// <summary>
        ///     The list of balances
        /// </summary>
        public BalanceData[] balances;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(WalletData other)
        {
            if (balances is null != other.balances is null) return false;

            if (balances != null)
            {
                if (balances.Length != other.balances.Length) return false;

                for (var i = 0; i < balances.Length; i++)
                {
                    if (!balances[i].Equals(other.balances[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Tells whether this <see cref="WalletData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is WalletData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="WalletData"/> instance.
        ///     Returns 0.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="WalletData"/> instance.
        /// </returns>
        public override int GetHashCode() => 0;
    }
}
