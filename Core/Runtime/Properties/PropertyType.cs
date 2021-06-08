namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Enum to specify the type of value stored in a property.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        ///     Designates a property as a <see cref="long"/> value.
        /// </summary>
        Long,

        /// <summary>
        ///     Designates a property as a <see cref="double"/> value.
        /// </summary>
        Double,

        /// <summary>
        ///     Designates a property as a <see cref="bool"/> value.
        /// </summary>
        Bool,

        /// <summary>
        ///     Designates a property as a <see cref="string"/> value.
        /// </summary>
        String,

        /// <summary>
        ///     Designates a property as an asset.
        /// </summary>
        ResourcesAsset,

        /// <summary>
        ///     Designates a property as an Addressables address value.
        /// </summary>
        Addressables
    }

    /// <summary>
    ///     Extension methods for the <see cref="PropertyType"/> type.
    /// </summary>
    public static class PropertyTypeExtensions
    {
        /// <summary>
        ///     Tells whether or not this <see cref="PropertyType"/> is a numeric value.
        /// </summary>
        /// <param name="this">
        ///     The <see cref="PropertyType"/> value to test.
        /// </param>
        /// <returns>
        ///     <c>true</c> if it is a numeric value, <c>false</c> otherwise.
        /// </returns>
        public static bool IsNumber(this PropertyType @this)
        {
            return @this == PropertyType.Double
                || @this == PropertyType.Long;
        }
    }
}
