using System;

namespace UnityEngine.GameFoundation
{
    public partial struct Property
    {
        /// <summary>
        ///     Stored bool value.
        ///     Relevant only if this property is a <see cref="PropertyType.Bool"/> type.
        /// </summary>
        internal bool boolValue
        {
            get => m_ValueType.boolValue;
            set => m_ValueType.boolValue = value;
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into a bool.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool"/> value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public bool AsBool()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue != 0;
                case PropertyType.Double:
                    return Math.Abs(doubleValue) > double.Epsilon;
                case PropertyType.Bool:
                    return boolValue;

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a bool, because it contains a {type} value.");
            }
        }

        /// <inheritdoc cref="AsBool"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="bool"/> value of the property.
        /// </returns>
        public static implicit operator bool(Property value) => value.AsBool();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>
        /// </returns>
        public static implicit operator Property(bool value)
        {
            return new Property
            {
                type = PropertyType.Bool,
                boolValue = value
            };
        }
    }
}
