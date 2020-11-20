using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Base configurator of a <see cref="CatalogItem"/> instance.
    /// </summary>
    public abstract partial class CatalogItemConfig
    {
        /// <summary>
        ///     The identifier of the item.
        /// </summary>
        public string key { get; internal set; }

        /// <summary>
        ///     The friendly name of the item.
        /// </summary>
        public string displayName;

        /// <summary>
        ///     The identifiers of the tags the item will be linked to.
        /// </summary>
        public readonly List<string> tags = new List<string>();

        /// <inheritdoc cref="CatalogItem.staticProperties"/>
        public readonly Dictionary<string, Property> staticProperties = new Dictionary<string, Property>();
    }
}
