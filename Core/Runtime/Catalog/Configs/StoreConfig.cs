using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="Store"/> instance.
    /// </summary>
    public sealed partial class StoreConfig : CatalogItemConfig<Store>
    {
        /// <summary>
        ///     The identifiers of the <see cref="BaseTransaction"/> this store will
        ///     expose.
        /// </summary>
        public readonly List<string> transactions = new List<string>();
    }
}
