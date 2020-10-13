namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the currency change of a <see cref="Payout"/> or
    ///     a <see cref="TransactionCosts"/>.
    /// </summary>
    public sealed class CurrencyExchange : ITradable
    {
        /// <summary>
        ///     The currency of the exchange.
        /// </summary>
        public Currency currency { get; internal set; }

        /// <summary>
        ///     The amount of the currency.
        ///     As a payout, this amount is added to the wallet.
        ///     As a cost, this amount is removed from the wallet.
        /// </summary>
        public long amount { get; internal set; }
    }
}
