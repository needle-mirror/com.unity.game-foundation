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
        public readonly string currencyKey;

        /// <summary>
        ///     The expected balance.
        /// </summary>
        public readonly long expectedBalance;

        /// <summary>
        ///     The actual balance.
        /// </summary>
        public readonly long actualBalance;

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
        internal NotEnoughBalanceException
            (string currencyKey, long expectedBalance, long actualBalance)
        {
            this.currencyKey = currencyKey;
            this.expectedBalance = expectedBalance;
            this.actualBalance = actualBalance;
        }

        /// <inheritdoc/>
        public override string Message
            => $"Not enough balance for {currencyKey}. Expected: {expectedBalance}, found: {actualBalance}";
    }
}
