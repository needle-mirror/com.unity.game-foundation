using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class InventoryItemDefinitionConfig
    {
        /// <inheritdoc/>
        protected override InventoryItemDefinition CompileItem(Rejectable rejectable)
        {
            var definition = new InventoryItemDefinition(properties);

            return definition;
        }
    }
}
