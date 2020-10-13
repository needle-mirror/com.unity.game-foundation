namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Contract for objects that want to override properties and fields
    ///     of catalog items from a <see cref="CatalogAsset"/>.
    /// </summary>
    /// <remarks>
    ///     Implement this interface and feed it to your database if you want to use
    ///     A/B testing, or Live Ops edit without rebuilding the game nor the assets.
    /// </remarks>
    public interface IExternalValueProvider
    {
        /// <summary>
        ///     Get a value for the given <paramref name="catalogItemFieldName"/> and
        ///     <paramref name="catalogItemKey"/> to override the one declared in the database.
        /// </summary>
        /// <param name="catalogItemFieldName">
        ///     The field name of the catalog item to get the value of.
        /// </param>
        /// <param name="catalogItemKey">
        ///     The key of the catalog item to get the value of.
        /// </param>
        /// <param name="value">
        ///     The value to use to override the one declared in the database.
        /// </param>
        /// <returns>
        ///     Return true if an override was defined for the given <paramref name="catalogItemFieldName"/>
        ///     and <paramref name="catalogItemKey"/>;
        ///     return false otherwise.
        /// </returns>
        bool TryGetValue(string catalogItemFieldName, string catalogItemKey, out Property value);
    }
}
