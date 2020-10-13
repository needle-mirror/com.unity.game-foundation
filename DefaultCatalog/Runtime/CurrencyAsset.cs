using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Stores the information of a currency in a <see cref="ScriptableObject"/>.
    /// </summary>
    public partial class CurrencyAsset : TradableDefinitionAsset
    {
        /// <summary>
        ///     The balance provided to a new player.
        /// </summary>
        [Tooltip("The balance given to a new player")]
        [SerializeField]
        internal long m_InitialBalance;

        /// <summary>
        ///     External override for <see cref="m_InitialBalance"/>.
        /// </summary>
        [NonSerialized]
        long? m_ExternalInitialBalance;

        /// <summary>
        ///     The maximum balance a player can have.
        /// </summary>
        [Tooltip("The maximum balance a player can have")]
        [SerializeField]
        internal long m_MaximumBalance;

        /// <summary>
        ///     Tells whether the currency is Soft or Hard.
        ///     This value has no impact on the behaviour of the currency,
        ///     and is just informational.
        ///     All currencies are handled the same way in Game Foundation.
        /// </summary>
        [SerializeField]
        internal CurrencyType m_Type;

        /// <inheritdoc cref="m_InitialBalance"/>
        public long initialBalance => m_ExternalInitialBalance ?? m_InitialBalance;

        /// <inheritdoc cref="m_MaximumBalance"/>
        public long maximumBalance => m_MaximumBalance;

        /// <inheritdoc cref="m_Type"/>
        public CurrencyType type => m_Type;

        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var config = builder.Create<CurrencyConfig>(key);
            if (valueProvider == null
                || !valueProvider.TryGetValue(
                    ExternalValueProviderNames.maximumBalance, key, out var externalMaximumBalance))
            {
                config.maximumBalance = m_MaximumBalance;
            }
            else
            {
                config.maximumBalance = externalMaximumBalance;
            }

            if (valueProvider == null
                || !valueProvider.TryGetValue(
                    ExternalValueProviderNames.currencyType, key, out var externalType))
            {
                config.type = m_Type;
            }
            else
            {
                config.type = (CurrencyType)externalType.AsInt();
            }

            return config;
        }

        /// <inheritdoc/>
        internal override void OverrideNonConfiguredData(IExternalValueProvider valueProvider)
        {
            if (valueProvider != null
                && valueProvider.TryGetValue(
                    ExternalValueProviderNames.initialBalance, key, out var externalInitialBalance))
            {
                m_ExternalInitialBalance = externalInitialBalance;
            }
            else
            {
                m_ExternalInitialBalance = null;
            }
        }
    }
}
