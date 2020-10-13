using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class TagAssetEditor : CollectionEditorBase<TagAsset>
    {
        string m_CurrentTagId;
        readonly TagSearchFieldEditor m_TagSearchFieldEditor = new TagSearchFieldEditor();

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Tags;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<TagAssetEditor>();

        public TagAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override List<TagAsset> GetValidItems()
        {
            return m_TagSearchFieldEditor.GetFoundItems(m_Items);
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            CatalogSettings.catalogAsset.tagCatalog.GetTags(m_Items);
        }

        protected override void SelectItem(TagAsset tag)
        {
            if (tag != null)
            {
                m_ReadableNameKeyEditor = new ReadableNameKeyEditor(false, new HashSet<string>(m_Items.Select(i => i.key)));
                m_CurrentTagId = tag.key;
            }

            base.SelectItem(tag);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameKeyEditor = new ReadableNameKeyEditor(true, new HashSet<string>(m_Items.Select(i => i.key)));
        }

        protected override void CreateNewItemFinalize()
        {
            var catalog = CatalogSettings.catalogAsset;
            if (catalog is null)
            {
                k_GFLogger.LogError($"Could not create new {nameof(TagAsset)} with id {m_NewItemKey} because " +
                    "the Game Foundation database is null.");
                return;
            }

            var tagCatalog = catalog.tagCatalog;
            if (tagCatalog is null)
            {
                k_GFLogger.LogError($"Could not create new {nameof(TagAsset)} with id {m_NewItemKey} " +
                    "because the tag catalog is null.");
                return;
            }

            var tagAsset = TagAsset.Editor_Create(m_NewItemKey);
            if (tagAsset is null)
            {
                return;
            }

            tagCatalog.Editor_AddTag(tagAsset);
            CollectionEditorTools.AssetDatabaseAddObject(tagAsset, tagCatalog);

            SelectItem(tagAsset);

            m_CurrentTagId = m_NewItemKey;

            RefreshItems();

            DrawDetail(tagAsset);
        }

        protected override void DrawCreateInputFields()
        {
            string disableDisplayName = null;
            m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_NewItemKey, ref disableDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("key");
            }

            // display name is unused, but base class requires it not be empty to enable 'create' button so we fake it.
            m_NewItemDisplayName = "ignored non-null string";
        }

        protected override void DrawDetail(TagAsset tag)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string disableDisplayName = null;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentTagId, ref disableDisplayName);
            }
        }

        protected override void DrawSidebarUtils()
        {
            m_TagSearchFieldEditor.OnGUI();
        }

        protected override void DrawSidebarListItem(TagAsset tag)
        {
            BeginSidebarItem(tag, new Vector2(242f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(tag.key, 242, GameFoundationEditorStyles.boldTextStyle);

            EndSidebarItem();
        }

        protected override void OnRemoveItem(TagAsset item)
        {
            if (item is null)
            {
                return;
            }

            CatalogSettings.catalogAsset.Editor_RemoveTag(item);

            Object.DestroyImmediate(item, true);

            m_TagFilterEditor.ResetTagFilter();
        }
    }
}
