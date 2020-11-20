using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class InventoryCatalogAssetEditor : BaseCatalogAssetEditor<InventoryItemDefinitionAsset>
    {
        /// <summary>
        ///     Label for Initial Allocation entry for this <see cref="InventoryItemDefinitionAsset"/>.
        /// </summary>
        static readonly GUIContent k_InitialAllocationLabel = new GUIContent(
            "Initial Allocation",
            "The quantity of this item to automatically add to player's inventory at startup.");

        /// <summary>
        ///     Quantity at which warning should be added to the Editor regarding slow startup times.
        /// </summary>
        const int k_InitialAllocationWarningSize = 1000;

        /// <summary>
        ///     Quantity-large warning.
        ///     Added when Initial Allocation is set larger than <see cref="k_InitialAllocationWarningSize"/>.
        /// </summary>
        static readonly string k_InitialAllocationLargeLabel =
            "Large Initial Allocations may adversely effect performance and memory consumption.\n" +
            "Unless Stackable Inventory Items are utilized, Game Foundation requires that each item be added iteratively so " +
            "startup time will be slowed by large Initial Allocation quantities.  To avoid this, please consider using Stackable " +
            "Items (check box available in item creation) or Currency (if possible/applicable) or consider rethinking your " +
            "game economy to require fewer InventoryItems at startup.";

        /// <summary>
        ///     Label for is-stackable entry for this <see cref="InventoryItemDefinitionAsset"/>.
        /// </summary>
        static readonly GUIContent k_IsStackableLabel = new GUIContent(
            "Is Stackable",
            "If true, InventoryItems created by this Definition will have a quantity which can be adjusted at runtime without requiring unique items to be created and destroyed individually.");
        static readonly GUIContent k_IsStackableTrueLabel = new GUIContent(
            "Stackable Inventory Item",
            "InventoryItems created by this Definition will have a quantity which can be adjusted at runtime without requiring unique items to be created and destroyed individually.");
        static readonly GUIContent k_IsStackableFalseLabel = new GUIContent(
            "Individual Inventory Item",
            "InventoryItems created by this Definition will NOT be stackable so each item instantiated will represent 1 item in Inventory thus requiring unique items to be created and destroyed individually.");

        /// <summary>
        ///     Label for initial stack allocation entry for this <see cref="InventoryItemDefinitionAsset"/>.
        /// </summary>
        static readonly GUIContent k_QuantityPerInitialStackLabel = new GUIContent(
            "Quantity per Initial Stack",
            "Because this item is Stackable, if 'Initial Allocation' is set (above), each INITIAL stack allocated will start with this quantity (runtime-created stackable items will always start with 1 quantity as do non-stackable items).");

        bool m_NewItemIsStackable = true;
        long m_NewItemQuantityPerInitialStack = 1;

        readonly MutablePropertiesEditor m_MutablePropertiesEditor = new MutablePropertiesEditor();

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.InventoryItems;

        public InventoryCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void SelectItem(InventoryItemDefinitionAsset item)
        {
            base.SelectItem(item);

            m_MutablePropertiesEditor.SelectItem(item);
        }

        protected override void DrawTypeSpecificBlocks(InventoryItemDefinitionAsset catalogItem)
        {
            // Draw properties.
            EditorGUILayout.LabelField(MutablePropertiesEditor.mutablePropertiesLabel, GameFoundationEditorStyles.titleStyle);
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                m_MutablePropertiesEditor.Draw();
            }

            EditorGUILayout.Space();
        }

        protected override void DrawCustomCreateFormUI()
        {
            GUI.SetNextControlName("Is Stackable");
            m_NewItemIsStackable = EditorGUILayout.Toggle(k_IsStackableLabel, m_NewItemIsStackable);

            EditorGUILayout.HelpBox(
                "Once the Create button is clicked, neither the Key nor 'Is Stackable' can be changed.",
                MessageType.Warning);
        }

        protected override void StartDuplicating(CatalogItemAsset item)
        {
            // first setup the duplicate
            base.StartDuplicating(item);

            // next save off values from item being duplicated
            // note: must be done last because StartDuplicating clears them
            if (item is InventoryItemDefinitionAsset inventoryItemDefinition)
            {
                m_NewItemIsStackable = inventoryItemDefinition.isStackableFlag;
                m_NewItemQuantityPerInitialStack = inventoryItemDefinition.initialQuantityPerStack;
            }
        }

        protected override void CreateNewItem()
        {
            m_NewItemQuantityPerInitialStack = 1;
            base.CreateNewItem();
        }

        // Called by CreateNewItemFinalize to set stackable flag
        protected override void HandleCreateNewItemClassSpecificProcessing(
            InventoryItemDefinitionAsset catalogItemAsset)
        {
            catalogItemAsset.Editor_SetIsStackableFlag(m_NewItemIsStackable);
            catalogItemAsset.Editor_SetInitialQuantityPerStack(m_NewItemQuantityPerInitialStack);
            m_NewItemQuantityPerInitialStack = 1;
        }

        protected override void DrawGeneralDetail(InventoryItemDefinitionAsset inventoryItemDefinition)
        {
            if (CatalogSettings.catalogAsset is null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                EditorGUILayout.LabelField(inventoryItemDefinition.isStackableFlag
                    ? k_IsStackableTrueLabel
                    : k_IsStackableFalseLabel, EditorStyles.boldLabel);

                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    string displayName = inventoryItemDefinition.displayName;
                    m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentItemKey, ref displayName);

                    if (checkScope.changed)
                    {
                        inventoryItemDefinition.Editor_SetDisplayName(displayName);
                    }
                }

                EditorGUILayout.Space();

                // allow dev to set initial allocation
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Initial Allocation");
                    var initialAllocation = EditorGUILayout.IntField(k_InitialAllocationLabel,
                        inventoryItemDefinition.initialAllocation);

                    if (checkScope.changed)
                    {
                        inventoryItemDefinition.Editor_SetInitialAllocation(initialAllocation);
                    }
                }

                // add warning
                // TODO: this should be removed once non-iterative, stacked inventory items can be added
                if (inventoryItemDefinition.initialAllocation >= k_InitialAllocationWarningSize)
                {
                    EditorGUILayout.HelpBox(k_InitialAllocationLargeLabel, MessageType.Warning);
                }

                EditorGUILayout.Space();

                if (inventoryItemDefinition.isStackableFlag)
                {
                    using (var checkScope = new EditorGUI.ChangeCheckScope())
                    {
                        GUI.SetNextControlName("Quantity per Initial Stack");
                        var newInitialQuantityPerStack = EditorGUILayout.LongField(k_QuantityPerInitialStackLabel,
                            inventoryItemDefinition.initialQuantityPerStack);

                        if (checkScope.changed)
                        {
                            inventoryItemDefinition.Editor_SetInitialQuantityPerStack(newInitialQuantityPerStack);
                        }
                    }
                }
            }
        }
    }
}
