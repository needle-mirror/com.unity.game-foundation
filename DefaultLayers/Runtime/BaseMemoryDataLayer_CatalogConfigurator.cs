using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     The static data of the data layer.
        /// </summary>
        protected readonly CatalogAsset m_CatalogAsset;

        /// <summary>
        ///     Create a data layer using the given <paramref name="catalogAsset"/> for static data.
        /// </summary>
        /// <param name="catalogAsset">
        ///     The static data provider.
        /// </param>
        protected BaseMemoryDataLayer(CatalogAsset catalogAsset = null)
        {
            if (catalogAsset is null)
            {
                catalogAsset = CatalogSettings.catalogAsset;
            }

            m_CatalogAsset = catalogAsset;
        }

        /// <inheritdoc/>
        void ICatalogConfigurator.Configure(CatalogBuilder builder)
            => (m_CatalogAsset as ICatalogConfigurator).Configure(builder);
    }
}
