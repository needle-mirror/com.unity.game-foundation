namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configuration object for a <see cref="GameParameter"/> instance.
    /// </summary>
    public class GameParameterConfig : CatalogItemConfig<GameParameter>
    {
        /// <inheritdoc/>
        protected internal override GameParameter CompileItem() => new GameParameter();
    }
}
