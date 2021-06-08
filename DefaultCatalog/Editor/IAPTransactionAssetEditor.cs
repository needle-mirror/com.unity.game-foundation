using System;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using static UnityEditor.EditorGUILayout;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class IAPTransactionAssetEditor : BaseTransactionCatalogAssetEditor<IAPTransactionAssetEditor, IAPTransactionAsset>
    {
        public IAPTransactionAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawTypeSpecificBlocks(IAPTransactionAsset transaction)
        {
            using (new VerticalScope())
            {
                DrawProductIdFields(transaction);

                Space();

                if (PurchasingSettings.enabled)
                {
                    switch (transaction.productType)
                    {
                        case IAPProductType.Undetermined:

                            using (new VerticalScope())
                            {
                                LabelField("Payouts", GameFoundationEditorStyles.titleStyle);

                                using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                                {
                                    HelpBox(
                                        "To enable payouts, first supply a product id that matches a consumable product configured in the IAP Catalog.",
                                        MessageType.Info);
                                }
                            }
                            break;

                        case IAPProductType.Consumable:

                            DrawConsumablePayouts(transaction);

                            break;

                        case IAPProductType.NonConsumable:
                        case IAPProductType.Subscription:

                            using (new VerticalScope())
                            {
                                LabelField("Payouts", GameFoundationEditorStyles.titleStyle);

                                using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                                {
                                    HelpBox(
                                        "Automatic payouts are only supported for Consumable IAP products.",
                                        MessageType.Info);
                                }
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                    {
                        HelpBox(
                            "Purchasing is not enabled. You can enable it by going to Window > General > Services.",
                            MessageType.Info);
                    }
                }
            }

            Space();
        }

        static void DrawProductIdFields(IAPTransactionAsset iapTransaction)
        {
            GUILayout.Label("Product Ids", GameFoundationEditorStyles.titleStyle);

            using (new VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    var appleId = DelayedTextField(new GUIContent("Apple Identifier"), iapTransaction.appleId);
                    var googleId = DelayedTextField(new GUIContent("Google Product ID"), iapTransaction.googleId);

                    if (checkScope.changed)
                    {
                        iapTransaction.Editor_SetAppleId(appleId);
                        iapTransaction.Editor_SetGoogleId(googleId);
                        iapTransaction.RefreshProductType();
                    }
                }

                using (new HorizontalScope())
                {
                    PrefixLabel("IAP Product Type:");
                    LabelField(iapTransaction.productType.ToString());
                }

                if (iapTransaction.productType == IAPProductType.Subscription)
                {
                    HelpBox(
                        "Subscription purchases are not yet supported by Game Foundation.",
                        MessageType.Warning);
                }
            }
        }
    }
}
