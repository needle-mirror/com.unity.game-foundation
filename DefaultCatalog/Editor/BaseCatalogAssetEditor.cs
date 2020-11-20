using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using DCTools = UnityEngine.GameFoundation.DefaultCatalog.Tools;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Base editor class for editing a subset of the Catalog for a certain <see cref="CatalogItemAsset"/> subtype.
    /// </summary>
    /// <typeparam name="TCatalogItemAsset">
    ///     The type of <see cref="CatalogItemAsset"/> being managed.
    /// </typeparam>
    abstract class BaseCatalogAssetEditor<TCatalogItemAsset> : CollectionEditorBase<TCatalogItemAsset>
        where TCatalogItemAsset : CatalogItemAsset
    {
        readonly PropertiesEditor<TCatalogItemAsset> m_StaticPropertiesEditor = new PropertiesEditor<TCatalogItemAsset>();

        readonly TagPickerEditor m_TagPicker;
        readonly CatalogSearchFieldEditor m_CatalogSearchFieldEditor = new CatalogSearchFieldEditor();

        protected string m_CurrentItemKey;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<BaseCatalogAssetEditor<TCatalogItemAsset>>();

        protected BaseCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window)
        {
            m_TagPicker = new TagPickerEditor();
        }

        public sealed override void RefreshItems()
        {
            base.RefreshItems();

            m_TagPicker.RefreshTags();

            if (CatalogSettings.catalogAsset != null)
            {
                CatalogSettings.catalogAsset.GetItems(m_Items);

                m_TagFilterEditor.RefreshSidebarTagFilterList(selectedItem, m_Items, m_TagPicker.tags);
            }
        }

        protected sealed override List<TCatalogItemAsset> GetValidItems()
        {
            if (m_TagPicker == null)
            {
                return null;
            }

            var filteredItems = m_TagFilterEditor.GetFilteredItems(m_Items, m_TagPicker.tags);
            var foundItems = m_CatalogSearchFieldEditor.GetFoundItems(m_Items);

            return filteredItems.Intersect(foundItems).ToList();
        }

        protected override void CreateNewItem()
        {
            var catalogAsset = CatalogSettings.catalogAsset;
            if (catalogAsset == null)
            {
                k_GFLogger.LogError($"Could not create a new {typeof(TCatalogItemAsset).Name} because the " +
                    $"Game Foundation {nameof(CatalogSettings.catalogAsset)} is null.");
                return;
            }

            var oldKeys = new HashSet<string>();
            var catalogItems = new List<CatalogItemAsset>();

            catalogAsset.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                if (item == null)
                {
                    k_GFLogger.LogWarning($"One or more null {nameof(CatalogItemAsset)}s found in the {nameof(CatalogSettings.catalogAsset)}.");
                    continue;
                }

                oldKeys.Add(item.key);
            }

            m_ReadableNameKeyEditor = new ReadableNameKeyEditor(true, oldKeys);
        }

        protected sealed override void CreateNewItemFinalize()
        {
            if (CatalogSettings.catalogAsset == null)
            {
                k_GFLogger.LogError($"Could not create new {typeof(TCatalogItemAsset).Name} because the Game " +
                    $"Foundation {nameof(CatalogSettings.catalogAsset)} is null.");
                return;
            }

            var catalogItemAsset = CatalogSettings.catalogAsset.Editor_CreateItem<TCatalogItemAsset>(m_NewItemKey);
            catalogItemAsset.Editor_SetDisplayName(m_NewItemDisplayName);

            // permit custom special processing ie for InventoryCatalog to set stackable flag
            HandleCreateNewItemClassSpecificProcessing(catalogItemAsset);

            CollectionEditorTools.AssetDatabaseAddObject(catalogItemAsset, CatalogSettings.catalogAsset);

            if (m_ItemToDuplicate != null)
            {
                m_ItemToDuplicate.CopyValues(catalogItemAsset);

                // need to clear out item to duplicate or things like initial allocation will 
                // ...continue to copy into subsequent items
                m_ItemToDuplicate = null;
            }

            // If filter is currently set to a tag, add that tag to the tag list of the item currently being created
            var currentFilteredTag = m_TagFilterEditor.GetCurrentFilteredTag(m_TagPicker.tags);

            if (currentFilteredTag != null)
            {
                var existingItemTags = new List<TagAsset>();
                catalogItemAsset.GetTags(existingItemTags);

                var isNewTag = true;
                foreach (var existingTag in existingItemTags)
                {
                    if (existingTag.key == currentFilteredTag.key)
                    {
                        isNewTag = false;
                        break;
                    }
                }

                if (isNewTag)
                    catalogItemAsset.Editor_AddTag(currentFilteredTag);
            }

            SelectItem(catalogItemAsset);

            m_CurrentItemKey = m_NewItemKey;

            RefreshItems();

            DrawGeneralDetail(catalogItemAsset);
        }

        // permit custom special processing ie for InventoryCatalogAssetEditor to set stackable flag
        protected virtual void HandleCreateNewItemClassSpecificProcessing(TCatalogItemAsset catalogItemAsset) { }

        protected sealed override void DrawSidebarUtils()
        {
            m_CatalogSearchFieldEditor.OnGUI();

            m_TagFilterEditor.DrawTagFilter(out var tagChanged);
            if (tagChanged)
            {
                var selectedTag = m_TagFilterEditor.GetCurrentFilteredTag(m_TagPicker.tags);
                if (selectedItem == null || selectedTag == null || !selectedItem.HasTag(selectedTag))
                {
                    SelectValidItem(0);
                }
            }

            EditorGUILayout.Space();
        }

        protected override void DrawSidebarListItem(TCatalogItemAsset catalogItem)
        {
            BeginSidebarItem(catalogItem, new Vector2(242f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(catalogItem.displayName, 242, GameFoundationEditorStyles.boldTextStyle);

            EndSidebarItem();
        }

        protected sealed override void DrawDetail(TCatalogItemAsset catalogItem)
        {
            DrawGeneralDetail(catalogItem);

            EditorGUILayout.Space();

            // save off previous state of tags so we can detect changes
            using (DCTools.Pools.tagList.Get(out var previousTags))
            {
                catalogItem.GetTags(previousTags);

                m_TagPicker.DrawTagPicker(catalogItem);

                EditorGUILayout.Space();

                DrawTypeSpecificBlocks(catalogItem);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(PropertiesEditor.staticPropertiesLabel, GameFoundationEditorStyles.titleStyle);
                using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                {
                    m_StaticPropertiesEditor.Draw();
                }

                // make sure this is the last to draw
                m_TagPicker.DrawTagPickerPopup(catalogItem);

                // if tags changed then refresh item, including tags so tag filter can be updated
                if (!previousTags.SequenceEqual(catalogItem.m_Tags))
                {
                    RefreshItems();
                }
            }
        }

        protected override void SelectItem(TCatalogItemAsset catalogItem)
        {
            base.SelectItem(catalogItem);

            m_TagPicker.ResetTagSearch();

            if (catalogItem != null)
            {
                var oldKeys = new HashSet<string>();
                foreach (var item in m_Items)
                {
                    oldKeys.Add(item.key);
                }

                m_ReadableNameKeyEditor = new ReadableNameKeyEditor(false, oldKeys);
                m_CurrentItemKey = catalogItem.key;
            }

            m_StaticPropertiesEditor.SelectItem(catalogItem);
        }

        protected sealed override void OnRemoveItem(TCatalogItemAsset item)
        {
            CatalogSettings.catalogAsset.Editor_RemoveItem(item);
            Object.DestroyImmediate(item, true);
        }

        /// <summary>
        ///     Draw additional fields of the given <paramref name="catalogItem"/>
        ///     that must appear in the General block.
        /// </summary>
        /// <param name="catalogItem">
        ///     Currently edited item.
        /// </param>
        protected virtual void DrawGeneralFields(TCatalogItemAsset catalogItem) { }

        /// <summary>
        ///     Draw additional blocks that are specific to the type of the current item.
        /// </summary>
        /// <param name="catalogItem">
        ///     The catalog item to draw additional blocks for.
        /// </param>
        protected virtual void DrawTypeSpecificBlocks(TCatalogItemAsset catalogItem) { }

        protected virtual void DrawGeneralDetail(TCatalogItemAsset catalogItem)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = catalogItem.displayName;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentItemKey, ref displayName);

                if (catalogItem.displayName != displayName)
                {
                    catalogItem.Editor_SetDisplayName(displayName);
                }

                DrawGeneralFields(catalogItem);
            }
        }
    }
}
