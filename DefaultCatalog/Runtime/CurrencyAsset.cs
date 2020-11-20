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
        [Min(0)]
        [Tooltip("The balance given to a new player")]
        [SerializeField]
        long m_InitialBalance;

        /// <summary>
        ///     Wrapper for <see cref="m_InitialBalance"/>.
        /// </summary>
        ExternalizableValue<long> m_InitialBalanceWrapper;

        /// <summary>
        ///     The maximum balance a player can have.
        /// </summary>
        [Min(0)]
        [Tooltip("The maximum balance a player can have")]
        [SerializeField]
        long m_MaximumBalance;

        /// <summary>
        ///     Wrapper for <see cref="m_MaximumBalance"/>.
        /// </summary>
        ExternalizableValue<long> m_MaximumBalanceWrapper;

        /// <summary>
        ///     Tells whether the currency is Soft or Hard.
        ///     This value has no impact on the behaviour of the currency,
        ///     and is just informational.
        ///     All currencies are handled the same way in Game Foundation.
        /// </summary>
        [SerializeField]
        CurrencyType m_Type;

        /// <summary>
        ///     Wrapper for <see cref="m_Type"/>.
        /// </summary>
        ExternalizableValue<CurrencyType> m_TypeWrapper;

        /// <inheritdoc cref="m_InitialBalance"/>
        public ExternalizableValue<long> initialBalance
        {
            get
            {
                if (m_InitialBalanceWrapper is null)
                {
                    m_InitialBalanceWrapper = new ExternalizableValue<long>(m_InitialBalance);
                }

                return m_InitialBalanceWrapper;
            }
        }

        /// <inheritdoc cref="m_MaximumBalance"/>
        public ExternalizableValue<long> maximumBalance
        {
            get
            {
                if (m_MaximumBalanceWrapper is null)
                {
                    m_MaximumBalanceWrapper = new ExternalizableValue<long>(m_MaximumBalance);
                }

                return m_MaximumBalanceWrapper;
            }
        }

        /// <inheritdoc cref="m_Type"/>
        public ExternalizableValue<CurrencyType> type
        {
            get
            {
                if (m_TypeWrapper is null)
                {
                    m_TypeWrapper = new ExternalizableValue<CurrencyType>(m_Type);
                }

                return m_TypeWrapper;
            }
        }

        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var config = builder.Create<CurrencyConfig>(key);
            var hasValueProvider = !(valueProvider is null);

            config.maximumBalance = maximumBalance;

            // Type.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.currencyType, key, out var externalType))
                {
                    var castExternalType = (CurrencyType)externalType.AsInt();
                    type.SetExternalValue(castExternalType);
                }
                else
                {
                    type.ResetExternalValue();
                }

                config.type = type;
            }

            return config;
        }

        /// <inheritdoc/>
        internal override void OverridePreConfigurationData(IExternalValueProvider valueProvider)
        {
            var hasValueProvider = !(valueProvider is null);

            // Initial balance.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.initialBalance, key, out var externalInitialBalance))
                {
                    initialBalance.SetExternalValue(externalInitialBalance);
                }
                else
                {
                    initialBalance.ResetExternalValue();
                }
            }

            // Maximum balance must be set now because it is used before
            // the configuration phase by CatalogAsset.CreateDefaultData.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.maximumBalance, key, out var externalMaximumBalance))
                {
                    maximumBalance.SetExternalValue(externalMaximumBalance);
                }
                else
                {
                    maximumBalance.ResetExternalValue();
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnAfterItemDeserialize()
        {
            base.OnAfterItemDeserialize();

            m_InitialBalanceWrapper = new ExternalizableValue<long>(m_InitialBalance);
            m_MaximumBalanceWrapper = new ExternalizableValue<long>(m_MaximumBalance);
            m_TypeWrapper = new ExternalizableValue<CurrencyType>(m_Type);
        }
    }
}
