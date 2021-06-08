#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;

#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the IAP transaction info, and outcome.
    /// </summary>
    public sealed class IAPTransaction : BaseTransaction
    {
        /// <summary>
        ///     The type of IAP product.
        /// </summary>
        public IAPProductType productType { get; internal set; }

        /// <summary>
        ///     The product identifier defined in the platform store.
        /// </summary>
        public string productId { get; internal set; }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

        Product m_Product;

        /// <summary>
        ///     This returns the <see cref="Product"/> object for this transaction's product id,
        ///     as long as it exists in the initialized Unity Purchasing instance
        ///     (which is populated by the IAP Catalog).
        /// </summary>
        public Product product
        {
            get
            {
                if (m_Product == null)
                {
                    m_Product = (GameFoundationSdk.transactions as TransactionManagerImpl).FindIAPProduct(productId);
                }

                return m_Product;
            }
        }
#endif
    }
}
