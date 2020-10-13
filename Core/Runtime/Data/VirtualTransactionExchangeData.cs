namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Description of the result of a virtual transaction.
    /// </summary>
    public struct VirtualTransactionExchangeData
    {
        /// <summary>
        ///     Description of the cost
        /// </summary>
        public TransactionExchangeData cost;

        /// <summary>
        ///     Description of the payout
        /// </summary>
        public TransactionExchangeData payout;
    }
}
