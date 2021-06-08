#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class TagCatalogAsset
    {
        /// <summary>
        ///     Adds the given <paramref name="tag"/> to this Catalog.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> instance to add.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="tag"/> was added successfully.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if a duplicate entry is given.
        /// </exception>
        internal void Editor_AddTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            if (FindTag(tag.key) != null)
            {
                throw new ArgumentException
                ($"{nameof(TagCatalogAsset)} The {nameof(TagAsset)} '{tag.key}' cannot be added because " +
                    "it is already registered within this Catalog.");
            }

            m_Tags.Add(tag);
            tag.m_Catalog = this;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Removes the given <paramref name="tag"/> from this catalog.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> to remove.
        /// </param>
        /// <returns>
        ///     Whether or not the <paramref name="tag"/> was successfully removed.
        /// </returns>
        internal bool Editor_RemoveTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            var removed = m_Tags.Remove(tag);
            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }

        /// <summary>
        ///     Gets all the subassets of this catalog.
        /// </summary>
        /// <param name="target">
        ///     The target collection to where subassets are added.
        /// </param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            if (m_Tags == null) return;

            foreach (var item in m_Tags)
            {
                if (item is null)
                {
                    continue;
                }

                target.Add(item);
            }
        }

        /// <summary>
        ///     Deep copy this instance.
        /// </summary>
        /// <returns>
        ///     Return a deep copy of this instance.
        /// </returns>
        internal TagCatalogAsset Clone()
        {
            var clone = CreateInstance<TagCatalogAsset>();
            clone.m_CatalogAsset = m_CatalogAsset;

            foreach (var tag in m_Tags)
            {
                var tagClone = tag.Clone();

                clone.m_Tags.Add(tagClone);
            }

            return clone;
        }

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal void RefreshReferences(CatalogAsset owner)
        {
            m_CatalogAsset = owner;

            foreach (var tag in m_Tags)
            {
                tag.RefreshReferences(owner);
            }
        }
    }
}
#endif
