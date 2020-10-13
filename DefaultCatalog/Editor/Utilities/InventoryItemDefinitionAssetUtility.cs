namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="InventoryItemDefinitionAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class InventoryItemDefinitionAssetUtility
    {
        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_AddMutableProperty"/>
        public static bool AddMutableProperty(this InventoryItemDefinitionAsset @this, string name, Property defaultValue)
            => @this.Editor_AddMutableProperty(name, defaultValue);

        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_RemoveMutableProperty"/>
        public static bool RemoveMutableProperty(this InventoryItemDefinitionAsset @this, string name)
            => @this.Editor_RemoveMutableProperty(name);

        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_UpdateMutableProperty(string, Property)"/>
        public static bool UpdateMutableProperty(this InventoryItemDefinitionAsset @this, string name, Property value)
            => @this.Editor_UpdateMutableProperty(name, value);

        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_SetInitialAllocation"/>
        public static void SetInitialAllocation(this InventoryItemDefinitionAsset @this, int value)
            => @this.Editor_SetInitialAllocation(value);

        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_SetIsStackableFlag"/>
        public static void SetIsStackableFlag(this InventoryItemDefinitionAsset @this, bool value)
            => @this.Editor_SetIsStackableFlag(value);

        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_SetInitialQuantityPerStack"/>
        public static void SetInitialQuantityPerStack(this InventoryItemDefinitionAsset @this, int value)
            => @this.Editor_SetInitialQuantityPerStack(value);
    }
}
