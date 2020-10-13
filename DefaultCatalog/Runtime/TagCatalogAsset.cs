using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Serialization;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Stores and provides <see cref="TagAsset"/>.
    /// </summary>
    public sealed partial class TagCatalogAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        ///     The list of <see cref="TagAsset"/> of this catalog.
        /// </summary>
        [SerializeField]
        internal List<TagAsset> m_Tags = new List<TagAsset>();

        /// <inheritdoc cref="catalogAsset"/>
        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("m_Database")]
        internal CatalogAsset m_CatalogAsset;

        /// <summary>
        ///     A reference to the database owning this catalog.
        /// </summary>
        public CatalogAsset catalogAsset => m_CatalogAsset;

        /// <summary>
        ///     Utility methods getting a <see cref="TagAsset"/> by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the <see cref="Tag"/> to find.
        /// </param>
        /// <param name="paramName">
        ///     The name of the <paramref name="id"/> parameter from the caller method.
        ///     It makes the <see cref="ArgumentException"/> display the name of the
        ///     erroneous parameter of the caller instead of the one used in this
        ///     utility method.
        /// </param>
        /// <returns>
        ///     Returns the <see cref="TagAsset"/> instance.
        /// </returns>
        internal TagAsset GetTagOrDie(string id, string paramName)
        {
            GFTools.ThrowIfArgNull(id, paramName);

            var tag = FindTag(id);
            if (tag is null)
            {
                throw new TagNotFoundException(id);
            }

            return tag;
        }

        /// <summary>
        ///     Returns specified <see cref="TagAsset"/> by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the <see cref="TagAsset"/> to find.
        /// </param>
        /// <returns>
        ///     If found, it returns the requested <see cref="TagAsset"/>, otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="id"/> parameter is null, empty, of whitespace.
        /// </exception>
        public TagAsset FindTag(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach (var tag in m_Tags)
            {
                if (tag.key == id)
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        ///     Tells whether or not a <see cref="TagAsset"/> with the given
        ///     <paramref name="id"/> exists in this catalog.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the <see cref="TagAsset"/>
        ///     to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> if found, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="id"/> parameter is null, empty or whitespace
        /// </exception>
        public bool ContainsTag(string id) => FindTag(id) != null;

        /// <summary>
        ///     Fills in the given <paramref name="target"/> collection with all the <see cref="TagAsset"/> of this
        ///     catalog.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="TagAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="TagAsset"/> instances of this catalog.
        /// </returns>
        public int GetTags(ICollection<TagAsset> target = null, bool clearTarget = true)
            => GFTools.Copy(m_Tags, target, clearTarget);

        /// <summary>
        ///     Configures the specified <paramref name="builder"/> with the content
        ///     of this catalog.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        internal void Configure(CatalogBuilder builder)
        {
            ConfigureCatalog(builder);
        }

        /// <summary>
        ///     Do thing. Only <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/> is necessry.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        /// <summary>
        ///     Links the tag to this catalog.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_Tags != null)
            {
                foreach (var tag in m_Tags)
                {
                    if (tag is null)
                    {
                        continue;
                    }

                    tag.m_Catalog = this;
                }
            }
        }

        /// <summary>
        ///     Configures the specified <paramref name="builder"/> with the
        ///     specific content of this catalog.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        void ConfigureCatalog(CatalogBuilder builder)
        {
            foreach (var tagAsset in m_Tags)
            {
                tagAsset.Configure(builder);
            }
        }
    }
}
