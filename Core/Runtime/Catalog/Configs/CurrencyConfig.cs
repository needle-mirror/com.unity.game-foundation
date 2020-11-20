namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator the a <see cref="Currency"/> instance.
    /// </summary>
    public sealed partial class CurrencyConfig : CatalogItemConfig<Currency>
    {
        /// <summary>
        ///     The maximum balance for the currency.
        /// </summary>
        public long maximumBalance;

        /// <summary>
        ///     The type of the currency.
        /// </summary>
        public CurrencyType type;
    }
}
