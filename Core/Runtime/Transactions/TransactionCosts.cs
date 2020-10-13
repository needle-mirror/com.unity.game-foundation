using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the costs of a transaction process.
    /// </summary>
    public struct TransactionCosts
    {
        /// <summary>
        ///     The ID of the items removed.
        /// </summary>
        public IReadOnlyCollection<string> itemIds { get; internal set; }

        /// <summary>
        ///     The currency balances removed.
        /// </summary>
        public IReadOnlyCollection<CurrencyExchange> currencies { get; internal set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionCosts"/>
        ///     struct.
        /// </summary>
        /// <param name="itemIds">The items of the transaction.</param>
        /// <param name="currencies">The currencies of th transaction</param>
        internal TransactionCosts(
            IReadOnlyCollection<string> itemIds,
            IReadOnlyCollection<CurrencyExchange> currencies)
        {
            this.itemIds = itemIds;
            this.currencies = currencies;
        }
    }
}
