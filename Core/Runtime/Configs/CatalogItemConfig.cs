using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator of a <see cref="CatalogItem"/> instance.
    /// </summary>
    public abstract class CatalogItemConfig
    {
        /// <summary>
        ///     The <see cref="CatalogItem"/> built by this configurator.
        /// </summary>
        internal CatalogItem runtimeItem;

        /// <summary>
        ///     The identifier of the item.
        /// </summary>
        public string key { get; internal set; }

        /// <summary>
        ///     The friendly name of the item.
        /// </summary>
        public string displayName;

        /// <summary>
        ///     The identifiers of the tags the item will be linked to.
        /// </summary>
        public readonly List<string> tags = new List<string>();

        /// <inheritdoc cref="CatalogItem.staticProperties"/>
        public readonly Dictionary<string, Property> staticProperties = new Dictionary<string, Property>();

        /// <summary>
        ///     Checks the configuration and creates the <see cref="CatalogItem"/> instance.
        ///     This method doesn't check the references this item could contain.
        ///     <seealso cref="Link(CatalogBuilder)"/>
        /// </summary>
        internal void Compile()
        {
            if (!Tools.IsValidId(key))
                throw new Exception($"{nameof(key)} {key} invalid");

            Tools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            runtimeItem = CompileGeneric();
            runtimeItem.key = key;
            runtimeItem.displayName = displayName;
            runtimeItem.tags = new Tag[tags.Count];
        }

        /// <summary>
        ///     Resolves the possible references the <see cref="CatalogItem"/> may
        ///     contain and builds them.
        /// </summary>
        /// <param name="builder">
        ///     The builder of the catalogs, where the references can be found
        /// </param>
        internal void Link(CatalogBuilder builder)
        {
            for (var i = 0; i < tags.Count; i++)
            {
                var tagKey = tags[i];
                if (string.IsNullOrEmpty(tagKey))
                    throw new Exception("Tag key empty or null.");

                runtimeItem.tags[i] = new Tag(tagKey);
            }

            foreach (var staticProperty in staticProperties)
            {
                runtimeItem.staticProperties.Add(staticProperty.Key, staticProperty.Value);
            }

            LinkGeneric(builder);
        }

        /// <summary>
        ///     This method is called by <see cref="Compile"/> to checks the
        ///     specific configuration of the inherited types.
        /// </summary>
        /// <inheritdoc cref="Compile"/>
        protected internal abstract CatalogItem CompileGeneric();

        /// <summary>
        ///     This method is called by <see cref="Compile"/> to checks the
        ///     links of the inherited types.
        /// </summary>
        /// <inheritdoc cref="Link(CatalogBuilder)"/>
        protected internal virtual void LinkGeneric(CatalogBuilder builder) { }
    }

    /// <inheritdoc/>
    /// <typeparam name="TRuntimeItem">
    ///     The <typeparamref name="TRuntimeItem"/>
    ///     built by this configurator.
    /// </typeparam>
    public abstract class CatalogItemConfig<TRuntimeItem> : CatalogItemConfig
        where TRuntimeItem : CatalogItem
    {
        /// <inheritdoc cref="CatalogItemConfig.runtimeItem"/>
        internal new TRuntimeItem runtimeItem
            => base.runtimeItem as TRuntimeItem;

        /// <inheritdoc/>
        protected internal sealed override CatalogItem CompileGeneric()
            => CompileItem();

        /// <inheritdoc/>
        protected internal sealed override void LinkGeneric(CatalogBuilder builder)
            => LinkItem(builder);

        /// <summary>
        ///     Checks the configuration and builds the
        ///     <typeparamref name="TRuntimeItem"/> instance.
        /// </summary>
        protected internal abstract TRuntimeItem CompileItem();

        /// <summary>
        ///     Resolves the possible referenes the
        ///     <typeparamref name="TRuntimeItem"/> instance may contain.
        /// </summary>
        /// <param name="builder">
        ///     The builder where the references can be
        ///     found
        /// </param>
        protected internal virtual void LinkItem(CatalogBuilder builder) { }
    }
}
