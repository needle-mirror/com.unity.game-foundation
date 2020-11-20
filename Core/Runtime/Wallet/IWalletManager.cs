using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Manages the player currency balances.
    /// </summary>
    public interface IWalletManager
    {
        /// <summary>
        ///     Triggered every time a balance is modified, whether added, removed, or set.
        /// </summary>
        event Action<IQuantifiable, long> balanceChanged;

        /// <summary>
        ///     Gets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">
        ///     The currency you want the balance of.
        /// </param>
        /// <returns>
        ///     The balance of the specified currency.
        /// </returns>
        long Get(Currency currency);

        /// <summary>
        ///     Sets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">
        ///     The currency you want to set the balance.
        /// </param>
        /// <param name="balance">
        ///     The amount to add to the balance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the balance has been updated, <c>false</c> otherwise.
        /// </returns>
        bool Set(Currency currency, long balance);

        /// <summary>
        ///     Increases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">
        ///     The currency you want to increase the balance.
        /// </param>
        /// <param name="balance">
        ///     The amount to add to the balance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the update is valid, <c>false</c> otherwise.
        /// </returns>
        bool Add(Currency currency, long balance);

        /// <summary>
        ///     Decreases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">
        ///     The currency you want to decrease the balance.
        /// </param>
        /// <param name="balance">
        ///     The amount to remove to the balance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the update is valid, <c>false</c> otherwise.
        /// </returns>
        bool Remove(Currency currency, long balance);
    }
}
