using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the items and currencies granted by a
    ///     <see cref="RewardItemDefinition"/> or <see cref="BaseTransaction"/>.
    /// </summary>
    public struct Payout
    {
        /// <summary>
        ///     The <see cref="ITradable"/> added.
        /// </summary>
        public IReadOnlyCollection<ITradable> products { get; internal set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Payout"/> struct.
        /// </summary>
        /// <param name="products">
        ///     The tradables given by the transaction.
        /// </param>
        internal Payout(IReadOnlyCollection<ITradable> products)
        {
            this.products = products;
        }
    }
}
