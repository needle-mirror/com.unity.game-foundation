namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility class to store all field names provided to <see cref="IExternalValueProvider"/>.
    /// </summary>
    /// <remarks>
    ///     Some names are abbreviated in case external systems only accept a limited amount of characters.
    /// </remarks>
    public static class ExternalValueProviderNames
    {
        /// <summary>
        ///     External name for <see cref="CatalogItemAsset.displayName"/>.
        /// </summary>
        public const string displayName = "displayName";

        /// <summary>
        ///     External name prefix for <see cref="CatalogItemAsset.staticProperties"/>.
        /// </summary>
        public const string staticPropertyPrefix = "stc-";

        /// <summary>
        ///     External name prefix for <see cref="InventoryItemDefinitionAsset.mutableProperties"/>.
        /// </summary>
        public const string mutablePropertyPrefix = "mtb-";

        /// <summary>
        ///     External name for <see cref="CurrencyAsset.type"/>.
        /// </summary>
        public const string currencyType = "type";

        /// <summary>
        ///     External name for <see cref="CurrencyAsset.initialBalance"/>.
        /// </summary>
        public const string initialBalance = "iniBalance";

        /// <summary>
        ///     External name for <see cref="CurrencyAsset.maximumBalance"/>.
        /// </summary>
        public const string maximumBalance = "maxBalance";

        /// <summary>
        ///     External name for <see cref="InventoryItemDefinitionAsset.initialAllocation"/>.
        /// </summary>
        public const string initialAllocation = "iniAlloc";

        /// <summary>
        ///     External name for <see cref="InventoryItemDefinitionAsset.initialQuantityPerStack"/>.
        /// </summary>
        public const string initialQuantityPerStack = "iniQuantity";

        /// <summary>
        ///     External name for <see cref="IAPTransactionAsset.appleId"/>.
        /// </summary>
        public const string appleProductId = "appleId";

        /// <summary>
        ///     External name for <see cref="IAPTransactionAsset.googleId"/>.
        /// </summary>
        public const string googleProductId = "googleId";

        /// <summary>
        ///     External name for <see cref="RewardDefinition.cooldownSeconds"/>.
        /// </summary>
        public const string cooldownSeconds = "cooldownSeconds";

        /// <summary>
        ///     External name for <see cref="RewardDefinition.expirationSeconds"/>.
        /// </summary>
        public const string expirationSeconds = "expirationSeconds";

        /// <summary>
        ///     External name for <see cref="RewardDefinition.resetIfExpired"/>.
        /// </summary>
        public const string resetIfExpired = "resetIfExpired";

        /// <summary>
        ///     Get the static property name for the given <paramref name="propertyKey"/>
        ///     to feed to an <see cref="IExternalValueProvider"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The property's key to build a name for.
        /// </param>
        /// <returns>
        ///     Return a name ready to use by an <see cref="IExternalValueProvider"/>.
        /// </returns>
        public static string GetStaticPropertyName(string propertyKey)
        {
            return staticPropertyPrefix + propertyKey;
        }

        /// <summary>
        ///     Get the mutable property name for the given <paramref name="propertyKey"/>
        ///     to feed to an <see cref="IExternalValueProvider"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The property's key to build a name for.
        /// </param>
        /// <returns>
        ///     Return a name ready to use by an <see cref="IExternalValueProvider"/>.
        /// </returns>
        public static string GetMutablePropertyName(string propertyKey)
        {
            return mutablePropertyPrefix + propertyKey;
        }
    }
}
