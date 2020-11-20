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
        public string tagId { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TagNotFoundException"/> class.
        /// </summary>
        /// <param name="id">
        ///     The id of the <see cref="Tag"/> not found.
        /// </param>
        public TagNotFoundException(string id)
            : base(BuildMessage(id))
        {
            tagId = id;
        }

        /// <summary>
        ///     Get the error message for the given <paramref name="tagId"/>.
        /// </summary>
        /// <param name="tagId">
        ///     The id of the <see cref="Tag"/> not found.
        /// </param>
        static string BuildMessage(string tagId)
        {
            k_MessageBuilder.Clear()
                .Append($"{nameof(Tag)} {tagId} not found");

            return k_MessageBuilder.ToString();
        }
    }
}
