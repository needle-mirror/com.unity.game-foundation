using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class GameParameterConfig
    {
        /// <inheritdoc/>
        protected override GameParameter CompileItem(Rejectable rejectable) => new GameParameter();

        /// <inheritdoc/>
        protected override bool DoRequireDisplayName() => false;
    }
}
