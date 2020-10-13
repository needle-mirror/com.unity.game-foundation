namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when the <see cref="InventoryItem"/> fails to be found by its <see cref="InventoryItem.id"/>.
    /// </summary>
    public class InventoryItemNotFoundException : GameFoundationException
    {
        /// <summary>
        ///     The identifier of the <see cref="InventoryItem"/> not found.
        /// </summary>
        public readonly string itemId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryItemNotFoundException"/> class.
        /// </summary>
        /// <param name="id">
        ///     The id of the item not found.
        /// </param>
        public InventoryItemNotFoundException(string id)
        {
            itemId = id;
        }

        /// <inheritdoc/>
        public override string Message => $"{nameof(InventoryItem)} {itemId} not found";
    }
}
