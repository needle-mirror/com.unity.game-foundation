#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CatalogItemAsset
    {
        /// <summary>
        ///     Returns the prefix used to give a name to the asset.
        /// </summary>
        internal abstract string editorAssetPrefix { get; }

        /// <summary>
        ///     Returns the name to assign to the asset.
        /// </summary>
        internal string editorAssetName => $"{editorAssetPrefix}_{key}";

        /// <summary>
        ///     Initializes this catalog item with the given data.
        /// </summary>
        /// <param name="newKey">
        ///     The identifier of this catalog item.
        /// </param>
        /// <param name="newDisplayName">
        ///     The display name of this catalog item.
        /// </param>
        internal void Editor_Initialize(string newKey, string newDisplayName)
        {
            GFTools.ThrowIfArgNullOrEmpty(newKey, nameof(newKey));
            GFTools.ThrowIfArgNullOrEmpty(newDisplayName, nameof(newDisplayName));

            Editor_SetKey(newKey);
            Editor_SetDisplayName(newDisplayName);
            name = editorAssetName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Sets the key of this catalog item.
        /// </summary>
        /// <param name="value">
        ///     The identifier to assign to this catalog item.
        /// </param>
        internal void Editor_SetKey(string value)
        {
            GFTools.ThrowIfArgNullOrEmpty(value, nameof(value));

            if (!GFTools.IsValidId(value))
            {
                throw new ArgumentException
                    ($"{nameof(CatalogItemAsset)}: Key can only be alphanumeric with optional dashes or underscores.");
            }

            m_Key = value;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Sets the display name of this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="value">
        ///     The display name to assign to this catalog item.
        /// </param>
        internal void Editor_SetDisplayName(string value)
        {
            GFTools.ThrowIfArgNullOrEmpty(value, nameof(value));

            SetDisplayName(value);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Adds a static property to this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the new static property.
        /// </param>
        /// <param name="value">
        ///     The value of this new static property.
        ///     Also defines its type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if created, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_AddStaticProperty(string propertyName, Property value)
        {
            if (staticProperties.ContainsKey(propertyName)
                || !UnityEngine.GameFoundation.Tools.IsValidId(propertyName))
                return false;

            staticProperties.Add(propertyName, value);
            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        ///     Remove the static property with the given <paramref name="propertyName"/> from this catalog.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the static property to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if removed, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveStaticProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return false;

            var isRemoved = staticProperties.Remove(propertyName);

            if (isRemoved)
            {
                EditorUtility.SetDirty(this);
            }

            return isRemoved;
        }

        /// <summary>
        ///     Update a static property of this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the static property to update.
        /// </param>
        /// <param name="value">
        ///     The updated value of the static property.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if updated, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_UpdateStaticProperty(string propertyName, Property value)
        {
            if (!staticProperties.ContainsKey(propertyName))
                return false;

            //Don't forget to update the entry since we are working with structs.
            staticProperties[propertyName] = value;
            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        ///     Adds the given <paramref name="tag"/> to this item.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> instance to add.
        /// </param>
        /// <returns>
        ///     Whether or not adding the <paramref name="tag"/> was successful.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the <paramref name="tag"/> is already on this catalog item.
        /// </exception>
        internal bool Editor_AddTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            if (m_Tags.Contains(tag))
            {
                throw new ArgumentException($"{nameof(CatalogItemAsset)}: {nameof(TagAsset)} {tag.key} " +
                    "cannot be added because a tag with this key already exists.");
            }

            m_Tags.Add(tag);

            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        ///     Removes the <paramref name="tag"/> from this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> to remove.
        /// </param>
        /// <returns>
        ///     Whether or not the removal was successful.
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

        protected void OnDestroy()
        {
            OnBeforeDestroy();
        }

        protected virtual void OnBeforeDestroy()
        {
            CatalogSettings.catalogAsset.m_Items.Remove(this);
        }

        /// <summary>
        ///     Copies all values from this instance to the target instance, except the display name and key.
        ///     (The display name and key should be provided by the user before this method is called.)
        /// </summary>
        /// <param name="target">
        ///     The target instance will be wiped clean and all fields will be
        ///     replaced with the data from this instance (except display name and key).
        /// </param>
        internal virtual void CopyValues(CatalogItemAsset target)
        {
            target.m_Tags = m_Tags.ToList();

            foreach (var kvp in staticProperties)
            {
                target.Editor_AddStaticProperty(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal virtual void RefreshReferences(CatalogAsset owner)
        {
            for (var i = 0; i < m_Tags.Count; i++)
            {
                var tag = m_Tags[i];
                var newRefTag = owner.tagCatalog.GetTagOrDie(tag.key, nameof(tag));

                m_Tags[i] = newRefTag;
            }
        }
    }
}

#endif
