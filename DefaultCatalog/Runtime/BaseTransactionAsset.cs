using UnityEngine.GameFoundation.Configs;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Base asset to contain <see cref="BaseTransaction"/> data.
    /// </summary>
    public abstract partial class BaseTransactionAsset : CatalogItemAsset
    {
        /// <inheritdoc cref="payout"/>
        [SerializeField]
        [FormerlySerializedAs("m_Rewards")]
        internal TransactionExchangeDefinitionObject m_Payout;

        /// <summary>
        ///     The payout description of the transaction.
        /// </summary>
        public TransactionExchangeDefinitionObject payout => m_Payout;

        /// <inheritdoc/>
        protected sealed override void AwakeDefinition()
        {
            if (m_Payout is null)
            {
                m_Payout = new TransactionExchangeDefinitionObject();
            }

            AwakeTransaction();
        }

        /// <summary>
        ///     Overriden by inherited classes to initialize specific members.
        /// </summary>
        protected virtual void AwakeTransaction() { }
    }

    /// <inheritdoc/>
    /// <typeparam name="TTransaction">
    ///     The type of <see cref="BaseTransaction"/> data contained by this asset.
    /// </typeparam>
    /// <typeparam name="TConfig">
    ///     The type of <see cref="BaseTransactionConfig{TTransaction}"/> created by this asset.
    /// </typeparam>
    public abstract class BaseTransactionAsset<TTransaction, TConfig> : BaseTransactionAsset
        where TTransaction : BaseTransaction
        where TConfig : BaseTransactionConfig<TTransaction>
    {
        /// <inheritdoc/>
        protected sealed override CatalogItemConfig ConfigureItem(
            CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var item = ConfigureTransaction(builder, valueProvider);
            var payoutConfig = m_Payout.Configure();
            item.payout = payoutConfig;
            return item;
        }

        /// <summary>
        ///     Configures the specified <paramref name="builder"/>
        ///     with the specific content of this transaction.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data
        ///     with an external source when configuring them.
        /// </param>
        /// <returns>
        ///     The config object.
        /// </returns>
        protected abstract TConfig ConfigureTransaction(CatalogBuilder builder, IExternalValueProvider valueProvider);
    }
}
