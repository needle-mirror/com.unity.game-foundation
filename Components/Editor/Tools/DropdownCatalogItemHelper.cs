using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Catalog Item Assets' display name and key for dropdown menus.
    /// </summary>
    /// <typeparam name="TCatalogItemAsset">
    ///     Type of the Catalog Item Asset.
    /// </typeparam>
    class DropdownCatalogItemHelper<TCatalogItemAsset> : DropdownPopulateHelper where TCatalogItemAsset : CatalogItemAsset
    {
        List<TCatalogItemAsset> m_CatalogItemAssets = new List<TCatalogItemAsset>();
        List<DropdownItem> m_DropdownItems = new List<DropdownItem>();

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
        public int Populate(string selectedKey, bool noneAsFirstItem = true)
        {
            m_CatalogItemAssets.Clear();
            m_DropdownItems.Clear();

            PrefabTools.GetLookUpCatalogAsset().GetItems(m_CatalogItemAssets);
            
            foreach (var catalogItemAsset in m_CatalogItemAssets)
            {
                m_DropdownItems.Add(new DropdownItem(catalogItemAsset.displayName, catalogItemAsset.key));
            }

            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }
    }
}
