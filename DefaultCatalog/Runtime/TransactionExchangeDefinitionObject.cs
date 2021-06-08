using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Description for a <see cref="TransactionExchangeDefinition"/>
    /// </summary>
    [Serializable]
    public class TransactionExchangeDefinitionObject
    {
        /// <summary>
        ///     The list of <see cref="ExchangeDefinitionObject"/>.
        /// </summary>
        [SerializeField]
        internal List<ExchangeDefinitionObject> m_Exchanges;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionExchangeDefinitionObject"/> class.
        /// </summary>
        internal TransactionExchangeDefinitionObject()
        {
            m_Exchanges = new List<ExchangeDefinitionObject>();
        }

        /// <summary>
        ///     Adds the item exchanges to the given <paramref name="target"/> collection.
        /// </summary>
        /// <param name="target">
        ///     The target collection where the item exchange are added.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of item added.
        /// </returns>
        public int GetItems(ICollection<ExchangeDefinitionObject> target = null, bool clearTarget = true)
            => GFTools.Copy(m_Exchanges, target, clearTarget);

        /// <summary>
        ///     Gets a <see cref="ExchangeDefinitionObject"/> by its index.
        /// </summary>
        /// <param name="index">
        ///     The index of the item exchange.
        /// </param>
        /// <returns>
        ///     The item exchange.
        /// </returns>
        public ExchangeDefinitionObject GetItem(int index)
        {
            GFTools.ThrowIfOutOfRange(index, 0, m_Exchanges.Count, nameof(index));
            return m_Exchanges[index];
        }

        /// <summary>
        ///     Creates a configuration for a <see cref="TransactionExchangeDefinition"/>
        /// </summary>
        /// <returns>
        ///     The configuration.
        /// </returns>
        internal TransactionExchangeDefinitionConfig Configure()
        {
            var config = new TransactionExchangeDefinitionConfig();

            foreach (var itemExchangeAsset in m_Exchanges)
            {
                if (itemExchangeAsset == null)
                {
                    throw new InvalidOperationException($"{nameof(TransactionExchangeDefinitionObject)}:" +
                        $"Cannot configure {nameof(itemExchangeAsset)} in " +
                        $"{nameof(m_Exchanges)} because Cost or Payout has a null entry.");
                }

                var itemConfig = itemExchangeAsset.Configure();
                config.exchangeConfigs.Add(itemConfig);
            }

            return config;
        }

        /// <summary>
        ///     Creates a deep copy of this object.
        /// </summary>
        /// <returns>
        ///     A new instance with the same values.
        /// </returns>
        internal TransactionExchangeDefinitionObject Clone()
        {
            var clone = new TransactionExchangeDefinitionObject();

            foreach (var exchange in m_Exchanges)
            {
                clone.m_Exchanges.Add(exchange.Clone());
            }

            return clone;
        }

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal void RefreshReferences(CatalogAsset owner)
        {
            foreach (var exchange in m_Exchanges)
            {
                exchange.RefreshReferences(owner);
            }
        }
    }
}
