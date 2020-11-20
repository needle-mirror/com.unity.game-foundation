using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

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
                throw new GameFoundationException
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
        ///     Return a promise handle to track the operation's status.
        ///     The operation's result is a tuple of a new <see cref="Catalog"/>
        ///     and a new <see cref="TagCatalog"/> if it is successful.
        /// </returns>
        public Deferred<(Catalog, TagCatalog)> Build()
        {
            var compiledItems = new Dictionary<string, CatalogItem>();
            var compiledTags = new List<Tag>(m_Tags.Count);

            Promises.GetHandles<(Catalog, TagCatalog)>(out var deferred, out var completer);

            using (var compilation = Compile(compiledItems, compiledTags))
            {
                if (!compilation.isFulfilled)
                {
                    completer.Reject(compilation.error);

                    return deferred;
                }
            }

            using (var linking = Link(compiledItems))
            {
                if (!linking.isFulfilled)
                {
                    completer.Reject(linking.error);

                    return deferred;
                }
            }

            var catalog = new Catalog(compiledItems);

            var tags = new TagCatalog
            {
                m_Tags = compiledTags.ToArray()
            };

            completer.Resolve((catalog, tags));

            return deferred;
        }

        /// <summary>
        ///     Checks the configurations and build the elements of each configuration objects.
        /// </summary>
        Deferred Compile(IDictionary<string, CatalogItem> compiledItems, ICollection<Tag> compiledTags)
        {
            Promises.GetHandles(out var deferred, out var completer);
            var errors = new List<Exception>();

            foreach (var tag in m_Tags.Values)
            {
                using (var tagCompilation = tag.Compile())
                {
                    if (tagCompilation.isFulfilled)
                    {
                        compiledTags.Add(tagCompilation.result);
                    }
                    else
                    {
                        errors.Add(tagCompilation.error);
                    }
                }
            }

            foreach (var config in m_Items.Values)
            {
                using (var itemCompilation = config.CompileGeneric())
                {
                    if (itemCompilation.isFulfilled)
                    {
                        compiledItems.Add(itemCompilation.result.key, itemCompilation.result);
                    }
                    else
                    {
                        errors.Add(itemCompilation.error);
                    }
                }
            }

            if (errors.Count > 0)
            {
                const string message = "Some errors occured while creating catalog items, see inner exceptions for details.";
                completer.Reject(new AggregateException(message, errors));
            }
            else
            {
                completer.Resolve();
            }

            return deferred;
        }

        /// <summary>
        ///     Resolves all the possible references each <see cref="CatalogItem"/> may contain.
        /// </summary>
        Deferred Link(Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);
            var errors = new List<Exception>();

            foreach (var config in m_Items.Values)
            {
                //The only reason for an item missing at this point is if its compilation failed
                //and since error have most likely already been logged, we simply skip this item.
                if (!compiledItems.TryGetValue(config.key, out var runtimeItem))
                {
                    continue;
                }

                using (var itemLinking = config.LinkGeneric(runtimeItem, compiledItems))
                {
                    if (!itemLinking.isFulfilled)
                    {
                        errors.Add(itemLinking.error);
                    }
                }
            }

            if (errors.Count > 0)
            {
                const string message = "Some errors occurred while verifying the linkages between Catalog Items, see inner exceptions for details.";
                completer.Reject(new AggregateException(message, errors));
            }
            else
            {
                completer.Resolve();
            }

            return deferred;
        }
    }
}
