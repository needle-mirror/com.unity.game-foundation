using System;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying an individual Reward Item.
    /// </summary>
    [AddComponentMenu("Game Foundation/Reward Claim Button", 8)]
    [RequireComponent(typeof(RewardClaimManager), typeof(Button))]
    public class RewardClaimButton : MonoBehaviour
    {
        /// <summary>
        ///     The definition key of the Reward Item being displayed.
        /// </summary>
        public string rewardItemDefinitionKey => m_RewardItemDefinitionKey;

        [SerializeField]
        internal string m_RewardItemDefinitionKey;

        /// <summary>
        ///     The definition key of the Reward that the Reward Item being displayed belongs to.
        /// </summary>
        public string rewardDefinitionKey => m_RewardDefinitionKey;

        [SerializeField]
        internal string m_RewardDefinitionKey;

        /// <summary>
        /// </summary>
        public RewardItemDefinition rewardItemDefinition => m_RewardItemDefinition;

        RewardItemDefinition m_RewardItemDefinition;

        /// <summary>
        ///     Button component attached to the RewardClaimButton's parent.
        /// </summary>
        Button m_Button;

        /// <summary>
        ///     Specifies the public interactability of the claim button.
        /// </summary>
        bool m_Interactable = true;

        /// <summary>
        ///     Specifies whether the button is interactable internally.
        /// </summary>
        bool m_InteractableInternal = true;

        /// <summary>
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        bool m_IsDirty;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

        /// <summary>
        ///     Specifies whether the button is driven by other component.
        /// </summary>
        internal bool m_IsDrivenByOtherComponent;
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardClaimButton>();

        /// <summary>
        ///     Adds listeners, if the application is playing.
        /// </summary>
        void OnEnable()
        {
            GameFoundationSdk.initialized += RegisterEvents;
            GameFoundationSdk.initialized += InitializeComponentData;
            GameFoundationSdk.willUninitialize += UnregisterEvents;

            if (GameFoundationSdk.IsInitialized)
            {
                RegisterEvents();
            }
        }

        /// <summary>
        ///     Removes listeners, if the application is playing.
        /// </summary>
        void OnDisable()
        {
            GameFoundationSdk.initialized -= RegisterEvents;
            GameFoundationSdk.initialized -= InitializeComponentData;
            GameFoundationSdk.willUninitialize -= UnregisterEvents;

            if (GameFoundationSdk.IsInitialized)
            {
                UnregisterEvents();
            }
        }

        /// <summary>
        ///     Add necessary events for this view to Game Foundation.
        /// </summary>
        void RegisterEvents()
        {
            if (GameFoundationSdk.rewards == null)
                return;

            GameFoundationSdk.rewards.rewardItemClaimSucceeded += OnRewardItemClaimSucceeded;
            GameFoundationSdk.rewards.rewardItemClaimFailed += OnRewardItemClaimFailed;
            GameFoundationSdk.rewards.rewardItemClaimInitiated += OnRewardItemClaimInitiated;
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
        }

        /// <summary>
        ///     Initializes RewardClaimButton with needed info.
        /// </summary>
        /// <param name="rewardKey">
        ///     The key of the Reward that the Reward Item being displayed belongs to.
        ///     If the rewardDefinitionKey is null or empty an error will be logged and the RewardClaimButton will not be
        ///     initialized.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The key of the Reward Item being displayed.
        ///     If the rewardItemDefinitionKey is null or empty an error will be logged and the RewardClaimButton will not be
        ///     initialized.
        /// </param>
        internal void Init(string rewardKey, string rewardItemKey)
        {
            m_IsDrivenByOtherComponent = true;

            SetRewardItemDefinition(rewardKey, rewardItemKey);
        }

        /// <summary>
        ///     Gets the Button component of the PurchaseButton and sets the onClick listener to call <see cref="Purchase"/>.
        /// </summary>
        void Awake()
        {
            m_Button = GetComponent<Button>();

            if (Application.isPlaying && m_Button.onClick.GetPersistentEventCount() <= 0)
            {
                k_GFLogger.LogWarning("There are no onClick listeners attached to the RewardClaimButton named " + m_Button.name + " via the Inspector UI. This may cause unexpected behavior when trying to claim reward.");
            }
        }

        /// <summary>
        ///     Initializes RewardClaimButton before the first frame update if Game Foundation Sdk was already
        ///     initialized before RewardClaimButton was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        ///     If m_RewardDefinitionKey or m_RewardItemDefinitionKey is null or can't be found in the Reward Catalog or Reward it
        ///     will log error and return without setting button info.
        /// </summary>
        void Start()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Waiting for initialization.");
                SetButtonEnabledStateInternal(false);
                return;
            }

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_RewardItemDefinition is null)
            {
                InitializeComponentData();
            }
        }

        /// <summary>
        ///     Initializes RewardClaimButton data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            SetRewardItemDefinition(m_RewardDefinitionKey, m_RewardItemDefinitionKey);
        }

        /// <summary>
        ///     Sets the RewardItem that will be claimed when Claim method is called.
        /// </summary>
        /// <param name="rewardDefinitionKey">
        ///     The key of the Reward the RewardItem belongs to.
        ///     If null or empty, claim button will be disabled.
        /// </param>
        /// <param name="rewardItemDefinitionKey">
        ///     The key of the RewardItem to be displayed.
        ///     If null or empty, claim button will be disabled.
        /// </param>
        internal void SetRewardItemDefinition(string rewardDefinitionKey, string rewardItemDefinitionKey)
        {
            m_RewardDefinitionKey = rewardDefinitionKey;
            m_RewardItemDefinitionKey = rewardItemDefinitionKey;

            m_RewardItemDefinition = GetRewardItemDefinition(rewardDefinitionKey, rewardItemDefinitionKey);

            if (Application.isPlaying)
            {
                SetButtonEnabledStateInternal(m_RewardItemDefinition != null);
            }
        }

        /// <summary>
        ///     Finds Reward Item Definition in the Reward catalog.
        /// </summary>
        /// <param name="rewardDefinitionKey">
        ///     The definition key of Reward definition.
        /// </param>
        /// <param name="rewardItemDefinitionKey">
        ///     The definition key of Reward Item definition.
        /// </param>
        /// <returns>
        ///     A reference to Reward Item definition.
        /// </returns>
        RewardItemDefinition GetRewardItemDefinition(string rewardDefinitionKey, string rewardItemDefinitionKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(rewardDefinitionKey) || string.IsNullOrEmpty(rewardItemDefinitionKey))
                return null;

            var rewardDefinition = GameFoundationSdk.catalog?.Find<RewardDefinition>(rewardDefinitionKey);
            if (rewardDefinition == null)
            {
                if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning($"Reward \"{rewardDefinitionKey}\" doesn't exist in Reward catalog.");
                }

                return null;
            }

            var rewardItemDefinition = rewardDefinition.FindRewardItem(rewardItemDefinitionKey);
            if (rewardItemDefinition == null && m_ShowDebugLogs)
            {
                k_GFLogger.LogWarning($"No Reward Item with the key \" {rewardItemDefinitionKey}\" can be found in the Reward catalog.");
            }

            return rewardItemDefinition;
        }

        /// <summary>
        ///     Sets the RewardItem that will be claimed when Claim method is called.
        /// </summary>
        /// <param name="definition">
        ///     The runtime RewardItem that should be claimed when Claim() is called.
        ///     If null the Claim button will be disabled.
        /// </param>
        public void SetRewardItemDefinition(RewardItemDefinition definition)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetRewardItemDefinition)))
            {
                return;
            }

            m_RewardItemDefinition = definition;

            if (definition is null)
            {
                m_RewardDefinitionKey = null;
                m_RewardItemDefinitionKey = null;

                SetButtonEnabledStateInternal(false);
            }
            else
            {
                m_RewardDefinitionKey = definition.rewardDefinition?.key;
                m_RewardItemDefinitionKey = definition.key;

                SetButtonEnabledStateInternal(true);
            }
        }

        /// <summary>
        ///     Calls RewardManager.Claim with the RewardItem component has stored.
        /// </summary>
        public void Claim()
        {
            var rewardClaim = GetComponent<RewardClaimManager>();

            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(Claim))
                || m_RewardItemDefinition == null
                || rewardClaim.IsInQueue(m_RewardItemDefinition))
            {
                return;
            }

            SetButtonEnabledStateInternal(false);

            GetComponent<RewardClaimManager>().Claim(m_RewardItemDefinition);
        }

        /// <summary>
        ///     Sets whether the button component attached to RewardClaimButton's parent should be enabled or not.
        ///     Can be overridden by internal state like whether the button has a valid RewardItem key attached.
        /// </summary>
        /// <param name="enableState">
        ///     Whether or not to enable the button.
        /// </param>
        public void SetButtonEnabledState(bool enableState)
        {
            if (m_Interactable == enableState)
                return;

            m_Interactable = enableState;
            UpdateButtonStatus();
        }

        /// <summary>
        ///     Checks to see whether any properties have been changed (by checking <see cref="m_IsDirty"/>) and
        ///     if so, calls <see cref="UpdateContent"/> before resetting the flag.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;

                m_RewardItemDefinition = GetRewardItemDefinition(rewardDefinitionKey, rewardItemDefinitionKey);

                if (Application.isPlaying)
                {
                    SetButtonEnabledStateInternal(m_RewardItemDefinition != null);
                }
            }
        }

        /// <summary>
        ///     Sets whether the button component attached to RewardClaimButton's parent should be enabled or not.
        /// </summary>
        /// <param name="enableState">
        ///     Whether or not to enable the button.
        /// </param>
        void SetButtonEnabledStateInternal(bool enableState)
        {
            if (m_RewardDefinitionKey == null || m_RewardItemDefinitionKey == null)
            {
                m_InteractableInternal = false;
            }
            else
            {
                m_InteractableInternal = enableState;
            }

            UpdateButtonStatus();
        }

        /// <summary>
        ///     Updates button status according to user defined setting and internal status like whether the Reward is in
        ///     a claimable state.
        /// </summary>
        void UpdateButtonStatus()
        {
            if (m_Button is null)
                return;

            m_Button.interactable = m_Interactable && m_InteractableInternal;
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
            if (m_RewardItemDefinition != null && reward.key == m_RewardDefinitionKey && rewardItemKey == m_RewardItemDefinitionKey)
            {
                SetButtonEnabledStateInternal(true);
            }
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
            if (m_RewardItemDefinition != null && rewardKey == m_RewardDefinitionKey && rewardItemKey == m_RewardItemDefinitionKey)
            {
                SetButtonEnabledStateInternal(true);
            }
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
            if (m_RewardItemDefinition != null && rewardKey == m_RewardDefinitionKey && rewardItemKey == m_RewardItemDefinitionKey)
            {
                SetButtonEnabledStateInternal(false);
            }
        }

        /// <summary>
        ///     When changes are made via the Inspector, trigger <see cref="UpdateContent"/>
        /// </summary>
        void OnValidate()
        {
            m_IsDirty = true;
        }
    }
}
