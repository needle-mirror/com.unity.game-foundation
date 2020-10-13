namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     The result of a transaction.
    /// </summary>
    public struct TransactionResult
    {
        /// <summary>
        ///     The items or currency which the transaction took from the player.
        /// </summary>
        public readonly TransactionCosts costs;

        /// <summary>
        ///     The items or currency which the transaction granted to the player.
        /// </summary>
        public readonly Payout payout;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionResult"/> type.
        /// </summary>
        /// <param name="costs">
        ///     The cost of the transaction.
        /// </param>
        /// <param name="payout">
        ///     The payout of the transaction.
        /// </param>
        internal TransactionResult(TransactionCosts costs, Payout payout)
        {
            this.costs = costs;
            this.payout = payout;
        }
    }
}
