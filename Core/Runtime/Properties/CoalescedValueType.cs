using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Structure to store all built-in value types,
    ///     except <see cref="decimal"/> in the same memory block.
    /// </summary>
    /// <remarks>
    ///     Only the bigger type is serialized to avoid conflicting
    ///     serialization and minimize the serialization size.
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    struct CoalescedValueType : IEquatable<CoalescedValueType>,
        IEqualityComparer<CoalescedValueType>,
        IComparable<CoalescedValueType>
    {
        [SerializeField]
        [FieldOffset(0)]
        public long longValue;

        [NonSerialized]
        [FieldOffset(0)]
        public ulong ulongValue;

        [NonSerialized]
        [FieldOffset(0)]
        public byte byteValue;

        [NonSerialized]
        [FieldOffset(0)]
        public sbyte sbyteValue;

        [NonSerialized]
        [FieldOffset(0)]
        public short shortValue;

        [NonSerialized]
        [FieldOffset(0)]
        public ushort ushortValue;

        [NonSerialized]
        [FieldOffset(0)]
        public int intValue;

        [NonSerialized]
        [FieldOffset(0)]
        public uint uintValue;

        [NonSerialized]
        [FieldOffset(0)]
        public float floatValue;

        [NonSerialized]
        [FieldOffset(0)]
        public double doubleValue;

        [NonSerialized]
        [FieldOffset(0)]
        public bool boolValue;

        [NonSerialized]
        [FieldOffset(0)]
        public char charValue;

        public static implicit operator long(CoalescedValueType value) => value.longValue;

        public static implicit operator CoalescedValueType(long value)
        {
            return new CoalescedValueType
            {
                longValue = value
            };
        }

        public static implicit operator ulong(CoalescedValueType value) => value.ulongValue;

        public static implicit operator CoalescedValueType(ulong value)
        {
            return new CoalescedValueType
            {
                ulongValue = value
            };
        }

        public static implicit operator byte(CoalescedValueType value) => value.byteValue;

        public static implicit operator CoalescedValueType(byte value)
        {
            return new CoalescedValueType
            {
                byteValue = value
            };
        }

        public static implicit operator sbyte(CoalescedValueType value) => value.sbyteValue;

        public static implicit operator CoalescedValueType(sbyte value)
        {
            return new CoalescedValueType
            {
                sbyteValue = value
            };
        }

        public static implicit operator short(CoalescedValueType value) => value.shortValue;

        public static implicit operator CoalescedValueType(short value)
        {
            return new CoalescedValueType
            {
                shortValue = value
            };
        }

        public static implicit operator ushort(CoalescedValueType value) => value.ushortValue;

        public static implicit operator CoalescedValueType(ushort value)
        {
            return new CoalescedValueType
            {
                ushortValue = value
            };
        }

        public static implicit operator int(CoalescedValueType value) => value.intValue;

        public static implicit operator CoalescedValueType(int value)
        {
            return new CoalescedValueType
            {
                intValue = value
            };
        }

        public static implicit operator uint(CoalescedValueType value) => value.uintValue;

        public static implicit operator CoalescedValueType(uint value)
        {
            return new CoalescedValueType
            {
                uintValue = value
            };
        }

        public static implicit operator float(CoalescedValueType value) => value.floatValue;

        public static implicit operator CoalescedValueType(float value)
        {
            return new CoalescedValueType
            {
                floatValue = value
            };
        }

        public static implicit operator double(CoalescedValueType value) => value.doubleValue;

        public static implicit operator CoalescedValueType(double value)
        {
            return new CoalescedValueType
            {
                doubleValue = value
            };
        }

        public static implicit operator bool(CoalescedValueType value) => value.boolValue;

        public static implicit operator CoalescedValueType(bool value)
        {
            return new CoalescedValueType
            {
                boolValue = value
            };
        }

        public static implicit operator char(CoalescedValueType value) => value.charValue;

        public static implicit operator CoalescedValueType(char value)
        {
            return new CoalescedValueType
            {
                charValue = value
            };
        }

        public bool Equals(CoalescedValueType other) => longValue.Equals(other.longValue);

        public bool Equals(CoalescedValueType x, CoalescedValueType y)
            => x.longValue.Equals(y.longValue);

        /// <summary>
        ///     Gets the hash code of this <see cref="CoalescedValueType"/> instance.
        ///     Returns the hash code of its <see cref="longValue"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="CoalescedValueType"/> instance.
        /// </returns>
        public override int GetHashCode() => longValue.GetHashCode();

        public int GetHashCode(CoalescedValueType obj) => obj.GetHashCode();

        public int CompareTo(CoalescedValueType other) => longValue.CompareTo(other.longValue);
    }
}
