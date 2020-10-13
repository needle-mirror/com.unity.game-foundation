namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when a <see cref="Tag"/> fails to be found by its <see cref="Tag.key"/>.
    /// </summary>
    public class TagNotFoundException : GameFoundationException
    {
        /// <summary>
        ///     The id of the tag not found.
        /// </summary>
        public readonly string tagId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TagNotFoundException"/> class.
        /// </summary>
        /// <param name="id">
        ///     The id of the <see cref="Tag"/> not found.
        /// </param>
        public TagNotFoundException(string id)
        {
            tagId = id;
        }

        /// <inheritdoc/>
        public override string Message => $"{nameof(Tag)} {tagId} not found";
    }
}
