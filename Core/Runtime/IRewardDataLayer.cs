using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects providing data to the <see cref="IRewardManager"/>.
    /// </summary>
    public interface IRewardDataLayer
    {
        /// <summary>
        ///     Get the serializable data which is managed by <see cref="IRewardManager"/>.
        /// </summary>
        /// <returns>
        ///     The player's data for the <see cref="IRewardManager"/>.
        /// </returns>
        RewardManagerData GetData();

        /// <summary>
        ///     Grants the <see cref="Payout"/> to the player if the reward item key is currently valid.
        /// </summary>
        /// <param name="rewardKey">
        ///     The key of the reward that contains the reward item to claim.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the reward item to claim.
        /// </param>
        /// <param name="completer">
        ///     Maintains a reference to the original asynchronous promise.
        /// </param>
        void Claim(
            string rewardKey,
            string rewardItemKey,
            Completer<TransactionExchangeData> completer);
    }
}
