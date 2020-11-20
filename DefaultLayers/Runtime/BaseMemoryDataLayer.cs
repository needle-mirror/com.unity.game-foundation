using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Base for the memory data layers.
    /// </summary>
    public abstract partial class BaseMemoryDataLayer : IDataAccessLayer
    {
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_Logger = GameFoundationDebug.Get<BaseMemoryDataLayer>();

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="ExchangeDefinitionObject"/>.
        /// </summary>
        static readonly Pool<List<ExchangeDefinitionObject>> k_ExchangeDefinitionsListPool =
            new Pool<List<ExchangeDefinitionObject>>(
                () => new List<ExchangeDefinitionObject>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="RewardItemObject"/>.
        /// </summary>
        static readonly Pool<List<RewardItemObject>> k_RewardItemObjectListPool =
            new Pool<List<RewardItemObject>>(
                () => new List<RewardItemObject>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="ExchangeDefinitionObject"/>.
        /// </summary>
        static readonly Pool<List<ExchangeDefinitionObject>> k_ExchangesListPool =
            new Pool<List<ExchangeDefinitionObject>>(
                () => new List<ExchangeDefinitionObject>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="Exception"/>.
        /// </summary>
        static readonly Pool<List<Exception>> k_ExceptionListPool =
            new Pool<List<Exception>>(
                () => new List<Exception>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="CurrencyExchangeData"/>.
        /// </summary>
        static readonly Pool<List<CurrencyExchangeData>> k_CurrencyDataListPool =
            new Pool<List<CurrencyExchangeData>>(
                () => new List<CurrencyExchangeData>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="List{T}"/> of <see cref="InventoryItemData"/>.
        /// </summary>
        static readonly Pool<List<InventoryItemData>> k_ItemDataListPool =
            new Pool<List<InventoryItemData>>(
                () => new List<InventoryItemData>(),
                list => list.Clear());

        /// <summary>
        ///     Provides instances of <see cref="Dictionary{TKey,TValue}"/>
        ///     of <see cref="string"/>/<see cref="long"/>.
        /// </summary>
        static readonly Pool<Dictionary<string, long>> k_DictionaryStringLongPool =
            new Pool<Dictionary<string, long>>(
                () => new Dictionary<string, long>(),
                dic => dic.Clear());

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
