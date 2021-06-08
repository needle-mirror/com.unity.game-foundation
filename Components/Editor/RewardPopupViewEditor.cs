using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="RewardPopupView"/> component.
    /// </summary>
    [CustomEditor(typeof(RewardPopupView))]
    class RewardPopupViewEditor : Editor
    {
        RewardPopupView m_RewardPopupView;
        RewardAsset m_SelectedRewardAsset;

        string m_SelectedRewardKey;
        int m_SelectedRewardIndex = -1;
        int m_SelectedDescriptionPropertyIndex = -1;
        int m_SelectedRewardItemTitlePropertyIndex = -1;
        int m_SelectedPayoutItemImagePropertyIndex = -1;

        DropdownCatalogItemHelper<RewardAsset> m_RewardDropdownPopulateHelper = new DropdownCatalogItemHelper<RewardAsset>();
        DropdownStaticPropertyHelper m_DescriptionPropertyPopulateHelper = new DropdownStaticPropertyHelper();
        DropdownStaticPropertyHelper m_RewardItemTitlePropertyPopulateHelper = new DropdownStaticPropertyHelper();
        DropdownMultipleCatalogItemsHelper m_PayoutImagePropertyDropdownHelper = new DropdownMultipleCatalogItemsHelper();

        SerializedProperty m_RewardKey_SerializedProperty;
        SerializedProperty m_DescriptionPropertyKey_SerializedProperty;
        SerializedProperty m_PayoutItemIconSpritePropertyKey_SerializedProperty;
        SerializedProperty m_RewardItemTitlePropertyKey_SerializedProperty;
        SerializedProperty m_CountdownDisplayFormat_SerializedProperty;
        SerializedProperty m_CountdownTextField_SerializedProperty;
        SerializedProperty m_CountdownCooldownDescription_SerializedProperty;
        SerializedProperty m_CountdownExpirationDescription_SerializedProperty;
        SerializedProperty m_LockedRewardItemPrefab_SerializedProperty;
        SerializedProperty m_ClaimableRewardItemPrefab_SerializedProperty;
        SerializedProperty m_ClaimedRewardItemPrefab_SerializedProperty;
        SerializedProperty m_MissedRewardItemPrefab_SerializedProperty;
        SerializedProperty m_ClaimButton_SerializedProperty;
        SerializedProperty m_CloseButton_SerializedProperty;
        SerializedProperty m_AutoHideCloseButton_SerializedProperty;
        SerializedProperty m_TitleTextField_SerializedProperty;
        SerializedProperty m_DescriptionTextField_SerializedProperty;
        SerializedProperty m_AutoPopulatedRewardItemContainer_SerializedProperty;
        SerializedProperty m_AutoPopulatedRewardItemEditorFields_SerializedProperty;
        SerializedProperty m_ShowCountdownEditorFields_SerializedProperty;
        SerializedProperty m_ShowTextEditorFields_SerializedProperty;
        SerializedProperty m_ShowButtonEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(RewardPopupView.m_RewardKey),
            nameof(RewardPopupView.m_DescriptionPropertyKey),
            nameof(RewardPopupView.m_PayoutItemIconSpritePropertyKey),
            nameof(RewardPopupView.m_RewardItemTitlePropertyKey),
            nameof(RewardPopupView.m_CountdownDisplayFormat),
            nameof(RewardPopupView.m_CountdownTextField),
            nameof(RewardPopupView.m_CountdownCooldownDescription),
            nameof(RewardPopupView.m_CountdownExpirationDescription),
            nameof(RewardPopupView.m_LockedRewardItemPrefab),
            nameof(RewardPopupView.m_ClaimableRewardItemPrefab),
            nameof(RewardPopupView.m_ClaimedRewardItemPrefab),
            nameof(RewardPopupView.m_MissedRewardItemPrefab),
            nameof(RewardPopupView.m_ClaimButton),
            nameof(RewardPopupView.m_CloseButton),
            nameof(RewardPopupView.m_AutoHideCloseButton),
            nameof(RewardPopupView.m_TitleTextField),
            nameof(RewardPopupView.m_DescriptionTextField),
            nameof(RewardPopupView.m_AutoPopulatedRewardItemContainer),
            nameof(RewardPopupView.showCountdownEditorFields),
            nameof(RewardPopupView.showTextEditorFields),
            nameof(RewardPopupView.showButtonEditorFields),
            nameof(RewardPopupView.showAutoPopRewardItemsEditorFields),
        };

        void OnEnable()
        {
            m_RewardPopupView = target as RewardPopupView;

            if (m_RewardPopupView == null)
                return;

            m_RewardKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_RewardKey));
            m_DescriptionPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_DescriptionPropertyKey));
            m_PayoutItemIconSpritePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_PayoutItemIconSpritePropertyKey));
            m_RewardItemTitlePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_RewardItemTitlePropertyKey));
            m_CountdownTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_CountdownTextField));
            m_CountdownDisplayFormat_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_CountdownDisplayFormat));
            m_CountdownCooldownDescription_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_CountdownCooldownDescription));
            m_CountdownExpirationDescription_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_CountdownExpirationDescription));
            m_LockedRewardItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_LockedRewardItemPrefab));
            m_ClaimableRewardItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_ClaimableRewardItemPrefab));
            m_ClaimedRewardItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_ClaimedRewardItemPrefab));
            m_MissedRewardItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_MissedRewardItemPrefab));
            m_ClaimButton_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_ClaimButton));
            m_CloseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_CloseButton));
            m_AutoHideCloseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_AutoHideCloseButton));
            m_TitleTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_TitleTextField));
            m_DescriptionTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_DescriptionTextField));
            m_AutoPopulatedRewardItemContainer_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.m_AutoPopulatedRewardItemContainer));
            m_AutoPopulatedRewardItemEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.showAutoPopRewardItemsEditorFields));
            m_ShowCountdownEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.showCountdownEditorFields));
            m_ShowTextEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.showTextEditorFields));
            m_ShowButtonEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_RewardPopupView.showButtonEditorFields));

            m_SelectedRewardKey = m_RewardKey_SerializedProperty.stringValue;
            m_SelectedRewardAsset = !string.IsNullOrEmpty(m_SelectedRewardKey)
                ? PrefabTools.GetLookUpCatalogAsset().FindItem(m_SelectedRewardKey) as RewardAsset
                : null;

            PopulateRewardItems();
            PopulatePropertyKeys();

            // To update the content when the GameObject is selected
            m_RewardPopupView.UpdateContent();
        }

        void PopulateRewardItems()
        {
            m_SelectedRewardIndex = m_RewardDropdownPopulateHelper.Populate(m_RewardKey_SerializedProperty.stringValue);
        }

        void PopulatePropertyKeys()
        {
            m_SelectedDescriptionPropertyIndex = m_DescriptionPropertyPopulateHelper.Populate(m_SelectedRewardAsset, m_DescriptionPropertyKey_SerializedProperty.stringValue, PropertyType.String);
            m_SelectedRewardItemTitlePropertyIndex = m_RewardItemTitlePropertyPopulateHelper.Populate(m_SelectedRewardAsset, m_RewardItemTitlePropertyKey_SerializedProperty.stringValue, PropertyType.String);

            PopulatePayoutItemIconPropertyKeys();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            DrawBasicRewardContent();
            EditorGUILayout.Space();

            DrawAutoPopulateRewardItemsSection();
            EditorGUILayout.Space();

            DrawTextSection();
            EditorGUILayout.Space();

            DrawCountdownSection();
            EditorGUILayout.Space();

            DrawButtonSection();
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void PopulatePayoutItemIconPropertyKeys()
        {
            var catalogItemAssets = new List<CatalogItemAsset>();

            if (m_SelectedRewardAsset != null)
            {
                var rewardItems = new List<RewardItemObject>();
                m_SelectedRewardAsset.GetRewardItems(rewardItems);

                foreach (var rewardItem in rewardItems)
                {
                    var exchangeObjects = new List<ExchangeDefinitionObject>();
                    rewardItem.payout.GetItems(exchangeObjects);

                    CatalogItemAsset firstPayoutItem = exchangeObjects.Count > 0 ? exchangeObjects[0].catalogItem : null;
                    if (firstPayoutItem != null)
                    {
                        catalogItemAssets.Add(firstPayoutItem);
                    }
                }
            }

            m_SelectedPayoutItemImagePropertyIndex = m_PayoutImagePropertyDropdownHelper.Populate(
                catalogItemAssets, m_PayoutItemIconSpritePropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables });
        }

        void DrawBasicRewardContent()
        {
            var rewardContent = new GUIContent("Reward", "The Reward to display in this view.");
            var rewardItemTitleProperty = new GUIContent("Reward Item Title Property Key",
                "The key defined in the Reward's Static Properties that specifies how to label each " +
                "Reward Item. If none is specified no title will be displayed. Suggested format for the string value " +
                "the key maps to would be something like \"Day {0}\" where {0} will be replaced with the count + 1 " +
                "of the item in the Reward's list. Used when auto populating reward items.");

            PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedRewardIndex = EditorGUILayout.Popup(rewardContent, m_SelectedRewardIndex, 
                    m_RewardDropdownPopulateHelper.displayNames);
                m_RewardKey_SerializedProperty.stringValue = m_RewardDropdownPopulateHelper
                    .GetKey(m_SelectedRewardIndex);

                if (check.changed)
                {
                    m_SelectedRewardAsset = !string.IsNullOrEmpty(m_RewardKey_SerializedProperty.stringValue)
                        ? PrefabTools.GetLookUpCatalogAsset().FindItem(m_RewardKey_SerializedProperty.stringValue) as RewardAsset
                        : null;
                    PopulatePropertyKeys();
                }
            }

            EditorGUILayout.Space();

            m_SelectedRewardItemTitlePropertyIndex = EditorGUILayout.Popup(rewardItemTitleProperty,
                m_SelectedRewardItemTitlePropertyIndex, m_RewardItemTitlePropertyPopulateHelper.displayNames);
            m_RewardItemTitlePropertyKey_SerializedProperty.stringValue = m_RewardItemTitlePropertyPopulateHelper
                .GetKey(m_SelectedRewardItemTitlePropertyIndex);
        }

        void DrawAutoPopulateRewardItemsSection()
        {
            var payoutItemIconPropertyContent = new GUIContent("Payout Item Icon Property Key",
                "The key for the sprite that is defined in the Static Properties of each of the " +
                "Reward's payout items. If none is specified no image will be displayed. Used when auto populating " +
                "Reward Items.");
            var autoPopulatedRewardItemContent = new GUIContent("Populated Item Container",
                "The Game Object in which to display the Reward Item prefabs.");
            var lockedRewardItemContent = new GUIContent("Locked Prefab",
                "Prefab to use when generating a Reward Item in locked state. Also used as the default " +
                "for other other Reward Item states if they're particular prefab is null. Used when auto " +
                "populating reward items.'");
            var claimableRewardItemContent = new GUIContent("Claimable Prefab",
                "Prefab to use when generating a Reward Item in claimable state. Used when auto " +
                "populating reward items.'");
            var claimedRewardItemContent = new GUIContent("Claimed Prefab",
                "Prefab to use when generating a Reward Item that has been successfully claimed. " +
                "Used when auto populating reward items.'");
            var missedRewardItemContent = new GUIContent("Missed Prefab",
                "Prefab to use when generating a Reward Item that had the opportunity to be claimed but " +
                "was missed. Used when auto populating reward items.'");

            m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue = EditorGUILayout
                .Foldout(m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue,
                    "Auto Populated Reward Items", true);

            if (m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                m_SelectedPayoutItemImagePropertyIndex = EditorGUILayout.Popup(payoutItemIconPropertyContent, 
                    m_SelectedPayoutItemImagePropertyIndex, m_PayoutImagePropertyDropdownHelper.displayNames);
                m_PayoutItemIconSpritePropertyKey_SerializedProperty.stringValue = m_PayoutImagePropertyDropdownHelper
                    .GetKey(m_SelectedPayoutItemImagePropertyIndex);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_AutoPopulatedRewardItemContainer_SerializedProperty, autoPopulatedRewardItemContent);
                EditorGUILayout.PropertyField(m_LockedRewardItemPrefab_SerializedProperty, lockedRewardItemContent);
                EditorGUILayout.PropertyField(m_ClaimableRewardItemPrefab_SerializedProperty, claimableRewardItemContent);
                EditorGUILayout.PropertyField(m_ClaimedRewardItemPrefab_SerializedProperty, claimedRewardItemContent);

                if (m_SelectedRewardAsset == null || !m_SelectedRewardAsset.resetIfExpired)
                {
                    EditorGUILayout.PropertyField(m_MissedRewardItemPrefab_SerializedProperty, missedRewardItemContent);
                }

                EditorGUI.indentLevel--;
            }
        }

        void DrawCountdownSection()
        {
            var countdownFormatContent = new GUIContent("Timer Format",
                "Format in which to display the countdown time.");
            var countdownCooldownDescriptionContent = new GUIContent("Cooldown Description",
                "The string to use as the description on the countdown during Reward cooldown periods.");
            var countdownExpirationDescriptionContent = new GUIContent("Expiration Description",
                "The string to use as the description on the countdown when a Reward Item is available to " +
                "claim, but will expire.");
            var countdownTimerFieldContent = new GUIContent("Countdown Text Field", "Text field in which " +
                "to display the reward's countdown timer. If null, no countdown time will be displayed.");

            m_ShowCountdownEditorFields_SerializedProperty.boolValue = EditorGUILayout
                .Foldout(m_ShowCountdownEditorFields_SerializedProperty.boolValue,
                    "Countdown Display", true);

            if (m_ShowCountdownEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_CountdownDisplayFormat_SerializedProperty, countdownFormatContent);
                EditorGUILayout.PropertyField(m_CountdownCooldownDescription_SerializedProperty,
                    countdownCooldownDescriptionContent);
                EditorGUILayout.PropertyField(m_CountdownExpirationDescription_SerializedProperty,
                    countdownExpirationDescriptionContent);
                EditorGUILayout.PropertyField(m_CountdownTextField_SerializedProperty, countdownTimerFieldContent);

                EditorGUI.indentLevel--;
            }
        }

        void DrawTextSection()
        {
            var titleTextFieldContent = new GUIContent("Title Text Field", "Text field in which to " +
                "display the reward's Display Name.If null, no Reward name will be displayed.");
            var descriptionContent = new GUIContent("Description Property Key",
                "The key defined in the Reward's Static Property which provides the Reward's description. " +
                "If empty or can't be found no description will be shown.");
            var rewardDescriptionFieldContent = new GUIContent("Description Field",
                "The string to use as the description of the Reward. If null, no description will be displayed.");

            m_ShowTextEditorFields_SerializedProperty.boolValue = EditorGUILayout
                .Foldout(m_ShowTextEditorFields_SerializedProperty.boolValue, "Texts", true);

            if (m_ShowTextEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_TitleTextField_SerializedProperty, titleTextFieldContent);

                EditorGUILayout.Space();

                m_SelectedDescriptionPropertyIndex = EditorGUILayout.Popup(descriptionContent,
                    m_SelectedDescriptionPropertyIndex, m_DescriptionPropertyPopulateHelper.displayNames);
                m_DescriptionPropertyKey_SerializedProperty.stringValue = m_DescriptionPropertyPopulateHelper
                    .GetKey(m_SelectedDescriptionPropertyIndex);

                EditorGUILayout.PropertyField(m_DescriptionTextField_SerializedProperty, rewardDescriptionFieldContent);

                EditorGUI.indentLevel--;
            }
        }

        void DrawButtonSection()
        {
            var closeButtonContent = new GUIContent("Close Button",
                "Game object to use for closing the Reward Popup. Will be hidden if Auto Hide Close Button is true");
            var autoHideCloseButtonContent = new GUIContent("Auto Hide Close Button",
                "Specifies whether the close button is invisible when there is claimable Reward Item.");
            var claimButtonContent = new GUIContent("Claim Button", "The Reward Claim Button to use when " +
                "initiating claims from this view. Whichever Reward Item the Reward System says is claimable will be " +
                "set as the item to claim on the button. If null, items will need to be claimed manually.");

            m_ShowButtonEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(
                m_ShowButtonEditorFields_SerializedProperty.boolValue, "Buttons", true);
            if (m_ShowButtonEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_CloseButton_SerializedProperty, closeButtonContent);
                EditorGUILayout.PropertyField(m_AutoHideCloseButton_SerializedProperty, autoHideCloseButtonContent);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_ClaimButton_SerializedProperty, claimButtonContent);

                EditorGUI.indentLevel--;
            }
        }
    }
}
