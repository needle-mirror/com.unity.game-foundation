namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Reasons a Transaction can fail.
    /// </summary>
    public enum TransactionFailureReason
    {
        /// <summary>
        ///     The <see cref="IAPTransaction.productId"/> is not valid.
        /// </summary>
        InvalidProductId,

        /// <summary>
        ///     The cost cannot be fulfilled.
        /// </summary>
        InsufficientItems,

        /// <summary>
        ///     No, or unknown reason.
        /// </summary>
        None
    }
}
