using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Base class for catalog items producing <see cref="ITradable"/> objects.
    /// </summary>
    public abstract class TradableDefinition : CatalogItem
    {
        /// <summary>
        ///     Verify if the player has enough resources to pay the given <paramref name="cost"/>.
        /// </summary>
        /// <param name="cost">
        ///     The amount of catalog item to check the availability of.
        /// </param>
        /// <param name="failReason">
        ///     An exception explaining why the cost can't be payed.
        /// </param>
        /// <returns>
        ///     Return true if the player has enough resources to pay the given <paramref name="cost"/>;
        ///     return false otherwise.
        /// </returns>
        internal abstract bool VerifyCost(long cost, out Exception failReason);
    }
}
