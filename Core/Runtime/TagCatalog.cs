using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     The catalog storing tags.
    /// </summary>
    public class TagCatalog
    {
        /// <summary>
        ///     The list of <see cref="Tag"/> we use within this catalog.
        /// </summary>
        internal Tag[] m_Tags;

        /// <summary>
        ///     Gets a Tag from its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="Tag"/> to find.
        /// </param>
        /// <param name="paramName">
        ///     The name of the <paramref name="key"/> variable in the caller method. Used to build a better exception.
        /// </param>
        /// <returns>
        ///     The <see cref="Tag"/> instance with the specified <paramref name="key"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Throws an <see cref="ArgumentException"/> if the given tag key is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Throws an <see cref="ArgumentException"/> if the given tag key is not found.
        /// </exception>
        internal Tag GetTagOrDie(string key, string paramName)
        {
            Tools.ThrowIfArgNullOrEmpty(key, paramName);

            foreach (var tag in m_Tags)
            {
                if (tag.key == key)
                {
                    return tag;
                }
            }

            throw new ArgumentException($"{nameof(TagCatalog)}: {nameof(Tag)} {key} not found.", paramName);
        }

        /// <summary>
        ///     Gets a collection of tags from their <paramref name="ids"/> or throw an <see cref="ArgumentException"/>
        ///     if not found.
        /// </summary>
        /// <param name="ids">
        ///     The identifiers of the <see cref="Tag"/> instances to find.
        /// </param>
        /// <param name="paramName">
        ///     The name of the <paramref name="ids"/> variable in the caller method.
        ///     Used to build a better exception.
        /// </param>
        /// <param name="target">
        ///     The target collection for the tags.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="Tag"/> found.
        /// </returns>
        internal int GetTagsOrDie(ICollection<string> ids, ICollection<Tag> target, string paramName, bool clearTarget = true)
        {
            Tools.ThrowIfArgNull(ids, paramName);

            if (clearTarget) target?.Clear();

            foreach (var id in ids)
            {
                var tag = GetTagOrDie(id, paramName);

                target?.Add(tag);
            }

            return ids.Count;
        }

        /// <summary>
        ///     Looks for a <see cref="Tag"/> by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The <see cref="Tag.key"/> of the <see cref="Tag"/> to find.
        /// </param>
        /// <returns>
        ///     The requested <see cref="Tag"/> instance, or <pre>null</pre> if not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="key"/> parameter is <c>null</c>.
        /// </exception>
        public Tag Find(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            foreach (var tag in m_Tags)
            {
                if (tag.key == key)
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        ///     Tells whether or not this catalog contains the specified <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag"/> to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a <see cref="Tag"/> instance exists in this catalog with
        ///     the specified <paramref name="tag"/>, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> parameter is <c>null</c>.
        /// </exception>
        public bool Contains(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return Array.IndexOf(m_Tags, tag) >= 0;
        }

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all the <see cref="Tag"/> instances of this
        ///     catalog, and returns their count.
        ///     The <paramref name="target"/> collection is cleared before being populated.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="Tag"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="Tag"/> instances found.
        /// </returns>
        public int GetTags(ICollection<Tag> target = null, bool clearTarget = true)
            => Tools.Copy(m_Tags, target, clearTarget);
    }
}
