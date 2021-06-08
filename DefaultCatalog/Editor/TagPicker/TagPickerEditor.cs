using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     UI Module for tag selection UI.
    /// </summary>
    class TagPickerEditor
    {
        class TagRow
        {
            public readonly List<TagAsset> tags;

            public TagRow()
            {
                tags = new List<TagAsset>();
            }
        }

        TagAsset[] m_Tags;

        public TagAsset[] tags => m_Tags;

        List<TagRow> m_WrappableTagRows = new List<TagRow>();
        List<TagAsset> m_TagSearchResults = new List<TagAsset>();
        List<TagAsset> m_AssignedTags;

        Rect m_TagItemsRect;
        string m_TagSearchString = string.Empty;
        string m_TagSearchStringPrevious = string.Empty;
        readonly SearchField m_TagSearchField = new SearchField();

        Rect m_SuggestRect;
        Vector2 m_TagSearchSuggestScrollPosition = Vector2.zero;
        int m_TagSuggestSelectedIndex = -1;
        bool m_UsedScrollWheelInSuggestBox;

        readonly TagFilterEditor m_TagFilterEditor = new TagFilterEditor();

        static readonly GUIContent s_TagLabel = new GUIContent(
            "Tags",
            "Assign existing tags or create new ones. Use tags to filter/group items in the editor and code.");

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<TagPickerEditor>();

        /// <summary>
        ///     Re-cache the collection of tags from the catalog.
        /// </summary>
        public void RefreshTags()
        {
            if (CatalogSettings.catalogAsset == null) return;

            var tags = GetTagsFromCatalogAsset();
            if (tags == null) tags = new TagAsset[0];

            m_Tags = tags;
        }

        /// <summary>
        ///     Gets all tags in the game foundation catalog asset.
        /// </summary>
        /// <returns>
        ///     All tags in the game foundation catalog asset.
        /// </returns>
        protected TagAsset[] GetTagsFromCatalogAsset()
        {
            var tags = new List<TagAsset>();
            CatalogSettings.catalogAsset.tagCatalog.GetTags(tags);
            return tags.ToArray();
        }

        void RefreshAssignedTags(CatalogItemAsset catalogItem)
        {
            if (m_AssignedTags == null)
            {
                m_AssignedTags = new List<TagAsset>();
            }

            m_AssignedTags.Clear();

            if (catalogItem == null)
            {
                return;
            }

            catalogItem.GetTags(m_AssignedTags);
        }

        /// <summary>
        ///     Draws tag selection search bar and selected tags.
        /// </summary>
        /// <param name="catalogItem">
        ///     The GameItemDefinition of the item that is currently selected for tagy selection.
        /// </param>
        public void DrawTagPicker(CatalogItemAsset catalogItem)
        {
            RefreshAssignedTags(catalogItem);

            DrawTagsDetail(catalogItem);
        }

        /// <summary>
        ///     Draws tag search suggestion view. NOTE: This needs to be the last GUI call
        ///     in the given window otherwise other elements will be drawn over it.
        /// </summary>
        /// <param name="catalogItem">
        ///     The GameItemDefinition of the item that is currently selected for tag selection.
        /// </param>
        public void DrawTagPickerPopup(CatalogItemAsset catalogItem)
        {
            DrawTagSearchSuggest(catalogItem);
            HandleTagSearchInput(catalogItem);
        }

        /// <summary>
        ///     Resets tag search string.
        /// </summary>
        public void ResetTagSearch(bool takeFocus = false)
        {
            m_TagSuggestSelectedIndex = -1;
            m_TagSearchString = string.Empty;

            if (takeFocus)
            {
                // do both of these - the first one just makes sure the next control doesn't get focused, the second one makes sure the text is being edited
                EditorGUI.FocusTextInControl("search field");
                m_TagSearchField.SetFocus();
            }

            m_UsedScrollWheelInSuggestBox = false;
        }

        void DrawTagsDetail(CatalogItemAsset catalogItem)
        {
            EditorGUILayout.LabelField(s_TagLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            m_TagSearchString = m_TagSearchField.OnGUI(m_TagSearchString);

                            if (m_TagSearchStringPrevious != m_TagSearchString)
                            {
                                UpdateTagSuggestions();
                            }

                            m_TagSearchStringPrevious = m_TagSearchString;

                            // only show the Add button if:
                            //  • we are searching
                            //  • there are no suggestions found

                            if (!string.IsNullOrEmpty(m_TagSearchString) && m_TagSearchResults.Count <= 0)
                            {
                                // Disable Tag Add Button if the tag they are trying to add is not a valid key
                                var tagAlreadyAdded = Array.Exists(
                                    m_Tags, tag => tag.key == m_TagSearchString);
                                var tagValid = CollectionEditorTools.IsValidId(
                                    m_TagSearchString);
                                var addButtonDisabled = tagAlreadyAdded ||
                                    !tagValid;

                                using (new EditorGUI.DisabledGroupScope(addButtonDisabled))
                                {
                                    string tooltip = string.Empty;
                                    if (!tagValid)
                                    {
                                        tooltip = "The current Key is not valid. A valid Key must begin with an " +
                                            "alphabetic character, and may be followed by any number of alphanumeric " +
                                            "characters, dashes (-) and/or underscores (_)";
                                    }

                                    if (tagAlreadyAdded)
                                    {
                                        tooltip = "Tag already added to this item. Please type a new unique name or select an existing tag.";
                                    }

                                    if (GUILayout.Button(new GUIContent("Add", tooltip), GUILayout.Width(TagPickerStyles.tagAddButtonWidth)))
                                    {
                                        // same as if user presses Enter or Return
                                        CreateAndAssignTagFromSearchField(catalogItem);
                                    }
                                }
                            }
                        }

                        EditorGUILayout.Space();

                        // dimensions should be calculated during Repaint because during Layout they aren't calculated yet
                        if (Event.current.type == EventType.Repaint)
                        {
                            m_SuggestRect = GUILayoutUtility.GetLastRect();
                            m_SuggestRect.x += 24;
                            m_SuggestRect.width -= 40;
                            m_SuggestRect.height = 220;

                            m_TagItemsRect = GUILayoutUtility.GetLastRect();
                            m_TagItemsRect.x += 12f;
                            m_TagItemsRect.y += 18f;

                            RecalculateTagBoxHeight();
                        }

                        // don't modify a collection while iterating through it
                        TagAsset tagToRemove = null;

                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                        {
                            // make enough room
                            GUILayout.Space(m_TagItemsRect.height);

                            // inside this vertical area, we cannot use GUILayout anymore because
                            // for/while inside GUILayout horizontal and vertical scopes will
                            // generate errors between Layout and Repaint events (chicken/egg problem)

                            for (var tagRowIndex = 0; tagRowIndex < m_WrappableTagRows.Count; tagRowIndex++)
                            {
                                var row = m_WrappableTagRows[tagRowIndex];

                                var rowHeight = TagPickerStyles.tagListItemStyle.CalcHeight(new GUIContent("lorem ipsum"), 1000f);

                                var rowRect = new Rect(m_TagItemsRect) { height = rowHeight };
                                rowRect.y += tagRowIndex * rowHeight;
                                rowRect.y += tagRowIndex * TagPickerStyles.tagItemMargin;

                                var curX = 0f;

                                foreach (var tag in row.tags)
                                {
                                    var tagNameContentSize = TagPickerStyles.tagListItemStyle.CalcSize(new GUIContent(tag.key));

                                    var itemRect = new Rect(rowRect);
                                    itemRect.x += curX;
                                    itemRect.width = tagNameContentSize.x + TagPickerStyles.tagListItemStyle.padding.horizontal;

                                    var tagDeleteButtonRect = new Rect(itemRect)
                                    {
                                        x = itemRect.x + itemRect.width - TagPickerStyles.tagRemoveButtonSpaceWidth,
                                        width = TagPickerStyles.tagRemoveButtonSpaceWidth
                                    };

                                    // adjust the X rect over to the right side

                                    // nudge it a bit to look better
                                    tagDeleteButtonRect.x -= 2;
                                    tagDeleteButtonRect.y += 4;

                                    GUI.Box(itemRect, tag.key, TagPickerStyles.tagListItemStyle);

                                    if (GUI.Button(tagDeleteButtonRect, "<b>X</b>", GameFoundationEditorStyles.deleteButtonStyle))
                                    {
                                        tagToRemove = tag;
                                    }

                                    curX += itemRect.width + TagPickerStyles.tagItemMargin;
                                }
                            }
                        }

                        if (tagToRemove == null)
                        {
                            return;
                        }

                        catalogItem.Editor_RemoveTag(tagToRemove);
                        RefreshAssignedTags(catalogItem);
                    }
                }
            }
        }

        void DrawTagSearchSuggest(CatalogItemAsset catalogItem)
        {
            // only show the search suggest window and handle input for it if...
            // - the search field is currently in focus
            // - there is text in the search field
            // - there are suggestions to show
            if (string.IsNullOrEmpty(m_TagSearchString)) return;
            if (m_TagSearchResults.Count <= 0) return;

            // adjust scroll position if the highlighted item is not visible
            // but if the scroll wheel is used, then obey the scroll wheel instead

            if (Event.current.type == EventType.ScrollWheel) m_UsedScrollWheelInSuggestBox = true;

            if (!m_UsedScrollWheelInSuggestBox)
            {
                var rowHeight = TagPickerStyles.tagSuggestItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
                var minVisibleY = m_TagSearchSuggestScrollPosition.y;
                var maxVisibleY = m_SuggestRect.height + m_TagSearchSuggestScrollPosition.y;
                var selectedItemTopY = rowHeight * m_TagSuggestSelectedIndex;
                var selectedItemBottomY = selectedItemTopY + rowHeight;

                if (minVisibleY > selectedItemTopY)
                {
                    m_TagSearchSuggestScrollPosition.Set(0, selectedItemTopY);
                }

                if (maxVisibleY < selectedItemBottomY)
                {
                    m_TagSearchSuggestScrollPosition.Set(0, selectedItemBottomY - m_SuggestRect.height);
                }
            }

            // RENDER

            using (new GUILayout.AreaScope(m_SuggestRect, "", TagPickerStyles.searchSuggestAreaStyle))
            {
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(m_TagSearchSuggestScrollPosition, false, true))
                {
                    m_TagSearchSuggestScrollPosition = scrollViewScope.scrollPosition;

                    for (var resultIndex = 0; resultIndex < m_TagSearchResults.Count; resultIndex++)
                    {
                        var suggestedTag = m_TagSearchResults[resultIndex];

                        // use the normal style, unless this is the highlighted item, in which case use the highlighted style
                        var style = resultIndex == m_TagSuggestSelectedIndex
                            ? TagPickerStyles.tagSuggestItemStyleSelected
                            : TagPickerStyles.tagSuggestItemStyle;

                        if (GUILayout.Button(suggestedTag.key, style, GUILayout.ExpandWidth(true)))
                        {
                            AssignTag(catalogItem, suggestedTag);
                            ResetTagSearch(true);
                            UpdateTagSuggestions();
                            RecalculateTagBoxHeight();
                        }
                    }
                }
            }
        }

        void HandleTagSearchInput(CatalogItemAsset catalogItem)
        {
            if (string.IsNullOrEmpty(m_TagSearchString)) return;

            if (Event.current.type != EventType.KeyUp)
            {
                return;
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    if (m_TagSearchResults.Count > 0)
                    {
                        Event.current.Use();

                        m_TagSuggestSelectedIndex -= 1;
                        m_UsedScrollWheelInSuggestBox = false;
                    }

                    break;

                case KeyCode.DownArrow:
                    if (m_TagSearchResults.Count > 0)
                    {
                        Event.current.Use();
                        m_TagSuggestSelectedIndex += 1;
                        m_UsedScrollWheelInSuggestBox = false;
                    }

                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                case KeyCode.Tab:
                    Event.current.Use();

                    if (m_TagSearchResults.Count > 0)
                    {
                        if (m_TagSuggestSelectedIndex >= 0)
                        {
                            // if there are results and one is selected, then assign it
                            AssignTag(catalogItem, m_TagSearchResults[m_TagSuggestSelectedIndex]);
                            RecalculateTagBoxHeight();
                        }

                        // if there are results but none are selected, then do nothing
                    }
                    else if (Event.current.keyCode != KeyCode.Tab)
                    {
                        // same as if "Add" is clicked
                        // if there are no suggestions but there is search string, then create a new Tag
                        // but it's probably not expected when tab key is used, so we'll exclude that one

                        if (!CollectionEditorTools.IsValidId(m_TagSearchString))
                        {
                            UpdateTagSuggestions();
                            break;
                        }

                        CreateAndAssignTagFromSearchField(catalogItem);
                    }

                    ResetTagSearch(true);
                    UpdateTagSuggestions();
                    break;

                case KeyCode.Escape:
                    Event.current.Use();
                    ResetTagSearch();
                    UpdateTagSuggestions();
                    break;
            }

            CorrectTagSearchSuggestSelectedIndex();
        }

        protected void AddItem(TagAsset tag)
        {
            var catalog = CatalogSettings.catalogAsset.tagCatalog;
            catalog.Editor_AddTag(tag);
            CollectionEditorTools.AssetDatabaseAddObject(tag, catalog);
        }

        void CreateAndAssignTagFromSearchField(CatalogItemAsset catalogItem)
        {
            if (CatalogSettings.catalogAsset == null)
            {
                k_GFLogger.LogError($"Could not create and assign {nameof(TagAsset)} '{m_TagSearchString}' " +
                    $"to {nameof(CatalogItemAsset)} '{catalogItem.displayName}' because the Game Foundation " +
                    $"{nameof(CatalogSettings.catalogAsset)} is null");

                return;
            }

            if (CatalogSettings.catalogAsset == null) return;

            if (m_Tags == null) return;

            if (!CollectionEditorTools.IsValidId(m_TagSearchString) || m_Tags.Any(tag => tag.key == m_TagSearchString))
            {
                return;
            }

            var newTag = TagAsset.Editor_Create(m_TagSearchString);
            if (newTag != null)
            {
                AddItem(newTag);
                RefreshTags();
                AssignTag(catalogItem, newTag);
            }

            // Refresh settings with new Tag
            RecalculateTagBoxHeight();
            ResetTagSearch(true);
            UpdateTagSuggestions();
        }

        void RecalculateTagBoxHeight()
        {
            var currentRowContentWidth = 0f;

            m_WrappableTagRows = new List<TagRow> { new TagRow() };

            if (m_AssignedTags != null)
            {
                foreach (var tag in m_AssignedTags)
                {
                    var contentSize = TagPickerStyles.tagListItemStyle.CalcSize(new GUIContent(tag.key));
                    contentSize.x += TagPickerStyles.tagListItemStyle.padding.horizontal + TagPickerStyles.tagRemoveButtonSpaceWidth;

                    if (currentRowContentWidth + contentSize.x > m_TagItemsRect.width)
                    {
                        m_WrappableTagRows.Add(new TagRow());
                        currentRowContentWidth = 0f;
                    }

                    m_WrappableTagRows.Last().tags.Add(tag);
                    currentRowContentWidth += contentSize.x;
                }
            }

            m_TagItemsRect.height = m_WrappableTagRows.Count * TagPickerStyles.tagListItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
            m_TagItemsRect.height += (m_WrappableTagRows.Count - 1) * TagPickerStyles.tagItemMargin;
        }

        void UpdateTagSuggestions()
        {
            if (string.IsNullOrEmpty(m_TagSearchString) || m_Tags == null)
            {
                m_TagSearchResults = new List<TagAsset>();
                m_TagSuggestSelectedIndex = -1;
                return;
            }

            var potentialMatches =
                Array.FindAll(
                    m_Tags,
                    tag => tag.key.ToLowerInvariant().Contains(m_TagSearchString.ToLowerInvariant()));

            m_TagSearchResults = potentialMatches
                .Where(potentialTag =>
                {
                    return m_AssignedTags != null && m_AssignedTags.All(existingTag => existingTag != potentialTag);
                })
                .ToList();

            CorrectTagSearchSuggestSelectedIndex();
        }

        void CorrectTagSearchSuggestSelectedIndex()
        {
            if (m_TagSearchResults.Count <= 0)
            {
                m_TagSuggestSelectedIndex = -1;
            }
            else if (m_TagSuggestSelectedIndex < 0)
            {
                m_TagSuggestSelectedIndex = m_TagSearchResults.Count - 1;
            }
            else if (m_TagSuggestSelectedIndex >= m_TagSearchResults.Count)
            {
                m_TagSuggestSelectedIndex = 0;
            }
        }

        void AssignTag(CatalogItemAsset catalogItem, TagAsset tag)
        {
            if (catalogItem == null || tag == null)
            {
                return;
            }

            catalogItem.Editor_AddTag(tag);

            RefreshAssignedTags(catalogItem);
        }
    }
}
