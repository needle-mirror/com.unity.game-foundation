using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    ///     Regroups the transaction methods.
    /// </summary>
    public interface ITransactionDataLayer
    {
        /// <summary>
        ///     Performs a purchase defined by a <see cref="VirtualTransaction"/>
        ///     specified by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Identifier of a <see cref="VirtualTransaction"/></param>
        /// <param name="itemIds">
        ///     The list of items to use in the
        ///     transaction.
        /// </param>
        /// <param name="completer">The transaction result</param>
        void MakeVirtualTransaction(
            string key,
            ICollection<string> itemIds,
            Completer<VirtualTransactionExchangeData> completer);

        /// <summary>
        ///     Validates a Receipt from a successful purchase on a Google device
        ///     and applies the payouts to the players account.
        /// </summary>
        /// <param name="key">Identifier of a <see cref="IAPTransaction"/></param>
        /// <param name="purchaseData">
        ///     A JSON encoded string returned from a
        ///     successful in app billing purchase.
        /// </param>
        /// <param name="purchaseDataSignature">
        ///     A signature of the PurchaseData
        ///     returned from a successful in app billing purchase
        /// </param>
        /// <param name="completer">The redeem result</param>
        void RedeemGoogleIap(
            string key,
            string purchaseData,
            string purchaseDataSignature,
            Completer<TransactionExchangeData> completer);

        /// <summary>
        ///     Validates a Receipt from a successful purchase on an Apple device
        ///     and applies the payouts to the players account.
        /// </summary>
        /// <param name="key">Identifier of a <see cref="IAPTransaction"/></param>
        /// <param name="receipt">
        ///     Receipt data returned from the App Store as a
        ///     result of a successful purchase.
        ///     This should be base64 encoded
        /// </param>
        /// <param name="completer">The redeem result</param>
        void RedeemAppleIap(
            string key,
            string receipt,
            Completer<TransactionExchangeData> completer);
    }
}
