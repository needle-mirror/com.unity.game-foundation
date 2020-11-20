using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     An asset for storing a <see cref="RewardDefinition"/> in your Assets.
    /// </summary>
    public sealed partial class RewardAsset : CatalogItemAsset
    {
        /// <inheritdoc cref="cooldownSeconds"/>
        [Min(0)]
        [SerializeField]
        int m_CooldownSeconds;

        /// <summary>
        ///     Wrapper for <see cref="m_CooldownSeconds"/>.
        /// </summary>
        ExternalizableValue<int> m_CooldownWrapper = new ExternalizableValue<int>();

        /// <summary>
        ///     The amount of time, in seconds, between when a RewardItem is claimed
        ///     and when the next one will become available.
        /// </summary>
        public ExternalizableValue<int> cooldownSeconds
        {
            get
            {
                if (m_CooldownWrapper is null)
                {
                    m_CooldownWrapper = new ExternalizableValue<int>(m_CooldownSeconds);
                }

                return m_CooldownWrapper;
            }
        }

        /// <inheritdoc cref="cooldownDisplayUnits"/>
        [SerializeField]
        TimeUnit m_CooldownDisplayUnits = TimeUnit.Days;

        /// <summary>
        ///     The units to display cooldown time in on the editor
        /// </summary>
        public TimeUnit cooldownDisplayUnits
        {
            get => m_CooldownDisplayUnits;
            set => m_CooldownDisplayUnits = value;
        }

        /// <inheritdoc cref="expirationSeconds"/>
        [Min(0)]
        [SerializeField]
        int m_ExpirationSeconds;

        /// <summary>
        ///     Wrapper for <see cref="m_ExpirationSeconds"/>.
        /// </summary>
        ExternalizableValue<int> m_ExpirationWrapper;

        /// <summary>
        ///     The amount of time, in seconds, that a RewardItem will be claimable before either
        ///     moving on to the next RewardItem, or resetting back to the first RewardItem.
        /// </summary>
        public ExternalizableValue<int> expirationSeconds
        {
            get
            {
                if (m_ExpirationWrapper is null)
                {
                    m_ExpirationWrapper = new ExternalizableValue<int>(m_ExpirationSeconds);
                }

                return m_ExpirationWrapper;
            }
        }

        /// <inheritdoc cref="expirationDisplayUnits"/>
        [SerializeField]
        TimeUnit m_ExpirationDisplayUnits = TimeUnit.Days;

        /// <summary>
        ///     The units to display expiration time in on the editor
        /// </summary>
        public TimeUnit expirationDisplayUnits
        {
            get => m_ExpirationDisplayUnits;
            set => m_ExpirationDisplayUnits = value;
        }

        /// <inheritdoc cref="resetIfExpired"/>
        [SerializeField]
        bool m_ResetIfExpired;

        /// <summary>
        ///     Wrapper for <see cref="m_ResetIfExpired"/>.
        /// </summary>
        ExternalizableValue<bool> m_ResetIfExpiredWrapper;

        /// <summary>
        ///     If a RewardItem expires, this determines whether the next RewardItem will become available,
        ///     or if the Reward is reset back to the first RewardItem.
        /// </summary>
        public ExternalizableValue<bool> resetIfExpired
        {
            get
            {
                if (m_ResetIfExpiredWrapper is null)
                {
                    m_ResetIfExpiredWrapper = new ExternalizableValue<bool>(m_ResetIfExpired);
                }

                return m_ResetIfExpiredWrapper;
            }
        }

        /// <summary>
        ///     The collection of reward items which are presented by this <see cref="RewardDefinition"/>.
        /// </summary>
        [SerializeField]
        internal List<RewardItemObject> m_RewardItems;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardAsset>();

        /// <summary>
        ///     Looks for a <see cref="RewardItemDefinition"/> based on its key.
        /// </summary>
        /// <param name="rewardItemKey">
        ///     The unique key string of the reward item to find.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="RewardItemObject"/> if one is found, otherwise null.
        /// </returns>
        public RewardItemObject FindRewardItem(string rewardItemKey)
        {
            GFTools.ThrowIfArgNull(rewardItemKey, nameof(rewardItemKey));

            foreach (var rewardItem in m_RewardItems)
            {
                if (rewardItem.key.Equals(rewardItemKey))
                {
                    return rewardItem;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds all the <see cref="RewardItemObject"/>s to the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">
        ///     The target collection where the <see cref="RewardItemObject"/>s are added.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="RewardItemObject"/> added.
        /// </returns>
        public int GetRewardItems(ICollection<RewardItemObject> target = null, bool clearTarget = true)
            => GFTools.Copy(m_RewardItems, target, clearTarget);

        /// <summary>
        ///     Tells whether or not the reward contains the given <paramref name="rewardItem"/>.
        /// </summary>
        /// <param name="rewardItem">
        ///     The <see cref="RewardItemObject"/> to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this <see cref="RewardAsset"/> instance contains the <paramref name="rewardItem"/>.
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Contains(RewardItemObject rewardItem)
        {
            GFTools.ThrowIfArgNull(rewardItem, nameof(rewardItem));

            return IndexOf(rewardItem) >= 0;
        }

        /// <summary>
        ///     Get the index of a <see cref="RewardItemObject"/> in this RewardAsset.
        /// </summary>
        /// <param name="rewardItemObject">
        ///     The reward item object to find the index of.
        /// </param>
        /// <returns>
        ///     The index of the reward item object.
        /// </returns>
        public int IndexOf(RewardItemObject rewardItemObject)
        {
            if (rewardItemObject == null || m_RewardItems == null)
            {
                return -1;
            }

            for (var i = 0; i < m_RewardItems.Count; i++)
            {
                if (ReferenceEquals(m_RewardItems[i], rewardItemObject))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Initializes the reward items collection.
        /// </summary>
        protected override void AwakeDefinition()
        {
            if (m_RewardItems is null)
            {
                m_RewardItems = new List<RewardItemObject>();
            }

            foreach (var item in m_RewardItems)
            {
                item.reward = this;
            }
        }

        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            GFTools.ThrowIfArgNull(builder, nameof(builder));

            var rewardConfig = builder.Create<RewardConfig>(key);
            var hasValueProvider = !(valueProvider is null);

            // Cooldown.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.cooldownSeconds, key, out var externalCooldownSeconds))
                {
                    cooldownSeconds.SetExternalValue(externalCooldownSeconds);
                }
                else
                {
                    cooldownSeconds.ResetExternalValue();
                }

                rewardConfig.cooldownSeconds = cooldownSeconds;
            }

            // Expiration.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.expirationSeconds, key, out var externalExpirationSeconds))
                {
                    expirationSeconds.SetExternalValue(externalExpirationSeconds);
                }
                else
                {
                    expirationSeconds.ResetExternalValue();
                }

                rewardConfig.expirationSeconds = expirationSeconds;
            }

            // Reset if expired.
            {
                if (hasValueProvider
                    && valueProvider.TryGetValue(
                        ExternalValueProviderNames.resetIfExpired, key, out var externalResetIfExpired))
                {
                    resetIfExpired.SetExternalValue(externalResetIfExpired);
                }
                else
                {
                    resetIfExpired.ResetExternalValue();
                }

                rewardConfig.resetIfExpired = resetIfExpired;
            }

            for (var i = 0; i < m_RewardItems.Count; i++)
            {
                var rewardItem = m_RewardItems[i];

                if (rewardItem == null)
                {
                    k_GFLogger.LogWarning($"The RewardItem at index {i} in Reward {m_Key} is null during item " +
                        "configuration.");
                    continue;
                }

                if (rewardItem.m_Payout is null)
                {
                    k_GFLogger.LogWarning($"Null value in m_Payout for RewardItem at index {i} in Reward " +
                        $"{m_Key} during item configuration.");
                    continue;
                }

                if (rewardItem.m_Payout.m_Exchanges is null)
                {
                    k_GFLogger.LogWarning($"Null value in m_Exchanges in payout in RewardItem at index {i} in " +
                        $"Reward {m_Key} during item configuration.");
                    continue;
                }

                if (rewardItem.m_Payout.m_Exchanges.Count == 0)
                {
                    k_GFLogger.LogWarning($"The payout for RewardItem at index {i} in Reward {m_Key} has no " +
                        "exchanges configured during item configuration.");
                }

                rewardConfig.rewardItemConfigs.Add(m_RewardItems[i].Configure());
            }

            return rewardConfig;
        }

        /// <inheritdoc/>
        protected override void OnAfterItemDeserialize()
        {
            base.OnAfterItemDeserialize();

            m_CooldownWrapper = new ExternalizableValue<int>(m_CooldownSeconds);
            m_ExpirationWrapper = new ExternalizableValue<int>(m_ExpirationSeconds);
            m_ResetIfExpiredWrapper = new ExternalizableValue<bool>(m_ResetIfExpired);
        }
    }
}
