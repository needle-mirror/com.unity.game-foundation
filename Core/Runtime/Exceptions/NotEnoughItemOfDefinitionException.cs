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
        public readonly string definitionKey;

        /// <summary>
        ///     The expected balance.
        /// </summary>
        public readonly long expectedCount;

        /// <summary>
        ///     The actual balance.
        /// </summary>
        public readonly long actualCount;

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
        internal NotEnoughItemOfDefinitionException
            (string definitionKey, long expectedCount, long actualCount)
        {
            this.definitionKey = definitionKey;
            this.expectedCount = expectedCount;
            this.actualCount = actualCount;
        }

        /// <inheritdoc/>
        public override string Message
            => $"Not enough item of definition {definitionKey}. Expected: {expectedCount}, found: {actualCount}";
    }
}
