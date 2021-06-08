namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when a <see cref="CatalogItem"/> fails to be found by its <see cref="CatalogItem.key"/>.
    /// </summary>
    public class CatalogItemNotFoundException : GameFoundationException
    {
        /// <summary>
        ///     The identifier of the item not found.
        /// </summary>
        public string itemKey { get; }

        /// <summary>
        ///     The type name of the missing catalog item.
        /// </summary>
        public string catalogItemTypeName { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CatalogItemNotFoundException"/> class.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItem"/> not found.
        /// </param>
        public CatalogItemNotFoundException(string key)
            : this(key, nameof(CatalogItem)) { }

        /// <inheritdoc cref="CatalogItemNotFoundException(string)"/>
        /// <param name="missingItemTypeName">
        ///     The type name of the missing catalog item.
        /// </param>
        protected CatalogItemNotFoundException(string key, string missingItemTypeName)
            : base(BuildMessage(key, missingItemTypeName))
        {
            itemKey = key;
            catalogItemTypeName = missingItemTypeName;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="key"/>, and <paramref name="catalogItemTypeName"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItem"/> not found.
        /// </param>
        /// <param name="catalogItemTypeName">
        ///     The type name of the missing catalog item.
        /// </param>
        static string BuildMessage(string key, string catalogItemTypeName)
        {
            k_MessageBuilder.Clear()
                .Append($"{catalogItemTypeName} {key} not found.");

            return k_MessageBuilder.ToString();
        }
    }

    /// <inheritdoc/>
    public sealed class CatalogItemNotFoundException<TCatalogItem> : CatalogItemNotFoundException
        where TCatalogItem : CatalogItem
    {
        /// <inheritdoc/>
        public CatalogItemNotFoundException(string key)
            : base(key, typeof(TCatalogItem).Name) { }
    }
}
