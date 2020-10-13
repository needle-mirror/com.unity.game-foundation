using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="RewardClaimButton"/> component.
    /// </summary>
    [CustomEditor(typeof(RewardClaimButton))]
    class RewardClaimButtonEditor : Editor
    {
        RewardClaimButton m_RewardClaimButton;

        StringBuilder m_RewardItemName = new StringBuilder();
        List<ExchangeDefinitionObject> m_Payouts = new List<ExchangeDefinitionObject>();

        int m_SelectedRewardIndex = -1;
        int m_SelectedRewardItemIndex = -1;

        List<Tuple<string, string>> m_RewardItemNames = new List<Tuple<string, string>>();

        DropdownCatalogItemHelper<RewardAsset> m_RewardDropdownHelper = new DropdownCatalogItemHelper<RewardAsset>();
        DropdownPopulateHelper m_RewardItemDropdownHelper = new DropdownPopulateHelper();

        SerializedProperty m_RewardKey_SerializedProperty;
        SerializedProperty m_RewardItemKey_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(RewardClaimButton.m_RewardDefinitionKey),
            nameof(RewardClaimButton.m_RewardItemDefinitionKey)
        };

        void OnEnable()
        {
            m_RewardClaimButton = target as RewardClaimButton;

            if (m_RewardClaimButton == null)
                return;

            m_RewardKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardClaimButton.m_RewardDefinitionKey));
            m_RewardItemKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardClaimButton.m_RewardItemDefinitionKey));

            PopulateRewards();
            PopulateRewardItems();
        }

        void PopulateRewards()
        {
            m_SelectedRewardIndex = m_RewardDropdownHelper.Populate(m_RewardKey_SerializedProperty.stringValue);
        }

        void PopulateRewardItems()
        {
            m_RewardItemNames.Clear();

            var selectedRewardKey = m_RewardKey_SerializedProperty.stringValue;
            if (!string.IsNullOrEmpty(selectedRewardKey))
            {
                var selectedRewardDefinition = CatalogSettings.catalogAsset.FindItem(selectedRewardKey) as RewardAsset;

                if (selectedRewardDefinition != null)
                {
                    var rewardItemObjects = new List<RewardItemObject>();
                    selectedRewardDefinition.GetRewardItems(rewardItemObjects);

                    for (int i = 0; i < rewardItemObjects.Count; i++)
                    {
                        var rewardItem = rewardItemObjects[i];
                        m_RewardItemNames.Add(new Tuple<string, string>(BuildRewardItemName(rewardItem, i), rewardItem.key));
                    }
                }
            }

            m_SelectedRewardItemIndex = m_RewardItemDropdownHelper.Populate(m_RewardItemNames, m_RewardItemKey_SerializedProperty.stringValue);
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (m_RewardClaimButton.m_IsDrivenByOtherComponent)
            {
                EditorGUILayout.HelpBox("Some item settings are driven by Reward Popup View", MessageType.None);
            }
            else
            {
                DrawRewardItemSelection();
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawRewardItemSelection()
        {
            var rewardContent = new GUIContent("Reward",
                "The Reward that the RewardItem to claim belongs to.");
            if (m_RewardDropdownHelper.displayNames != null && m_RewardDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedRewardIndex = EditorGUILayout.Popup(rewardContent, m_SelectedRewardIndex, m_RewardDropdownHelper.displayNames);
                var rewardKey = m_RewardDropdownHelper.GetKey(m_SelectedRewardIndex);
                if (m_RewardClaimButton.rewardDefinitionKey != rewardKey)
                {
                    SetSelectedRewardKey(rewardKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(rewardContent, 0, new[] { "None" });
            }

            var rewardItemContent = new GUIContent("Reward Item", "The RewardItem to claim.");
            if (m_RewardItemDropdownHelper.displayNames != null && m_RewardItemDropdownHelper.displayNames.Length > 0)
            {
                m_SelectedRewardItemIndex = EditorGUILayout.Popup(rewardItemContent, m_SelectedRewardItemIndex, m_RewardItemDropdownHelper.displayNames);
                var rewardItemKey = m_RewardItemDropdownHelper.GetKey(m_SelectedRewardItemIndex);
                if (m_RewardClaimButton.rewardItemDefinitionKey != rewardItemKey)
                {
                    SetSelectedRewardItemKey(rewardItemKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(rewardItemContent, 0, new[] { "None" });
            }
        }

        void SetSelectedRewardKey(string key)
        {
            // Update the serialized value
            m_RewardKey_SerializedProperty.stringValue = key;
            m_RewardClaimButton.SetRewardItemDefinition(key, m_RewardItemKey_SerializedProperty.stringValue);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulateRewardItems();
        }

        string BuildRewardItemName(RewardItemObject rewardItem, int index)
        {
            // Examples of the strings this method builds:
            // 1: Currencies: { Gold: 50, Silver: 10 }, Items: { Sword: 1, Metal: 5 }
            // 2: Currencies: { Gold: 50 }, Items: { Sword: 1 }
            // 3: Items: { Sword: 1 }
            // 4: Currencies: { Gold: 50 }
            // 5: No Payouts defined

            m_RewardItemName.Clear();
            m_RewardItemName.Append(index + 1 + ": ");

            // rewardItem.payout.GetCurrencies(m_PayoutCurrencies);
            rewardItem.payout.GetItems(m_Payouts);

            if (m_Payouts.Count == 0)
            {
                m_RewardItemName.Append("No Payouts defined");
                return m_RewardItemName.ToString();
            }

            var currencies = new Dictionary<string, long>();
            var inventoryItems = new Dictionary<string, long>();

            foreach (var payout in m_Payouts)
            {
                if (payout.catalogItem is CurrencyAsset currencyAsset)
                {
                    var displayName = currencyAsset.displayName;
                    if (currencies.ContainsKey(displayName))
                    {
                        currencies[displayName] += payout.amount;
                    }
                    else
                    {
                        currencies[displayName] = payout.amount;
                    }
                }
                else if (payout.catalogItem is InventoryItemDefinitionAsset inventoryItemAsset)
                {
                    var displayName = inventoryItemAsset.displayName;
                    if (inventoryItems.ContainsKey(displayName))
                    {
                        inventoryItems[displayName] += payout.amount;
                    }
                    else
                    {
                        inventoryItems[displayName] = payout.amount;
                    }
                }
            }

            if (currencies.Count > 0)
            {
                m_RewardItemName.Append("Currencies: { ");
            }

            int i = 0;
            foreach (var entity in currencies)
            {
                m_RewardItemName.Append(entity.Key + ": " + entity.Value);
                m_RewardItemName.Append(i == currencies.Count - 1 ? " }" : ", ");
                i++;
            }

            if (inventoryItems.Count > 0)
            {
                if (currencies.Count > 0)
                {
                    m_RewardItemName.Append(", ");
                }

                m_RewardItemName.Append("Items: {");
            }

            i = 0;
            foreach (var entity in inventoryItems)
            {
                m_RewardItemName.Append(entity.Key + ": " + entity.Value);
                m_RewardItemName.Append(i == inventoryItems.Count - 1 ? " }" : ", ");
                i++;
            }

            return m_RewardItemName.ToString();
        }

        void SetSelectedRewardItemKey(string key)
        {
            // Update the serialized value
            m_RewardItemKey_SerializedProperty.stringValue = key;
            m_RewardClaimButton.SetRewardItemDefinition(m_RewardKey_SerializedProperty.stringValue, key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
