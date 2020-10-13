using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class VirtualTransactionCatalogAssetEditor : BaseTransactionCatalogAssetEditor<VirtualTransactionCatalogAssetEditor, VirtualTransactionAsset>
    {
        public VirtualTransactionCatalogAssetEditor(string name, EditorWindow window)
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

                DrawPayouts(transaction);
            }

            EditorGUILayout.Space();
        }
    }
}
