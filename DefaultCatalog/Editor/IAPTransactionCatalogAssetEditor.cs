using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using static UnityEditor.EditorGUILayout;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class IAPTransactionCatalogAssetEditor : BaseTransactionCatalogAssetEditor<IAPTransactionCatalogAssetEditor, IAPTransactionAsset>
    {
        public IAPTransactionCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawTypeSpecificBlocks(IAPTransactionAsset transaction)
        {
            using (new VerticalScope())
            {
                DrawProductIdFields(transaction);

                Space();

                DrawPayouts(transaction);
            }

            Space();
        }

        static void DrawProductIdFields(IAPTransactionAsset iapTransaction)
        {
            GUILayout.Label("Product Ids", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    var appleId = TextField(new GUIContent("Apple Identifier"), iapTransaction.appleId);
                    if (checkScope.changed)
                    {
                        iapTransaction.Editor_SetAppleId(appleId);
                    }
                }

                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    var googleId = TextField(new GUIContent("Google Product ID"), iapTransaction.googleId);

                    if (checkScope.changed)
                    {
                        iapTransaction.Editor_SetGoogleId(googleId);
                    }
                }
            }
        }
    }
}
