namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="CatalogAsset"/>.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class CatalogAssetUtility
    {
        /// <inheritdoc cref="CatalogAsset.Editor_AddItem"/>
        public static void AddItem(this CatalogAsset @this, CatalogItemAsset item)
            => @this.Editor_AddItem(item);

        /// <inheritdoc cref="CatalogAsset.Editor_CreateItem{TCatalogItem}"/>
        public static TCatalogItem CreateItem<TCatalogItem>(this CatalogAsset @this, string key)
            where TCatalogItem : CatalogItemAsset, new()
            => @this.Editor_CreateItem<TCatalogItem>(key);

        /// <inheritdoc cref="CatalogAsset.Editor_RemoveItem"/>
        public static bool RemoveItem(this CatalogAsset @this, CatalogItemAsset item)
            => @this.Editor_RemoveItem(item);

        /// <inheritdoc cref="CatalogAsset.Editor_CreateTag"/>
        public static TagAsset CreateTag(this CatalogAsset @this, string id)
            => @this.Editor_CreateTag(id);

        /// <inheritdoc cref="CatalogAsset.Editor_RemoveTag"/>
        public static bool RemoveTag(this CatalogAsset @this, TagAsset tag)
            => @this.Editor_RemoveTag(tag);

        /// <inheritdoc cref="CatalogAsset.Editor_Save"/>
        public static void Save(this CatalogAsset @this, string path)
            => @this.Editor_Save(path);
    }
}
