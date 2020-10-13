namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator for a <see cref="RewardItemDefinition"/> instance.
    /// </summary>
    public class RewardItemConfig
    {
        /// <summary>
        ///     A globally-unique identifier of this reward item.
        /// </summary>
        internal string m_Key;

        /// <summary>
        ///     The currencies and inventory items to be granted by this reward item.
        /// </summary>
        internal TransactionExchangeDefinitionConfig m_Payout;

        /// <summary>
        ///     Checks the configuration and builds the <see cref="RewardItemDefinition"/> data.
        /// </summary>
        protected internal RewardItemDefinition Compile()
        {
            return new RewardItemDefinition
            {
                key = m_Key,
                payout = m_Payout.Compile()
            };
        }

        /// <summary>
        ///     Resolves the possible references the <paramref name="transactionExchange"/> may contain.
        /// </summary>
        /// <param name="builder">
        ///     The builder where the references can be found.
        /// </param>
        /// <param name="transactionExchange">
        ///     The <see cref="TransactionExchangeDefinition"/> in which to look for references.
        /// </param>
        internal void Link(CatalogBuilder builder, TransactionExchangeDefinition transactionExchange)
        {
            Tools.ThrowIfArgNull(transactionExchange, nameof(transactionExchange));

            m_Payout.Link(builder, transactionExchange);
        }
    }
}
