using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     The store entry, associating a transaction and an enable flag.
    /// </summary>
    [Serializable]
    public sealed partial class StoreItemObject
    {
        /// <inheritdoc cref="transaction"/>
        [SerializeField]
        internal BaseTransactionAsset m_Transaction;

        /// <inheritdoc cref="enabled"/>
        [SerializeField]
        internal bool m_Enabled;

        /// <summary>
        ///     A link to the store which owns this <see cref="StoreItemObject"/>.
        /// </summary>
        public StoreAsset store { get; internal set; }

        /// <summary>
        ///     A reference to a <see cref="BaseTransactionAsset"/>.
        /// </summary>
        public BaseTransactionAsset transaction => m_Transaction;

        /// <summary>
        ///     Tells whether or not the transaction will be added to the <see cref="StoreConfig"/>.
        /// </summary>
        public bool enabled => m_Enabled;
    }
}
