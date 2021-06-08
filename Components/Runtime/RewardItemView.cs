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

        internal string m_RewardItemDefinitionKey;

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
        ///     The Static Property key string that should be used for getting the image of each item in the Payout
        ///     of a Reward Item for displaying. The key must exist on each inventory item or currency added as a
        ///     payout; any that don't have the key won't have their images displayed.
        /// </summary>
        public string payoutItemIconSpritePropertyKey => m_PayoutItemIconSpritePropertyKey;

        string m_PayoutItemIconSpritePropertyKey;

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
        ///     Tracks whether any properties have been changed.
        ///     Checked by Update() to see whether content should be updated.
        /// </summary>
        bool m_IsDirty;

        /// <summary>
        ///     Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;
        
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardItemView>();

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
        internal void Init(string rewardDefinitionKey, string rewardItemDefinitionKey,
            string payoutItemIconSpritePropertyKey, string titleFormat = kDefaultTitleFormat,
            string payoutCountPrefix = kDefaultCountPrefix)
        {
            m_PayoutItemIconSpritePropertyKey = payoutItemIconSpritePropertyKey;
            m_PayoutCountPrefix = payoutCountPrefix;
            m_TitleDisplayFormat = titleFormat;
            SetRewardItemDefinition(rewardDefinitionKey, rewardItemDefinitionKey);

            // Must call UpdateContent instead of setting m_IsDirty because setting m_IsDirty here causes a frame delay
            // when being driven by a parent component that makes this object look out of sync with its parent.
            UpdateContent();
        }

        /// <summary>
        ///     Initializes RewardItemView before the first frame update.
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
                return;
            }

            m_RewardItemDefinition = GetRewardItemDefinition(m_RewardDefinitionKey,
                m_RewardItemDefinitionKey);
            m_IsDirty = true;
        }

        /// <summary>
        ///     Sets the key of the Reward displayed in this Reward Item
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
            m_IsDirty = true;
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
            if (!Application.isPlaying || string.IsNullOrEmpty(rewardDefinitionKey) 
                                       || string.IsNullOrEmpty(rewardItemDefinitionKey))
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

            var itemDefinition = rewardDefinition.FindRewardItem(rewardItemDefinitionKey);
            if (itemDefinition == null && m_ShowDebugLogs)
            {
                k_GFLogger.LogWarning($"No Reward Item with the key \" {rewardItemDefinitionKey}\" can be " +
                                      $"found in the Reward catalog.");
            }

            return itemDefinition;
        }

        /// <summary>
        ///     Sets the key of the Reward displayed in this Reward Item
        /// </summary>
        /// <param name="definition">
        ///     A reference to a Reward Item definition.
        /// </param>
        public void SetRewardItemDefinition(RewardItemDefinition definition)
        {
            if (PrefabTools.FailIfNotInitialized(k_GFLogger, nameof(SetRewardItemDefinition))
                || ReferenceEquals(definition, m_RewardItemDefinition))
            {
                return;
            }

            m_RewardItemDefinition = definition;
            m_RewardItemDefinitionKey = definition?.key;
            m_RewardDefinitionKey = definition?.rewardDefinition.key;
            m_IsDirty = true;
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
            m_IsDirty = true;
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
            m_IsDirty = true;
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
            m_IsDirty = true;
        }

        /// <summary>
        ///     Checks to see whether any properties have been changed (by checking <see cref="m_IsDirty"/>) and
        ///     if so, calls <see cref="UpdateContent"/> before resetting the flag.
        ///
        ///     At runtime, also assigns the appropriate value for <see cref="m_Transaction"/> from the Catalog if needed.
        ///     If m_Transaction and m_TransactionKey don't currently match, this replaces m_Transaction with the
        ///     correct transaction by searching the Catalog for m_TransactionKey.
        /// </summary>
        void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;
                UpdateContent();
            }
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
            var rewardDefinition = !string.IsNullOrEmpty(m_RewardDefinitionKey) ? GameFoundationSdk.catalog
                .Find<RewardDefinition>(m_RewardDefinitionKey) : null;
            if (rewardDefinition != null && m_RewardItemDefinition != null)
            {
                // Get title to display
                string title = string.IsNullOrEmpty(m_TitleDisplayFormat) ? string.Empty :
                    string.Format(m_TitleDisplayFormat, rewardDefinition.IndexOf(m_RewardItemDefinition) + 1);
                if (m_ShowDebugLogs && string.IsNullOrEmpty(m_TitleDisplayFormat))
                {
                    k_GFLogger.LogWarning(
                        "Reward Item's title is not defined on the Reward as a static property.");
                }

                // RewardItemView currently only supports RewardItems with only one Exchange Object
                var firstExchangeDefinition = GetFirstExchangeDefinition(m_RewardItemDefinition);
                var description = "";
                if (!(firstExchangeDefinition is null))
                {
                    description = GetDescriptionText(firstExchangeDefinition.amount);
                    LoadAndSetIconSprite(firstExchangeDefinition, m_RewardDefinitionKey, 1);
                }
                SetTextContent(title, description);
            }
            else
            {
                SetTextContent(null, null);
                SetIconSprite(null);
            }
        }
        
        ExchangeDefinition GetFirstExchangeDefinition(RewardItemDefinition rewardItemDefinition)
        {
            if (rewardItemDefinition is null)
            {
                return null;
            }

            ExchangeDefinition firstExchangeDefinition = null;
            var exchangeDefinitions = new List<ExchangeDefinition>();
            
            rewardItemDefinition.payout.GetExchanges(exchangeDefinitions);

            if (exchangeDefinitions.Count > 0)
            {
                firstExchangeDefinition = exchangeDefinitions[0];
            }

            if (exchangeDefinitions.Count > 1)
            {
                k_GFLogger.LogWarning(
                    $"Note that Reward \"{rewardItemDefinition.rewardDefinition.key}\" has multiple Exchange " +
                    $"Items in a Payout. {nameof(RewardItemView)} will only show the first item on the UI.");
            }

            return firstExchangeDefinition;
        }

        void LoadAndSetIconSprite(ExchangeDefinition exchangeDefinition, string definitionKey, int rewardItemOrdinal)
        {
            if (!string.IsNullOrEmpty(m_PayoutItemIconSpritePropertyKey))
            {
                if (exchangeDefinition.tradableDefinition.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey,
                    out var iconProperty))
                {
                    PrefabTools.LoadSprite(iconProperty, SetIconSprite, OnSpriteLoadFailed);
                }
                else
                {
                    k_GFLogger.LogWarning($"The #{rewardItemOrdinal} Exchange Definition in the Reward " +
                                          $"\"{definitionKey}\" does not have a Static " +
                                          $"Property with the key \"{m_PayoutItemIconSpritePropertyKey}\" so an icon " +
                                          $"cannot be shown.");
                }
            }
            else
            {
                SetIconSprite(null);
            }
            
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Updates the content displayed in the Reward Item prefab during editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            string title = null;
            string description = null;

            if (!string.IsNullOrEmpty(m_RewardDefinitionKey) && !string.IsNullOrEmpty(m_RewardItemDefinitionKey))
            {
                var rewardAsset = PrefabTools.GetLookUpCatalogAsset().FindItem(m_RewardDefinitionKey) as RewardAsset;
                var rewardItemObject = rewardAsset?.FindRewardItem(m_RewardItemDefinitionKey);

                if (rewardItemObject != null)
                {
                    title = string.IsNullOrEmpty(m_TitleDisplayFormat) ? string.Empty :
                        string.Format(m_TitleDisplayFormat, rewardAsset.IndexOf(rewardItemObject) + 1);
                    if (m_ShowDebugLogs && string.IsNullOrEmpty(m_TitleDisplayFormat))
                    {
                        k_GFLogger.LogWarning(
                            "Reward Item's title is not defined on the Reward as a static property.");
                    }
                    
                    // RewardItemView currently only supports RewardItems with only one Exchange Object
                    if (GetFirstExchangeObject(rewardItemObject, m_RewardDefinitionKey) 
                        is ExchangeDefinitionObject firstExchangeObject)
                    {
                        description = GetDescriptionText(firstExchangeObject.amount);
                        LoadAndSetIconSprite(firstExchangeObject, m_RewardDefinitionKey, 1);
                    }
                }
            }

            SetTextContent(title, description);
        }

        ExchangeDefinitionObject GetFirstExchangeObject(RewardItemObject rewardItemObject, string rewardDefinitionKey)
        {
            if (rewardItemObject is null)
            {
                return null;
            }

            ExchangeDefinitionObject firstExchangeObject = null;
            var exchangeObjects = new List<ExchangeDefinitionObject>();

            rewardItemObject.payout.GetItems(exchangeObjects);

            if (exchangeObjects.Count > 0)
            {
                firstExchangeObject = exchangeObjects[0];
            }

            if (exchangeObjects.Count > 1)
            {
                k_GFLogger.LogWarning($"Note that Reward \"{rewardDefinitionKey}\" has multiple Exchange " +
                                      $"Items in a Payout. {nameof(RewardItemView)} will only show the first item on " +
                                      $"the UI.");
            }

            return firstExchangeObject;
        }

        void LoadAndSetIconSprite(ExchangeDefinitionObject exchangeObject, string definitionKey, int rewardItemOrdinal)
        {
            if (!string.IsNullOrEmpty(m_PayoutItemIconSpritePropertyKey))
            {
                if (exchangeObject.catalogItem.TryGetStaticProperty(m_PayoutItemIconSpritePropertyKey, out var iconProperty))
                {
                    PrefabTools.LoadSprite(iconProperty, SetIconSprite, OnSpriteLoadFailed);
                }
                else
                {
                    k_GFLogger.LogWarning($"The #{rewardItemOrdinal} Exchange Object in the Reward " +
                                          $"\"{definitionKey}\" does not have a Static Property with the key " +
                                          $"\"{m_PayoutItemIconSpritePropertyKey}\" so an icon cannot be shown.");
                }
            }
            else
            {
                SetIconSprite(null);
            }
        }
#endif
        string GetDescriptionText(long itemAmount)
        {
            return (m_PayoutCountPrefix ?? "") + itemAmount;
        }

        /// <summary>
        ///     Sets content of Reward Item view
        /// </summary>
        /// <param name="title">
        ///     A title for the Reward Item View
        /// </param>
        /// <param name="description">
        ///     A description of the Reward Item View
        /// </param>
        void SetTextContent(string title, string description)
        {
            SetDisplayName(title);
            SetDescription(description);
        }

        /// <summary>
        ///     Sets sprite of item in display.
        /// </summary>
        /// <param name="icon">
        ///     The new sprite to display.
        /// </param>
        void SetIconSprite(Sprite icon)
        {
            SetIcon(icon);
        }

        /// <summary>
        ///     Callback for if there is an error while trying to load a sprite from its Property.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message returned by <see cref="PrefabTools.LoadSprite"/>.
        /// </param>
        void OnSpriteLoadFailed(string errorMessage)
        {
            k_GFLogger.LogWarning(errorMessage);
        }
    }
}
