using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     The static data of the data layer.
        /// </summary>
        internal CatalogAsset catalogAsset;

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

            this.catalogAsset = catalogAsset;
        }

        /// <inheritdoc/>
        void ICatalogConfigurator.Configure(CatalogBuilder builder)
            => (catalogAsset as ICatalogConfigurator).Configure(builder);
    }
}
