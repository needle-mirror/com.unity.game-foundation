using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure that contains the state of Game Foundation.
    /// </summary>
    [Serializable]
    public struct GameFoundationData : IEquatable<GameFoundationData>
    {
        internal const int k_CurrentSchemaVersion = 0;

        /// <summary>
        ///     Get an empty instance of this struct.
        /// </summary>
        public static GameFoundationData Empty => new GameFoundationData
        {
            inventoryManagerData = new InventoryManagerData(),
            walletData = WalletData.Empty,
            rewardManagerData = RewardManagerData.Empty
        };

        /// <summary>
        ///     The data of InventoryManager to be persisted.
        /// </summary>
        public InventoryManagerData inventoryManagerData;

        /// <summary>
        ///     The data of Wallet to be persisted.
        /// </summary>
        public WalletData walletData;

        /// <summary>
        ///     The data of the RewardManager to be persisted.
        /// </summary>
        public RewardManagerData rewardManagerData;

        /// <summary>
        ///     The version of of the save schematic
        /// </summary>
        public int version;

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(GameFoundationData other)
        {
            return inventoryManagerData.Equals(other.inventoryManagerData)
                && walletData.Equals(other.walletData)
                && (rewardManagerData is null == other.rewardManagerData is null
                    || rewardManagerData != null && rewardManagerData.Equals(other.rewardManagerData));
        }

        /// <summary>
        ///     Tells whether this <see cref="GameFoundationData"/> instance equals <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">
        ///     The other object to compare this instance with.
        /// </param>
        /// <returns>
        ///     <c>true</c> if equals, <c>false</c> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is GameFoundationData other && Equals(other);

        /// <summary>
        ///     Gets the hash code of this <see cref="GameFoundationData"/> instance.
        ///     Returns its <see cref="version"/> value.
        /// </summary>
        /// <returns>
        ///     The hash code of this <see cref="GameFoundationData"/> instance.
        /// </returns>
        public override int GetHashCode() => version;
    }
}
