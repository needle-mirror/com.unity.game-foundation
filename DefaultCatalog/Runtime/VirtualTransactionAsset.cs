using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Description for <see cref="VirtualTransaction"/>
    /// </summary>
    public sealed partial class VirtualTransactionAsset
        : BaseTransactionAsset<VirtualTransaction, VirtualTransactionConfig>
    {
        /// <inheritdoc cref="costs"/>
        [SerializeField]
        internal TransactionExchangeDefinitionObject m_Costs;

        /// <summary>
        ///     The costs of the <see cref="VirtualTransactionAsset"/>.
        /// </summary>
        public TransactionExchangeDefinitionObject costs => m_Costs;

        /// <inheritdoc/>
        protected override void AwakeTransaction()
        {
            if (m_Costs is null)
            {
                m_Costs = new TransactionExchangeDefinitionObject();
            }
        }

        /// <inheritdoc/>
        protected override VirtualTransactionConfig ConfigureTransaction(
            CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var config = builder.Create<VirtualTransactionConfig>(key);
            config.costs = m_Costs.Configure();
            return config;
        }
    }
}
