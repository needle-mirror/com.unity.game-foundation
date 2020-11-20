namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown during a process manipulating the wallet when the player tries to spend more than he has.
    /// </summary>
    public class NotEnoughItemOfDefinitionException : GameFoundationException
    {
        /// <summary>
        ///     The id of the currency.
        /// </summary>
        public string definitionKey { get; }

        /// <summary>
        ///     The expected balance.
        /// </summary>
        public long expectedCount { get; }

        /// <summary>
        ///     The actual balance.
        /// </summary>
        public long actualCount { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotEnoughItemOfDefinitionException"/> class.
        /// </summary>
        /// <param name="definitionKey">
        ///     The identifier of the <see cref="InventoryItemDefinition"/>.
        /// </param>
        /// <param name="expectedCount">
        ///     The expected number of the given <see cref="InventoryItemDefinition"/>.
        /// </param>
        /// <param name="actualCount">
        ///     The available number of items in the <see cref="IInventoryManager"/>
        /// </param>
        internal NotEnoughItemOfDefinitionException(string definitionKey, long expectedCount, long actualCount)
            : base(BuildMessage(definitionKey, expectedCount, actualCount))
        {
            this.definitionKey = definitionKey;
            this.expectedCount = expectedCount;
            this.actualCount = actualCount;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="definitionKey"/>,
        ///     <paramref name="expectedCount"/>, and <paramref name="actualCount"/>.
        /// </summary>
        /// <param name="definitionKey">
        ///     The identifier of the <see cref="InventoryItemDefinition"/>.
        /// </param>
        /// <param name="expectedCount">
        ///     The expected number of the given <see cref="InventoryItemDefinition"/>.
        /// </param>
        /// <param name="actualCount">
        ///     The available number of items in the <see cref="IInventoryManager"/>
        /// </param>
        static string BuildMessage(string definitionKey, long expectedCount, long actualCount)
        {
            k_MessageBuilder.Clear()
                .Append($"Not enough item of definition {definitionKey}. Expected: {expectedCount.ToString()}, found: {actualCount.ToString()}");

            return k_MessageBuilder.ToString();
        }
    }
}
