using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Base class for most of the static data in Game Foundation.
    /// </summary>
    public abstract partial class CatalogItem : IEquatable<CatalogItem>, IComparable<CatalogItem>
    {
        /// <summary>
        ///     The <see cref="Tag"/> instances this item is linked to.
        /// </summary>
        internal Tag[] tags;

        /// <summary>
        ///     The readable name of this <see cref="CatalogItem"/> instance.
        ///     It is used to make the Editor more comfortable, but it can also be used at runtime to display a readable
        ///     information to the players.
        /// </summary>
        public string displayName { get; internal set; }

        /// <summary>
        ///     The unique identifier of this <see cref="CatalogItem"/>.
        /// </summary>
        public string key { get; internal set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CatalogItem"/> class.
        /// </summary>
        internal CatalogItem() { }

        /// <summary>
        ///     Fills the given <paramref name="target"/> collection with all the <see cref="Tag"/> instances linked
        ///     to this <see cref="CatalogItem"/> instance.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="Tag"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="Tag"/> instances linked to this <see cref="CatalogItem"/> instance.
        /// </returns>
        public int GetTags(ICollection<Tag> target = null, bool clearTarget = true)
            => Tools.Copy(tags, target, clearTarget);

        /// <summary>
        ///     Tells whether or not the given <paramref name="tag"/> is within this <see cref="CatalogItem"/> instance.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> instance to search for.
        /// </param>
        /// <returns>
        ///     Whether or not this <see cref="CatalogItem"/> instance has the specified <see cref="Tag"/> included.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> is <c>null</c>.
        /// </exception>
        public bool HasTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return Array.IndexOf(tags, tag) >= 0;
        }

        /// <summary>
        ///     Gets the hash code of this <see cref="CatalogItem"/> instance.
        ///     Returns the hash code of its <see cref="key"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="CatalogItem"/> instance.
        /// </returns>
        public override int GetHashCode() => key.GetHashCode();

        /// <summary>
        ///     Tells whether this <see cref="CatalogItem"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        /// <summary>
        ///     Determines if this instance equals the <paramref name="other"/> instance.
        /// </summary>
        /// <param name="other">
        ///     The other object to compare this <see cref="CatalogItem"/> instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        bool IEquatable<CatalogItem>.Equals(CatalogItem other) => ReferenceEquals(this, other);

        /// <summary>
        ///     Compares this instance with an <paramref name="other"/> one.
        /// </summary>
        /// <param name="other">
        ///     The other instance to compare this one with.
        /// </param>
        /// <returns>
        ///     <c>0</c> if equivalent, non-<c>0</c> otherwise.
        /// </returns>
        int IComparable<CatalogItem>.CompareTo(CatalogItem other) => key.CompareTo(other.key);
    }
}
