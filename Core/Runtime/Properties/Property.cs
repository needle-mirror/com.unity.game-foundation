using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// ReSharper disable Unity.RedundantFormerlySerializedAsAttribute

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Struct of this type store a property value and its current type.
    /// </summary>
    /// <remarks>
    ///     All fields should be declared in this script to avoid the compilation warning
    ///     CS0282 (see https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0282).
    /// </remarks>
    [Serializable]
    public partial struct Property : IEquatable<Property>,
        IEqualityComparer<Property>,
        IComparable<Property>
    {
        /// <summary>
        ///     Stored value's type.
        /// </summary>
        [field: SerializeField]
        [field: FormerlySerializedAs("m_Type")]
        public PropertyType type { get; internal set; }

        /// <summary>
        ///     Stored built-in value type.
        ///     Relevant only if this property is one of the following types:
        ///     - <see cref="PropertyType.Long"/>.
        ///     - <see cref="PropertyType.Double"/>.
        ///     - <see cref="PropertyType.Bool"/>.
        ///     - <see cref="PropertyType.Addressables"/>.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("valueType")]
        internal CoalescedValueType m_ValueType;

        /// <summary>
        ///     Stored string value if this property is a <see cref="PropertyType.String"/> type.
        ///     Path to the stored asset if this property is an <see cref="PropertyType.ResourcesAsset"/> type.
        ///     Address if this property is an Addressables type.
        ///     Null otherwise.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("stringValue")]
        internal string m_StringValue;

        /// <summary>
        ///     Compare the two given operands for equality.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if both operands have the same property type and value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public static bool operator ==(Property a, Property b)
        {
            if (a.type != b.type)
                return false;

            switch (a.type)
            {
                case PropertyType.Long:
                    return a.longValue == b.longValue;

                case PropertyType.Double:
                    return Math.Abs(a.doubleValue - b.doubleValue) <= double.Epsilon;

                case PropertyType.Bool:
                    return a.boolValue == b.boolValue;

                case PropertyType.String:
                case PropertyType.ResourcesAsset:
                case PropertyType.Addressables:
                    return a.m_StringValue == b.m_StringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands for inequality.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if both operands have different property type or value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public static bool operator !=(Property a, Property b)
        {
            if (a.type != b.type)
                return true;

            switch (a.type)
            {
                case PropertyType.Long:
                    return a.longValue != b.longValue;

                case PropertyType.Double:
                    return Math.Abs(a.doubleValue - b.doubleValue) > double.Epsilon;

                case PropertyType.Bool:
                    return a.boolValue != b.boolValue;

                case PropertyType.String:
                case PropertyType.ResourcesAsset:
                case PropertyType.Addressables:
                    return a.m_StringValue != b.m_StringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Add the two given operands into a new <see cref="Property"/>.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     A new <see cref="Property"/> resulting from the addition of both operands.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static Property operator +(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.longValue + b.AsLong();

                    return result;
                }

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.doubleValue + b.AsDouble();

                    return result;
                }

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                case PropertyType.String:
                {
                    var result = a.m_StringValue + b.AsString();

                    return result;
                }

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Subtract the two given operands into a new <see cref="Property"/>.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     A new <see cref="Property"/> resulting from the subtraction of both operands.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static Property operator -(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.longValue - b.AsLong();

                    return result;
                }

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.doubleValue - b.AsDouble();

                    return result;
                }

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a"/>'s value is smaller than <paramref name="b"/>'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator <(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue < b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() < b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue < b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a"/>'s value is greater than <paramref name="b"/>'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator >(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue > b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() > b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue > b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a"/>'s value is smaller or equal to <paramref name="a"/>'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator <=(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue <= b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() <= b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue <= b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a"/>'s value is greater or equal to <paramref name="b"/>'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator >=(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue >= b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() >= b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue >= b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(Property other) => this == other;

        /// <summary>
        ///     Tells whether this <see cref="Property"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is Property other && this == other;

        /// <inheritdoc cref="IEqualityComparer{T}"/>
        public bool Equals(Property x, Property y) => x.Equals(y);

        /// <summary>
        ///     Gets the hash code of this <see cref="Property"/> instance.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="Property"/> instance.
        /// </returns>
        public override int GetHashCode()
        {
            //Generated with Rider.
            unchecked
            {
                var hashCode = (int)type;
                hashCode = (hashCode * 397) ^ m_ValueType.GetHashCode();
                hashCode = (hashCode * 397) ^ (m_StringValue != null ? m_StringValue.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc cref="IEqualityComparer{T}"/>
        public int GetHashCode(Property obj) => obj.GetHashCode();

        /// <inheritdoc cref="IEquatable{T}"/>
        public int CompareTo(Property other)
        {
            if (type != other.type)
            {
                if ((int)type < (int)other.type)
                    return -1;

                return 1;
            }

            switch (type)
            {
                case PropertyType.Long:
                    return m_ValueType.longValue.CompareTo(other.m_ValueType.longValue);

                case PropertyType.Double:
                    return m_ValueType.doubleValue.CompareTo(other.m_ValueType.doubleValue);

                case PropertyType.Bool:
                    return m_ValueType.boolValue.CompareTo(other.m_ValueType.boolValue);

                case PropertyType.String:
                case PropertyType.ResourcesAsset:
                case PropertyType.Addressables:
                    return string.Compare(m_StringValue, other.m_StringValue, StringComparison.Ordinal);

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(type));
            }
        }

        /// <summary>
        ///     Create a new <see cref="Property"/> instance by parsing the given
        ///     <paramref name="rawPropertyType"/> and <paramref name="rawValue"/>.
        /// </summary>
        /// <param name="rawPropertyType">
        ///     Property type to parse.
        /// </param>
        /// <param name="rawValue">
        ///     Property value to parse.
        /// </param>
        /// <param name="property">
        ///     Created <see cref="Property"/> if the parsing was successful.
        /// </param>
        /// <returns>
        ///     Return true if the given strings could be parsed into a valid <see cref="Property"/>;
        ///     return false otherwise.
        /// </returns>
        public static bool TryParse(string rawPropertyType, string rawValue, out Property property)
        {
            if (!Enum.TryParse(rawPropertyType, true, out PropertyType propertyType))
            {
                property = default;

                return false;
            }

            switch (propertyType)
            {
                case PropertyType.Long:
                {
                    if (int.TryParse(rawValue, out var intValue))
                    {
                        property = intValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.Double:
                {
                    if (float.TryParse(rawValue, out var floatValue))
                    {
                        property = floatValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.Bool:
                {
                    if (bool.TryParse(rawValue, out var boolValue))
                    {
                        property = boolValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.String:
                {
                    property = rawValue;

                    return true;
                }

                case PropertyType.ResourcesAsset:
                {
                    property = new Property
                    {
                        type = PropertyType.ResourcesAsset,
                        m_StringValue = rawValue
                    };

                    return true;
                }

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(propertyType));
            }

            property = default;

            return false;
        }

        static string GetUnsupportedTypeErrorMessage(PropertyType propertyType)
            => $"The property type \"{propertyType}\" isn't supported.";
    }
}
