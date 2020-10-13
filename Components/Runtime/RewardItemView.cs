using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine.GameFoundation.DefaultCatalog;

#endif

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying an individual Reward Item.
    /// </summary>
    [AddComponentMenu("Game Foundation/Reward Item View", 7)]
    [ExecuteInEditMode]
    public class RewardItemView : ItemView
    {
        /// <summary>
        ///     The definition key of the Reward Item being displayed.
        /// </summary>
        public string rewardItemDefinitionKey => m_RewardItemDefinitionKey;

        string m_RewardItemDefinitionKey;

        /// <summary>
        ///     The definition key of the Reward that the Reward Item being displayed belongs to.
        /// </summary>
        public string rewardDefinitionKey => m_RewardDefinitionKey;

        string m_RewardDefinitionKey;

        /// <summary>
        ///     The string to prefix the payout counts with when generating display.
        /// </summary>
        public string payoutCountPrefix => m_PayoutCountPrefix;

        string m_PayoutCountPrefix = kDefaultCountPrefix;

        /// <summary>
        ///     The Static Property key string that should be used for getting the image of each item in the Payout of a Reward
        ///     Item
        ///     for displaying. The key must exist on each inventory item or currency added as a payout; any that don't have the
        ///     key won't have
        ///     their images displayed.
        /// </summary>
        public string payoutItemIconSpritePropertyKey => m_PayoutItemIconSpritePropertyKey;

        string m_PayoutItemIconSpritePropertyKey = "reward_icon";

        /// <summary>
        ///     The format string to use when generating title. Only used when being instantiated by RewardPopupView.
        /// </summary>
        string m_TitleDisplayFormat = kDefaultTitleFormat;

        /// <summary>
        ///     Default count prefix for inventory item and currency.
        /// </summary>
        const string kDefaultCountPrefix = "+";

        /// <summary>
        ///     Default format string to use for formatting RewardItem title.
        /// </summary>
        const string kDefaultTitleFormat = "{0}";

        /// <summary>
        ///     A reference to the Reward Item Definition.
        /// </summary>
        public RewardItemDefinition rewardItemDefinition => m_RewardItemDefinition;

        RewardItemDefinition m_RewardItemDefinition;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

        /// <summary>
        ///     Initializes RewardItemView with needed info.
        /// </summary>
        /// <param name="rewardDefinitionKey">
        ///     The key of the Reward that the Reward Item being displayed belongs to.
        /// </param>
        /// <param name="rewardItemDefinitionKey">
        ///     The key of the Reward Item being displayed.
        /// </param>
        /// <param name="payoutItemIconSpritePropertyKey">
        ///     The Static Property key string used for getting the image of each item in the Payout of a Reward Item
        ///     for displaying. The key must exist on each inventory item or currency added as a payout;
        ///     any that don't have the key won't have their images displayed.
        /// </param>
        /// <param name="payoutCountPrefix">
        ///     The string to prefix the payout counts with when generating display.
        /// </param>
        /// <param name="titleFormat">
        ///     The format string to use to create the Reward Item's title.
        /// </param>
        internal void Init(string rewardDefinitionKey, string rewardItemDefinitionKey, string payoutItemIconSpritePropertyKey, string titleFormat = kDefaultTitleFormat, string payoutCountPrefix = kDefaultCountPrefix)
        {
            m_PayoutItemIconSpritePropertyKey = payoutItemIconSpritePropertyKey;
            m_PayoutCountPrefix = payoutCountPrefix;
            m_TitleDisplayFormat = titleFormat;

            SetRewardItemDefinition(rewardDefinitionKey, rewardItemDefinitionKey);
        }

        /// <summary>
        ///     Initializes RewardItemView before the first frame update.
        /// </summary>
        void Start()
        {
            if (!Application.isPlaying)
                return;

            ThrowIfNotInitialized();

            m_RewardItemDefinition = GetRewardItemDefinition(m_RewardDefinitionKey, m_RewardItemDefinitionKey);

            UpdateContent();
        }

        /// <summary>
        ///     Sets the key of the Reward displayed in this Reward Popup
        /// </summary>
        /// <param name="definition">
        ///     A reference to a Reward Item definition.
        /// </param>
        public void SetRewardItemDefinition(RewardItemDefinition definition)
        {
            ThrowIfNotInitialized();

            if (ReferenceEquals(definition, m_RewardItemDefinition))
            {
                return;
            }

            m_RewardItemDefinition = definition;
            m_RewardItemDefinitionKey = definition?.key;
            m_RewardDefinitionKey = definition?.rewardDefinition.key;

            UpdateContent();
        }

        /// <summary>
        ///     Sets the key of the Reward displayed in this Reward Popup
        /// </summary>
        /// <param name="rewardDefinitionKey">
        ///     The definition key of the Reward the RewardItem belongs to.
        /// </param>
        /// <param name="rewardItemDefinitionKey">
        ///     The definition key of the RewardItem to be displayed.
        /// </param>
        void SetRewardItemDefinition(string rewardDefinitionKey, string rewardItemDefinitionKey)
        {
            m_RewardDefinitionKey = rewardDefinitionKey;
            m_RewardItemDefinitionKey = rewardItemDefinitionKey;
            m_RewardItemDefinition = GetRewardItemDefinition(rewardDefinitionKey, rewardItemDefinitionKey);

            UpdateContent();
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
                    Debug.LogWarning($"{nameof(RewardItemView)} - Reward \"{rewardDefinitionKey}\" doesn't exist in Reward catalog.");
                }

                return null;
            }

            var rewardItemDefinition = rewardDefinition.FindRewardItem(rewardItemDefinitionKey);
            if (rewardItemDefinition == null && m_ShowDebugLogs)
            {
                Debug.LogWarning($"{nameof(RewardItemView)} - No Reward Item with the key \" {rewardItemDefinitionKey}\" can be found in the Reward catalog.");
            }

            return rewardItemDefinition;
        }

        /// <summary>
        ///     Sets the string to prefix the payout counts with when generating display.
        /// </summary>
        /// <param name="prefix">
        ///     The string to prefix.
        /// </param>
        public void SetPayoutCountPrefix(string prefix)
        {
            if (string.Equals(m_PayoutCountPrefix, prefix))
                return;

            m_PayoutCountPrefix = prefix;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the format string to use when generating title.
        ///     Only used when being instantiated by RewardPopupView.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when generating title. Suggested format would be something like "Day {0}"
        ///     where {0} will be replaced with the count + 1 of the item in the Reward's list.
        /// </param>
        public void SetTitleDisplayFormat(string format)
        {
            if (string.Equals(m_TitleDisplayFormat, format))
                return;

            m_TitleDisplayFormat = format;
            UpdateContent();
        }

        /// <summary>
        ///     Sets the key to use when getting the images of the Inventory and Currency items that are paid out in
        ///     this RewardItem. Each item should have this key in its Static Properties.
        /// </summary>
        /// <param name="propertyKey">
        ///     The Static Property key to look up image by.
        ///     Any Inventory or Currency Items that don't have this key in their Static Properties will not display an image.
        ///     If the key is null, no change from the current key will be made.
        /// </param>
        public void SetPayoutItemIconSpritePropertyKey(string propertyKey)
        {
            if (string.IsNullOrEmpty(propertyKey) || string.Equals(m_PayoutItemIconSpritePropertyKey, propertyKey))
                return;

            m_PayoutItemIconSpritePropertyKey = propertyKey;
            UpdateContent();
        }

        /// <summary>
        ///     Updates the content displayed in the Reward Item.
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
        ///     Updates the content displayed in the Reward Item prefab during runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            var rewardDefinition = !string.IsNullOrEmpty(m_RewardDefinitionKey) ? GameFoundationSdk.catalog.Find<RewardDefinition>(m_RewardDefinitionKey) : null;
            if (rewardDefinition != null && m_RewardItemDefinition != null)
            {
                // Get title to display
                string title = string.IsNullOrEmpty(m_TitleDisplayFormat) ? string.Empty : string.Format(m_TitleDisplayFormat, rewardDefinition.IndexOf(m_RewardItemDefinition) + 1);
                if (m_ShowDebugLogs && string.IsNullOrEmpty(m_TitleDisplayFormat))
                {
                    Debug.LogWarning($"{nameof(RewardItemView)} - Reward Item's title is not defined on the Reward as a static property.");
                }
                
                GetPayoutIconAndDescription(m_RewardItemDefinition, out var icon, out var description);
                SetContent(title, icon, description);
            }
            else
            {
                SetContent(null, null, null);
            }
        }

        /// <summary>
        ///     Gets the first Payout Item's sprite and description at runtime.
        /// </summary>
        /// <param name="rewardItemDefinition">
        ///     The RewardItemObject used for this view.
        /// </param>
        /// <param name="icon">
        ///     The icon of the first Exhange Item.
        /// </param>
        /// <param name="description">
        ///     The description of the first Exchange Item.
        /// </param>
        void GetPayoutIconAndDescription(RewardItemDefinition rewardItemDefinition, out Sprite icon, out string description)
        {
            icon = null;
            description = null;

            if (rewardItemDefinition == null) return;

            var exchangeObjects = new List<ExchangeDefinition>();
            rewardItemDefinition.payout.GetExchanges(exchangeObjects);

            if (exchangeObjects.Count > 0)
            {
                var exchangeObject = exchangeObjects[0];
                if (!string.IsNullOrEmpty(m_PayoutItemIconSpritePropertyKey))
                {
                    if (exchangeObject.tradableDefinition.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey, out var iconProperty))
                    {
                        icon = iconProperty.AsAsset<Sprite>();
                    
                        if (ReferenceEquals(icon, null))
                        {
                            Debug.LogWarning(
                                $"{nameof(RewardItemView)} - One of the Exchange Items in the Reward \"{rewardItemDefinition.rewardDefinition.key}\" does not have a Resources Asset Static Property with the key \"{m_PayoutItemIconSpritePropertyKey}\" so it will not be shown in the Reward Popup.");
                        }
                    }    
                }
                else if (m_ShowDebugLogs)
                {
                    Debug.LogWarning(
                        $"{nameof(RewardItemView)} - Payout Item Icon Property key of Reward \"{rewardItemDefinition.rewardDefinition.key}\" is null.");
                }

                description = (m_PayoutCountPrefix ?? "") + exchangeObject.amount;

                if (exchangeObjects.Count > 1)
                {
                    Debug.LogWarning(
                        $"{nameof(RewardItemView)} - Reward \"{rewardItemDefinition.rewardDefinition.key}\" has multiple Exchange Items in a Payout. {nameof(RewardItemView)} can only show the first Exchange Item on UI.");
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Updates the content displayed in the Reward Item prefab during editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            string title = null;
            Sprite icon = null;
            string description = null;

            if (!string.IsNullOrEmpty(m_RewardDefinitionKey) && !string.IsNullOrEmpty(m_RewardItemDefinitionKey))
            {
                var rewardAsset = CatalogSettings.catalogAsset.FindItem(m_RewardDefinitionKey) as RewardAsset;
                var rewardItemAsset = rewardAsset?.FindRewardItem(m_RewardItemDefinitionKey);

                if (rewardItemAsset != null)
                {
                    title = string.IsNullOrEmpty(m_TitleDisplayFormat) ? string.Empty : string.Format(m_TitleDisplayFormat, rewardAsset.IndexOf(rewardItemAsset) + 1);
                    if (m_ShowDebugLogs && string.IsNullOrEmpty(m_TitleDisplayFormat))
                    {
                        Debug.LogWarning($"{nameof(RewardItemView)} - Reward Item's title is not defined on the Reward as a static property.");
                    }
                    
                    GetPayoutIconAndDescription(rewardItemAsset, out icon, out description);
                }
            }

            SetContent(title, icon, description);
        }

        /// <summary>
        ///     Gets the first Payout Item's sprite and description at editor time.
        /// </summary>
        /// <param name="rewardItemObject">
        ///     The RewardItemObject used for this view.
        /// </param>
        /// <param name="icon">
        ///     The icon of the first Exhange Item.
        /// </param>
        /// <param name="description">
        ///     The description of the first Exchange Item.
        /// </param>
        void GetPayoutIconAndDescription(RewardItemObject rewardItemObject, out Sprite icon, out string description)
        {
            icon = null;
            description = null;

            if (rewardItemObject == null) return;

            var exchangeObjects = new List<ExchangeDefinitionObject>();
            rewardItemObject.payout.GetItems(exchangeObjects);

            var definitionKey = rewardItemObject.reward?.key;
            if (exchangeObjects.Count > 0)
            {
                var exchangeObject = exchangeObjects[0];
                if (!string.IsNullOrEmpty(m_PayoutItemIconSpritePropertyKey))
                {
                    if (exchangeObject.catalogItem.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey, out var iconProperty))
                    {
                        icon = iconProperty.AsAsset<Sprite>();
                    
                        if (ReferenceEquals(icon, null) && !string.IsNullOrEmpty(definitionKey))
                        {
                            Debug.LogWarning(
                                $"{nameof(RewardItemView)} - One of the Exchange Items in the Reward \"{definitionKey}\" does not have a Resources Asset Static Property with the key \"{m_PayoutItemIconSpritePropertyKey}\" so it will not be shown in the Reward Popup.");
                        }
                    }   
                }
                else if (m_ShowDebugLogs && !string.IsNullOrEmpty(definitionKey))
                {
                    Debug.LogWarning(
                        $"{nameof(RewardItemView)} - Payout Item Icon Property key of Reward \"{definitionKey}\" is null.");
                }

                description = (m_PayoutCountPrefix ?? "") + exchangeObject.amount;

                if (exchangeObjects.Count > 1 && !string.IsNullOrEmpty(definitionKey))
                {
                    Debug.LogWarning(
                        $"{nameof(RewardItemView)} - Reward \"{definitionKey}\" has multiple Exchange Items in a Payout. {nameof(RewardItemView)} can only show the first Exchange Item on UI.");
                }
            }
        }
#endif

        /// <summary>
        ///     Sets content of Reward Item view
        /// </summary>
        /// <param name="title">A title for the Reward Item View</param>
        /// <param name="icon">An icon for the Reward Item View</param>
        /// <param name="description">A description of the Reward Item View</param>
        void SetContent(string title, Sprite icon, string description)
        {
            SetDisplayName(title);
            SetIcon(icon);
            SetDescription(description);
        }

        /// <summary>
        ///     Throws an Invalid Operation Exception if GameFoundation has not been initialized before this view is used.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void ThrowIfNotInitialized()
        {
            if (!GameFoundationSdk.IsInitialized)
            {
                throw new InvalidOperationException($"Error: GameFoundation.Initialize() must be called before the {nameof(RewardItemView)} is used.");
            }
        }
    }
}
