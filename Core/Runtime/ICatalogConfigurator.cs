namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects providing catalogs to the <see cref="Catalog"/>.
    /// </summary>
    public interface ICatalogConfigurator
    {
        /// <summary>
        ///     Configures the specified <paramref name="builder"/> with the content of this provider.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        void Configure(CatalogBuilder builder);
    }
}
