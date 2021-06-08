namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Enum to specify the purchasable status of the Transaction Item.
    /// </summary>
    public enum PurchasableStatus
    {
        /// <summary>
        ///     The item can be purchased.
        /// </summary>
        AvailableToPurchase,

        /// <summary>
        ///     The item is currently being purchased.
        /// </summary>
        ItemPurchaseInProgress,

        /// <summary>
        ///     The item cannot be purchased because you cannot cover the cost.
        /// </summary>
        ItemUnaffordable,

        /// <summary>
        ///     You already own this item.
        /// </summary>
        ItemOwned,

        /// <summary>
        ///     No in app purchases can be made because no purchasing adapter is available.
        /// </summary>
        PurchasingAdapterUnavailable,

        /// <summary>
        ///     Purchases cannot be made because Game Foundation is not available (for example, it has not yet been initialized).
        /// </summary>
        GameFoundationUnavailable,

        /// <summary>
        ///     This purchase cannot be made because the PurchaseButton has erroneous or missing configuration data.
        /// </summary>
        PurchaseButtonMisconfigured,

        /// <summary>
        ///     This purchase cannot be made because the Transaction has erroneous or missing configuration data.
        /// </summary>
        TransactionMisconfigured
    }
}
