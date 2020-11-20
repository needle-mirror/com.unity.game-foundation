using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Contract for object that can create new instances of a <typeparamref name="TRuntimeObject"/>.
    /// </summary>
    /// <typeparam name="TRuntimeObject">
    ///     The type of object to create.
    /// </typeparam>
    public interface IBuildable<TRuntimeObject>
    {
        /// <summary>
        ///     Create a new <typeparamref name="TRuntimeObject"/>.
        /// </summary>
        /// <returns>
        ///     Return a promise handle to report how the operation went.
        ///     The handle's result is the compiled item if the compilation was successful.
        /// </returns>
        Deferred<TRuntimeObject> Compile();

        /// <summary>
        ///     Resolves the references from the given <paramref name="runtimeObject"/>
        ///     using the given <paramref name="compiledItems"/>.
        /// </summary>
        /// <param name="runtimeObject">
        ///     The runtime object to update references of.
        /// </param>
        /// <param name="compiledItems">
        ///     The collection of existing catalog items where references can be found.
        /// </param>
        /// <returns>
        ///     Return a promise handle to report how the operation went.
        /// </returns>
        Deferred Link(TRuntimeObject runtimeObject, Dictionary<string, CatalogItem> compiledItems);
    }
}
