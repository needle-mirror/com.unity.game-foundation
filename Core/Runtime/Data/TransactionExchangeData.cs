namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Description of the result of a transaction.
    /// </summary>
    public struct TransactionExchangeData
    {
        /// <summary>
        ///     The updated currencies
        /// </summary>
        public CurrencyExchangeData[] currencies;

        /// <summary>
        ///     The created/removed items
        /// </summary>
        public InventoryItemData[] items;
    }
}
