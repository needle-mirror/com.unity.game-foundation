using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     The builder of all the runtime static data of Game Foundation.
    ///     It is given to the data layer so it can configure the data based on its
    ///     internal data structure, that Game Foundation doesn't need to know.
    /// </summary>
    public class CatalogBuilder
    {
        /// <summary>
        ///     The configurations of the <see cref="Tag"/> instances to build.
        /// </summary>
        readonly Dictionary<string, TagConfig> m_Tags =
            new Dictionary<string, TagConfig>();

        /// <summary>
        ///     The configurations of the <see cref="CatalogItem"/> instances to
        ///     build.
        /// </summary>
        readonly Dictionary<string, CatalogItemConfig> m_Items =
            new Dictionary<string, CatalogItemConfig>();

        /// <summary>
        ///     Gets a <see cref="CatalogItemConfig"/> from its <paramref name="key"/>
        ///     or die trying (throws an <see cref="Exception"/>).
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItemConfig"/> to find.
        /// </param>
        /// <returns>
        ///     The <see cref="CatalogItemConfig"/> corresponding to the <paramref name="key"/> parameter.
        /// </returns>
        internal CatalogItemConfig GetConfigOrDie(string key)
        {
            var found = m_Items.TryGetValue(key, out var item);
            if (!found)
            {
                throw new Exception($"{nameof(CatalogItemConfig)} {key} not found");
            }

            return item;
        }

        /// <summary>
        ///     Adds all the <see cref="CatalogItemConfig"/> instances of the
        ///     given <typeparamref name="TCatalogItem"/> type into the given
        ///     <paramref name="target"/> collection.
        /// </summary>
        /// <typeparam name="TCatalogItem">
        ///     The type of items to find.
        /// </typeparam>
        /// <param name="target">
        ///     The target collection where the items will be added.
        /// </param>
        void GetItems<TCatalogItem>(List<CatalogItemConfig> target)
            where TCatalogItem : CatalogItemConfig
        {
            foreach (var item in m_Items.Values)
            {
                if (item is TCatalogItem catalogItem)
                {
                    target.Add(catalogItem);
                }
            }
        }

        /// <summary>
        ///     Creates a new <typeparamref name="TCatalogItem"/> instance.
        /// </summary>
        /// <typeparam name="TCatalogItem">
        ///     The type of the item to create.
        /// </typeparam>
        /// <param name="key">
        ///     The identifier of the <typeparamref name="TCatalogItem"/> instance to create.
        /// </param>
        /// <returns>
        ///     The new <typeparamref name="TCatalogItem"/> instance.
        /// </returns>
        public TCatalogItem Create<TCatalogItem>(string key)
            where TCatalogItem : CatalogItemConfig, new()
        {
            var found = m_Items.TryGetValue(key, out var existingItem);
            if (found)
            {
                throw new Exception
                    ($"Another {nameof(CatalogItem)} of type {existingItem.GetType()} and key {key} already exists");
            }

            var item = new TCatalogItem();
            item.key = key;
            m_Items.Add(key, item);
            return item;
        }

        /// <summary>
        ///     Creates a new Tag instance.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the new tag.
        /// </param>
        /// <returns>
        ///     The new Tag instance.
        /// </returns>
        [Obsolete("Use GetTag(string, bool) instead.")]
        public TagConfig CreateTag(string key) => GetTag(key);

        /// <summary>
        ///     Gets a new Tag instance by its key.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the tag.
        /// </param>
        /// <param name="create">
        ///     Creates the tag config if not found.
        /// </param>
        /// <returns>
        ///     The <see cref="Tag"/> instance.
        /// </returns>
        public TagConfig GetTag(string key, bool create = true)
        {
            var found = m_Tags.TryGetValue(key, out var tag);
            if (!found && create)
            {
                tag = new TagConfig
                {
                    key = key
                };
                m_Tags.Add(key, tag);
            }

            return tag;
        }

        /// <summary>
        ///     Aggregate current data of this builder to create new catalogs.
        /// </summary>
        /// <returns>
        ///     Return a new <see cref="Catalog"/> and a new <see cref="TagCatalog"/>
        ///     built from current data of this builder.
        /// </returns>
        public (Catalog, TagCatalog) Build()
        {
            Compile();
            Link();
            return BuildCatalog();
        }

        /// <summary>
        ///     Checks the configurations and build the elements of each configuration objects.
        /// </summary>
        void Compile()
        {
            foreach (var tag in m_Tags.Values)
            {
                tag.Compile();
            }

            foreach (var item in m_Items.Values)
            {
                item.Compile();
            }
        }

        /// <summary>
        ///     Resolves all the possible references each <see cref="CatalogItem"/> may contain.
        /// </summary>
        void Link()
        {
            foreach (var item in m_Items.Values)
            {
                item.Link(this);
            }
        }

        /// <inheritdoc cref="Build"/>
        (Catalog, TagCatalog) BuildCatalog()
        {
            // tags
            var tags = new TagCatalog { m_Tags = new Tag[m_Tags.Count] };

            var index = 0;

            foreach (var tagConfig in m_Tags.Values)
            {
                tags.m_Tags[index++] = tagConfig.runtimeTag;
            }

            // everything else
            var catalog = new Catalog();

            foreach (var catalogItemConfig in m_Items.Values)
            {
                catalog.m_Items.Add(catalogItemConfig.key, catalogItemConfig.runtimeItem);
            }

            return (catalog, tags);
        }
    }
}
