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
            : base(BuildMessage(key, expectedType, givenType))
        {
            propertyKey = key;
            expectedPropertyType = expectedType;
            givenPropertyType = givenType;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="key"/>,
        ///     <paramref name="expectedType"/>, and <paramref name="givenType"/>.
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
        static string BuildMessage(string key, PropertyType expectedType, PropertyType givenType)
        {
            k_MessageBuilder.Clear()
                .Append($"Trying to set a \"{givenType}\" value to the property \"{key}\" that only handle \"{expectedType}\" values.");

            return k_MessageBuilder.ToString();
        }
    }
}
