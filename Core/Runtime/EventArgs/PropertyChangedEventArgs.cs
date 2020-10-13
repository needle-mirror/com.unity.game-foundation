namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Data sent when the property of a game item changed.
    /// </summary>
    public readonly struct PropertyChangedEventArgs
    {
        /// <summary>
        ///     The item that had its property changed.
        /// </summary>
        public InventoryItem item { get; }

        /// <summary>
        ///     The key of the changed property.
        /// </summary>
        public string propertyKey { get; }

        /// <summary>
        ///     The new value of the changed property.
        /// </summary>
        public Property newValue { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyChangedEventArgs"/> type.
        /// </summary>
        /// <param name="item">
        ///     The item that had its property changed.
        /// </param>
        /// <param name="propertyKey">
        ///     The key of the changed property.
        /// </param>
        /// <param name="newValue">
        ///     The new value of the changed property.
        /// </param>
        public PropertyChangedEventArgs(InventoryItem item, string propertyKey, Property newValue)
        {
            this.item = item;
            this.propertyKey = propertyKey;
            this.newValue = newValue;
        }
    }
}
