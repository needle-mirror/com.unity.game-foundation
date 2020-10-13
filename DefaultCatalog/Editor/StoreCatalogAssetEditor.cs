using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using DCTools = UnityEngine.GameFoundation.DefaultCatalog.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class StoreCatalogAssetEditor : BaseCatalogAssetEditor<StoreAsset>
    {
        StoreItemObject[] m_StoreItems;
        StoreItemObject m_StoreItemToMoveDown;
        StoreItemObject m_StoreItemToMoveUp;
        StoreItemObject m_StoreItemToRemove;

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Stores;

        public StoreCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawSidebarListItem(StoreAsset store)
        {
            BeginSidebarItem(store, new Vector2(242f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(store.displayName, 242, GameFoundationEditorStyles.boldTextStyle);

            EndSidebarItem();
        }

        protected override void DrawGeneralFields(StoreAsset catalogItemAsset)
        {
            if (IsKeyReserved(catalogItemAsset.key))
            {
                GUI.enabled = false;
                CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
            }
        }

        protected override void DrawTypeSpecificBlocks(StoreAsset store)
        {
            using (DCTools.Pools.storeItemList.Get(out var storeItems))
            {
                store.GetStoreItems(storeItems);
                m_StoreItems = storeItems.ToArray();
            }

            DrawItemsInStore(store);

            EditorGUILayout.Space();

            DrawItemsNotInStore(store);

            EditorGUILayout.Space();
        }

        void DrawItemsInStore(StoreAsset store)
        {
            m_StoreItemToMoveUp = null;
            m_StoreItemToMoveDown = null;
            m_StoreItemToRemove = null;

            var transactionAssets = new List<BaseTransactionAsset>();
            CatalogSettings.catalogAsset.GetItems(transactionAssets);

            var storeTransactionsLabel =
                new GUIContent(
                    "Store Transactions",
                    "At runtime, the store will only show the transactions in this list, as long as they are marked as visible.");

            EditorGUILayout.LabelField(storeTransactionsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var anyAddedTransactionsFlag = m_StoreItems != null && m_StoreItems.Length > 0;

                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Transactions", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(anyAddedTransactionsFlag ? "Visible" : "", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(80));

                    GUILayout.Space(64);
                }

                if (anyAddedTransactionsFlag)
                {
                    for (var i = 0; i < m_StoreItems.Length; i++)
                    {
                        var defaultStoreItem = m_StoreItems[i];

                        var transaction = transactionAssets.FirstOrDefault(item => item == defaultStoreItem.m_Transaction);

                        if (transaction == null)
                        {
                            continue;
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.LabelField(transaction.displayName, GUILayout.Width(300));

                            GUILayout.FlexibleSpace();

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(transaction != null);

                            var newVisibleFlag = EditorGUILayout.Toggle(defaultStoreItem.enabled, GUILayout.Width(70));
                            if (newVisibleFlag != defaultStoreItem.enabled)
                            {
                                defaultStoreItem.Editor_SetEnabled(newVisibleFlag);
                            }

                            GUILayout.Space(5);

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_StoreItems.Length - 1);

                            if (GUILayout.Button("\u25BC", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToMoveDown = defaultStoreItem;
                                m_StoreItemToMoveUp = m_StoreItems[i + 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i > 0);

                            if (GUILayout.Button("\u25B2", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToMoveUp = defaultStoreItem;
                                m_StoreItemToMoveDown = m_StoreItems[i - 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(true);

                            GUILayout.Space(5);

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToRemove = defaultStoreItem;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("no transactions in store");
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUILayout.Space();
                }
            }

            if (m_StoreItemToMoveUp != null && m_StoreItemToMoveDown != null)
            {
                SwapStoreItems(store, m_StoreItemToMoveUp, m_StoreItemToMoveDown);
            }

            if (m_StoreItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    store.Editor_RemoveItem(m_StoreItemToRemove);
                }
            }

            if (m_StoreItemToMoveUp != null || m_StoreItemToMoveDown != null || m_StoreItemToRemove != null)
            {
                GUI.FocusControl(null);
            }
        }

        void DrawItemsNotInStore(StoreAsset store)
        {
            var transactionAssets = new List<BaseTransactionAsset>();
            CatalogSettings.catalogAsset.GetItems(transactionAssets);

            var otherAvailableItemsLabel =
                new GUIContent(
                    "Other Available Transactions",
                    "Transactions that are eligible to be added to this store.");

            EditorGUILayout.LabelField(otherAvailableItemsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Transactions", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
                    GUILayout.FlexibleSpace();
                }

                var validItemCount = 0;

                foreach (var transactionAsset in transactionAssets)
                {
                    if (m_StoreItems.Length > 0 && m_StoreItems.Any(storeItem => storeItem.m_Transaction == transactionAsset))
                    {
                        continue;
                    }

                    validItemCount++;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(5);

                        EditorGUILayout.LabelField(transactionAsset.displayName);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add To Store", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(150)))
                        {
                            store.Editor_AddItem(transactionAsset);
                        }
                    }
                }

                if (validItemCount > 0)
                {
                    return;
                }

                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("no transactions available");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }
        }

        void SwapStoreItems(StoreAsset store, StoreItemObject storeItem1, StoreItemObject storeItem2)
        {
            store.Editor_SwapItemsListOrder(storeItem1, storeItem2);
        }

        static bool IsKeyReserved(string key)
        {
            return key == CatalogAsset.k_MainStoreDefinitionKey;
        }
    }
}
