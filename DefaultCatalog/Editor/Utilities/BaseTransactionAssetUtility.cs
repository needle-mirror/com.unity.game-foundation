namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="BaseTransactionAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class BaseTransactionAssetUtility
    {
        /// <inheritdoc cref="BaseTransactionAsset.Editor_AddPayout"/>
        public static void AddPayout(this BaseTransactionAsset @this, TradableDefinitionAsset catalogItem, long amount)
            => @this.Editor_AddPayout(catalogItem, amount);

        /// <inheritdoc cref="BaseTransactionAsset.Editor_RemovePayout"/>
        public static bool RemovePayout(this BaseTransactionAsset @this, ExchangeDefinitionObject item)
            => @this.Editor_RemovePayout(item);
    }
}
