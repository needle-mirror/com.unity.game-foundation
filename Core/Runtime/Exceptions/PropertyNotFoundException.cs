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
        public readonly string itemId;

        /// <summary>
        ///     The property's identifier not found.
        /// </summary>
        public readonly string propertyKey;

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
        {
            this.itemId = itemId;
            this.propertyKey = propertyKey;
        }

        /// <inheritdoc/>
        public override string Message =>
            $"The property \"{propertyKey}\" doesn't exist in the item \"{itemId}\"";
    }
}
