namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Interface for objects which know and can describe their
    ///     quantity value, when requested.
    /// </summary>
    public interface IQuantifiable
    {
        /// <summary>
        ///     Quantity of this object.
        /// </summary>
        long quantity { get; }
    }
}
