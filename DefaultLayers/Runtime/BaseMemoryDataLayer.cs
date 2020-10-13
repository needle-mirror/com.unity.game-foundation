using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Base for the memory data layers.
    /// </summary>
    public abstract partial class BaseMemoryDataLayer : IDataAccessLayer
    {
        /// <summary>
        ///     Stores the version of the data structure.
        /// </summary>
        protected int m_Version;

        /// <inheritdoc/>
        public abstract void Initialize(Completer completer);

        /// <summary>
        ///     Gets all the data from Game Foundation (for persistence)
        /// </summary>
        /// <returns>
        ///     The player's data.
        /// </returns>
        protected GameFoundationData GetData()
        {
            var inventoryData = (this as IInventoryDataLayer).GetData();
            var walletData = (this as IWalletDataLayer).GetData();
            var rewardManagerData = (this as IRewardDataLayer).GetData();

            var data = new GameFoundationData
            {
                version = m_Version,
                inventoryManagerData = inventoryData,
                walletData = walletData,
                rewardManagerData = rewardManagerData,
            };

            return data;
        }
    }
}
