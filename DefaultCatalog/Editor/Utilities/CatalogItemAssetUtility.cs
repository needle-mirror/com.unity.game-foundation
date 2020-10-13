namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="CatalogItemAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class CatalogItemAssetUtility
    {
        /// <inheritdoc cref="CatalogItemAsset.Editor_SetDisplayName(string)"/>
        public static void SetDisplayName(this CatalogItemAsset @this, string displayName)
            => @this.Editor_SetDisplayName(displayName);

        /// <inheritdoc cref="CatalogItemAsset.Editor_AddStaticProperty(string, Property)"/>
        public static bool AddStaticProperty(this CatalogItemAsset @this, string name, Property value)
            => @this.Editor_AddStaticProperty(name, value);

        /// <inheritdoc cref="CatalogItemAsset.Editor_UpdateStaticProperty(string, Property)"/>
        public static bool UpdateStaticProperty(this CatalogItemAsset @this, string name, Property value)
            => @this.Editor_UpdateStaticProperty(name, value);

        /// <inheritdoc cref="CatalogItemAsset.Editor_RemoveStaticProperty"/>
        public static bool RemoveStaticProperty(this CatalogItemAsset @this, string name)
            => @this.Editor_RemoveStaticProperty(name);

        /// <inheritdoc cref="CatalogItemAsset.Editor_AddTag(TagAsset)"/>
        public static void AddTag(this CatalogItemAsset @this, TagAsset tag) => @this.Editor_AddTag(tag);

        /// <inheritdoc cref="CatalogItemAsset.Editor_RemoveTag(TagAsset)"/>
        public static void RemoveTag(this CatalogItemAsset @this, TagAsset tag) => @this.Editor_RemoveTag(tag);
    }
}
