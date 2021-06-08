using System;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using DCTools = UnityEngine.GameFoundation.DefaultCatalog.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class RewardCatalogAssetEditor : BaseCatalogAssetEditor<RewardAsset>
    {
        const string k_ResetIfExpiredDefaultTooltip = "When enabled, if player exceeds the cooldown " +
            "and expiration without claiming a reward, the streak will reset at the beginning of the reward item list.";

        static float m_TablePaddingTopLarge = 24;

        readonly struct TimeGroup
        {
            public readonly int time;
            public readonly TimeUnit timeUnit;

            public TimeGroup(int time, TimeUnit unit)
            {
                this.time = time;
                timeUnit = unit;
            }
        }

        RewardItemObject[] m_RewardItems;
        RewardItemObject m_RewardItemToMoveDown;
        RewardItemObject m_RewardItemToMoveUp;
        RewardItemObject m_RewardItemToRemove;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<RewardCatalogAssetEditor>();

        readonly GUIContent m_RewardAvailabilityLabel = new GUIContent(
            "Availability",
            "Group of settings for determining how frequently a reward is available and which reward in the list is available."
        );

        readonly GUIContent m_CooldownLabel = new GUIContent(
            "Cooldown",
            "Length of time between when a reward is claimed and when it is available for claiming again. " +
            "Stored in seconds regardless of what units it's entered in as."
        );

        readonly GUIContent m_CooldownUnitsLabel = new GUIContent(
            "Cooldown Units",
            "Unit of time for entering the cooldown value. " +
            "Value will be stored in seconds regardless of selection here.");

        readonly GUIContent m_ExpirationLabel = new GUIContent(
            "Expiration",
            "Length of time between when a reward becomes available and when it expires and the clock restarts. " +
            "If reset if expired is checked and user exceeds this time they will start again at the beginning of " +
            "the reward item list, otherwise it will move on to the next reward in the list. " +
            "Stored in seconds regardless of what units it's entered in as.");

        readonly GUIContent m_ExpirationUnitsLabel = new GUIContent(
            "Expiration Units",
            "Unit of time for entering the expiration value. " +
            "Value will be stored in seconds regardless of selection here.");

        readonly GUIContent m_ResetIfExpiredLabel = new GUIContent(
            "Reset if expired", k_ResetIfExpiredDefaultTooltip);

        readonly GUIContent m_RewardItemsLabel = new GUIContent(
            "Reward Items", "The list of reward items to be paid out in sequential order.");

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Rewards;

        public RewardCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawSidebarListItem(RewardAsset reward)
        {
            BeginSidebarItem(reward, new Vector2(242f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(reward.displayName, 242, GameFoundationEditorStyles.boldTextStyle);

            EndSidebarItem();
        }

        protected override void DrawTypeSpecificBlocks(RewardAsset reward)
        {
            using (DCTools.Pools.rewardItemList.Get(out var rewardItems))
            {
                reward.GetRewardItems(rewardItems);
                m_RewardItems = rewardItems.ToArray();
            }

            DrawRewardAvailability(reward);

            EditorGUILayout.Space();

            DrawRewardItems(reward);

            EditorGUILayout.Space();
        }

        void DrawRewardAvailability(RewardAsset reward)
        {
            EditorGUILayout.LabelField(m_RewardAvailabilityLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var newCooldownValues = DrawIntFieldWithUnits(reward.cooldownSeconds, reward.cooldownDisplayUnits,
                        m_CooldownLabel, m_CooldownUnitsLabel);
                    

                    var newExpirationValues = DrawIntFieldWithUnits(reward.expirationSeconds, reward.expirationDisplayUnits, 
                        m_ExpirationLabel, m_ExpirationUnitsLabel);
                        

                    bool resetIfExpired = reward.resetIfExpired;
                    var hasExpiration = newExpirationValues.time > 0;
                    if (hasExpiration)
                    {
                        m_ResetIfExpiredLabel.tooltip = k_ResetIfExpiredDefaultTooltip;
                    }
                    else
                    {
                        if (reward.resetIfExpired)
                        {
                            resetIfExpired = false;
                            reward.Editor_SetResetIfExpired(false);
                        }

                        m_ResetIfExpiredLabel.tooltip =
                            "Reset if Expired can only be enabled if Expiration is greater than 0. " +
                            k_ResetIfExpiredDefaultTooltip;
                    }

                    using (new EditorGUI.DisabledGroupScope(!hasExpiration))
                    {
                        resetIfExpired = EditorGUILayout.Toggle(m_ResetIfExpiredLabel, resetIfExpired);
                    }
                    
                    if (check.changed)
                    {
                        reward.Editor_SetCooldown(newCooldownValues.time, newCooldownValues.timeUnit);
                        reward.Editor_SetExpiration(newExpirationValues.time, newExpirationValues.timeUnit);
                        reward.Editor_SetResetIfExpired(resetIfExpired);
                    }
                }
            }
        }

        void DrawRewardItems(RewardAsset reward)
        {
            m_RewardItemToMoveUp = null;
            m_RewardItemToMoveDown = null;
            m_RewardItemToRemove = null;

            EditorGUILayout.LabelField(m_RewardItemsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var anyAddedRewardItemsFlag = m_RewardItems != null && m_RewardItems.Length > 0;

                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(25));

                    EditorGUILayout.LabelField("Payout", GameFoundationEditorStyles.tableViewToolbarTextStyle);

                    GUILayout.FlexibleSpace();
                }

                if (anyAddedRewardItemsFlag)
                {
                    for (var i = 0; i < m_RewardItems.Length; i++)
                    {
                        var defaultStoreItem = m_RewardItems[i];

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(GameFoundationEditorStyles.tablePadding);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Space(m_TablePaddingTopLarge);
                                EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(25));
                                GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                            }

                            GUILayout.Space(GameFoundationEditorStyles.tablePadding);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                                if (CollectionEditorTools.IsNarrowWindow(RewardEditorWindow.ShowWindow()))
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        ExchangeEditor.DrawExchangeUI(m_RewardItems[i].m_Payout);
                                    }
                                }
                                else
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        ExchangeEditor.DrawExchangeUI(m_RewardItems[i].m_Payout);
                                    }
                                }

                                GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                            }

                            GUILayout.Space(GameFoundationEditorStyles.tablePadding);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Space(m_TablePaddingTopLarge);
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_RewardItems.Length - 1);

                                    if (GUILayout.Button("\u25BC", GameFoundationEditorStyles.tableViewButtonStyle,
                                        GUILayout.Width(18)))
                                    {
                                        m_RewardItemToMoveDown = defaultStoreItem;
                                        m_RewardItemToMoveUp = m_RewardItems[i + 1];
                                    }

                                    CollectionEditorTools.SetGUIEnabledAtEditorTime(i > 0);

                                    if (GUILayout.Button("\u25B2", GameFoundationEditorStyles.tableViewButtonStyle,
                                        GUILayout.Width(18)))
                                    {
                                        m_RewardItemToMoveUp = defaultStoreItem;
                                        m_RewardItemToMoveDown = m_RewardItems[i - 1];
                                    }

                                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                                }

                                GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                            }

                            GUILayout.Space(GameFoundationEditorStyles.tablePadding);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Space(m_TablePaddingTopLarge);
                                if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                                {
                                    m_RewardItemToRemove = defaultStoreItem;
                                }

                                GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Space(GameFoundationEditorStyles.tablePadding);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("no reward items specified");
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(GameFoundationEditorStyles.tablePadding);
                }

                // Draw horizontal separator.
                EditorGUILayout.Space();
                var separator = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(separator, EditorGUIUtility.isProSkin ? Color.black : Color.gray);

                if (GUILayout.Button("+"))
                {
                    reward.Editor_AddItem(new RewardItemObject());
                }
            }

            if (m_RewardItemToMoveUp != null && m_RewardItemToMoveDown != null)
            {
                reward.Editor_SwapItemsListOrder(m_RewardItemToMoveUp, m_RewardItemToMoveDown);
            }

            if (m_RewardItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    reward.Editor_RemoveItem(m_RewardItemToRemove);
                }
            }

            if (m_RewardItemToMoveUp != null || m_RewardItemToMoveDown != null || m_RewardItemToRemove != null)
            {
                GUI.FocusControl(null);
            }
        }

        static TimeGroup DrawIntFieldWithUnits(
            int valueInSeconds, TimeUnit unitSelection, GUIContent fieldLabel, GUIContent unitsLabel)
        {
            var inputTime = unitSelection.ConvertFromSeconds(valueInSeconds);
            TimeUnit inputUnit;

            if (CollectionEditorTools.IsNarrowWindow(RewardEditorWindow.ShowWindow()))
            {
                inputTime = EditorGUILayout.IntField(fieldLabel, inputTime);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(unitsLabel);
                    inputUnit = (TimeUnit)EditorGUILayout.Popup(
                        (int)unitSelection, Enum.GetNames(typeof(TimeUnit)), GUILayout.MaxWidth(150));
                }
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    inputTime = EditorGUILayout.IntField(fieldLabel, inputTime);
                    inputUnit = (TimeUnit)EditorGUILayout.Popup(
                        (int)unitSelection, Enum.GetNames(typeof(TimeUnit)), GUILayout.MaxWidth(150));
                }
            }

            return new TimeGroup(inputTime, inputUnit);
        }
    }
}
