using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class StackableInventoryItemDefinitionConfig
    {
        /// <inheritdoc/>
        protected override InventoryItemDefinition CompileItem(Rejectable rejectable)
        {
            var definition = new StackableInventoryItemDefinition(properties);

            return definition;
        }
    }
}
