using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     The reward entry which comprises a segment of a reward schedule.
    /// </summary>
    [Serializable]
    public sealed partial class RewardItemObject
    {
        /// <summary>
        ///     The globally-unique identifier for this RewardItem.
        /// </summary>
        [field: SerializeField]
        public string key { get; internal set; }

        /// <summary>
        ///     A link to the <see cref="RewardAsset"/> which owns this <see cref="RewardItemObject"/>.
        /// </summary>
        public RewardAsset reward { get; internal set; }

        /// <inheritdoc cref="payout"/>
        [SerializeField]
        internal TransactionExchangeDefinitionObject m_Payout
            = new TransactionExchangeDefinitionObject();

        /// <summary>
        ///     A reference to a <see cref="TransactionExchangeDefinitionObject"/>.
        /// </summary>
        public TransactionExchangeDefinitionObject payout => m_Payout;

        /// <summary>
        ///     Constructor that initializes the key to a generated GUID.
        /// </summary>
        public RewardItemObject()
        {
            key = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Creates a configuration for a <see cref="RewardItemDefinition"/>
        /// </summary>
        /// <returns>
        ///     Returns a Game Foundation runtime-compatible <see cref="RewardItemDefinition"/> object.
        /// </returns>
        internal RewardItemConfig Configure()
        {
            return new RewardItemConfig
            {
                key = key,
                payout = m_Payout.Configure()
            };
        }
    }
}
