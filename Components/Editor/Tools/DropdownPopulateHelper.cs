using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Data;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     A helper class to populate display name and keys for dropdown menu.
    /// </summary>
    class DropdownPopulateHelper
    {
        public string[] displayNames => m_DisplayNames;
        string[] m_DisplayNames;

        public string[] keys => m_Keys;
        string[] m_Keys;

        /// <summary>
        ///     Returns the key for a particular index.
        /// </summary>
        /// <param name="index">Index of the key.</param>
        /// <returns>A key</returns>
        public string GetKey(int index)
        {
            if (m_Keys == null || index < 0 || m_Keys.Length <= index) return null;

            return m_Keys[index];
        }

        /// <summary>
        ///     Returns the display name for a particular index.
        /// </summary>
        /// <param name="index">Index of the display name.</param>
        /// <returns>Display name</returns>
        public string GetDisplayName(int index)
        {
            if (displayNames == null || index < 0 || displayNames.Length <= index) return null;

            return displayNames[index];
        }

        /// <summary>
        ///     Populates arrays for dropdown menu.
        /// </summary>
        /// <param name="entries">
        ///     A collection of <see cref="DropdownItem"/>s.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(List<DropdownItem> entries, string selectedKey, bool noneAsFirstItem = true)
        {
            var count = 0;
            var indexOfKey = 0;
            var notFound = false;

            if (entries != null)
            {
                count = entries.Count;
                notFound = !string.IsNullOrEmpty(selectedKey) && !entries.Exists(x => x.key == selectedKey);
            }
            else if (!string.IsNullOrEmpty(selectedKey))
            {
                notFound = true;
            }

            if (entries == null || entries.Count == 0)
            {
                noneAsFirstItem = true;
            }

            if (noneAsFirstItem) count++;
            if (notFound) count++;

            if (m_DisplayNames == null || m_DisplayNames.Length != count)
            {
                m_DisplayNames = new string[count];
                m_Keys = new string[count];
            }

            int index = 0;

            if (notFound)
            {
                m_DisplayNames[index] = $"[Missing] - key: {selectedKey}";
                m_Keys[index] = selectedKey;
                indexOfKey = index;
                index++;
            }

            if (noneAsFirstItem)
            {
                m_DisplayNames[index] = "None";
                m_Keys[index] = null;
                if (!notFound) indexOfKey = index;
                index++;
            }

            if (entries != null)
            {
                int keyIndex = 0;
                for (; index < count; index++)
                {
                    var entry = entries[keyIndex++];
                    m_DisplayNames[index] = entry.displayName;
                    m_Keys[index] = entry.key;

                    if (!notFound && selectedKey == entry.key)
                    {
                        indexOfKey = index;
                    }
                }
            }

            return indexOfKey;
        }
    }
}
