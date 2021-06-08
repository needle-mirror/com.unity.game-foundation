using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     Custom inspector for a <see cref="DetailedTransactionItemView"/> component.
    /// </summary>
    [CustomEditor(typeof(DetailedTransactionItemView))]
    class DetailedTransactionItemViewEditor : TransactionItemViewEditor
    {
        DetailedTransactionItemView m_DetailedTransactionItemView;

        int m_SelectedPayoutItemIconPropertyIndex = -1;

        DropdownPayoutItemPropertyHelper m_PayoutItemIconPropertyDropdownHelper = new DropdownPayoutItemPropertyHelper();

        SerializedProperty m_ItemPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_CurrencyPayoutCountPrefix_SerializedProperty;
        SerializedProperty m_PayoutItemIconPropertyKey_SerializedProperty;
        SerializedProperty m_PayoutItemsImageContainer_SerializedProperty;
        SerializedProperty m_ShowPayoutEditorFields_SerializedProperty;

        protected override void Setup()
        {
            base.Setup();

            m_DetailedTransactionItemView = target as DetailedTransactionItemView;

            kExcludedFields.Add(nameof(DetailedTransactionItemView.m_ItemPayoutCountPrefix));
            kExcludedFields.Add(nameof(DetailedTransactionItemView.m_CurrencyPayoutCountPrefix));
            kExcludedFields.Add(nameof(DetailedTransactionItemView.m_PayoutItemIconPropertyKey));
            kExcludedFields.Add(nameof(DetailedTransactionItemView.m_PayoutItemsContainer));
            kExcludedFields.Add(nameof(DetailedTransactionItemView.showPayoutEditorFields));

            m_ItemPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_DetailedTransactionItemView.m_ItemPayoutCountPrefix));
            m_CurrencyPayoutCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_DetailedTransactionItemView.m_CurrencyPayoutCountPrefix));
            m_PayoutItemIconPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_DetailedTransactionItemView.m_PayoutItemIconPropertyKey));
            m_PayoutItemsImageContainer_SerializedProperty = serializedObject.FindProperty(nameof(m_DetailedTransactionItemView.m_PayoutItemsContainer));
            m_ShowPayoutEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_DetailedTransactionItemView.showPayoutEditorFields));
        }

        protected override void PopulateStaticPropertyKeys()
        {
            // Calling base even though it causes a double item lookup because it doesn't compile to have a field set
            // both in the parent and child classes.
            base.PopulateStaticPropertyKeys();

            var transactionItem = PrefabTools.GetLookUpCatalogAsset()
                .FindItem(m_TransactionKey_SerializedProperty.stringValue) as BaseTransactionAsset;

            m_SelectedPayoutItemIconPropertyIndex = m_PayoutItemIconPropertyDropdownHelper.Populate(transactionItem,
                m_PayoutItemIconPropertyKey_SerializedProperty.stringValue,
                new []{ PropertyType.ResourcesAsset, PropertyType.Addressables }, false);
        }

        protected override void DrawTransactionDetailSections()
        {
            DrawIconAndNameSection();
            EditorGUILayout.Space();

            DrawPayoutSection();
            EditorGUILayout.Space();

            DrawBadgeSection();
            EditorGUILayout.Space();

            DrawPurchaseButtonSection();
            EditorGUILayout.Space();
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
            var payoutItemsContainerContent = new GUIContent("Payout Items Container",
                "The Game Object in which to display the payout images.");

            m_ShowPayoutEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(
                m_ShowPayoutEditorFields_SerializedProperty.boolValue, "Payouts", true);
            if (m_ShowPayoutEditorFields_SerializedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                if (!m_TransactionItemView.m_IsDrivenByOtherComponent)
                {
                    m_SelectedPayoutItemIconPropertyIndex = EditorGUILayout.Popup(payoutItemIconPropertyKey,
                        m_SelectedPayoutItemIconPropertyIndex, m_PayoutItemIconPropertyDropdownHelper.displayNames);
                    m_PayoutItemIconPropertyKey_SerializedProperty.stringValue =
                        m_PayoutItemIconPropertyDropdownHelper.GetKey(m_SelectedPayoutItemIconPropertyIndex);

                    EditorGUILayout.PropertyField(m_ItemPayoutCountPrefix_SerializedProperty, itemPayoutPrefixContent);
                    EditorGUILayout.PropertyField(m_CurrencyPayoutCountPrefix_SerializedProperty, currencyPayoutPrefixContent);
                }

                EditorGUILayout.PropertyField(m_PayoutItemsImageContainer_SerializedProperty, payoutItemsContainerContent);

                EditorGUI.indentLevel--;
            }
        }
    }
}
