using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Base exception type for Game Foundation.
    /// </summary>
    public class GameFoundationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameFoundationException"/> type.
        /// </summary>
        public GameFoundationException() { }

        /// <inheritdoc cref="GameFoundationException"/>
        /// <param name="message">
        ///     Describes the exception.
        /// </param>
        public GameFoundationException(string message)
            : base(message) { }

        /// <inheritdoc cref="GameFoundationException(string)"/>
        /// <param name="innerException">
        ///     The internal exception.
        /// </param>
        public GameFoundationException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        ///     The message that describes the current exception.
        /// </summary>
        public override string Message => base.Message;
    }
}
