using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Serializable <see cref="ExchangeDefinition"/>.
    /// </summary>
    [Serializable]
    public class ExchangeDefinitionObject
    {
        /// <inheritdoc cref="catalogItem"/>
        [SerializeField]
        internal TradableDefinitionAsset m_CatalogItem;

        /// <inheritdoc cref="amount"/>
        [Min(0)]
        [SerializeField]
        internal long m_Amount;

        /// <summary>
        ///     The <see cref="TradableDefinitionAsset"/> of the exchange.
        /// </summary>
        public TradableDefinitionAsset catalogItem => m_CatalogItem;

        /// <summary>
        ///     The amount of items to exchange.
        /// </summary>
        public long amount => m_Amount;

        /// <summary>
        ///     Creates a <see cref="ExchangeDefinitionConfig"/> from the data of this object.
        /// </summary>
        /// <returns>
        ///     The config.
        /// </returns>
        internal ExchangeDefinitionConfig Configure() => new ExchangeDefinitionConfig(m_CatalogItem.key, m_Amount);

        /// <summary>
        ///     Creates a deep copy of this object.
        /// </summary>
        /// <returns>
        ///     A new instance with the same values.
        /// </returns>
        internal ExchangeDefinitionObject Clone() =>
            new ExchangeDefinitionObject
            {
                m_CatalogItem = m_CatalogItem,
                m_Amount = m_Amount
            };

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal void RefreshReferences(CatalogAsset owner)
        {
            m_CatalogItem = (TradableDefinitionAsset)owner.FindItem(m_CatalogItem.key);
        }
    }
}
