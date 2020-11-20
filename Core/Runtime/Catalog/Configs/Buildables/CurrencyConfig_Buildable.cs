using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class CurrencyConfig
    {
        static readonly GameFoundationDebug k_Logger = GameFoundationDebug.Get(typeof(CurrencyConfig));

        /// <inheritdoc/>
        protected override Currency CompileItem(Rejectable rejectable)
        {
            if (maximumBalance < 0)
            {
                var message = $"The {nameof(Currency)} \"{key}\" has a negative {nameof(maximumBalance)}. " +
                    "It has been set to 0 to avoid failure.";
                k_Logger.LogWarning(message);

                maximumBalance = 0;
            }

            var currency = new Currency
            {
                maximumBalance = maximumBalance,
                type = type
            };

            return currency;
        }
    }
}
