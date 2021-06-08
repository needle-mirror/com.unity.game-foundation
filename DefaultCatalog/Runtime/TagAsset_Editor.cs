#if UNITY_EDITOR

using System;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class TagAsset
    {
        /// <summary>
        ///     Returns the prefix used to give a name to the asset.
        /// </summary>
        internal const string editorAssetPrefix = "Tag";

        /// <summary>
        ///     Returns the name to assign to the asset.
        /// </summary>
        internal string editorAssetName => $"{editorAssetPrefix}_{key}";

        /// <summary>
        ///     Creates a TagAsset.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="TagAsset"/>.
        /// </param>
        /// <returns>
        ///     The newly created <see cref="TagAsset"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if an empty Id is given.
        /// </exception>
        internal static TagAsset Editor_Create(string key)
        {
            GFTools.ThrowIfArgNullOrEmpty(key, nameof(key));

            if (!Tools.IsValidKey(key))
            {
                throw new ArgumentException
                    ($"{nameof(TagAsset)} {nameof(TagAsset.key)} can only be alphanumeric with optional dashes or underscores.");
            }

            var tag = CreateInstance<TagAsset>();
            tag.Editor_Initialize(CatalogSettings.catalogAsset.tagCatalog, key);

            return tag;
        }

        /// <summary>
        ///     Initializes the id and object name.
        /// </summary>
        /// <param name="newCatalog">
        ///     The catalog this tag belongs to.
        /// </param>
        /// <param name="id">
        ///     The id of the definition.
        /// </param>
        internal void Editor_Initialize(TagCatalogAsset newCatalog, string id)
        {
            GFTools.ThrowIfArgNull(newCatalog, nameof(newCatalog));
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));

            m_Catalog = newCatalog;
            Editor_SetId(id);
            name = editorAssetName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Sets the identifier of this tag.
        /// </summary>
        /// <param name="id">
        ///     The identifier to assign to the definition.
        /// </param>
        internal void Editor_SetId(string id)
        {
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));

            if (!GFTools.IsValidId(id))
            {
                throw new ArgumentException
                    ($"{nameof(TagAsset)}: Id of {nameof(Tag)} can only be alphanumeric with optional dashes or underscores.");
            }

            m_Key = id;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Deep copy this instance.
        /// </summary>
        /// <returns>
        ///     Return a deep copy of this instance.
        /// </returns>
        internal TagAsset Clone()
        {
            var clone = CreateInstance<TagAsset>();
            clone.m_Catalog = m_Catalog;
            clone.m_Key = m_Key;

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
            m_Catalog = owner.tagCatalog;
        }

        public static bool operator ==(TagAsset a, TagAsset b)
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

        public static bool operator !=(TagAsset a, TagAsset b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Tells whether this <see cref="TagAsset"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is TagAsset other && this == other;

        /// <summary>
        ///     Gets the hash code of this <see cref="TagAsset"/> instance.
        ///     Returns the hash code of its <see cref="key"/>.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="TagAsset"/> instance.
        /// </returns>
        public override int GetHashCode() => key.GetHashCode();

        void OnDestroy()
        {
            if (catalog is null
                || catalog.catalogAsset is null)
            {
                return;
            }

            catalog.catalogAsset.Editor_RemoveTag(this);
        }
    }
}

#endif
