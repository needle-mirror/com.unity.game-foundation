using System;
using UnityEngine.Scripting;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class MigrateFromAttribute : PreserveAttribute
    {
        public readonly long fromVersion;

        /// <summary>
        ///     Add this attribute to a class to indicate that this class will automatically
        ///     migrate a given set of data using a certain schema version to a later schema version.
        ///     The target schema version is up to the implementation.
        /// </summary>
        /// <param name="fromVersion">
        ///     The version you want to migrate the data from.
        /// </param>
        public MigrateFromAttribute(long fromVersion)
        {
            this.fromVersion = fromVersion;
        }
    }
}
