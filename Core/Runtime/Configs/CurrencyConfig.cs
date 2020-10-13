using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator the a <see cref="Currency"/> instance.
    /// </summary>
    public class CurrencyConfig : CatalogItemConfig<Currency>
    {
        /// <summary>
        ///     The maximum balance for the currency.
        /// </summary>
        public long maximumBalance;

        /// <summary>
        ///     The type of the currency.
        /// </summary>
        public CurrencyType type;

        /// <inheritdoc/>
        protected internal override Currency CompileItem()
        {
            if (maximumBalance < 0)
            {
                throw new Exception
                    ($"{nameof(CurrencyConfig)}: {nameof(maximumBalance)} cannot be negative when compiling {nameof(Currency)}");
            }

            var currency = new Currency();
            currency.maximumBalance = maximumBalance;
            currency.type = type;

            return currency;
        }
    }
}
