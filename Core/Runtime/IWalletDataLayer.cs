using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects providing data to the <see cref="IWalletManager"/>.
    /// </summary>
    public interface IWalletDataLayer
    {
        /// <summary>
        ///     Get Wallet's serializable data.
        /// </summary>
        /// <returns>
        ///     The player's data for the <see cref="IWalletManager"/>.
        /// </returns>
        WalletData GetData();

        /// <summary>
        ///     Defines a new balance for the given <paramref name="key"/>
        /// </summary>
        /// <param name="key">
        ///     The identifier of the currency to update.
        /// </param>
        /// <param name="value">
        ///     The new balance.
        /// </param>
        /// <param name="completer">
        ///     The operation result.
        /// </param>
        void SetBalance(string key, long value, Completer completer);

        /// <summary>
        ///     Increase the balance for the given <paramref name="key"/>
        /// </summary>
        /// <param name="key">
        ///     The identifier of the currency to update.
        /// </param>
        /// <param name="value">
        ///     The amount to add.
        /// </param>
        /// <param name="completer">
        ///     The operation result.
        /// </param>
        void AddBalance(string key, long value, Completer<long> completer);

        /// <summary>
        ///     Decrease the balance for the given <paramref name="key"/>
        /// </summary>
        /// <param name="key">
        ///     The identifier of the currency to update.
        /// </param>
        /// <param name="value">
        ///     The amount to remove.
        /// </param>
        /// <param name="completer">
        ///     The operation result.
        /// </param>
        void RemoveBalance(string key, long value, Completer<long> completer);
    }
}
