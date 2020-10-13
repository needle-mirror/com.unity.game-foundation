namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="VirtualTransaction"/> instance.
    /// </summary>
    public sealed class VirtualTransactionConfig : BaseTransactionConfig<VirtualTransaction>
    {
        /// <summary>
        ///     The cost of the transaction.
        /// </summary>
        public TransactionExchangeDefinitionConfig costs;

        /// <inheritdoc/>
        protected internal override
            VirtualTransaction CompileTransaction()
        {
            var transaction = new VirtualTransaction();
            transaction.costs = costs.Compile();
            return transaction;
        }

        /// <inheritdoc/>
        protected internal override void
            LinkTransaction(CatalogBuilder builder)
        {
            costs.Link(builder, runtimeItem.costs);
        }
    }
}
