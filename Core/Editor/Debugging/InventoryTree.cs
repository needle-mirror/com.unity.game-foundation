using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEditor.GameFoundation.Debugging
{
    class InventoryTree : TreeView
    {
        enum Columns
        {
            Items,
            Value,
            Action,
        }

        static readonly Texture2D k_InventoryItemIcon = EditorGUIUtility.FindTexture("Prefab Icon");

        static readonly Texture2D k_PropertyItemIcon = EditorGUIUtility.FindTexture("GameManager Icon");

        /// <summary>
        ///     Contain all tree view item holding data about GameFoundation items.
        ///     Utility nodes like "Wallet" or "Items" are not included.
        /// </summary>
        List<TreeViewItem> m_AllTreeViewItems = new List<TreeViewItem>();

        IList<int> m_ExpandedIdsBeforeSearch;

        string m_SearchString = string.Empty;

        public string SearchString
        {
            get => m_SearchString;
            set
            {
                if (m_SearchString != value)
                {
                    m_SearchString = value;
                    Reload();
                }
            }
        }

        List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        public int itemCount => m_InventoryItems.Count;

        DebugEditorWindow m_Owner;

        ResourcesAssetDrawer m_ResourcesAssetDrawer;

        IInventoryManager m_SubscribedInventory;

        IWalletManager m_SubscribedWallet;

        IRewardManager m_SubscribedRewards;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<InventoryTree>();

        public InventoryTree(DebugEditorWindow owner, TreeViewState state = null, MultiColumnHeader multiColumnHeader = null)
            : base(state ?? new TreeViewState(), multiColumnHeader)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            //Reset AddItem PopUp Window Index when something else is selected
            m_Owner.ClearIndexes();

            base.SelectionChanged(selectedIds);
        }

        protected override void DoubleClickedItem(int id)
        {
            //Ensure only one row is selected at a time.
            state.selectedIDs.Clear();
            state.selectedIDs.Add(id);
        }

        protected override void SingleClickedItem(int id)
        {
            //Ensure only one row is selected at a time.
            state.selectedIDs.Clear();
            state.selectedIDs.Add(id);
        }

        protected override TreeViewItem BuildRoot()
        {
            var inventoryRoot = GenerateInventoryTreeRoot();

            //Filter generated root according to search and save previous expanded state.
            if (!string.IsNullOrEmpty(SearchString))
            {
                if (m_ExpandedIdsBeforeSearch == null)
                {
                    m_ExpandedIdsBeforeSearch = GetExpanded();
                }

                FilterRootOnSearch(inventoryRoot, SearchString, m_AllTreeViewItems);
            }
            else if (m_ExpandedIdsBeforeSearch != null)
            {
                SetExpanded(m_ExpandedIdsBeforeSearch);
                m_ExpandedIdsBeforeSearch = null;
            }

            if (!inventoryRoot.hasChildren)
            {
                inventoryRoot.AddChild(new TreeViewItem());
            }

            return inventoryRoot;
        }

        //Called when drawing the rows of the Tree View
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            if (item is null) return;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i));
            }
        }

        //This function processes a single cell based on the column its in and renders it in a different way.
        void CellGUI(Rect cellRect, TreeViewItem viewItem, Columns column)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Items:
                {
                    ItemColumnGUI(cellRect, viewItem);
                    break;
                }
                case Columns.Value:
                {
                    ValueColumnGUI(cellRect, viewItem);
                    break;
                }
                case Columns.Action:
                {
                    ActionColumnGUI(cellRect, viewItem);
                    break;
                }
            }
        }

        void ActionColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 50,
                alignment = TextAnchor.MiddleCenter
            };
            var centeredButtonPosition = cellRect;
            centeredButtonPosition.x += cellRect.width / 2 - buttonStyle.fixedWidth / 2;

            switch (viewItem)
            {
                case InventoryItemView itemView:
                {
                    var clicked = GUI.Button(centeredButtonPosition, "Delete", buttonStyle);

                    if (clicked)
                    {
                        HandleSelectionRemoved(viewItem);

                        //Get Inventory from tree item parent to get InventoryItem
                        var inventoryItem = itemView.inventoryItem;
                        GameFoundationSdk.inventory.Delete(inventoryItem);
                        CorrectFoldouts(viewItem);
                    }

                    break;
                }

                case PropertyView propertyView:
                {
                    var clicked = GUI.Button(centeredButtonPosition, "Reset", buttonStyle);

                    if (clicked)
                    {
                        var inventoryItem = propertyView.inventoryItem;
                        var definition = propertyView.property;
                        inventoryItem.ResetMutableProperty(definition.key);
                        CorrectFoldouts(viewItem);
                    }

                    break;
                }
            }
        }

        void ValueColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            var guiStyle = new GUIStyle(IsSelected(viewItem.id) ? GUI.skin.textField : GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            //Bold Item Quantities
            if (viewItem is InventoryItemView)
            {
                guiStyle.fontStyle = FontStyle.Bold;
            }

            switch (viewItem)
            {
                case PropertyView propertyView:
                    try
                    {
                        var inventoryItem = propertyView.inventoryItem;
                        var definition = propertyView.property;

                        var property = inventoryItem.GetMutableProperty(definition.key);
                        Property newValue = default;

                        switch (definition.value.type)
                        {
                            case PropertyType.Long:
                            {
                                newValue = EditorGUI.LongField(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.Double:
                            {
                                newValue = EditorGUI.DoubleField(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.Bool:
                            {
                                newValue = EditorGUI.Toggle(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.String:
                            {
                                newValue = EditorGUI.TextField(cellRect, property, guiStyle);

                                break;
                            }

                            //Not merged with String case since this is bound to change soon.
                            case PropertyType.ResourcesAsset:
                            {
                                if (m_ResourcesAssetDrawer == null)
                                {
                                    m_ResourcesAssetDrawer = new ResourcesAssetDrawer();
                                }

                                var assetPath = property.AsString();
                                assetPath = m_ResourcesAssetDrawer.Draw(cellRect, assetPath, null);
                                newValue = Property.CreateAssetProperty(assetPath);

                                break;
                            }

                            case PropertyType.Addressables:
                            {
                                newValue = EditorGUI.TextField(cellRect, property, guiStyle);

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException(
                                    $"{nameof(InventoryTree)}: {definition.value.type} of property isn't handled.");
                        }

                        if (newValue != property)
                        {
                            if (property.type == PropertyType.Addressables)
                            {
                                if (property.AsString() != newValue.AsString())
                                {
                                    newValue = Property.CreateAddressablesProperty(newValue.AsString());
                                    inventoryItem.SetMutableProperty(definition.key, newValue);
                                }
                            }
                            else
                            {
                                inventoryItem.SetMutableProperty(definition.key, newValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (GFTools.ShouldRethrowException(e))
                        {
                            throw;
                        }
                        string errorMessage = "The debugger encountered a problem while trying to display a property.";
                        k_GFLogger.LogException(errorMessage, e);
                    }

                    break;
                case CurrencyView currencyView:
                    try
                    {
                        var currency = currencyView.currency;

                        var balance = GameFoundationSdk.wallet.Get(currency);

                        var newValue = EditorGUI.LongField(cellRect, balance, guiStyle);
                        if (newValue != balance)
                        {
                            var done = GameFoundationSdk.wallet.Set(currency, newValue);
                            if (!done)
                            {
                                string errorMessage = "Debugger asked WalletManager to change the balance of " +
                                    $"{currency.displayName} from {balance} to {newValue} but it " +
                                    "was unable to change.";
                                k_GFLogger.LogError(errorMessage);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string errorMessage = "The debugger encountered a problem while trying to display a currency.";
                        k_GFLogger.LogException(errorMessage, e);
                    }

                    break;

                case InventoryItemView itemView when itemView.inventoryItem is StackableInventoryItem stackableItem:
                    try
                    {
                        var oldQuantity = stackableItem.quantity;
                        var newQuantity = Math.Abs(EditorGUI.LongField(cellRect, oldQuantity, guiStyle));
                        if (newQuantity != oldQuantity)
                        {
                            stackableItem.SetQuantity(newQuantity);
                        }
                    }
                    catch (Exception e)
                    {
                        string errorMessage = "The debugger encountered a problem while trying to display an inventory item.";
                        k_GFLogger.LogException(errorMessage, e);
                    }

                    break;

                case RewardView rewardView:
                    string rewardState;
                    if (rewardView.reward.IsInCooldown())
                    {
                        rewardState = "Cooldown";
                    }
                    else if (rewardView.reward.claimTimestamps?.Count <= 0)
                    {
                        rewardState = "Ready";
                    }
                    else if (rewardView.reward.rewardDefinition.expirationSeconds > 0)
                    {
                        rewardState = "Expiring";
                    }
                    else
                    {
                        rewardState = "Claimable";
                    }

                    EditorGUI.LabelField(cellRect, rewardState);
                    break;

                case RewardItemView rewardItemView:
                    EditorGUI.LabelField(cellRect, rewardItemView.rewardItem.value.ToString());
                    break;
            }
        }

        void ItemColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            //Make Room for Icon between Arrow and Label
            Rect tempRect = cellRect;
            tempRect.x += GetContentIndent(viewItem);
            tempRect.width = 16f;

            //Get clipping width for cell and remaining column width.
            tempRect.width = cellRect.width - GetContentIndent(viewItem);

            var labelText = viewItem.displayName;

            GUI.Label(tempRect, labelText, DefaultStyles.label);
        }

        void FilterRootOnSearch(TreeViewItem root, string search, IEnumerable<TreeViewItem> allItems)
        {
            var foundItems = new List<TreeViewItem>();
            foreach (var item in allItems)
            {
                var searchableString = GetSearchableString(item);
                if (searchableString.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    foundItems.Add(item);
            }

            var newExpandedState = new HashSet<int>();
            foreach (var foundItem in foundItems)
            {
                newExpandedState.UnionWith(AddItemAndItsAncestorsIDs(foundItem));
            }

            state.expandedIDs = new List<int>(newExpandedState);
            state.expandedIDs.Sort();

            RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(root, foundItems);
        }

        void RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(TreeViewItem item, List<TreeViewItem> foundItems)
        {
            if (!item.hasChildren)
                return;

            for (var i = item.children.Count - 1; i >= 0; i--)
            {
                var child = item.children[i];
                if (child.hasChildren)
                {
                    if (!IsExpanded(child.id))
                        item.children.RemoveAt(i);
                    else
                    {
                        // remove collapsed items
                        RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(child, foundItems);
                    }
                }
                else if (!foundItems.Contains(child))
                {
                    // remove leaf items not matching search result
                    item.children.RemoveAt(i);
                }
            }
        }

        static IEnumerable<int> AddItemAndItsAncestorsIDs(TreeViewItem item)
        {
            var results = new List<int> { item.id };
            var cur = item;
            while (cur.parent != null)
            {
                results.Add(cur.parent.id);
                cur = cur.parent;
            }

            return results;
        }

        void HandleSelectionRemoved(TreeViewItem item)
        {
            var selectedIds = new List<int>(GetSelection());
            selectedIds.Remove(item.id);

            foreach (var id in selectedIds)
            {
                if (FindItem(id) == null)
                {
                    selectedIds.Remove(id);
                }
            }

            SetSelection(selectedIds);
        }

        /// <summary>
        ///     Correct expanded foldouts state when removing a row.
        /// </summary>
        /// <param name="deletedItem">
        ///     The removed row.
        /// </param>
        void CorrectFoldouts(TreeViewItem deletedItem)
        {
            int GetLastId(TreeViewItem item)
            {
                while (item.hasChildren)
                {
                    item = item.children[item.children.Count - 1];
                }

                return item.id;
            }

            var lastIdInChildren = GetLastId(deletedItem);
            var lastRemovedIndexInExpandedIds = deletedItem.id;
            for (var i = deletedItem.id; i <= lastIdInChildren; i++)
            {
                if (state.expandedIDs.Remove(i))
                {
                    lastRemovedIndexInExpandedIds = i;
                }
            }

            var removeOffset = lastIdInChildren - deletedItem.id + 1;
            for (var i = 0; i < state.expandedIDs.Count; i++)
            {
                if (state.expandedIDs[i] > lastRemovedIndexInExpandedIds)
                    state.expandedIDs[i] -= removeOffset;
            }
        }

        TreeViewItem GenerateInventoryTreeRoot()
        {
            if (GameFoundationSdk.catalog is null || GameFoundationSdk.inventory is null)
            {
                return new TreeViewItem(0, -1, "Root");
            }

            var id = 0;
            m_AllTreeViewItems.Clear();
            var rootView = new TreeViewItem(id++, -1, "Root");

            var currenciesView = new TreeViewItem(id++, 0, "Wallet");
            rootView.AddChild(currenciesView);

            using (GFTools.Pools.currencyList.Get(out var currencies))
            {
                GameFoundationSdk.catalog.GetItems(currencies);
                foreach (var currency in currencies)
                {
                    var currencyView = new CurrencyView(id++, 1, $"{currency.displayName}", currency);
                    m_AllTreeViewItems.Add(currencyView);
                    currenciesView.AddChild(currencyView);
                }
            }

            var itemsView = new TreeViewItem(id++, 0, "Inventory");
            rootView.AddChild(itemsView);

            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            var itemDefinitionNodes = new Dictionary<string, TreeViewItem>(m_InventoryItems.Count);
            foreach (var item in m_InventoryItems)
            {
                if (!itemDefinitionNodes.TryGetValue(item.definition.key, out var definitionNode))
                {
                    definitionNode = new InventoryItemDefinitionView(
                        id++,
                        itemsView.depth + 1,
                        item.definition);

                    itemsView.AddChild(definitionNode);

                    itemDefinitionNodes[item.definition.key] = definitionNode;
                    m_AllTreeViewItems.Add(definitionNode);
                }

                var itemView = new InventoryItemView(
                    id++,
                    definitionNode.depth + 1,
                    item);

                definitionNode.AddChild(itemView);

                itemView.icon = k_InventoryItemIcon;
                m_AllTreeViewItems.Add(itemView);

                var properties = item.definition.defaultMutableProperties;
                if (properties == null)
                    continue;

                foreach (var propertyEntry in properties)
                {
                    var propertyView = new PropertyView(
                        id++,
                        itemView.depth + 1,
                        propertyEntry.Key,
                        item,
                        (propertyEntry.Key, propertyEntry.Value));

                    itemView.AddChild(propertyView);
                    propertyView.icon = k_PropertyItemIcon;
                    m_AllTreeViewItems.Add(propertyView);
                }
            }

            var rewardDefinitionsView = new TreeViewItem(id++, 0, "Rewards");
            using (GFTools.Pools.rewardList.Get(out var rewards))
            {
                GameFoundationSdk.rewards.GetRewards(rewards);
                if (rewards.Count > 0)
                {
                    rootView.AddChild(rewardDefinitionsView);
                }

                foreach (var reward in rewards)
                {
                    var rewardView = new RewardView(id++, 1, $"{reward.rewardDefinition.displayName}", reward);

                    using (GFTools.Pools.rewardItemsList.Get(out var rewardItemDefinitions))
                    {
                        using (GFTools.Pools.rewardItemStateDictionary.Get(out var rewardItems))
                        {
                            reward.rewardDefinition.GetRewardItems(rewardItemDefinitions);
                            rewardItems = reward.rewardItemStates;
                            int count = 0;

                            foreach (var rewardItemDefinition in rewardItemDefinitions)
                            {
                                count++;
                                var rewardItemView = new RewardItemView(id++, rewardDefinitionsView.depth + 1,
                                    $"Reward Item {count}", reward, (rewardItemDefinition.key, rewardItems[rewardItemDefinition.key]));
                                m_AllTreeViewItems.Add(rewardItemView);
                                rewardView.AddChild(rewardItemView);
                            }
                        }
                    }

                    m_AllTreeViewItems.Add(rewardView);
                    rewardDefinitionsView.AddChild(rewardView);
                }
            }

            return rootView;
        }

        public TreeViewItem FindItem(int id)
        {
            return base.FindItem(id, rootItem);
        }

        internal void AttachListeners()
        {
            GameFoundationSdk.initialized += SubscribeIfPossible;
            SubscribeIfPossible();
        }

        internal void DetachListeners()
        {
            GameFoundationSdk.initialized -= SubscribeIfPossible;
            UnsubscribeIfNecessary();
        }

        void SubscribeIfPossible()
        {
            if (GameFoundationSdk.inventory is null)
            {
                m_SubscribedInventory = null;
            }
            else if (!ReferenceEquals(GameFoundationSdk.inventory, m_SubscribedInventory))
            {
                GameFoundationSdk.inventory.itemAdded += OnItemAddedOrRemoved;
                GameFoundationSdk.inventory.itemDeleted += OnItemAddedOrRemoved;
                GameFoundationSdk.inventory.itemQuantityChanged += OnQuantifiableChanged;
                GameFoundationSdk.inventory.itemMutablePropertyChanged += OnPropertyChanged;
                m_SubscribedInventory = GameFoundationSdk.inventory;
            }

            if (GameFoundationSdk.wallet is null)
            {
                m_SubscribedWallet = null;
            }
            else if (!ReferenceEquals(GameFoundationSdk.wallet, m_SubscribedWallet))
            {
                GameFoundationSdk.wallet.balanceChanged += OnQuantifiableChanged;
                m_SubscribedWallet = GameFoundationSdk.wallet;
            }

            if (GameFoundationSdk.rewards is null)
            {
                m_SubscribedRewards = null;
            }
            else if (!ReferenceEquals(GameFoundationSdk.rewards, m_SubscribedRewards))
            {
                GameFoundationSdk.rewards.rewardItemClaimFailed += OnRewardClaimFailed;
                GameFoundationSdk.rewards.rewardItemClaimSucceeded += OnRewardClaimSucceeded;
                Reward.rewardStateChanged += OnRewardStateChanged;
                m_SubscribedRewards = GameFoundationSdk.rewards;
            }
        }

        void UnsubscribeIfNecessary()
        {
            if (!(m_SubscribedInventory is null))
            {
                m_SubscribedInventory.itemAdded -= OnItemAddedOrRemoved;
                m_SubscribedInventory.itemDeleted -= OnItemAddedOrRemoved;
                m_SubscribedInventory.itemQuantityChanged -= OnQuantifiableChanged;
                m_SubscribedInventory.itemMutablePropertyChanged -= OnPropertyChanged;
                m_SubscribedInventory = null;
            }
            if (!(m_SubscribedWallet is null))
            {
                m_SubscribedWallet.balanceChanged -= OnQuantifiableChanged;
                m_SubscribedWallet = null;
            }

            if (!(m_SubscribedRewards is null))
            {
                m_SubscribedRewards.rewardItemClaimFailed -= OnRewardClaimFailed;
                m_SubscribedRewards.rewardItemClaimSucceeded -= OnRewardClaimSucceeded;
                Reward.rewardStateChanged -= OnRewardStateChanged;
                m_SubscribedRewards = null;
            }
        }

        /// <summary>
        ///     Callback to <see cref="IInventoryManager.itemAdded"/>
        ///     and <see cref="IInventoryManager.itemDeleted"/>.
        ///     Reload this tree.
        /// </summary>
        void OnItemAddedOrRemoved(InventoryItem _)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        /// <summary>
        ///     Callback to <see cref="IWalletManager.balanceChanged"/>.
        ///     Reload this tree.
        /// </summary>
        void OnQuantifiableChanged(IQuantifiable _, long __)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        void OnPropertyChanged(PropertyChangedEventArgs _)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        void OnRewardClaimSucceeded(Reward _, string __, Payout ___)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        void OnRewardClaimFailed(string _, string __, Exception ___)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        void OnRewardStateChanged(Reward _)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        /// <summary>
        ///     Get a string from the given <paramref name="itemView"/> to compare to the researched user string.
        /// </summary>
        static string GetSearchableString(TreeViewItem itemView)
        {
            switch (itemView)
            {
                case InventoryItemDefinitionView inventoryItemDefinitionView:
                {
                    return inventoryItemDefinitionView.definition.key;
                }

                case InventoryItemView inventoryItemView:
                {
                    var inventoryItem = inventoryItemView.inventoryItem;
                    return $"{inventoryItem.definition.key} #{inventoryItem.id}";
                }

                case PropertyView propertyView:
                {
                    return propertyView.property.key;
                }

                case CurrencyView currencyView:
                {
                    return currencyView.displayName;
                }

                case RewardView rewardView:
                {
                    return rewardView.displayName;
                }

                case RewardItemView rewardItemView:
                {
                    return $"{rewardItemView.displayName} {rewardItemView.reward.rewardDefinition.displayName}";
                }

                default:
                    throw new ArgumentException($"{nameof(InventoryTree)}: Cannot get real display name of " +
                        $"this {nameof(TreeViewItem)}, unsupported type.");
            }
        }

        //Used by Debug Editor Window
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Items",
                    headerTextAlignment = TextAlignment.Center,
                    width = treeViewWidth - 180,
                    autoResize = true,
                    allowToggleVisibility = true,
                    canSort = false,
                },
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Value",
                    headerContent = new GUIContent("Value"),
                    width = 100,
                    headerTextAlignment = TextAlignment.Center,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Trigger action on Item",
                    headerContent = new GUIContent("Actions"),
                    width = 70,
                    headerTextAlignment = TextAlignment.Center,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
            };
            return new MultiColumnHeaderState(columns);
        }
    }
}
