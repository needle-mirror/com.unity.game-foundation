using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class VirtualTransactionAssetEditor : BaseTransactionCatalogAssetEditor<VirtualTransactionAssetEditor, VirtualTransactionAsset>
    {
        public VirtualTransactionAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawTypeSpecificBlocks(VirtualTransactionAsset transaction)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUI.SetNextControlName("Costs");
                    DrawExchangeDefinition(transaction.m_Costs, "Costs");
                }

                DrawConsumablePayouts(transaction);
            }

            EditorGUILayout.Space();
        }
    }
}
