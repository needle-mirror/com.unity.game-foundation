using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     <see cref="TagAsset"/> instance are used as filter arguments when
    ///     querying <see cref="CatalogItemAsset"/> instances within their catalog.
    ///     Each catalog is responsible for containing and managing its own set of
    ///     <see cref="TagAsset"/> instance.
    /// </summary>
    public sealed partial class TagAsset : ScriptableObject
    {
        /// <inheritdoc cref="key"/>
        [SerializeField]
        internal string m_Key;

        /// <summary>
        ///     The string Id of this GameItemDefinition.
        /// </summary>
        public string key => m_Key;

        /// <inheritdoc cref="catalog"/>
        [SerializeField]
        [HideInInspector]
        internal TagCatalogAsset m_Catalog;

        /// <summary>
        ///     Reference to the catalog of this item.
        /// </summary>
        public TagCatalogAsset catalog => m_Catalog;

        /// <summary>
        ///     Configures a specified <paramref name="builder"/> with this item.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        internal void Configure(CatalogBuilder builder)
        {
            ConfigureItem(builder);
        }

        /// <summary>
        ///     Configures a specified <paramref name="builder"/> with the specifics
        ///     of this item.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        /// <returns>
        ///     The item config.
        /// </returns>
        TagConfig ConfigureItem(CatalogBuilder builder)
        {
            var config = builder.GetTag(key);
            return config;
        }
    }
}
