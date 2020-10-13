namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="StoreAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class StoreAssetUtility
    {
        /// <inheritdoc cref="StoreAsset.Editor_AddItem"/>
        public static void AddItem(this StoreAsset @this, BaseTransactionAsset transaction, bool enabled = true)
            => @this.Editor_AddItem(transaction, enabled);

        /// <inheritdoc cref="StoreAsset.Editor_RemoveItem"/>
        public static bool RemoveItem(this StoreAsset @this, StoreItemObject storeItem)
            => @this.Editor_RemoveItem(storeItem);

        /// <inheritdoc cref="StoreAsset.Editor_SetEnable"/>
        public static void SetEnable(this StoreAsset @this, BaseTransactionAsset transaction, bool enabled = true)
            => @this.Editor_SetEnable(transaction, enabled);
    }
}
