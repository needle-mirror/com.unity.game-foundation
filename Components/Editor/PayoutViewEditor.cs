using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="PayoutView"/> component.
    /// </summary>
    [CustomEditor(typeof(PayoutView))]
    public class PayoutViewEditor : Editor
    {
        PayoutView m_PayoutView;

        int m_SelectedTransactionIndex = -1;
        int m_SelectedPayoutItemIconPropertyIndex = -1;

        DropdownCatalogItemHelper<BaseTransactionAsset> m_TransactionDropdownHelper = new DropdownCatalogItemHelper<BaseTransactionAsset>();
        DropdownPayoutItemPropertyHelper m_PayoutItemIconPropertyDropdownHelper = new DropdownPayoutItemPropertyHelper();

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_ItemPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_CurrencyPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_PayoutItemIconPropertyKey_SerializedProperty;
        SerializedProperty m_PayoutItemPrefab_SerializedProperty;
        SerializedProperty m_SeparatorPrefab_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(PayoutView.m_TransactionKey),
            nameof(PayoutView.m_ItemPayoutCountPrefix),
            nameof(PayoutView.m_CurrencyPayoutCountPrefix),
            nameof(PayoutView.m_PayoutItemIconPropertyKey),
            nameof(PayoutView.m_PayoutItemPrefab),
            nameof(PayoutView.m_SeparatorPrefab)
        };

        void OnEnable()
        {
            m_PayoutView = target as PayoutView;

            Setup();
            PopulateTransactions();
            PopulateStaticPropertyKeys();

            // To update the content when the GameObject is selected
            m_PayoutView.UpdateContent();
        }

        void Setup()
        {
            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_TransactionKey));
            m_ItemPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_ItemPayoutCountPrefix));
            m_CurrencyPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_CurrencyPayoutCountPrefix));
            m_PayoutItemIconPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_PayoutItemIconPropertyKey));
            m_PayoutItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_PayoutItemPrefab));
            m_SeparatorPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_PayoutView.m_SeparatorPrefab));
        }

        void PopulateTransactions()
        {
            m_SelectedTransactionIndex = m_TransactionDropdownHelper.Populate(m_TransactionKey_SerializedProperty.stringValue);
        }

        void PopulateStaticPropertyKeys()
        {
            var transactionItem = PrefabTools.GetLookUpCatalogAsset()
                .FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset;

            m_SelectedPayoutItemIconPropertyIndex = m_PayoutItemIconPropertyDropdownHelper.Populate(transactionItem,
                m_PayoutItemIconPropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables }, false);
        }

        /// <summary>
        ///     Draw the Inspector GUI for the selected PayoutView.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (m_PayoutView.m_IsDrivenByOtherComponent)
            {
                EditorGUILayout.HelpBox("Settings are driven by a parent object.", MessageType.None);
            }
            else
            {
                DrawTransactionSection();
                EditorGUILayout.Space();
                DrawPayoutSection();
                EditorGUILayout.Space();
            }
            DrawPrefabsSection();
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void DrawTransactionSection()
        {
            var itemDisplayContent = new GUIContent("Transaction Item", 
                "The Transaction Item to display in this button");

            PrefabTools.DisplayCatalogOverrideAlertIfNecessary();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedTransactionIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedTransactionIndex,
                    m_TransactionDropdownHelper.displayNames);
                m_TransactionKey_SerializedProperty.stringValue = m_TransactionDropdownHelper
                    .GetKey(m_SelectedTransactionIndex);

                if (check.changed)
                {
                    PopulateStaticPropertyKeys();
                }
            }
        }

        void DrawPayoutSection()
        {
            var payoutItemIconPropertyKey = new GUIContent("Payout Items Icon Property Key",
                "The key for the payout items' icons, defined in the Static Properties of each of the " +
                "Transaction's Payout Items. If none is specified no payouts will be displayed.");
            var itemPayoutPrefixContent = new GUIContent("Item Count Prefix",
                "The string to add as a prefix to each item's payout count.");
            var currencyPayoutPrefixContent = new GUIContent("Currency Count Prefix",
                "The string to add as a prefix to each currency's payout count.");

            m_SelectedPayoutItemIconPropertyIndex = EditorGUILayout.Popup(payoutItemIconPropertyKey,
                    m_SelectedPayoutItemIconPropertyIndex, m_PayoutItemIconPropertyDropdownHelper.displayNames);
            m_PayoutItemIconPropertyKey_SerializedProperty.stringValue =
                    m_PayoutItemIconPropertyDropdownHelper.GetKey(m_SelectedPayoutItemIconPropertyIndex);

            EditorGUILayout.PropertyField(m_ItemPayoutCountPrefix_SerializedProperty, itemPayoutPrefixContent);
            EditorGUILayout.PropertyField(m_CurrencyPayoutCountPrefix_SerializedProperty, currencyPayoutPrefixContent);
        }

        void DrawPrefabsSection()
        {
            var payoutItemContent = new GUIContent("Payout Item Prefab",
                "Prefab to use for displaying payout item images.");
            var separatorContent = new GUIContent("Separator Prefab", 
                "Game Object to place in between each payout item in the auto generated promo image. " +
                "If none is assigned, no separator will be used.");

            EditorGUILayout.PropertyField(m_PayoutItemPrefab_SerializedProperty, payoutItemContent);
            EditorGUILayout.PropertyField(m_SeparatorPrefab_SerializedProperty, separatorContent);
        }
    }
}
