namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown during a process manipulating the wallet when the player tries to spend more than they have.
    /// </summary>
    public class NotEnoughBalanceException : GameFoundationException
    {
        /// <summary>
        ///     The id of the currency.
        /// </summary>
        public string currencyKey { get; }

        /// <summary>
        ///     The expected balance.
        /// </summary>
        public long expectedBalance { get; }

        /// <summary>
        ///     The actual balance.
        /// </summary>
        public long actualBalance { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotEnoughBalanceException"/> class.
        /// </summary>
        /// <param name="currencyKey">
        ///     The identifier of the <see cref="Currency"/>
        /// </param>
        /// <param name="expectedBalance">
        ///     The necessary balance.
        /// </param>
        /// <param name="actualBalance">
        ///     The available balance.
        /// </param>
        internal NotEnoughBalanceException(string currencyKey, long expectedBalance, long actualBalance)
            : base(BuildMessage(currencyKey, expectedBalance, actualBalance))
        {
            this.currencyKey = currencyKey;
            this.expectedBalance = expectedBalance;
            this.actualBalance = actualBalance;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="currencyKey"/>,
        ///     <paramref name="expectedBalance"/>, and <paramref name="actualBalance"/>.
        /// </summary>
        /// <param name="currencyKey">
        ///     The identifier of the <see cref="Currency"/>
        /// </param>
        /// <param name="expectedBalance">
        ///     The necessary balance.
        /// </param>
        /// <param name="actualBalance">
        ///     The available balance.
        /// </param>
        static string BuildMessage(string currencyKey, long expectedBalance, long actualBalance)
        {
            k_MessageBuilder.Clear()
                .Append($"Not enough balance for {currencyKey}. Expected: {expectedBalance.ToString()}, found: {actualBalance.ToString()}");

            return k_MessageBuilder.ToString();
        }
    }
}
