using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for an <see cref="InventoryItemDefinition"/> instance.
    /// </summary>
    public class InventoryItemDefinitionConfig : CatalogItemConfig<InventoryItemDefinition>
    {
        /// <inheritdoc cref="InventoryItemDefinition.defaultProperties"/>
        public readonly Dictionary<string, Property> properties = new Dictionary<string, Property>();

        /// <inheritdoc/>
        protected internal override InventoryItemDefinition CompileItem()
        {
            var definition = new InventoryItemDefinition(properties);

            return definition;
        }
    }
}
