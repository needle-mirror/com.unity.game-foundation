namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator for a <see cref="BaseTransaction"/> instance.
    /// </summary>
    public abstract class BaseTransactionConfig : CatalogItemConfig<BaseTransaction>
    {
        /// <summary>
        ///     The configurator of the payout.
        /// </summary>
        public TransactionExchangeDefinitionConfig payout;

        /// <inheritdoc/>
        protected internal sealed override BaseTransaction CompileItem()
        {
            var transaction = CompileGenericTransaction();
            transaction.payout = payout.Compile();
            return transaction;
        }

        /// <inheritdoc/>
        protected internal sealed override void LinkItem(CatalogBuilder builder)
        {
            payout.Link(builder, runtimeItem.payout);
            LinkGenericTransaction(builder);
        }

        /// <summary>
        ///     Checks the configuration and builds the
        ///     <see cref="BaseTransaction"/> instance.
        /// </summary>
        /// <returns>The newly created <see cref="BaseTransaction"/></returns>
        protected internal abstract BaseTransaction CompileGenericTransaction();

        /// <summary>
        ///     Resolves the possible references the <see cref="CatalogItem"/> may
        ///     contain.
        /// </summary>
        protected internal virtual void
            LinkGenericTransaction(CatalogBuilder builder) { }
    }

    /// <inheritdoc/>
    public abstract class BaseTransactionConfig<TRuntimeTransaction>
        : BaseTransactionConfig
        where TRuntimeTransaction : BaseTransaction
    {
        /// <inheritdoc cref="CatalogItemConfig{TRuntimeItem}.runtimeItem"/>
        internal new TRuntimeTransaction runtimeItem =>
            base.runtimeItem as TRuntimeTransaction;

        /// <inheritdoc/>
        protected internal sealed override BaseTransaction CompileGenericTransaction()
            => CompileTransaction();

        /// <inheritdoc/>
        protected internal sealed override void LinkGenericTransaction(CatalogBuilder builder)
            => LinkTransaction(builder);

        /// <summary>
        ///     Checks the specific configuration of the
        ///     <see cref="BaseTransactionConfig{TRuntimeTransaction}"/> and builds
        ///     the <typeparamref name="TRuntimeTransaction"/> instance.
        /// </summary>
        /// <returns>
        ///     The newly created <typeparamref name="TRuntimeTransaction"/>
        ///     instance.
        /// </returns>
        protected internal abstract TRuntimeTransaction CompileTransaction();

        /// <summary>
        ///     Resolves the possible references the
        ///     <typeparamref name="TRuntimeTransaction"/> instance may contain.
        /// </summary>
        /// <param name="builder">
        ///     The builder where the references can be
        ///     found.
        /// </param>
        protected internal virtual void LinkTransaction(CatalogBuilder builder) { }
    }
}
