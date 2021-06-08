using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Description for a <see cref="IAPTransaction"/>.
    /// </summary>
    public partial class IAPTransactionAsset : BaseTransactionAsset<IAPTransaction, IAPTransactionConfig>
    {
        /// <summary>
        ///     The type of product.
        ///     This field is populated dynamically during edit mode.
        ///     Unlike other fields, this one can't be externalized,
        ///     because its value should ideally never change once it's set.
        /// </summary>
        [SerializeField]
        IAPProductType m_ProductType = IAPProductType.Undetermined;

        internal IAPProductType productType
        {
            get => m_ProductType;
            private set => m_ProductType = value;
        }

        /// <summary>
        ///     The product ID for the Apple platform
        /// </summary>
        [SerializeField]
        string m_AppleId;

        /// <summary>
        ///     Wrapper for <see cref="m_AppleId"/>.
        /// </summary>
        ExternalizableValue<string> m_AppleIdWrapper;

        /// <summary>
        ///     The product ID for the Google platform
        /// </summary>
        [SerializeField]
        string m_GoogleId;

        /// <summary>
        ///     Wrapper for <see cref="m_GoogleId"/>.
        /// </summary>
        ExternalizableValue<string> m_GoogleIdWrapper;

        /// <inheritdoc cref="m_AppleId"/>
        public ExternalizableValue<string> appleId
        {
            get
            {
                if (m_AppleIdWrapper is null)
                {
                    m_AppleIdWrapper = new ExternalizableValue<string>(m_AppleId);
                }

                return m_AppleIdWrapper;
            }
        }

        /// <inheritdoc cref="m_GoogleId"/>
        public ExternalizableValue<string> googleId
        {
            get
            {
                if (m_GoogleIdWrapper is null)
                {
                    m_GoogleIdWrapper = new ExternalizableValue<string>(m_GoogleId);
                }

                return m_GoogleIdWrapper;
            }
        }

        /// <summary>
        ///     The product ID for the platform store
        /// </summary>
        /// <remarks>
        ///     TODO: We currently default to apple to permit unit testing. We need a better solution.
        /// </remarks>
        public ExternalizableValue<string> productId
#if UNITY_IOS
            => appleId;
#elif UNITY_ANDROID
            => googleId;
#else
            => appleId;
#endif

        /// <inheritdoc/>
        protected override IAPTransactionConfig ConfigureTransaction(
            CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var item = builder.Create<IAPTransactionConfig>(key);

            item.productType = productType;

            if (valueProvider is null)
            {
                appleId.ResetExternalValue();
                googleId.ResetExternalValue();
            }
            else
            {
                if (valueProvider.TryGetValue(ExternalValueProviderNames.appleProductId, key, out var externalProductId))
                {
                    appleId.SetExternalValue(externalProductId);
                }
                else
                {
                    appleId.ResetExternalValue();
                }

                if (valueProvider.TryGetValue(ExternalValueProviderNames.googleProductId, key, out externalProductId))
                {
                    googleId.SetExternalValue(externalProductId);
                }
                else
                {
                    googleId.ResetExternalValue();
                }
            }

            item.productId = productId;

            return item;
        }

        /// <inheritdoc/>
        protected override void OnAfterItemDeserialize()
        {
            base.OnAfterItemDeserialize();

            m_AppleIdWrapper = new ExternalizableValue<string>(m_AppleId);
            m_GoogleIdWrapper = new ExternalizableValue<string>(m_GoogleId);
        }
    }
}
