using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     The description of a <see cref="GameParameter"/>.
    /// </summary>
    public partial class GameParameterAsset : CatalogItemAsset
    {
        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
            => builder.Create<GameParameterConfig>(key);
    }
}
