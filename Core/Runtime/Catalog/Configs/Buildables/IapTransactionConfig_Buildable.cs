using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class IAPTransactionConfig
    {
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<IAPTransactionConfig>();

        /// <inheritdoc/>
        protected override IAPTransaction CompileTransaction(Rejectable rejectable)
        {
            if (string.IsNullOrEmpty(productId))
            {
                k_GFLogger.LogWarning($"Transaction '{key}' doesn't have a product id assigned and will not be purchasable!");
            }

            return new IAPTransaction
            {
                productType = productType,
                productId = productId
            };
        }
    }
}
