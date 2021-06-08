using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages claiming Reward Items.
    /// </summary>
    public class RewardClaimManager : MonoBehaviour
    {
        /// <summary>
        ///     Callback that will get triggered if a Reward Item claim completes successfully.
        /// </summary>
        public RewardClaimSuccessEvent onRewardItemClaimSucceeded;

        /// <summary>
        ///     Callback that will get triggered if a Reward Item claim fails.
        /// </summary>
        public RewardClaimFailedEvent onRewardItemClaimFailed;

        /// <summary>
        ///     Callback that will get triggered when a Reward Item claim is initiated.
        /// </summary>
        public RewardItemClaimInitiatedEvent onRewardItemClaimInitiated;

        /// <summary>
        ///     Callback that will get triggered when a Reward Item claim progresses.
        /// </summary>
        public RewardItemClaimProgressedEvent onRewardItemClaimProgressed;

        /// <summary>
        ///     A callback for when a Reward Item claim is completed successfully. Wraps UnityEvent and accepts
        ///     Reward, string (the RewardItemKey) and Payout as parameters.
        /// </summary>
        [Serializable]
        public class RewardClaimSuccessEvent : UnityEvent<Reward, string, Payout> { }

        /// <summary>
        ///     A callback for when a Reward Item claim fails. Wraps UnityEvent and accepts two strings (the rewardDefinitionKey
        ///     and rewardItemDefinitionKey) and Exception as parameters.
        /// </summary>
        [Serializable]
        public class RewardClaimFailedEvent : UnityEvent<string, string, Exception> { }

        /// <summary>
        ///     A callback for when a Reward Item claim is initiated. Wraps UnityEvent and accepts two strings
        ///     as parameters (the rewardDefinitionKey and the rewardItemDefinitionKey).
        /// </summary>
        [Serializable]
        public class RewardItemClaimInitiatedEvent : UnityEvent<string, string> { }

        /// <summary>
        ///     A callback for when a Reward Item claim progresses. Wraps UnityEvent and accepts Reward, string (the
        ///     rewardItemDefinitionKey), int (the current step, zero-indexed) and int (the total number of steps) as parameters.
        /// </summary>
        [Serializable]
        public class RewardItemClaimProgressedEvent : UnityEvent<Reward, string, int, int> { }

        /// <summary>
        ///     The queue for reward item definitions that are going to be initiated.
        /// </summary>
        Queue<RewardItemDefinition> m_RewardItemsToClaim = new Queue<RewardItemDefinition>();

        /// <summary>
        ///     The reward item definition that is currently initiated.
        ///     If null, no reward item is initiated at this moment.
        /// </summary>
        RewardItemDefinition m_CurrentRewardItem;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardClaimManager>();

        /// <summary>
        ///     Adds listeners to the RewardManager's claim success, failure and state changed events, if the application is
        ///     playing.
        /// </summary>
        void OnEnable()
        {
            GameFoundationSdk.initialized += OnGameFoundationInitialized;
            GameFoundationSdk.willUninitialize += OnGameFoundationWillUninitialize;

            if (GameFoundationSdk.IsInitialized)
            {
                RegisterEvents();
            }
        }

        /// <summary>
        ///     Removes listeners to the RewardManager's claim success, failure and state changed events, if the application is
        ///     playing.
        /// </summary>
        void OnDisable()
        {
            GameFoundationSdk.initialized -= OnGameFoundationInitialized;
            GameFoundationSdk.willUninitialize -= OnGameFoundationWillUninitialize;

            if (GameFoundationSdk.IsInitialized)
            {
                UnregisterEvents();
            }
        }

        /// <summary>
        ///     Add necessary events for this manager to Game Foundation.
        /// </summary>
        void RegisterEvents()
        {
            if (GameFoundationSdk.rewards == null)
                return;

            GameFoundationSdk.rewards.rewardItemClaimSucceeded += OnRewardItemClaimSucceeded;
            GameFoundationSdk.rewards.rewardItemClaimFailed += OnRewardItemClaimFailed;
            GameFoundationSdk.rewards.rewardItemClaimInitiated += OnRewardItemClaimInitiated;
            GameFoundationSdk.rewards.rewardItemClaimProgressed += OnRewardItemClaimProgressed;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            if (GameFoundationSdk.rewards == null)
                return;

            GameFoundationSdk.rewards.rewardItemClaimSucceeded -= OnRewardItemClaimSucceeded;
            GameFoundationSdk.rewards.rewardItemClaimFailed -= OnRewardItemClaimFailed;
            GameFoundationSdk.rewards.rewardItemClaimInitiated -= OnRewardItemClaimInitiated;
            GameFoundationSdk.rewards.rewardItemClaimProgressed -= OnRewardItemClaimProgressed;
        }

        /// <summary>
        ///     Claims a Reward Item.
        ///     If already a Reward Item is in the claim progress, it adds the new Reward Item to the queue.
        ///     If there is no Reward Item in the queue, it initiates a claim on the Reward Item.
        /// </summary>
        /// <param name="definition">
        ///     The reference to definition of Reward Item.
        /// </param>
        /// <returns>
        ///     <c>true</c> if successfully claimed, otherwise <c>false</c>.
        /// </returns>
        public bool Claim(RewardItemDefinition definition)
        {
            if (definition == null || IsInQueue(definition))
            {
                return false;
            }

            if (GameFoundationSdk.IsInitialized && m_RewardItemsToClaim.Count == 0)
            {
                InitiateClaimInCoroutine(definition);
            }
            else
            {
                m_RewardItemsToClaim.Enqueue(definition);
            }

            return true;
        }

        /// <summary>
        ///     Specifies whether the Reward Item is in the queue.
        /// </summary>
        /// <param name="definition">
        ///     The reference to Reward Item definition.
        /// </param>
        /// <returns>
        ///     Returns whether the Reward Item is in the queue.
        /// </returns>
        public bool IsInQueue(RewardItemDefinition definition)
        {
            return m_RewardItemsToClaim.Contains(definition) || ReferenceEquals(m_CurrentRewardItem, definition);
        }

        /// <summary>
        ///     Initiates a claim on Reward Item.
        /// </summary>
        /// <param name="definition">
        ///     A reference to Reward Item Definition to initiate a claim on.
        /// </param>
        void InitiateClaimInCoroutine(RewardItemDefinition definition)
        {
            m_CurrentRewardItem = definition;

            StartCoroutine(InitiateClaim(definition));
        }

        /// <summary>
        ///     Initiates a claim on the next Reward Item.
        /// </summary>
        void InitiateClaimNextReward()
        {
            if (GameFoundationSdk.IsInitialized && m_RewardItemsToClaim.Count > 0)
            {
                InitiateClaimInCoroutine(m_RewardItemsToClaim.Dequeue());
            }
        }

        /// <summary>
        ///     Calls RewardManager.InitiateClaimInCoroutine with the RewardItem parameter.
        /// </summary>
        /// <param name="rewardItem">
        ///     The reference to a RewardItem to initiate a claim on.
        /// </param>
        IEnumerator InitiateClaim(RewardItemDefinition rewardItem)
        {
            using (var deferred = GameFoundationSdk.rewards.Claim(rewardItem.rewardDefinition, rewardItem.key))
            {
                if (m_ShowDebugLogs)
                {
                    k_GFLogger.Log($"Now processing claim for: {rewardItem.key}");
                }

                int currentStep = 0;

                // wait for the reward to be processed
                while (!deferred.isDone)
                {
                    // keep track of the current step and possibly show a progress UI
                    if (deferred.currentStep != currentStep)
                    {
                        currentStep = deferred.currentStep;

                        if (m_ShowDebugLogs)
                        {
                            k_GFLogger.Log($"Reward is now on step {currentStep} of {deferred.totalSteps}");
                        }
                    }

                    yield return null;
                }

                // now that the claim has been processed, check for an error
                if (!deferred.isFulfilled)
                {
                    if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogError($"Reward Item Key: \"{rewardItem.key}\" - Error Message: {deferred.error}");
                    }

                    yield break;
                }

                // here we can assume success
                if (m_ShowDebugLogs)
                {
                    k_GFLogger.Log("The reward item was claimed successfully in both the platform and the data layer!");

                    foreach (var tradable in deferred.result.products)
                    {
                        if (tradable is CurrencyExchange currencyExchange)
                        {
                            k_GFLogger.Log($"Player was awarded {currencyExchange.amount} of currency '{{currencyExchange.currency.displayName}}'");
                        }
                        else if (tradable is InventoryItem inventoryItem)
                        {
                            k_GFLogger.Log($"Player was awarded 1 of Inventory Item '{inventoryItem.definition.displayName}'");
                        }
                    }
                }

                m_CurrentRewardItem = null;
            }
        }

        /// <summary>
        ///     When Game Foundation is initialized, it initiate a claim on a reward item if there is.
        /// </summary>
        void OnGameFoundationInitialized()
        {
            RegisterEvents();

            InitiateClaimNextReward();
        }

        /// <summary>
        ///     When Game Foundation is uninitialized, it reset the state of the queue.
        /// </summary>
        void OnGameFoundationWillUninitialize()
        {
            UnregisterEvents();

            m_CurrentRewardItem = null;
            m_RewardItemsToClaim.Clear();
        }

        /// <summary>
        ///     Gets triggered when a Reward Item is successfully claimed. Triggers the
        ///     user-specified onRewardItemClaimSucceeded callback.
        /// </summary>
        /// <param name="reward">
        ///     The Reward whose claim has progressed to the next step.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the Reward Item whose claim has progressed to the next step.
        /// </param>
        /// <param name="payout">
        ///     The payout element of the Reward Item.
        /// </param>
        void OnRewardItemClaimSucceeded(Reward reward, string rewardItemKey, Payout payout)
        {
            onRewardItemClaimSucceeded.Invoke(reward, rewardItemKey, payout);

            InitiateClaimNextReward();
        }

        /// <summary>
        ///     Gets triggered when claiming a Reward Item is attempted and fails. Triggers the
        ///     user-specified onRewardItemClaimFailed callback.
        /// </summary>
        /// <param name="rewardKey">
        ///     The key of the Reward whose claim has progressed to the next step.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the Reward Item whose claim has progressed to the next step.
        /// </param>
        /// <param name="exception">
        ///     The Exception that caused the claiming to fail.
        /// </param>
        void OnRewardItemClaimFailed(string rewardKey, string rewardItemKey, Exception exception)
        {
            onRewardItemClaimFailed.Invoke(rewardKey, rewardItemKey, exception);

            InitiateClaimNextReward();
        }

        /// <summary>
        ///     Gets triggered when a new Reward Item claim is initiated. Triggers the
        ///     user-specified onRewardItemClaimInitiated callback.
        /// </summary>
        /// <param name="rewardKey">
        ///     The key of the Reward whose claim has progressed to the next step.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the Reward Item whose claim has progressed to the next step.
        /// </param>
        void OnRewardItemClaimInitiated(string rewardKey, string rewardItemKey)
        {
            onRewardItemClaimInitiated.Invoke(rewardKey, rewardItemKey);
        }

        /// <summary>
        ///     Gets triggered when the Reward Item claim progresses. Triggers the
        ///     user-specified onRewardItemClaimProgressed callback.
        /// </summary>
        /// <param name="reward">
        ///     The Reward whose claim has progressed to the next step.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the Reward Item whose claim has progressed to the next step.
        /// </param>
        /// <param name="current">
        ///     The current step of the claim process (0-indexed).
        /// </param>
        /// <param name="total">
        ///     The total number of steps to complete a claim.
        /// </param>
        void OnRewardItemClaimProgressed(Reward reward, string rewardItemKey, int current, int total)
        {
            onRewardItemClaimProgressed.Invoke(reward, rewardItemKey, current, total);
        }
    }
}
