using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class BaseTransactionCatalogAssetEditor<TTransactionEditor, TTransactionAsset>
        : BaseCatalogAssetEditor<TTransactionAsset>
        where TTransactionEditor : BaseTransactionCatalogAssetEditor<TTransactionEditor, TTransactionAsset>
        where TTransactionAsset : BaseTransactionAsset
    {
        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Transactions;

        protected BaseTransactionCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected void DrawConsumablePayouts(TTransactionAsset transaction)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUI.SetNextControlName("Payouts");
                DrawExchangeDefinition(transaction.payout, "Payouts");
            }
        }

        protected void DrawExchangeDefinition(TransactionExchangeDefinitionObject exchangeDefinitionObject, string title)
        {
            EditorGUILayout.LabelField(title, GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                ExchangeEditor.DrawExchangeUI(exchangeDefinitionObject);
            }
        }
    }
}
