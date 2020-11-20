using System;
using UnityEngine.Internal;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Base class for implementations of the different Game Foundation managers.
    ///     This is mostly used to unify their initialization/cleanup flow.
    /// </summary>
    [ExcludeFromDocs]
    abstract class ManagerImplementation
    {
        /// <summary>
        ///     Initializes this manager using GameFoundation's <see cref="GameFoundationSdk.dataLayer"/>
        ///     and <see cref="GameFoundationSdk.catalog"/>.
        /// </summary>
        internal Deferred Initialize()
        {
            Promises.GetHandles(out var deferred, out var completer);

            try
            {
                InitializeData(completer);
            }
            catch (Exception e)
            {
                Uninitialize();

                completer.Reject(e);
            }

            return deferred;
        }

        /// <summary>
        ///     Read GameFoundation's data layer to fill this manager with the correct data.
        /// </summary>
        /// <param name="completer">
        ///     A handle to the initialization promise.
        /// </param>
        protected abstract void InitializeData(Completer completer);

        /// <summary>
        ///     Reset this manager.
        /// </summary>
        internal abstract void Uninitialize();
    }
}
