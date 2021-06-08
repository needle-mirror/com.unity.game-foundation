namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when an <see cref="IItemCollection"/> fails to be found by its <see cref="IItemCollection.id"/>.
    /// </summary>
    public class ItemCollectionNotFoundException : GameFoundationException
    {
        /// <summary>
        ///     The identifier of the collection not found.
        /// </summary>
        public string collectionId { get; }

        /// <summary>
        ///     The type name of the missing collection.
        /// </summary>
        public string collectionTypeName { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemCollectionNotFoundException"/> class.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the <see cref="IItemCollection"/> not found.
        /// </param>
        /// <param name="collectionTypeName">
        ///     The type name of the missing collection.
        /// </param>
        public ItemCollectionNotFoundException(string id, string collectionTypeName)
            : base(BuildMessage(id, collectionTypeName))
        {
            collectionId = id;
            this.collectionTypeName = collectionTypeName;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="id"/>, and <paramref name="collectionTypeName"/>.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the <see cref="IItemCollection"/> not found.
        /// </param>
        /// <param name="collectionTypeName">
        ///     The type name of the missing collection.
        /// </param>
        static string BuildMessage(string id, string collectionTypeName)
        {
            k_MessageBuilder.Clear()
                .Append($"No {collectionTypeName} with the id {id} could be found.");

            return k_MessageBuilder.ToString();
        }
    }
}
