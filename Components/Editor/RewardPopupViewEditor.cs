using System.Collections.Generic;
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
                ? CatalogSettings.catalogAsset.FindItem(m_SelectedRewardKey) as RewardAsset
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
            m_SelectedDescriptionPropertyIndex = m_DescriptionPropertyPopulateHelper.Populate(m_SelectedRewardAsset, m_DescriptionPropertyKey_SerializedProperty.stringValue, PropertyType.String, true);
            m_SelectedRewardItemTitlePropertyIndex = m_RewardItemTitlePropertyPopulateHelper.Populate(m_SelectedRewardAsset, m_RewardItemTitlePropertyKey_SerializedProperty.stringValue, PropertyType.String, true);

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

            m_SelectedPayoutItemImagePropertyIndex = m_PayoutImagePropertyDropdownHelper.Populate(catalogItemAssets, m_PayoutItemIconSpritePropertyKey_SerializedProperty.stringValue, PropertyType.ResourcesAsset, true);
        }

        void DrawBasicRewardContent()
        {
            var rewardContent = new GUIContent("Reward", "The Reward to display in this view.");
            if (m_RewardDropdownPopulateHelper.displayNames != null && m_RewardDropdownPopulateHelper.displayNames.Length > 0)
            {
                m_SelectedRewardIndex = EditorGUILayout.Popup(rewardContent, m_SelectedRewardIndex, m_RewardDropdownPopulateHelper.displayNames);
                var rewardKey = m_RewardDropdownPopulateHelper.GetKey(m_SelectedRewardIndex);
                if (m_RewardPopupView.rewardKey != rewardKey)
                {
                    SetRewardKey(rewardKey);
                }
            }
            else
            {
                EditorGUILayout.Popup(rewardContent, 0, new[] { "None" });
            }

            EditorGUILayout.Space();

            var rewardItemTitleProperty = new GUIContent("Reward Item Title Property Key",
                "The key defined in the Reward's Static Properties that specifies how to label each " +
                "Reward Item. If none is specified no title will be displayed. Suggested format for the string value " +
                "the key maps to would be something like \"Day {0}\" where {0} will be replaced with the count + 1 " +
                "of the item in the Reward's list. Used when auto populating reward items.");
            m_SelectedRewardItemTitlePropertyIndex = EditorGUILayout.Popup(rewardItemTitleProperty, m_SelectedRewardItemTitlePropertyIndex, m_RewardItemTitlePropertyPopulateHelper.displayNames);
            var rewardItemTitlePropertyKey = m_RewardItemTitlePropertyPopulateHelper.GetKey(m_SelectedRewardItemTitlePropertyIndex);
            if (m_RewardPopupView.rewardItemTitlePropertyKey != rewardItemTitlePropertyKey)
            {
                m_RewardPopupView.SetRewardItemTitlePropertyKey(rewardItemTitlePropertyKey);
                m_RewardItemTitlePropertyKey_SerializedProperty.stringValue = rewardItemTitlePropertyKey;
            }
        }

        void DrawAutoPopulateRewardItemsSection()
        {
            m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue, "Auto Populated Reward Items");
            if (m_AutoPopulatedRewardItemEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                var payoutItemIconPropertyContent = new GUIContent("Payout Item Icon Property Key",
                    "The key for the sprite that is defined in the Static Properties of each of the " +
                    "Reward's payout items. If none is specified no image will be displayed. Used when auto populating " +
                    "Reward Items.");
                m_SelectedPayoutItemImagePropertyIndex = EditorGUILayout.Popup(payoutItemIconPropertyContent, m_SelectedPayoutItemImagePropertyIndex, m_PayoutImagePropertyDropdownHelper.displayNames);
                var payoutItemIconPropertyKey = m_PayoutImagePropertyDropdownHelper.GetKey(m_SelectedPayoutItemImagePropertyIndex);
                if (m_RewardPopupView.payoutItemIconSpritePropertyKey != payoutItemIconPropertyKey)
                {
                    m_RewardPopupView.SetPayoutItemIconSpritePropertyKey(payoutItemIconPropertyKey);
                    m_PayoutItemIconSpritePropertyKey_SerializedProperty.stringValue = payoutItemIconPropertyKey;
                }

                EditorGUILayout.Space();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var autoPopulatedRewardItemContent = new GUIContent("Populated Item Container",
                        "The Game Object in which to display the Reward Item prefabs.");
                    Transform autoPopulatedRewardItemContainer = (Transform)EditorGUILayout.ObjectField(autoPopulatedRewardItemContent, m_RewardPopupView.autoPopulatedRewardItemContainer, typeof(Transform), true);
                    if (check.changed)
                    {
                        m_RewardPopupView.SetAutoPopulatedRewardItemContainer(autoPopulatedRewardItemContainer);
                        m_AutoPopulatedRewardItemContainer_SerializedProperty.objectReferenceValue = autoPopulatedRewardItemContainer;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var lockedRewardItemContent = new GUIContent("Locked Prefab",
                        "Prefab to use when generating a Reward Item in locked state. Also used as the default " +
                        "for other other Reward Item states if they're particular prefab is null. Used when auto " +
                        "populating reward items.'");
                    var lockedRewardItemPrefab = (RewardItemView)EditorGUILayout.ObjectField(lockedRewardItemContent, m_RewardPopupView.lockedRewardItemPrefab, typeof(RewardItemView), true);
                    if (check.changed)
                    {
                        m_RewardPopupView.SetLockedRewardItemPrefab(lockedRewardItemPrefab);
                        m_LockedRewardItemPrefab_SerializedProperty.objectReferenceValue = lockedRewardItemPrefab;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var claimableRewardItemContent = new GUIContent("Claimable Prefab",
                        "Prefab to use when generating a Reward Item in claimable state. Used when auto " +
                        "populating reward items.'");
                    var claimableRewardItemPrefab = (RewardItemView)EditorGUILayout.ObjectField(claimableRewardItemContent,
                        m_RewardPopupView.claimableRewardItemPrefab, typeof(RewardItemView), true);
                    if (check.changed)
                    {
                        m_RewardPopupView.SetClaimableRewardItemPrefab(claimableRewardItemPrefab);
                        m_ClaimableRewardItemPrefab_SerializedProperty.objectReferenceValue = claimableRewardItemPrefab;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var claimedRewardItemContent = new GUIContent("Claimed Prefab",
                        "Prefab to use when generating a Reward Item that has been successfully claimed. " +
                        "Used when auto populating reward items.'");
                    var claimedRewardItemPrefab = (RewardItemView)EditorGUILayout.ObjectField(claimedRewardItemContent, m_RewardPopupView.claimedRewardItemPrefab, typeof(RewardItemView), true);
                    if (check.changed)
                    {
                        m_RewardPopupView.SetClaimedRewardItemPrefab(claimedRewardItemPrefab);
                        m_ClaimedRewardItemPrefab_SerializedProperty.objectReferenceValue = claimedRewardItemPrefab;
                    }
                }

                if (m_SelectedRewardAsset == null || !m_SelectedRewardAsset.resetIfExpired)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var missedRewardItemContent = new GUIContent("Missed Prefab",
                            "Prefab to use when generating a Reward Item that had the opportunity to be claimed but " +
                            "was missed. Used when auto populating reward items.'");
                        var missedRewardItemPrefab = (RewardItemView)EditorGUILayout.ObjectField(missedRewardItemContent, m_RewardPopupView.m_MissedRewardItemPrefab, typeof(RewardItemView), true);

                        if (check.changed)
                        {
                            m_RewardPopupView.SetMissedRewardItemPrefab(missedRewardItemPrefab);
                            m_MissedRewardItemPrefab_SerializedProperty.objectReferenceValue = missedRewardItemPrefab;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        void DrawCountdownSection()
        {
            m_ShowCountdownEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowCountdownEditorFields_SerializedProperty.boolValue, "Countdown Display");
            if (m_ShowCountdownEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var countdownFormatContent = new GUIContent("Timer Format",
                        "Format in which to display the countdown time.");
                    var countdownFormat = EditorGUILayout.TextField(countdownFormatContent,
                        m_RewardPopupView.countdownDisplayFormat);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetCountdownDisplayFormat(countdownFormat);
                        m_CountdownDisplayFormat_SerializedProperty.stringValue = countdownFormat;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var countdownCooldownDescriptionContent = new GUIContent("Cooldown Description",
                        "The string to use as the description on the countdown during Reward cooldown periods.");
                    var countdownCooldownDescription = EditorGUILayout.TextField(countdownCooldownDescriptionContent,
                        m_RewardPopupView.countdownCooldownDescription);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetCountdownCooldownDescription(countdownCooldownDescription);
                        m_CountdownCooldownDescription_SerializedProperty.stringValue = countdownCooldownDescription;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var countdownExpirationDescriptionContent = new GUIContent("Expiration Description",
                        "The string to use as the description on the countdown when a Reward Item is available to claim, but will expire.");
                    var countdownExpirationDescription = EditorGUILayout.TextField(
                        countdownExpirationDescriptionContent, m_RewardPopupView.countdownExpirationDescription);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetCountdownExpirationDescription(countdownExpirationDescription);
                        m_CountdownExpirationDescription_SerializedProperty.stringValue =
                            countdownExpirationDescription;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var countdownTimerFieldContent = new GUIContent("Countdown Text Field",
                        "Text field in which to display the reward's countdown timer. If null, no countdown time will be displayed.");
                    var countdownTimerField =
                        (Text)EditorGUILayout.ObjectField(countdownTimerFieldContent,
                            m_RewardPopupView.countdownTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetCountdownTimerField(countdownTimerField);
                        m_CountdownTextField_SerializedProperty.objectReferenceValue = countdownTimerField;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        void DrawTextSection()
        {
            m_ShowTextEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowTextEditorFields_SerializedProperty.boolValue, "Texts");
            if (m_ShowTextEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var titleTextFieldContent = new GUIContent("Title Text Field",
                        "Text field in which to display the reward's Display Name. If null, no Reward name will be displayed.");
                    var titleTextField = (Text)EditorGUILayout.ObjectField(titleTextFieldContent,
                        m_RewardPopupView.titleTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetTitleTextField(titleTextField);
                        m_TitleTextField_SerializedProperty.objectReferenceValue = titleTextField;
                    }
                }

                EditorGUILayout.Space();

                var descriptionContent = new GUIContent("Description Property Key",
                    "The key defined in the Reward's Static Property which provides the Reward's description. If empty or can't be found no description will be shown.");
                m_SelectedDescriptionPropertyIndex = EditorGUILayout.Popup(descriptionContent, m_SelectedDescriptionPropertyIndex, m_DescriptionPropertyPopulateHelper.displayNames);
                var descriptionPropertyKey = m_DescriptionPropertyPopulateHelper.GetKey(m_SelectedDescriptionPropertyIndex);
                if (m_RewardPopupView.descriptionPropertyKey != descriptionPropertyKey)
                {
                    m_RewardPopupView.SetDescriptionPropertyKey(descriptionPropertyKey);
                    m_DescriptionPropertyKey_SerializedProperty.stringValue = descriptionPropertyKey;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var rewardDescriptionFieldContent = new GUIContent("Description Field",
                        "The string to use as the description of the Reward. If null, no description will be displayed.");
                    var rewardDescriptionTextField = (Text)EditorGUILayout.ObjectField(rewardDescriptionFieldContent,
                        m_RewardPopupView.descriptionTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetDescriptionTextField(rewardDescriptionTextField);
                        m_DescriptionTextField_SerializedProperty.objectReferenceValue = rewardDescriptionTextField;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        void DrawButtonSection()
        {
            m_ShowButtonEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowButtonEditorFields_SerializedProperty.boolValue, "Buttons");
            if (m_ShowButtonEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var closeButtonContent = new GUIContent("Close Button", string.Empty);
                    var closeButton = (Transform)EditorGUILayout.ObjectField(closeButtonContent,
                        m_RewardPopupView.closeButton, typeof(Transform), true);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetCloseButton(closeButton);
                        m_CloseButton_SerializedProperty.objectReferenceValue = closeButton;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var autoHideCloseButtonContent = new GUIContent("Auto Hide Close Button", "Specifies whether the close button is invisible when there is claimable Reward Item.");
                    var autoHideCloseButton = EditorGUILayout.Toggle(autoHideCloseButtonContent, m_RewardPopupView.m_AutoHideCloseButton);

                    if (check.changed)
                    {
                        m_RewardPopupView.m_AutoHideCloseButton = autoHideCloseButton;
                        m_AutoHideCloseButton_SerializedProperty.boolValue = autoHideCloseButton;
                    }
                }

                EditorGUILayout.Space();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var claimButtonContent = new GUIContent("Claim Button",
                        "The Reward Claim Button to use when initiating claims from this view. " +
                        "Whichever Reward Item the Reward System says is claimable will be set as the item to claim on the button. " +
                        "If null, items will need to be claimed manually.");
                    var claimButton = (RewardClaimButton)EditorGUILayout.ObjectField(claimButtonContent,
                        m_RewardPopupView.claimButton, typeof(RewardClaimButton), true);

                    if (check.changed)
                    {
                        m_RewardPopupView.SetClaimButton(claimButton);
                        m_ClaimButton_SerializedProperty.objectReferenceValue = claimButton;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        void SetRewardKey(string key)
        {
            if (m_SelectedRewardKey == key)
                return;

            m_SelectedRewardKey = key;
            m_SelectedRewardAsset = !string.IsNullOrEmpty(key)
                ? CatalogSettings.catalogAsset.FindItem(key) as RewardAsset
                : null;

            // Update the serialized value
            m_RewardKey_SerializedProperty.stringValue = key;
            m_RewardPopupView.SetRewardKey(key);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();

            PopulatePropertyKeys();
        }
    }
}
