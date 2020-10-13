namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="IAPTransactionAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class IapTransactionAssetUtility
    {
        /// <inheritdoc cref="IAPTransactionAsset.Editor_SetAppleId"/>
        public static void SetAppleId(this IAPTransactionAsset @this, string id)
            => @this.Editor_SetAppleId(id);

        /// <inheritdoc cref="IAPTransactionAsset.Editor_SetGoogleId"/>
        public static void SetGoogleId(this IAPTransactionAsset @this, string id)
            => @this.Editor_SetGoogleId(id);
    }
}
