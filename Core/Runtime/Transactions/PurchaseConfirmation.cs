namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contains portable information about a successful IAP purchase.
    /// </summary>
    struct PurchaseConfirmation
    {
        /// <summary>
        ///     The product identifier used by the interfaced purchasing system.
        /// </summary>
        public string productId;

        /// <summary>
        ///     The full receipt string provided by the interfaced purchasing system.
        /// </summary>
        public string receipt;

        /// <summary>
        ///     One or more strings given as a receipt by the interfaced purchasing system.
        /// </summary>
        public string[] receiptParts;
    }
}
