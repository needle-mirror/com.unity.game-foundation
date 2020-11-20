using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Helper class to store a value and a potential override.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the wrapped object you want to override.
    /// </typeparam>
    public sealed class ExternalizableValue<T> : IEquatable<ExternalizableValue<T>>
    {
        /// <summary>
        ///     Implicitly cast the given <paramref name="wrapper"/> to <typeparamref name="T"/> using its <see cref="currentValue"/>.
        /// </summary>
        /// <param name="wrapper">
        ///     The instance to cast.
        /// </param>
        /// <returns>
        ///     Return the <see cref="currentValue"/> of the given <paramref name="wrapper"/>.
        /// </returns>
        public static implicit operator T(ExternalizableValue<T> wrapper)
            => wrapper.currentValue;

        /// <summary>
        ///     Implicitly create a new wrapper for the given <paramref name="value"/> using it as <see cref="baseValue"/>.
        /// </summary>
        /// <param name="value">
        ///     The object to wrap.
        /// </param>
        /// <returns>
        ///     Return a new instance of <see cref="ExternalizableValue{T}"/> using the given <paramref name="value"/> as <see cref="baseValue"/>.
        /// </returns>
        public static implicit operator ExternalizableValue<T>(T value)
            => new ExternalizableValue<T>(value);

        /// <summary>
        ///     The override of the original value.
        /// </summary>
        T m_ExternalValue;

        /// <summary>
        ///     A flag to determine if an external value has been defined.
        ///     This flag is required to allow <c>default</c> value has a legit value.
        /// </summary>
        bool m_HasExternalValue;

        /// <summary>
        ///     The original value to wrap.
        /// </summary>
        public T baseValue { get; }

        /// <summary>
        ///     The current value exposed by this wrapper.
        /// </summary>
        public T currentValue => m_HasExternalValue ? m_ExternalValue : baseValue;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ExternalizableValue() { }

        /// <summary>
        ///     Create a new wrapper for the given <paramref name="baseValue"/>.
        /// </summary>
        /// <param name="baseValue">
        ///     The value to wrap.
        /// </param>
        public ExternalizableValue(in T baseValue)
        {
            this.baseValue = baseValue;
            m_ExternalValue = default;
            m_HasExternalValue = false;
        }

        /// <summary>
        ///     Set <see cref="m_ExternalValue"/> and make sure it is considered as the <see cref="currentValue"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to set as the external value.
        /// </param>
        /// <returns>
        ///     Return this wrapper.
        /// </returns>
        internal ExternalizableValue<T> SetExternalValue(in T value)
        {
            m_ExternalValue = value;
            m_HasExternalValue = true;

            return this;
        }

        /// <summary>
        ///     Reset <see cref="m_ExternalValue"/> to <c>default</c> and make
        ///     <see cref="baseValue"/> the <see cref="currentValue"/>.
        /// </summary>
        /// <returns>
        ///     Return this wrapper.
        /// </returns>
        internal ExternalizableValue<T> ResetExternalValue()
        {
            m_HasExternalValue = false;
            m_ExternalValue = default;

            return this;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ExternalizableValue<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(m_ExternalValue, other.m_ExternalValue)
                && m_HasExternalValue == other.m_HasExternalValue
                && EqualityComparer<T>.Default.Equals(baseValue, other.baseValue);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            switch (obj)
            {
                case ExternalizableValue<T> sameTypeCast:
                {
                    return Equals(sameTypeCast);
                }

                default:
                {
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Generated with Rider.
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(m_ExternalValue);
                hashCode = (hashCode * 397) ^ m_HasExternalValue.GetHashCode();
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(baseValue);
                return hashCode;
            }
        }
    }
}
