namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for an <see cref="StackableInventoryItemDefinition"/> instance.
    /// </summary>
    public sealed class StackableInventoryItemDefinitionConfig : InventoryItemDefinitionConfig
    {
        /// <inheritdoc/>
        protected internal override InventoryItemDefinition CompileItem()
        {
            var definition = new StackableInventoryItemDefinition(properties);

            return definition;
        }
    }
}
