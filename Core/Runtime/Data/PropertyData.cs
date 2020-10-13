using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of a Property.
    /// </summary>
    [Serializable]
    public struct PropertyData : IEquatable<PropertyData>
    {
        /// <summary>
        ///     Property's identifier.
        /// </summary>
        public string key;

        /// <summary>
        ///     Property's current value.
        /// </summary>
        public Property value;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(PropertyData other) => key.Equals(other.key) && value.Equals(other.value);

        /// <summary>
        ///     Tells whether this <see cref="PropertyData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is PropertyData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="PropertyData"/> instance.
        ///     Returns the hash code of its <see cref="key"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="PropertyData"/> instance.
        /// </returns>
        public override int GetHashCode() => key.GetHashCode();
    }
}
