namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Utility methods for <see cref="VirtualTransactionAsset"/> instances.
    /// </summary>
    /// <remarks>
    ///     Do not remove, this utility class is used to expose our editor API to external developers.
    /// </remarks>
    public static class RewardAssetUtility
    {
        /// <inheritdoc cref="RewardAsset.Editor_SetCooldown"/>
        public static void SetCooldown(this RewardAsset @this, int value, TimeUnit unit = TimeUnit.Seconds)
            => @this.Editor_SetCooldown(value, unit);

        /// <inheritdoc cref="RewardAsset.Editor_SetExpiration"/>
        public static void SetExpiration(this RewardAsset @this, int value, TimeUnit unit = TimeUnit.Seconds)
            => @this.Editor_SetExpiration(value, unit);

        /// <inheritdoc cref="RewardAsset.Editor_SetResetIfExpired"/>
        public static void SetResetIfExpired(this RewardAsset @this, bool value)
            => @this.Editor_SetResetIfExpired(value);

        /// <inheritdoc cref="RewardAsset.Editor_AddItem"/>
        public static void AddItem(this RewardAsset @this, RewardItemObject rewardItem)
            => @this.Editor_AddItem(rewardItem);

        /// <inheritdoc cref="RewardAsset.Editor_RemoveItem"/>
        public static bool RemoveItem(this RewardAsset @this, RewardItemObject rewardItem)
            => @this.Editor_RemoveItem(rewardItem);

        /// <inheritdoc cref="RewardAsset.Editor_SwapItemsListOrder"/>
        public static void SwapItemsListOrder(
            this RewardAsset @this, RewardItemObject rewardItem1, RewardItemObject rewardItem2)
            => @this.Editor_SwapItemsListOrder(rewardItem1, rewardItem2);

        /// <inheritdoc cref="RewardItemObject.Editor_AddPayout"/>
        public static void AddRewardItemPayout(
            this RewardItemObject @this, TradableDefinitionAsset catalogItem, long amount)
            => @this.Editor_AddPayout(catalogItem, amount);

        /// <inheritdoc cref="RewardItemObject.Editor_RemovePayout"/>
        public static bool RemoveRewardItemPayout(this RewardItemObject @this, ExchangeDefinitionObject item)
            => @this.Editor_RemovePayout(item);
    }
}
