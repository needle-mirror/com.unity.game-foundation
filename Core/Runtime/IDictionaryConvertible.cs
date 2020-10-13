using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects that can be:
    ///     - converted to a JSON ready dictionary
    ///     - filled by a JSON dictionary
    /// </summary>
    public interface IDictionaryConvertible
    {
        /// <summary>
        ///     Converts this object to a JSON ready dictionary.
        /// </summary>
        /// <returns>
        ///     Returns a dictionary containing this object's data.
        /// </returns>
        /// <remarks>
        ///     Avoid to use reflection here or this will strongly impact GameFoundation's Json serialization performances.
        /// </remarks>
        Dictionary<string, object> ToDictionary();

        /// <summary>
        ///     Reset this object and fill it with the given data.
        /// </summary>
        /// <param name="rawDictionary">
        ///     A dictionary containing JSON data.
        ///     Note: Expect integer values (enum included) to be stored as <see cref="long"/>
        ///     and floating values to be stored as <see cref="double"/>.
        /// </param>
        /// <remarks>
        ///     Avoid to use reflection here or this will strongly impact GameFoundation's Json deserialization performances.
        /// </remarks>
        void FillFromDictionary(Dictionary<string, object> rawDictionary);
    }
}
