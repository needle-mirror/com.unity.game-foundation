using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Module for UI and logic of tag filter.
    /// </summary>
    class TagFilterEditor
    {
        enum DefaultFilterOptions
        {
            All = 0,
            NoTags = 1,
        }

        const string k_All = "<All>";
        const string k_NoTags = "<No Tags>";
        const int k_ListOffset = 2;
        int m_SelectedFilterTagIndex = (int)DefaultFilterOptions.All;
        string m_SelectedFilterTagKey = k_All;
        string[] m_TagNamesForFilter;

        /// <summary>
        ///     Gets a filtered list of items based on the currently selected Tag.
        /// </summary>
        /// <param name="fullList">
        ///     The list of GameItemDefinitions being filtered to the current Tag.
        /// </param>
        /// <param name="tags">
        ///     The list of possible Tags that can be filtered to.
        /// </param>
        /// <returns>
        ///     Filtered list of GameItemDefinitions.
        /// </returns>
        public List<T> GetFilteredItems<T>(List<T> fullList, TagAsset[] tags) where T : CatalogItemAsset
        {
            if (fullList == null)
            {
                return null;
            }

            if (m_TagNamesForFilter is null)
            {
                return null;
            }

            if (m_SelectedFilterTagIndex < 0 || m_SelectedFilterTagIndex >= m_TagNamesForFilter.Length)
            {
                ResetTagFilterToAll();
            }

            if (m_SelectedFilterTagIndex == (int)DefaultFilterOptions.All)
            {
                return fullList;
            }

            if (m_SelectedFilterTagIndex == (int)DefaultFilterOptions.NoTags)
            {
                return fullList.FindAll(item =>
                {
                    var itemTags = item.m_Tags;
                    return itemTags == null || !itemTags.Any();
                });
            }

            return fullList.FindAll(item =>
            {
                var itemTags = item.m_Tags;
                if (itemTags == null || m_TagNamesForFilter == null)
                {
                    return false;
                }

                return itemTags.Any(tag => tag.key == m_SelectedFilterTagKey);
            });
        }

        /// <summary>
        ///     Draws the UI for the filter selection popup.
        /// </summary>
        /// <param name="tagChanged">
        ///     out parameter modifier. Returns bool for whether or not the Tag filter has been changed.
        ///     </returns>
        public void DrawTagFilter(out bool tagChanged)
        {
            CollectionEditorTools.SetGUIEnabledAtRunTime(true);

            m_SelectedFilterTagIndex = Array.IndexOf(m_TagNamesForFilter, m_SelectedFilterTagKey);
            if (m_SelectedFilterTagIndex < 0 || m_SelectedFilterTagIndex >= m_TagNamesForFilter.Length)
            {
                ResetTagFilterToAll();
            }

            int newFilterIndex = EditorGUILayout.Popup(m_SelectedFilterTagIndex, m_TagNamesForFilter);
            if (newFilterIndex != m_SelectedFilterTagIndex)
            {
                m_SelectedFilterTagIndex = newFilterIndex;
                m_SelectedFilterTagKey = m_TagNamesForFilter[newFilterIndex];
                tagChanged = true;
            }
            else
            {
                tagChanged = false;
            }

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);
        }

        /// <summary>
        ///     Refreshes the list of possible Tags that can be filtered to based on the given list.
        /// </summary>
        /// <param name="tags">
        ///     The list of possible Tags that can be filtered to.
        /// </param>
        public void RefreshSidebarTagFilterList<T>(T selectedItem, List<T> itemsList, TagAsset[] tags) where T : CatalogItemAsset
        {
            int tagFilterCount = k_ListOffset;
            if (tags != null)
            {
                for (int i = 0; i < tags.Count(); ++i)
                {
                    if (tags[i] != null)
                    {
                        foreach (var item in itemsList)
                        {
                            if (item.HasTag(tags[i]))
                            {
                                ++tagFilterCount;
                                break;
                            }
                        }
                    }
                }
            }

            // Create Names for Pull-down menus
            m_TagNamesForFilter = new string[tagFilterCount];
            m_TagNamesForFilter[(int)DefaultFilterOptions.All] = k_All;
            m_TagNamesForFilter[(int)DefaultFilterOptions.NoTags] = k_NoTags;

            int tagOn = k_ListOffset;

            if (tags != null)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i] != null)
                    {
                        foreach (var item in itemsList)
                        {
                            if (item.HasTag(tags[i]))
                            {
                                m_TagNamesForFilter[tagOn] = tags[i].key;
                                ++tagOn;
                                break;
                            }
                        }
                    }
                }
            }

            // if current tag no longer exists then reset to filter to 'all'
            if (!Array.Exists(m_TagNamesForFilter, t => t == m_SelectedFilterTagKey))
            {
                ResetTagFilterToAll();
            }
            else if (selectedItem != null
                && CatalogSettings.catalogAsset.tagCatalog != null)
            {
                var tag = CatalogSettings.catalogAsset.tagCatalog.FindTag(m_SelectedFilterTagKey);

                // if filtered to none and current item has a tag then reset filter to 'all'
                if (m_SelectedFilterTagIndex == (int)DefaultFilterOptions.NoTags &&
                    selectedItem.m_Tags.Count > 0)
                {
                    ResetTagFilterToAll();
                }

                // if filtered tag no longer exists on selected item then reset filter to 'all'
                else if (m_SelectedFilterTagIndex >= k_ListOffset && !selectedItem.HasTag(tag))
                {
                    ResetTagFilterToAll();
                }
            }
        }

        /// <summary>
        ///     Resets selected tag filter to All.
        /// </summary>
        void ResetTagFilterToAll()
        {
            m_SelectedFilterTagIndex = (int)DefaultFilterOptions.All;
            m_SelectedFilterTagKey = k_All;
        }

        /// <summary>
        ///     Returns the current Tag selected in the filter dropdown.
        /// </summary>
        /// <param name="tags">
        ///     The list of possible Tags that can be filtered to.
        /// </param>
        /// <returns>
        ///     The current TagDefinition selected by the filter.
        /// </returns>
        public TagAsset GetCurrentFilteredTag(TagAsset[] tags)
        {
            if (tags == null || m_SelectedFilterTagIndex < k_ListOffset)
            {
                return null;
            }

            return tags.FirstOrDefault(t => t.key == m_SelectedFilterTagKey);
        }

        /// <summary>
        ///     Resets Tag Filters list of potential Tag names and the selected filter index.
        /// </summary>
        public void ResetTagFilter()
        {
            ResetTagFilterToAll();
            m_TagNamesForFilter = null;
        }
    }
}
