namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for an <see cref="IAPTransaction"/> instance.
    /// </summary>
    public sealed partial class IAPTransactionConfig : BaseTransactionConfig<IAPTransaction>
    {
        /// <summary>
        ///     The type of IAP product.
        /// </summary>
        public IAPProductType productType;

        /// <summary>
        ///     The ID of the product.
        /// </summary>
        public string productId;
    }
}
