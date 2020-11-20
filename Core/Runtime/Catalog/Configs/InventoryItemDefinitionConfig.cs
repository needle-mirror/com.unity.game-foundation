using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for an <see cref="InventoryItemDefinition"/> instance.
    /// </summary>
    public partial class InventoryItemDefinitionConfig : CatalogItemConfig<InventoryItemDefinition>
    {
        /// <inheritdoc cref="InventoryItemDefinition.defaultMutableProperties"/>
        public readonly Dictionary<string, Property> properties = new Dictionary<string, Property>();
    }
}
