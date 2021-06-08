using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying a Reward's popup, including schedule of rewards with a button to claim a Reward
    ///     Item.
    /// </summary>
    [AddComponentMenu("Game Foundation/Reward Popup View", 6)]
    [ExecuteInEditMode]
    public class RewardPopupView : MonoBehaviour
    {
        /// <summary>
        ///     Provides reusable list of <see cref="RewardItemObject"/> instances.
        /// </summary>
        static readonly Pool<List<RewardItemObject>> s_RewardItemListPool = new Pool<List<RewardItemObject>>(
            () => new List<RewardItemObject>(),
            list => list.Clear());

        /// <summary>
        ///     The key of the Reward being displayed.
        /// </summary>
        public string rewardKey => m_RewardKey;

        [SerializeField]
        internal string m_RewardKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting description text.
        ///     If there is no Static Property with that key on the Reward, no description will be displayed.
        /// </summary>
        public string descriptionPropertyKey => m_DescriptionPropertyKey;

        [SerializeField]
        [Space]
        internal string m_DescriptionPropertyKey;

        /// <summary>
        ///     The Static Property key that should be used to get the title to display for each Reward Item.
        ///     If there is no Static Property with that key on the Reward, no item titles will be displayed.
        ///     Suggested format for the string value the key maps to would be something like "Day {0}" where {0} will
        ///     be replaced with the count + 1 of the item in the Reward's list.
        /// </summary>
        public string rewardItemTitlePropertyKey => m_RewardItemTitlePropertyKey;

        [SerializeField]
        internal string m_RewardItemTitlePropertyKey;

        /// <summary>
        ///     The Static Property key string that should be used for getting the image of each item in the Payout of a Reward
        ///     Item
        ///     for displaying in the RewardItemPrefabs. The key must exist on each inventory item or currency added as a payout.
        /// </summary>
        public string payoutItemIconSpritePropertyKey => m_PayoutItemIconSpritePropertyKey;

        [SerializeField]
        internal string m_PayoutItemIconSpritePropertyKey;

        /// <summary>
        ///     The format to display the countdown in.
        ///     Look at https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings and
        ///     https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings for lists of
        ///     valid formats.
        /// </summary>
        public string countdownDisplayFormat => m_CountdownDisplayFormat;

        [SerializeField]
        internal string m_CountdownDisplayFormat = kDisplayFormat;

        /// <summary>
        ///     The string description to display on the countdown, if any, during a cooldown period when no Reward Items are
        ///     claimable.
        ///     Unless null or empty, it will overwrite any description specified on the countdown prefab.
        /// </summary>
        public string countdownCooldownDescription => m_CountdownCooldownDescription;

        [SerializeField]
        [Space]
        internal string m_CountdownCooldownDescription = "Next Reward In:";

        /// <summary>
        ///     The string description to display on the countdown, if any, during an expiration period when a Reward Item is
        ///     claimable
        ///     for a certain amount of time.
        ///     Unless null or empty, it will overwrite any description specified on the countdown prefab.
        /// </summary>
        public string countdownExpirationDescription => m_CountdownExpirationDescription;

        [SerializeField]
        internal string m_CountdownExpirationDescription = "Expires In:";

        /// <summary>
        ///     The TextMeshProUGUI field to display the countdown time in.
        ///     If null no countdown will be displayed.
        /// </summary>
        public TextMeshProUGUI countdownTextField => m_CountdownTextField;

        [SerializeField]
        internal TextMeshProUGUI m_CountdownTextField;

        /// <summary>
        ///     The RewardItemView prefab to use to display rewards that are currently locked but will be claimable in the future.
        ///     It will also be used as the default prefab in the case that any of the special case prefabs are not provided.
        /// </summary>
        public RewardItemView lockedRewardItemPrefab => m_LockedRewardItemPrefab;

        [SerializeField]
        [Space]
        internal RewardItemView m_LockedRewardItemPrefab;

        /// <summary>
        ///     The RewardItemView prefab to use when displaying the claimable Reward Items.
        /// </summary>
        public RewardItemView claimableRewardItemPrefab => m_ClaimableRewardItemPrefab;

        [SerializeField]
        internal RewardItemView m_ClaimableRewardItemPrefab;

        /// <summary>
        ///     The RewardItemView prefab to use when displaying the previously claimed Reward Items.
        /// </summary>
        public RewardItemView claimedRewardItemPrefab => m_ClaimedRewardItemPrefab;

        [SerializeField]
        internal RewardItemView m_ClaimedRewardItemPrefab;

        /// <summary>
        ///     The RewardItemView prefab to use when displaying any previously available Reward Items that were not claimed.
        ///     This state is only possible when a Reward has an expiration time, has multiple Reward Items in the list,
        ///     and has resetIfExpired set to false.
        /// </summary>
        public RewardItemView missedRewardItemPrefab => m_MissedRewardItemPrefab;

        [SerializeField]
        internal RewardItemView m_MissedRewardItemPrefab;

        /// <summary>
        ///     The RewardClaimButton that will be used to initiate claims, if any.
        /// </summary>
        public RewardClaimButton claimButton => m_ClaimButton;

        [SerializeField]
        internal RewardClaimButton m_ClaimButton;

        /// <summary>
        ///     The GameObject for close button.
        /// </summary>
        public Transform closeButton => m_CloseButton;

        [SerializeField]
        internal Transform m_CloseButton;

        /// <summary>
        ///     Specifies whether the close button is invisible when there is claimable Reward Item.
        /// </summary>
        public bool autoHideCloseButton => m_AutoHideCloseButton;
        
        [SerializeField]
        internal bool m_AutoHideCloseButton = true;

        /// <summary>
        ///     The TextMeshProUGUI field to assign the Reward's displayName to.
        /// </summary>
        public TextMeshProUGUI titleTextField => m_TitleTextField;

        [SerializeField]
        [Space]
        internal TextMeshProUGUI m_TitleTextField;

        /// <summary>
        ///     The TextMeshProUGUI field to assign the description to.
        /// </summary>
        public TextMeshProUGUI descriptionTextField => m_DescriptionTextField;

        [SerializeField]
        internal TextMeshProUGUI m_DescriptionTextField;

        /// <summary>
        ///     The Transform in which to auto populate Reward Items.
        /// </summary>
        public Transform autoPopulatedRewardItemContainer => m_AutoPopulatedRewardItemContainer;

        [SerializeField]
        internal Transform m_AutoPopulatedRewardItemContainer;

        /// <summary>
        ///     Callback that will get triggered when the Reward Popup is opened.
        /// </summary>
        [Space]
        public PopupOpenedEvent onPopupOpened;

        /// <summary>
        ///     Callback that will get triggered when the Reward Popup is closed.
        /// </summary>
        public PopupClosedEvent onPopupClosed;

        /// <summary>
        ///     Callback that will get triggered when a Reward Item is claimed.
        /// </summary>
        public RewardClaimedEvent onRewardClaimed;
        
        /// <summary>
        ///     Callback that will get triggered when the state of the Reward changes.
        /// </summary>
        public RewardStateChangedEvent onRewardStateChanged;

        /// <summary>
        ///     A callback for when the Reward Popup opens. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupOpenedEvent : UnityEvent { }

        /// <summary>
        ///     A callback for when the Reward Popup closes. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupClosedEvent : UnityEvent { }

        /// <summary>
        ///     A callback for when the state of the Reward Items changes. Wraps UnityEvent and accepts
        ///     Reward as a parameter.
        /// </summary>
        [Serializable]
        public class RewardStateChangedEvent : UnityEvent<Reward> { }
        
        /// <summary>
        ///     A callback for when a Reward Items claimed. Wraps UnityEvent and accepts
        ///     Reward and Reward Item Key as parameters.
        /// </summary>
        [Serializable]
        public class RewardClaimedEvent : UnityEvent<Reward, RewardItemDefinition> { }

        /// <summary>
        ///     The format string to give to RewardItemView to use when creating Reward Item titles.
        /// </summary>
        string m_RewardItemTitleFormat;

        /// <summary>
        ///     A reference to the Reward instance specified by m_RewardDefinitionKey. Only valid at runtime.
        /// </summary>
        Reward m_Reward;

        /// <summary>
        ///     Returns the runtime instance of the Reward being displayed by the popup.
        /// </summary>
        public Reward reward => m_Reward;

        /// <summary>
        ///     The time since the last update to countdown and Reward state has happened. These updates should only happen once
        ///     per second.
        /// </summary>
        float m_TimeSinceLastCountdownUpdate;

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
        ///     Specifies whether the Countdown fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showAutoPopRewardItemsEditorFields = true;

        /// <summary>
        ///     Specifies whether the Countdown fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showCountdownEditorFields = true;

        /// <summary>
        ///     Specifies whether the Text fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showTextEditorFields = true;

        /// <summary>
        ///     Specifies whether the Button fields on the editor are visible.
        /// </summary>
        [SerializeField]
        internal bool showButtonEditorFields = true;

        /// <summary>
        ///     A name to use when generating PayoutItem GameObjects under Auto Generated Image Container.
        /// </summary>
        const string kRewardItemGameObjectName = "RewardItem";

        /// <summary>
        ///     The default format to display the countdown time in.
        /// </summary>
        const string kDisplayFormat = "hh\\:mm\\:ss";
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardPopupView>();

        /// <summary>
        ///     Clears any RewardItems that might be cached at runtime if GameFoundationSdk is not initialized.
        /// </summary>
        private void Awake()
        {
            if (Application.isPlaying && !GameFoundationSdk.IsInitialized)
            {
                ClearRewardItemPrefabs();
            }
        }

        /// <summary>
        ///     Adds listeners to the RewardManager's claim success, failure and state changed events, if the application is
        ///     playing.
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

            if (!(m_Reward is null))
            {
                UpdateContent();
            }
        }

        /// <summary>
        ///     Removes listeners from the RewardManager's claim success, failure and state changed events, if the application is
        ///     playing.
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
            Reward.rewardStateChanged += OnRewardStateChanged;

            if (GameFoundationSdk.rewards == null)
                return;

            GameFoundationSdk.rewards.rewardItemClaimSucceeded += OnRewardClaimSucceeded;
        }

        /// <summary>
        ///     Removes the events for this view from Game Foundation.
        /// </summary>
        void UnregisterEvents()
        {
            Reward.rewardStateChanged -= OnRewardStateChanged;

            if (GameFoundationSdk.rewards == null)
                return;

            GameFoundationSdk.rewards.rewardItemClaimSucceeded -= OnRewardClaimSucceeded;
        }

        /// <summary>
        ///     Initializes RewardPopupView before the first frame update if Game Foundation Sdk was already
        ///     initialized before RewardPopupView was enabled, otherwise sets content to a blank state in order
        ///     to wait for Game Foundation Sdk to initialize.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>) if the prefab is active in hierarchy at start.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Waiting for initialization.");
                m_IsDirty = true;
                return;
            }

            // This is to catch the case where Game Foundation initialized before OnEnable added the GameFoundationSdk initialize listener.
            if (GameFoundationSdk.IsInitialized && m_Reward is null)
            {
                InitializeComponentData();
            }
        }
        
        /// <summary>
        ///     Initializes RewardPopupView data from Game Foundation Sdk.
        /// </summary>
        void InitializeComponentData()
        {
            if (!Application.isPlaying)
                return;

            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the key of the Reward displayed in this Reward Popup
        /// </summary>
        /// <param name="rewardKey">
        ///     The key of the Reward to be displayed.
        /// </param>
        internal void SetRewardKey(string rewardKey)
        {
            m_RewardKey = rewardKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Finds the Reward instance that is in the Reward Manager
        /// </summary>
        /// <param name="rewardKey">The key of the Reward to be found.</param>
        /// <returns>Reward instance</returns>
        Reward GetReward(string rewardKey)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(rewardKey))
                return null;

            var reward = GameFoundationSdk.rewards?.FindReward(rewardKey);

            if (reward != null || !m_ShowDebugLogs)
                return reward;


            k_GFLogger.LogWarning($"No Reward with the key \"{rewardKey}\" can be found in the reward manager.");
            return null;
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
        RewardItemDefinition GetRewardItemDefinition(Reward reward, string rewardItemDefinitionKey)
        {
            if (!Application.isPlaying || reward == null || string.IsNullOrEmpty(rewardItemDefinitionKey))
                return null;
            
            var rewardItemDefinition = reward.rewardDefinition?.FindRewardItem(rewardItemDefinitionKey);
            if (rewardItemDefinition == null && m_ShowDebugLogs)
            {
                k_GFLogger.LogWarning($"No Reward Item with the key \" {rewardItemDefinitionKey}\" can be found in the Reward catalog.");
            }

            return rewardItemDefinition;
        }

        /// <summary>
        ///     Sets the Reward displayed in this Reward Popup
        /// </summary>
        /// <param name="reward">
        ///     The Reward to be displayed.
        /// </param>
        public void SetReward(Reward reward)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetReward)))
            {
                return;
            }

            if (m_RewardKey == reward?.key)
            {
                return;
            }

            m_RewardKey = reward?.key;
            m_Reward = reward;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Displays the Reward Popup.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        public void Open()
        {
            if (!Application.isPlaying)
                return;

            Open(m_Reward ?? GetReward(m_RewardKey));
        }

        /// <summary>
        ///     Displays the Reward Popup.
        ///     Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        /// <param name="reward">
        ///     The Reward being displayed.
        /// </param>
        public void Open(Reward reward)
        {
            if (!Application.isPlaying)
                return;

            if (gameObject.activeInHierarchy)
            {
                return;
            }
            
            if (!GameFoundationSdk.IsInitialized)
            {
                k_GFLogger.Log("Reward Popup has been opened when Game Foundation Sdk is not initialized. Content will be blank until Game Foundation initializes and no changes to state have been made.");
                m_Reward = null;
            }
            else
            {
                if (reward is null)
                {
                    k_GFLogger.LogWarning("Reward is null.");
                }

                m_RewardKey = reward?.key;
                m_Reward = reward;
            }

            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here because the popup is
            // set to active in the same frame as it becomes dirty and it can cause a flicker.
            UpdateContent();

            gameObject.SetActive(true);

            if (gameObject.activeInHierarchy)
            {
                onPopupOpened?.Invoke();
            }
        }

        /// <summary>
        ///     Hides the Reward Popup.
        ///     Will trigger a PopupClosedEvent (<see cref="PopupClosedEvent"/>).
        /// </summary>
        public void Close()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            gameObject.SetActive(false);
            onPopupClosed?.Invoke();
        }

        /// <summary>
        ///     Sets the key to use when getting a Reward's description from its Static Properties.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up description by.
        ///     If the key is not found in the Reward's Static Properties no description will be displayed.
        /// </param>
        public void SetDescriptionPropertyKey(string propertyKey)
        {
            if (string.Equals(m_DescriptionPropertyKey, propertyKey))
                return;

            m_DescriptionPropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the key to use when getting the images of the Inventory and Currency items that are paid out in
        ///     this Reward. Each item should have this key in its Static Properties.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up image by.
        ///     Any Inventory or Currency Items that don't have this key in their Static Properties will not display an image.
        /// </param>
        public void SetPayoutItemIconSpritePropertyKey(string propertyKey)
        {
            if (string.Equals(m_PayoutItemIconSpritePropertyKey, propertyKey))
                return;

            m_PayoutItemIconSpritePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the key to use when getting the title to display for each Reward Item from the Reward's Static Properties.
        ///     Suggested format for the string value the key maps to would be something like "Day {0}" where {0} will
        ///     be replaced with the count + 1 of the item in the Reward's list.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up Reward Item title by.
        ///     If the key is not found in the Reward's Static Properties no title will be displayed.
        /// </param>
        public void SetRewardItemTitlePropertyKey(string propertyKey)
        {
            if (string.Equals(m_RewardItemTitlePropertyKey, propertyKey))
                return;

            m_RewardItemTitlePropertyKey = propertyKey;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the container in which to auto populate Reward Items.
        /// </summary>
        /// <param name="container">The transform to use as a container to populate Reward Items.</param>
        public void SetAutoPopulatedRewardItemContainer(Transform container)
        {
            if (container == m_AutoPopulatedRewardItemContainer)
                return;

            m_AutoPopulatedRewardItemContainer = container;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the prefab to use when displaying a Reward's future claimable items. It is also used as the fallback
        ///     for any of the other Reward Item prefab fields, if they are null.
        /// </summary>
        /// <param name="lockedItemPrefab">
        ///     The prefab to use when displaying future claimable Reward Items and any other Reward Items that don't have
        ///     their own prefab specified.
        ///     If null, future Reward Items and any others defaulting to use this prefab will not be displayed.
        /// </param>
        public void SetLockedRewardItemPrefab(RewardItemView lockedItemPrefab)
        {
            if (m_LockedRewardItemPrefab == lockedItemPrefab)
                return;

            m_LockedRewardItemPrefab = lockedItemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the prefab to use when displaying the currently claimable Reward Item.
        /// </summary>
        /// <param name="claimableItemPrefab">
        ///     The prefab to use when displaying the currently claimable Reward Item.
        ///     If null, Locked Reward Item Prefab will be displayed.
        /// </param>
        public void SetClaimableRewardItemPrefab(RewardItemView claimableItemPrefab)
        {
            if (m_ClaimableRewardItemPrefab == claimableItemPrefab)
                return;

            m_ClaimableRewardItemPrefab = claimableItemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the prefab to use when displaying the Reward Items that have already been successfully claimed.
        /// </summary>
        /// <param name="claimedItemPrefab">
        ///     The prefab to use when displaying already claimed Reward Items.
        ///     If null, Locked Reward Item Prefab will be displayed.
        /// </param>
        public void SetClaimedRewardItemPrefab(RewardItemView claimedItemPrefab)
        {
            if (m_ClaimedRewardItemPrefab == claimedItemPrefab)
                return;

            m_ClaimedRewardItemPrefab = claimedItemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the prefab to use when displaying the previously available Reward Items that were not successfully
        ///     claimed.
        /// </summary>
        /// <param name="missedItemPrefab">
        ///     The prefab to use when displaying the missed Reward Items.
        ///     If null, Locked Reward Item Prefab will be displayed.
        /// </param>
        public void SetMissedRewardItemPrefab(RewardItemView missedItemPrefab)
        {
            if (m_MissedRewardItemPrefab == missedItemPrefab)
                return;

            m_MissedRewardItemPrefab = missedItemPrefab;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the RewardClaimButton that will be used for initiating claims.
        ///     RewardPopupView will update the claim button whenever the claimable RewardItem changes.
        /// </summary>
        /// <param name="claimButton">
        ///     The RewardClaimButton that will be used for initiating claims.
        ///     Handles null values gracefully, and claiming Reward Items will have to be done a different way.
        /// </param>
        public void SetClaimButton(RewardClaimButton claimButton)
        {
            if (m_ClaimButton == claimButton)
                return;

            m_ClaimButton = claimButton;
            UpdateClaimButton();
        }

        /// <summary>
        ///     Sets the RewardClaimButton that will be used for initiating claims.
        ///     RewardPopupView will update the claim button whenever the claimable RewardItem changes.
        /// </summary>
        /// <param name="closeButton">
        ///     The RewardClaimButton that will be used for initiating claims.
        ///     Handles null values gracefully, and claiming Reward Items will have to be done a different way.
        /// </param>
        public void SetCloseButton(Transform closeButton)
        {
            if (m_CloseButton == closeButton)
                return;

            m_CloseButton = closeButton;
            UpdateCloseButton();
        }

        /// <summary>
        ///     Sets the TextMeshProUGUI field in which to display the Reward's displayName.
        /// </summary>
        /// <param name="nameField">
        ///     The TextMeshProUGUI field in which to display the Reward's displayName.
        ///     If null, no title will be displayed.
        /// </param>
        public void SetTitleTextField(TextMeshProUGUI nameField)
        {
            if (m_TitleTextField == nameField)
                return;

            m_TitleTextField = nameField;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the TextMeshProUGUI field in which to display the Reward's description.
        /// </summary>
        /// <param name="descriptionField">
        ///     The TextMeshProUGUI field in which to display the Reward's description.
        ///     If null, no description will be displayed.
        /// </param>
        public void SetDescriptionTextField(TextMeshProUGUI descriptionField)
        {
            if (m_DescriptionTextField == descriptionField)
                return;

            m_DescriptionTextField = descriptionField;
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the string that designates the format in which to display the countdown timer.
        /// </summary>
        /// <param name="countdownFormat">
        ///     The string format for display.
        ///     See https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings and
        ///     https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings for lists of
        ///     valid formats.
        /// </param>
        public void SetCountdownDisplayFormat(string countdownFormat)
        {
            if (string.Equals(countdownFormat, m_CountdownDisplayFormat))
                return;

            m_CountdownDisplayFormat = countdownFormat;
            UpdateCountdown();
        }

        /// <summary>
        ///     Sets the string description to display on the countdown during a cooldown period when no Reward Items are
        ///     claimable.
        ///     Will not change visible description until the next cooldown period.
        /// </summary>
        /// <param name="description">
        ///     The string to display in the countdown description during a cooldown period when no Reward Items are claimable.
        ///     Will overwrite any description specified on the countdown prefab, unless null or empty.
        /// </param>
        public void SetCountdownCooldownDescription(string description)
        {
            if (string.Equals(m_CountdownCooldownDescription, description))
                return;

            m_CountdownCooldownDescription = description;
            UpdateCountdown();
        }

        /// <summary>
        ///     Sets the string description to display on the countdown during an expiration period when a Reward Item is claimable
        ///     for a certain amount of time.
        ///     Will not change visible description until the next expiration period.
        /// </summary>
        /// <param name="description">
        ///     The string to display in the countdown description during an expiration period when a Reward Item is claimable
        ///     for a certain amount of time.
        ///     Will overwrite any description specified on the countdown prefab, unless null or empty.
        /// </param>
        public void SetCountdownExpirationDescription(string description)
        {
            if (string.Equals(m_CountdownExpirationDescription, description))
                return;

            m_CountdownExpirationDescription = description;
            UpdateCountdown();
        }

        /// <summary>
        ///     Sets the TextMeshProUGUI field to use when displaying the countdown time.
        /// </summary>
        /// <param name="countdownField">
        ///     The TextMeshProUGUI field to use when displaying the countdown time.
        ///     If null, no countdown will be displayed.
        /// </param>
        public void SetCountdownTimerField(TextMeshProUGUI countdownField)
        {
            if (countdownField != m_CountdownTextField)
                return;

            m_CountdownTextField = countdownField;
            UpdateCountdown();
        }

        /// <summary>
        ///     Once per frame updates the content displayed in the Reward Popup.
        /// </summary>
        void LateUpdate()
        {
            if (!GameFoundationSdk.IsInitialized)
                return;

            m_TimeSinceLastCountdownUpdate += Time.deltaTime;
            if (m_TimeSinceLastCountdownUpdate >= 1f)
            {
                m_TimeSinceLastCountdownUpdate = 0f;
                UpdateCountdown();
            }
        }

        /// <summary>
        ///     Updates the countdown with new time and description, if one exists and is relevant in the current state of the
        ///     Reward.
        /// </summary>
        void UpdateCountdown()
        {
            if (m_CountdownTextField is null)
                return;

            if (Application.isPlaying && !GameFoundationSdk.IsInitialized)
            {
                m_CountdownTextField.text = string.Empty;
                return;
            }

            if (Application.isPlaying && !(m_Reward is null))
            {
                m_Reward.Update();

                var countdownText = string.Empty;
                var isInCooldown = m_Reward.IsInCooldown();
                var definition = m_Reward.rewardDefinition;

                if (!(definition is null) &&
                    (isInCooldown && definition.cooldownSeconds > 0 ||
                     !isInCooldown && definition.expirationSeconds > 0) &&
                    !float.IsPositiveInfinity(m_Reward.countdownSeconds))
                {
                    countdownText =
                        $"{(isInCooldown ? m_CountdownCooldownDescription : m_CountdownExpirationDescription)} {GetCountdownString((int) m_Reward.countdownSeconds)}";
                }

                m_CountdownTextField.text = countdownText;
            }
            else
            {
                var countdownText = $"{m_CountdownCooldownDescription} {GetCountdownString(0)}";
                if (m_CountdownTextField.text != countdownText)
                {
                    m_CountdownTextField.text = countdownText;
                }
            }
        }

        /// <summary>
        ///     Generates formatted countdown string
        /// </summary>
        /// <param name="seconds">Countdown time in seconds</param>
        /// <returns>Formatted countdown string</returns>
        string GetCountdownString(int seconds)
        {
            return new TimeSpan(0, 0, seconds).ToString(m_CountdownDisplayFormat);
        }

        /// <summary>
        ///     Updates the claim button as to which Reward Item is claimable, if a claim button exists.
        /// </summary>
        void UpdateClaimButton()
        {
            if (m_ClaimButton is null)
                return;

            if (m_Reward == null || m_Reward.IsInCooldown())
            {
                // Reward is in cooldown
                m_ClaimButton.Init(null, null);
            }
            else
            {
                // There should be a RewardItem available to be claimed
                m_ClaimButton.Init(m_RewardKey, m_Reward.GetLastClaimableRewardItemKey());
            }
        }

        /// <summary>
        ///     Updates the close button as to which Reward Item is claimable, if a claim button exists.
        /// </summary>
        void UpdateCloseButton()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_CloseButton?.gameObject.SetActive(!m_AutoHideCloseButton || m_Reward is null || m_Reward.IsInCooldown());
        }

        /// <summary>
        ///     Specifies whether the close button is invisible when there is claimable Reward Item.
        /// </summary>
        /// <param name="autoHide">
        ///     Used to determine if the close button should be hidden when there is a claimable Reward Item.
        /// </param>
        public void SetAutoHideCloseButton(bool autoHide)
        {
            if (autoHide == m_AutoHideCloseButton) return;

            m_AutoHideCloseButton = autoHide;
            UpdateCloseButton();
        }

        /// <summary>
        ///     Checks to see whether any properties have been changed (by checking <see cref="m_IsDirty"/>) and
        ///     if so, calls <see cref="UpdateContent"/> before resetting the flag.
        ///
        ///     At runtime, also assigns the appropriate value for <see cref="m_Reward"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;

                // UpdateContent doesn't update m_Currency, which is what is used to fetch icons at runtime.
                // If Game Foundation is initialized and m_Currency does not match m_CurrencyKey,
                // reset m_Currency based on m_CurrencyKey.
                if (GameFoundationSdk.IsInitialized &&
                    (m_Reward is null && !string.IsNullOrEmpty(m_RewardKey) || 
                     !(m_Reward is null) && m_Reward.key != m_RewardKey))
                {
                    m_Reward = GetReward(m_RewardKey);
                }

                UpdateContent();
            }
        }

        /// <summary>
        ///     Updates the content displayed in the Reward Popup.
        /// </summary>
        internal void UpdateContent()
        {
            if (Application.isPlaying)
            {
                UpdateContentAtRuntime();
            }
#if UNITY_EDITOR
            else
            {
                UpdateContentAtEditor();
            }
#endif
        }

        /// <summary>
        ///     Updates the content displayed in the Reward Popup during runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            string displayName = null;
            string descriptionText = null;

            var rewardDefinition = m_Reward?.rewardDefinition;
            if (rewardDefinition != null)
            {
                // Update Reward Item before updating content
                m_Reward.Update();

                // Get DisplayName
                displayName = rewardDefinition.displayName;

                // Get Description
                if (rewardDefinition.TryGetStaticProperty(m_DescriptionPropertyKey, out var descriptionProperty))
                {
                    descriptionText = descriptionProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning($"\"{rewardDefinition.displayName}\" Reward doesn't have Static Property called \"{m_DescriptionPropertyKey}\"");
                }

                // Get RewardItem Title Format
                if (rewardDefinition.TryGetStaticProperty(m_RewardItemTitlePropertyKey, out var rewardItemTitleProperty))
                {
                    m_RewardItemTitleFormat = rewardItemTitleProperty.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    k_GFLogger.LogWarning($"\"{rewardDefinition.displayName}\" Reward doesn't have Static Property called \"{m_RewardItemTitlePropertyKey}\"");
                }
            }

            // Set Title and Description
            SetTextContent(displayName, descriptionText);

            // Update Reward Items & Claim Button & Countdown
            UpdateGeneratedRewardItemsAtRuntime();
            UpdateClaimButton();
            UpdateCloseButton();
            UpdateCountdown();
        }

        /// <summary>
        ///     Updates the all the Reward Item Prefabs, by getting the updated list of rewardItemStates from the reward.
        ///     Triggers re-generation of Reward Item prefabs, and updates to claim button and countdowns, if necessary.
        ///     Triggers the onRewardStateChanged callback.
        /// </summary>
        void UpdateGeneratedRewardItemsAtRuntime()
        {
            if (m_AutoPopulatedRewardItemContainer is null)
            {
                return;
            }

            ClearRewardItemPrefabs();

            if (m_Reward != null)
            {
                var rewardItemStates = m_Reward.rewardItemStates;
                var rewardItemDefinitions = new List<RewardItemDefinition>();
                m_Reward.rewardDefinition.GetRewardItems(rewardItemDefinitions);
                foreach (var rewardItem in rewardItemDefinitions)
                {
                    InstantiateRewardItemPrefab(rewardItem.key, rewardItemStates[rewardItem.key]);
                }
            }

            // To force rebuilt Layouts at Editor and Runtime
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_AutoPopulatedRewardItemContainer.parent);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Updates the content displayed in the Reward Popup during editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            string displayName = null;
            string descriptionText = null;

            var rewardAsset =
                !string.IsNullOrEmpty(m_RewardKey)
                    ? PrefabTools.GetLookUpCatalogAsset().FindItem(m_RewardKey) as RewardAsset
                    : null;

            if (!(rewardAsset is null))
            {
                displayName = rewardAsset.displayName;

                var properties = rewardAsset.GetStaticProperties();
                if (!string.IsNullOrEmpty(m_DescriptionPropertyKey))
                {
                    var descriptionIndex = properties.FindIndex(x => x.key == m_DescriptionPropertyKey);
                    if (descriptionIndex >= 0)
                    {
                        descriptionText = properties[descriptionIndex].value.AsString();
                    }
                    else if (m_ShowDebugLogs)
                    {
                        k_GFLogger.LogWarning(
                            $"\"{rewardAsset.displayName}\" Reward doesn't have Static Property called \"{m_DescriptionPropertyKey}\"");
                    }
                }

                // Get RewardItem Title Format
                if (string.IsNullOrEmpty(m_RewardItemTitlePropertyKey))
                {
                    m_RewardItemTitleFormat = "";
                }
                else
                {
                    var titleFormatIndex = properties.FindIndex(x => x.key == m_RewardItemTitlePropertyKey);
                    if (titleFormatIndex >= 0)
                    {
                        var format = properties[titleFormatIndex].value.AsString();
                        if (!string.Equals(format, m_RewardItemTitleFormat))
                        {
                            m_RewardItemTitleFormat = format;
                        }
                    }
                    else
                    {
                        m_RewardItemTitleFormat = "";

                        if (m_ShowDebugLogs)
                        {
                            k_GFLogger.LogWarning(
                                $"\"{rewardAsset.displayName}\" Reward doesn't have Static Property called \"{m_RewardItemTitlePropertyKey}\"");
                        }
                    }
                }
            }

            // Set Title and Description
            SetTextContent(displayName, descriptionText);

            // Update Claim Button & Countdown
            UpdateGeneratedRewardItemsAtEditor(rewardAsset);
            UpdateClaimButton();
            UpdateCountdown();
        }

        /// <summary>
        ///     Clears existing Reward Item prefabs and instantiates new ones.
        /// </summary>
        void UpdateGeneratedRewardItemsAtEditor(RewardAsset rewardAsset)
        {
            if (m_AutoPopulatedRewardItemContainer == null)
                return;

            if (!this.ShouldRegenerateGameObjects())
                return;

            ClearRewardItemPrefabs();

            if (rewardAsset != null)
            {
                using (s_RewardItemListPool.Get(out var rewardItems))
                {
                    rewardAsset.GetRewardItems(rewardItems);
                    foreach (var rewardItem in rewardItems)
                    {
                        InstantiateRewardItemPrefab(rewardItem.key, RewardItemState.Locked);
                    }
                }
            }

            // To force rebuilt Layouts at Editor and Runtime
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_AutoPopulatedRewardItemContainer.parent);
        }
#endif

        /// <summary>
        ///     Clears auto-generated Container
        /// </summary>
        void ClearRewardItemPrefabs()
        {
            if (m_AutoPopulatedRewardItemContainer is null)
                return;

            if (!this.ShouldRegenerateGameObjects())
                return;

            var toRemove = new List<Transform>();

            foreach (Transform child in m_AutoPopulatedRewardItemContainer)
            {
                if (child.name == kRewardItemGameObjectName)
                {
                    toRemove.Add(child);
                }
            }

            foreach (var transformToRemove in toRemove)
            {
                if (Application.isPlaying)
                {
                    Destroy(transformToRemove.gameObject);
                }
                else
                {
                    DestroyImmediate(transformToRemove.gameObject);
                }
            }
        }

        /// <summary>
        ///     Selects which Reward Item prefab to instantiate based on the rewardItemState, and instantiates it
        ///     with the temp.
        /// </summary>
        /// <param name="rewardItemKey">
        ///     The key for the RewardItem which will be displayed in the prefab.
        /// </param>
        /// <param name="state">
        ///     The RewardItemState for the Reward Item with the specified key, as set by the Reward System.
        /// </param>
        void InstantiateRewardItemPrefab(string rewardItemKey, RewardItemState state)
        {
            if (m_AutoPopulatedRewardItemContainer is null)
                return;

            RewardItemView rewardItemPrefab = null;

            switch (state)
            {
                case RewardItemState.Claimable:
                    if (!(m_ClaimableRewardItemPrefab is null))
                    {
                        rewardItemPrefab = Instantiate(m_ClaimableRewardItemPrefab, m_AutoPopulatedRewardItemContainer);
                    }

                    break;

                case RewardItemState.Claimed:
                    if (!(m_ClaimedRewardItemPrefab is null))
                    {
                        rewardItemPrefab = Instantiate(m_ClaimedRewardItemPrefab, m_AutoPopulatedRewardItemContainer);
                    }

                    break;

                case RewardItemState.Missed:
                    if (!(m_MissedRewardItemPrefab is null))
                    {
                        rewardItemPrefab = Instantiate(m_MissedRewardItemPrefab, m_AutoPopulatedRewardItemContainer);
                    }

                    break;
            }

            if (rewardItemPrefab is null)
            {
                if (!(m_LockedRewardItemPrefab is null))
                {
                    rewardItemPrefab = Instantiate(m_LockedRewardItemPrefab, m_AutoPopulatedRewardItemContainer);
                }
            }

            if (!(rewardItemPrefab is null))
            {
                rewardItemPrefab.name = kRewardItemGameObjectName;
                rewardItemPrefab.Init(m_RewardKey, rewardItemKey, m_PayoutItemIconSpritePropertyKey, m_RewardItemTitleFormat);
            }
        }

        /// <summary>
        ///     Sets the Reward's Display Name and description.
        /// </summary>
        /// <param name="displayName">
        ///     The display name of the Reward shown in this view.
        /// </param>
        /// <param name="description">
        ///     The description of the Reward shown in this view, if any. If no description the field will be hidden.
        /// </param>
        void SetTextContent(string displayName, string description)
        {
            if (!(m_TitleTextField is null) && m_TitleTextField.text != displayName)
            {
                m_TitleTextField.text = displayName;
            }

            if (!(m_DescriptionTextField is null) && m_DescriptionTextField.text != description)
            {
                m_DescriptionTextField.text = description;
                m_DescriptionTextField.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }
        }

        /// <summary>
        ///     Gets triggered when the state of the Reward claim changes. Triggers the
        ///     user-specified onRewardStateChanged callback.
        /// </summary>
        void OnRewardStateChanged(Reward reward)
        {
            if (reward != m_Reward)
                return;

            UpdateGeneratedRewardItemsAtRuntime();
            UpdateClaimButton();
            UpdateCloseButton();
            UpdateCountdown();

            onRewardStateChanged?.Invoke(reward);
        }

        /// <summary>
        ///     Gets triggered when the current Reward Item is successfully claimed.
        /// </summary>
        /// <param name="reward">
        ///     The Reward that had a claim succeed on it.
        /// </param>
        /// <param name="rewardItemKey">
        ///     The Reward Item claimed.
        /// </param>
        /// <param name="payout">
        ///     The payout element of the Reward Item.
        /// </param>
        void OnRewardClaimSucceeded(Reward reward, string rewardItemKey, Payout payout)
        {
            if (reward != m_Reward)
                return;

            UpdateGeneratedRewardItemsAtRuntime();

            var rewardItemDefinition = GetRewardItemDefinition(reward, rewardItemKey);
            onRewardClaimed?.Invoke(reward, rewardItemDefinition);
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
