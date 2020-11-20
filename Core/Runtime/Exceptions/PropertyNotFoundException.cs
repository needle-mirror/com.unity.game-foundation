namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when a requested property key doesn't exist in an item.
    /// </summary>
    public class PropertyNotFoundException : GameFoundationException
    {
        /// <summary>
        ///     The item's identifier where the requested property doesn't exist.
        /// </summary>
        public string itemId { get; }

        /// <summary>
        ///     The property's identifier not found.
        /// </summary>
        public string propertyKey { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotFoundException"/> type.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier where the requested property doesn't exist.
        /// </param>
        /// <param name="propertyKey">
        ///     The property's identifier not found.
        /// </param>
        public PropertyNotFoundException(string itemId, string propertyKey)
            : base(BuildMessage(itemId, propertyKey))
        {
            this.itemId = itemId;
            this.propertyKey = propertyKey;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="itemId"/> and <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier where the requested property doesn't exist.
        /// </param>
        /// <param name="propertyKey">
        ///     The property's identifier not found.
        /// </param>
        static string BuildMessage(string itemId, string propertyKey)
        {
            k_MessageBuilder.Clear()
                .Append($"The property \"{propertyKey}\" doesn't exist in the item \"{itemId}\"");

            return k_MessageBuilder.ToString();
        }
    }
}
