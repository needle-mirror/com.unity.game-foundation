using System;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Straightforward synchronous data layer that keep data in memory for the session only.
    /// </summary>
    public class MemoryDataLayer : BaseMemoryDataLayer
    {
        bool m_IsInitialized;

        /// <summary>
        ///     The serialized data manipulated by this instance.
        /// </summary>
        GameFoundationData m_Data;

        /// <summary>
        ///     Creates a data layer with no player data.
        /// </summary>
        /// <param name="database">
        ///     Provides definitions to the runtime <see cref="Catalog"/>.
        /// </param>
        public MemoryDataLayer(CatalogAsset database = null)
            : base(database)
        {
            m_Data = m_CatalogAsset.CreateDefaultData();
        }

        /// <summary>
        ///     Create a data layer with the given catalog provider that will handle the given data for the current game
        ///     session only.
        /// </summary>
        /// <param name="database">
        ///     Provides definitions to the runtime <see cref="Catalog"/>.
        /// </param>
        /// <param name="data">
        ///     GameFoundation's serializable data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the given data contains invalid null values.
        /// </exception>
        public MemoryDataLayer(GameFoundationData data, CatalogAsset database = null)
            : base(database)
        {
            if (data.inventoryManagerData.items is null)
            {
                throw new ArgumentNullException(
                    $"{nameof(MemoryDataLayer)}: {nameof(InventoryManagerData)}'s " +
                    $"{nameof(InventoryManagerData.items)} mustn't be null when instantiating {nameof(MemoryDataLayer)}.",
                    new NullReferenceException());
            }

            if (data.walletData.balances is null)
            {
                throw new ArgumentNullException(
                    $"{nameof(MemoryDataLayer)}: {nameof(WalletData)}'s {nameof(WalletData.balances)} mustn't " +
                    $"be null when instantiating {nameof(MemoryDataLayer)}.",
                    new NullReferenceException());
            }

            if (data.rewardManagerData.rewards is null)
            {
                throw new ArgumentNullException(
                    $"{nameof(MemoryDataLayer)}: {nameof(RewardManagerData)}'s {nameof(RewardManagerData.rewards)} " +
                    $"mustn't be null when instantiating {nameof(MemoryDataLayer)}.",
                    new NullReferenceException());
            }

            m_Data = data;
        }

        /// <inheritdoc/>
        public override void Initialize(Completer completer)
        {
            if (m_IsInitialized)
            {
                // Re-initializing an object already initialized is a silent error.
                completer.Resolve();

                return;
            }

            try
            {
                InitializeInventoryDataLayer(m_Data.inventoryManagerData);
                InitializeWalletDataLayer(m_Data.walletData);

                // this must be called *after* inventory and wallet
                InitializeRewardDataLayer(m_Data.rewardManagerData);

                m_Version = m_Data.version;

                m_IsInitialized = true;

                // Reset data to loose references to the child objects.
                m_Data = new GameFoundationData();

                completer.Resolve();
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc cref="BaseMemoryDataLayer.GetData"/>
        public GameFoundationData GetLayerData() => GetData();
    }
}
