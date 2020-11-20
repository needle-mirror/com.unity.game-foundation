namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator for a <see cref="BaseTransaction"/> instance.
    /// </summary>
    /// <typeparam name="TTransaction">
    ///     The type of the configurable <see cref="BaseTransaction"/>
    /// </typeparam>
    public abstract partial class BaseTransactionConfig<TTransaction> : CatalogItemConfig<TTransaction>
        where TTransaction : BaseTransaction
    {
        /// <summary>
        ///     The configurator of the payout.
        /// </summary>
        public TransactionExchangeDefinitionConfig payout;
    }
}
