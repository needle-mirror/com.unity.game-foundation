namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Pass an instance of this class into <see cref="GameFoundationSdk.Initialize"/>
    ///     if you want to provide or override certain dependencies in Game Foundation's subsystems.
    /// </summary>
    class GameFoundationInitOptions
    {
        /// <summary>
        ///     The instance of an <see cref="IPurchasingAdapter"/> implementation that should override
        ///     the default <see cref="UnityPurchasingAdapter"/> instance which Game Foundation creates during initialization.
        /// </summary>
        public IPurchasingAdapter purchasingAdapter;
    }
}
