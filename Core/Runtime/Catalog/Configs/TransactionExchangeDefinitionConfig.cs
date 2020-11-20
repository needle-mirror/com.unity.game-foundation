using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="TransactionExchangeDefinition"/> data.
    /// </summary>
    public sealed partial class TransactionExchangeDefinitionConfig
    {
        /// <summary>
        ///     The list of <see cref="ExchangeDefinitionConfig"/> used to determine
        ///     what the <see cref="TransactionExchangeDefinition"/> will contain.
        /// </summary>
        public readonly List<ExchangeDefinitionConfig> exchangeConfigs =
            new List<ExchangeDefinitionConfig>();
    }
}
