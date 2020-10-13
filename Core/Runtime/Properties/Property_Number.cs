using System;

namespace UnityEngine.GameFoundation
{
    public partial struct Property
    {
        /// <summary>
        ///     Stored long value.
        ///     Relevant only if this property is a <see cref="PropertyType.Long"/> type.
        /// </summary>
        internal long longValue
        {
            get => m_ValueType.longValue;
            set => m_ValueType.longValue = value;
        }

        /// <summary>
        ///     Stored double value.
        ///     Relevant only if this property is a <see cref="PropertyType.Double"/> type.
        /// </summary>
        internal double doubleValue
        {
            get => m_ValueType.doubleValue;
            set => m_ValueType.doubleValue = value;
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into an integer.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="int"/> value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown if the stored value is outside int boundaries.
        /// </exception>
        public int AsInt()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return Convert.ToInt32(longValue);
                case PropertyType.Double:
                    return Convert.ToInt32(doubleValue);

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into an integer, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into a long.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="long"/> value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown if the stored value is outside long boundaries.
        /// </exception>
        public long AsLong()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return Convert.ToInt64(doubleValue);

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into an integer, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into a float.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="float"/> value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public float AsFloat()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return (float)doubleValue;

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a float, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into a double.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The <see cref="double"/> value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public double AsDouble()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return doubleValue;
                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a float, because it contains a {type} value.");
            }
        }

        /// <inheritdoc cref="AsInt"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="int"/> value of the property.
        /// </returns>
        public static implicit operator int(Property value) => value.AsInt();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>.
        /// </returns>
        public static implicit operator Property(int value)
        {
            return new Property
            {
                type = PropertyType.Long,
                longValue = value
            };
        }

        /// <inheritdoc cref="AsLong"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="long"/> value of the property.
        /// </returns>
        public static implicit operator long(Property value) => value.AsLong();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>.
        /// </returns>
        public static implicit operator Property(long value)
        {
            return new Property
            {
                type = PropertyType.Long,
                longValue = value
            };
        }

        /// <inheritdoc cref="AsFloat"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="float"/> value of the property.
        /// </returns>
        public static implicit operator float(Property value) => value.AsFloat();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>.
        /// </returns>
        public static implicit operator Property(float value)
        {
            return new Property
            {
                type = PropertyType.Double,
                doubleValue = value
            };
        }

        /// <inheritdoc cref="AsDouble"/>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The <see cref="double"/> value of the property.
        /// </returns>
        public static implicit operator double(Property value) => value.AsDouble();

        /// <summary>
        ///     Construct a <see cref="Property"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property"/>.
        /// </param>
        /// <returns>
        ///     The <paramref name="value"/> as a <see cref="Property"/>.
        /// </returns>
        public static implicit operator Property(double value)
        {
            return new Property
            {
                type = PropertyType.Double,
                doubleValue = value
            };
        }
    }
}
