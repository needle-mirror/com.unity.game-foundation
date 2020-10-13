namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Exception thrown when a wrong <see cref="PropertyType"/> is given to a property.
    /// </summary>
    public class PropertyInvalidCastException : GameFoundationException
    {
        /// <summary>
        ///     Key of the property that received the wrong property type.
        /// </summary>
        public string propertyKey { get; }

        /// <summary>
        ///     The expected property type.
        /// </summary>
        public PropertyType expectedPropertyType { get; }

        /// <summary>
        ///     The given property type.
        /// </summary>
        public PropertyType givenPropertyType { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyInvalidCastException"/> type.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the property.
        /// </param>
        /// <param name="expectedType">
        ///     The expected type of the property identified by the <paramref name="key"/>.
        /// </param>
        /// <param name="givenType">
        ///     The given type of the property.
        /// </param>
        public PropertyInvalidCastException(string key, PropertyType expectedType, PropertyType givenType)
        {
            propertyKey = key;
            expectedPropertyType = expectedType;
            givenPropertyType = givenType;
        }

        /// <inheritdoc/>
        public override string Message =>
            $"Trying to set a \"{givenPropertyType}\" value to the property \"{propertyKey}\" that only handle \"{expectedPropertyType}\" values.";
    }
}
