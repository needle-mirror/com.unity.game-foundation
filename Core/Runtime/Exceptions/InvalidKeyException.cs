using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when an invalid key is used to create a <see cref="CatalogItem"/>,
    ///     one of its <see cref="Property"/> or a <see cref="Tag"/>.
    /// </summary>
    public class InvalidKeyException : GameFoundationException
    {
        /// <summary>
        ///     The invalid key used to create a Game Foundation object.
        /// </summary>
        public string invalidKey { get; }

        /// <summary>
        ///     The name of the object's type that was supposed to be created with the invalid key.
        /// </summary>
        public string typeName { get; }

        /// <inheritdoc cref="InvalidKeyException"/>
        /// <param name="key">
        ///     The invalid key used to create a Game Foundation object.
        /// </param>
        /// <param name="objectTypeName">
        ///     The name of the object's type that was supposed to be created with the invalid key.
        /// </param>
        public InvalidKeyException(string key, string objectTypeName)
            : base(BuildMessage(key, objectTypeName))
        {
            invalidKey = key;
            typeName = objectTypeName;
        }

        /// <inheritdoc cref="InvalidKeyException"/>
        /// <param name="key">
        ///     The invalid key used to create a Game Foundation object.
        /// </param>
        /// <param name="objectType">
        ///     The type of the object that was supposed to be created with the invalid key.
        /// </param>
        public InvalidKeyException(string key, Type objectType)
            : this(key, objectType.Name) { }

        /// <summary>
        ///     Get the error message for the given <paramref name="key"/>, and <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="key">
        ///     The invalid key used to create a Game Foundation object.
        /// </param>
        /// <param name="objectTypeName">
        ///     The name of the object's type that was supposed to be created with the invalid key.
        /// </param>
        static string BuildMessage(string key, string objectTypeName)
        {
            k_MessageBuilder.Clear()
                .Append($"The key {key} isn't a valid key to create a {objectTypeName}.");

            return k_MessageBuilder.ToString();
        }
    }
}
