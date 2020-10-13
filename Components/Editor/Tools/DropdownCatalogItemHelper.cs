using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Catalog Item Assets' display name and key for dropdown menus.
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the Catalog Item Asset.
    /// </typeparam>
    class DropdownCatalogItemHelper<T> : DropdownPopulateHelper where T : CatalogItemAsset
    {
        List<Tuple<string, string>> m_TmpList = new List<Tuple<string, string>>();
        List<T> m_TmpAssets = new List<T>();

        /// <summary>
        ///     Populates arrays for dropdown menu from Catalog Assets' display name and key.
        /// </summary>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(string selectedKey, bool noneAsFirstItem = false)
        {
            m_TmpList.Clear();

            CatalogSettings.catalogAsset.GetItems(m_TmpAssets);
            foreach (var asset in m_TmpAssets)
            {
                m_TmpList.Add(new Tuple<string, string>(asset.displayName, asset.key));
            }

            m_TmpAssets.Clear();

            return Populate(m_TmpList, selectedKey, noneAsFirstItem);
        }
    }
}
