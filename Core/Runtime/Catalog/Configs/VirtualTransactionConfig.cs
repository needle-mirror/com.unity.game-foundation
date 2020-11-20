namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="VirtualTransaction"/> instance.
    /// </summary>
    public sealed partial class VirtualTransactionConfig : BaseTransactionConfig<VirtualTransaction>
    {
        /// <summary>
        ///     The cost of the transaction.
        /// </summary>
        public TransactionExchangeDefinitionConfig costs;
    }
}
