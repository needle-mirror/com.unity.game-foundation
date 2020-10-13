namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="VirtualTransactionAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class VirtualTransactionAssetUtility
    {
        /// <inheritdoc cref="VirtualTransactionAsset.Editor_AddCost"/>
        public static void AddCost(this VirtualTransactionAsset @this, TradableDefinitionAsset catalogItem, long amount)
            => @this.Editor_AddCost(catalogItem, amount);

        /// <inheritdoc cref="VirtualTransactionAsset.Editor_RemoveCost"/>
        public static bool RemoveCost(this VirtualTransactionAsset @this, ExchangeDefinitionObject item)
            => @this.Editor_RemoveCost(item);
    }
}
