using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for an <see cref="IAPTransaction"/> instance.
    /// </summary>
    public sealed class IAPTransactionConfig : BaseTransactionConfig<IAPTransaction>
    {
        /// <summary>
        ///     The ID of the product.
        /// </summary>
        public string productId;

        /// <inheritdoc/>
        protected internal override IAPTransaction CompileTransaction()
        {
            if (string.IsNullOrEmpty(productId))
            {
                throw new Exception($"Transaction '{key}' doesn't have a product id assigned!");
            }

            var iapTransaction = new IAPTransaction();
            iapTransaction.productId = productId;
            return iapTransaction;
        }
    }
}
