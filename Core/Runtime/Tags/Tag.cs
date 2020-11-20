using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     <see cref="Tag"/> instance are used as filter arguments when querying <see cref="CatalogItem"/> instances
    ///     within a catalog.
    /// </summary>
    public sealed class Tag : IEquatable<Tag>, IComparable<Tag>
    {
        /// <summary>
        ///     The identifier of this <see cref="Tag"/>.
        /// </summary>
        public string key { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="key">
        ///     The identifier of this <see cref="Tag"/>.
        /// </param>
        internal Tag(string key)
        {
            this.key = key;
        }

        /// <summary>
        ///     == Overload.
        ///     If <see cref="key"/> matches then the <see cref="Tag"/> instances are deemed equal.
        /// </summary>
        /// <remarks>
        ///     Two <c>null</c> objects are considered equal.
        /// </remarks>
        /// <param name="a">
        ///     A <see cref="Tag"/> instance to compare.
        /// </param>
        /// <param name="b">
        ///     The <see cref="Tag"/> instance to compare to.
        /// </param>
        /// <returns>
        ///     <c>true</c> if both <see cref="Tag"/> are the same.
        /// </returns>
        public static bool operator ==(Tag a, Tag b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.key == b.key;
        }

        /// <summary>
        ///     != Overload.
        ///     If <see cref="key"/> doesn't match then the <see cref="Tag"/> instances are deemed not equal.
        /// </summary>
        /// <remarks>
        ///     Two <c>null</c> objects are considered equal.
        /// </remarks>
        /// <param name="x">
        ///     A <see cref="Tag"/> instance to compare.
        /// </param>
        /// <param name="y">
        ///     The <see cref="Tag"/> instance to compare to.
        /// </param>
        /// <returns>
        ///     <c>true</c> if both <see cref="Tag"/> are not the same.
        /// </returns>
        public static bool operator !=(Tag x, Tag y) => !(x == y);

        /// <summary>
        ///     Tells whether this <see cref="Tag"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is Tag other && key == other.key;

        /// <summary>
        ///     Gets the hash code of this <see cref="Tag"/> instance.
        ///     Returns the hash code of its <see cref="key"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="Tag"/> instance.
        /// </returns>
        public override int GetHashCode() => key.GetHashCode();

        /// <inheritdoc cref="IEquatable{T}"/>
        bool IEquatable<Tag>.Equals(Tag other)
        {
            if (other is null)
            {
                return this is null;
            }

            return key == other.key;
        }

        /// <inheritdoc cref="IComparable{T}"/>
        int IComparable<Tag>.CompareTo(Tag other) => key.CompareTo(other.key);
    }
}
