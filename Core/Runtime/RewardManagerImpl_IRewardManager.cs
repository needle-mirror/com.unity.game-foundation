using System;
using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    partial class RewardManagerImpl : IRewardManager
    {
        event Action<string, string> IRewardManager.rewardItemClaimInitiated
        {
            add => rewardItemClaimInitiated += value;
            remove => rewardItemClaimInitiated -= value;
        }

        event Action<Reward, string, int, int> IRewardManager.rewardItemClaimProgressed
        {
            add => rewardItemClaimProgressed += value;
            remove => rewardItemClaimProgressed -= value;
        }

        event Action<Reward, string, Payout> IRewardManager.rewardItemClaimSucceeded
        {
            add => rewardItemClaimSucceeded += value;
            remove => rewardItemClaimSucceeded -= value;
        }

        event Action<string, string, Exception> IRewardManager.rewardItemClaimFailed
        {
            add => rewardItemClaimFailed += value;
            remove => rewardItemClaimFailed -= value;
        }

        Deferred<Payout> IRewardManager.Claim(RewardDefinition rewardDefinition, string rewardItemKey)
        {
            Tools.ThrowIfArgNull(rewardDefinition, nameof(rewardDefinition));
            Tools.ThrowIfArgNullOrEmpty(rewardItemKey, nameof(rewardItemKey));

            return Claim(rewardDefinition, rewardItemKey);
        }

        Reward IRewardManager.FindReward(string rewardKey)
        {
            Tools.ThrowIfArgNullOrEmpty(rewardKey, nameof(rewardKey));

            return FindReward(rewardKey);
        }

        string IRewardManager.GetLastClaimableRewardItemKey(RewardDefinition rewardDefinition)
        {
            Tools.ThrowIfArgNull(rewardDefinition, nameof(rewardDefinition));

            return GetLastClaimableRewardItemKey(rewardDefinition);
        }

        int IRewardManager.GetRewards(ICollection<Reward> target, bool clearTarget)
            => GetRewards(target, clearTarget);

        void IRewardManager.Update() => Update();
    }
}
