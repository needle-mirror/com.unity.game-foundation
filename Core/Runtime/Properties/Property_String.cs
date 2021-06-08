using System;

namespace UnityEngine.GameFoundation
{
    public partial struct Property
    {
        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into a string.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="string"/> value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public string AsString()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue.ToString();
                case PropertyType.Double:
                    return doubleValue.ToString();
                case PropertyType.Bool:
                    return boolValue.ToString();
                case PropertyType.String:
                case PropertyType.ResourcesAsset:
                case PropertyType.Addressables:
                    return m_StringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(type));
            }
        }

        /// <inheritdoc cref="AsString"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="string"/> value.
        /// </returns>
        public static implicit operator string(Property value) => value.AsString();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>.
        /// </returns>
        public static implicit operator Property(string value)
        {
            return new Property
            {
                type = PropertyType.String,
                m_StringValue = value
            };
        }

        /// <summary>
        ///     Gets a <see cref="string"/> respresentation of this <see cref="Property"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> respresentation of this <see cref="Property"/>.
        /// </returns>
        public override string ToString() => AsString();
    }
}
