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
        public readonly string itemKey;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CatalogItemNotFoundException"/> class.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItem"/> not found.
        /// </param>
        public CatalogItemNotFoundException(string key)
        {
            itemKey = key;
        }

        /// <inheritdoc/>
        public override string Message => $"{nameof(CatalogItem)} {itemKey} not found";
    }
}
