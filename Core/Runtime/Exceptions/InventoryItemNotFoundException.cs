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
        public string itemId { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryItemNotFoundException"/> class.
        /// </summary>
        /// <param name="id">
        ///     The id of the item not found.
        /// </param>
        public InventoryItemNotFoundException(string id)
            : base(BuildMessage(id))
        {
            itemId = id;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The id of the item not found.
        /// </param>
        static string BuildMessage(string id)
        {
            k_MessageBuilder.Clear()
                .Append($"{nameof(InventoryItem)} {id} not found");

            return k_MessageBuilder.ToString();
        }
    }
}
