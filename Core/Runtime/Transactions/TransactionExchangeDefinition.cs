using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the changes of a transaction definition.
    ///     As a payout, those changes are added.
    ///     As a cost, those changes are removed.
    /// </summary>
    public sealed class TransactionExchangeDefinition
    {
        /// <summary>
        ///     The list of tradables with their amount.
        /// </summary>
        internal ExchangeDefinition[] m_Exchanges;

        /// <summary>
        ///     Creates a new transaction exchange definition.
        /// </summary>
        internal TransactionExchangeDefinition() { }

        /// <summary>
        ///     Gets the <see cref="ExchangeDefinition"/> entries of this <see cref="TransactionExchangeDefinition"/>
        ///     instance.
        /// </summary>
        /// <param name="target">
        ///     The target collection the <see cref="ExchangeDefinition"/> entries are copied to.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="ExchangeDefinition"/> entries found.
        /// </returns>
        public int GetExchanges(ICollection<ExchangeDefinition> target = null, bool clearTarget = true)
            => Tools.Copy(m_Exchanges, target, clearTarget);

        /// <summary>
        ///     Gets an <see cref="ExchangeDefinition"/> instance by its <paramref name="index"/> within this
        ///     <see cref="TransactionExchangeDefinition"/> instance.
        /// </summary>
        /// <param name="index">
        ///     The index of the <see cref="ExchangeDefinition"/> instance to retrieve.
        /// </param>
        /// <returns>
        ///     The <see cref="ExchangeDefinition"/> instance at the given <paramref name="index"/>.
        /// </returns>
        public ExchangeDefinition GetExchange(int index)
        {
            Tools.ThrowIfOutOfRange(index, 0, m_Exchanges.Length - 1, nameof(index));
            return m_Exchanges[index];
        }

        /// <summary>
        ///     Gets the number of <see cref="ExchangeDefinition"/> trading a definition of the given
        ///     <typeparamref name="TDefinition"/>.
        /// </summary>
        /// <typeparam name="TDefinition">
        ///     The filter.
        /// </typeparam>
        /// <returns>
        ///     The number of <see cref="ExchangeDefinition"/> trading a definition of the given
        ///     <typeparamref name="TDefinition"/>
        /// </returns>
        public long GetDefinitionCount<TDefinition>() where TDefinition : TradableDefinition
        {
            long itemCount = 0;
            foreach (var exchange in m_Exchanges)
            {
                if (exchange.tradableDefinition is TDefinition)
                    itemCount += exchange.amount;
            }

            return itemCount;
        }
    }
}
