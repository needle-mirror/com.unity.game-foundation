using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="TransactionExchangeDefinition"/> data.
    /// </summary>
    public class TransactionExchangeDefinitionConfig
    {
        /// <summary>
        ///     The list of <see cref="ExchangeDefinitionConfig"/> used to determine
        ///     what the <see cref="TransactionExchangeDefinition"/> will contain.
        /// </summary>
        public readonly List<ExchangeDefinitionConfig> exchangeConfigs =
            new List<ExchangeDefinitionConfig>();

        /// <summary>
        ///     Checks the configuration and builds the <see cref="TransactionExchangeDefinition"/> data.
        /// </summary>
        internal TransactionExchangeDefinition Compile()
        {
            var transactionExchange = new TransactionExchangeDefinition
            {
                m_Exchanges = new ExchangeDefinition[exchangeConfigs.Count]
            };

            for (var i = 0; i < exchangeConfigs.Count; i++)
            {
                var exchangeConfig = exchangeConfigs[i];
                Tools.ThrowIfArgNull(exchangeConfig, nameof(exchangeConfigs), i);

                var exchange = exchangeConfig.Compile();
                transactionExchange.m_Exchanges[i] = exchange;
            }

            return transactionExchange;
        }

        /// <summary>
        ///     Resolves the possible references the <paramref name="transactionExchange"/> may contain.
        /// </summary>
        /// <param name="builder">
        ///     The builder where the references can be found.
        /// </param>
        /// <param name="transactionExchange">
        ///     The source to use for linking.
        /// </param>
        internal void Link(CatalogBuilder builder, TransactionExchangeDefinition transactionExchange)
        {
            Tools.ThrowIfArgNull(transactionExchange, nameof(transactionExchange));

            for (var i = 0; i < exchangeConfigs.Count; i++)
            {
                exchangeConfigs[i].Link(builder, ref transactionExchange.m_Exchanges[i]);
            }
        }
    }
}
