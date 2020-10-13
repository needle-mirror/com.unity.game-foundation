using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     Part of the data layer dedicated to managing serializable reward states.
        /// </summary>
        internal RewardDataLayer m_RewardDataLayer;

        /// <summary>
        ///     Initializes the data layer for the <see cref="IRewardManager"/>.
        /// </summary>
        /// <param name="data">
        ///     RewardManager's serializable data.
        /// </param>
        /// <param name="catalogAsset">
        ///     The catalog asset to be used as source of truth.
        /// </param>
        protected void InitializeRewardDataLayer(RewardManagerData data, CatalogAsset catalogAsset)
        {
            m_RewardDataLayer = new RewardDataLayer(data, catalogAsset, m_WalletDataLayer, m_InventoryDataLayer);
        }

        /// <inheritdoc/>
        RewardManagerData IRewardDataLayer.GetData() =>
            (m_RewardDataLayer as IRewardDataLayer).GetData();

        /// <inheritdoc/>
        void IRewardDataLayer.Claim
            (string rewardKey, string rewardItemKey, Completer<TransactionExchangeData> completer)
            => (m_RewardDataLayer as IRewardDataLayer).Claim(rewardKey, rewardItemKey, completer);
    }
}
