using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Description for a <see cref="IAPTransaction"/>.
    /// </summary>
    public partial class IAPTransactionAsset : BaseTransactionAsset
    {
        /// <summary>
        ///     The product ID for the Apple platform
        /// </summary>
        [SerializeField]
        internal string m_AppleId;

        /// <summary>
        ///     The product ID for the Google platform
        /// </summary>
        [SerializeField]
        internal string m_GoogleId;

        /// <inheritdoc cref="m_AppleId"/>
        public string appleId => m_AppleId;

        /// <inheritdoc cref="m_GoogleId"/>
        public string googleId => m_GoogleId;

        /// <summary>
        ///     The product ID for the platform store
        /// </summary>
        public string productId =>
#if UNITY_IOS
            m_AppleId;
#elif UNITY_ANDROID
            m_GoogleId;
#else
            m_AppleId; //TODO: defaulting to apple to permit unit testing--need better solution.
#endif

        /// <inheritdoc/>
        protected override BaseTransactionConfig ConfigureTransaction(
            CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var item = builder.Create<IAPTransactionConfig>(key);
#if UNITY_IOS
            const string productIdName = ExternalValueProviderNames.appleProductId;
#elif UNITY_ANDROID
            const string productIdName = ExternalValueProviderNames.googleProductId;
#else

            //TODO: defaulting to apple to permit unit testing--need better solution.
            const string productIdName = ExternalValueProviderNames.appleProductId;
#endif

            if (valueProvider == null
                || !valueProvider.TryGetValue(productIdName, key, out var externalProductId))
            {
                item.productId = productId;
            }
            else
                item.productId = externalProductId;

            return item;
        }
    }
}
