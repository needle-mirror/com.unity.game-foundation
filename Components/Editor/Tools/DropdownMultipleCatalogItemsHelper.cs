using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from a List of Catalog Item Assets' display name and key for dropdown menus.
    /// </summary>
    class DropdownMultipleCatalogItemsHelper : DropdownPopulateHelper
    {
        List<Tuple<string, string>> m_TmpList = new List<Tuple<string, string>>();
        Dictionary<string, int> m_TmpProperyList = new Dictionary<string, int>();

        /// <summary>
        ///     Populates arrays for dropdown menu from Catalog Item Assets' display name and key.
        /// </summary>
        /// <param name="catalogItemAssets">
        ///     A list of Catalog Item Asset that are used to populate the array.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="propertyType">
        ///     The type of the Property.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(List<CatalogItemAsset> catalogItemAssets, string selectedKey, PropertyType propertyType, bool noneAsFirstItem = false)
        {
            m_TmpList.Clear();
            m_TmpProperyList.Clear();

            var itemCount = 0;
            if (catalogItemAssets != null)
            {
                itemCount = catalogItemAssets.Count;

                foreach (var catalogItem in catalogItemAssets)
                {
                    var staticProperties = catalogItem.GetStaticProperties();
                    foreach (var staticProperty in staticProperties)
                    {
                        if (staticProperty.value.type == propertyType)
                        {
                            var key = staticProperty.key;
                            m_TmpProperyList[key] = m_TmpProperyList.ContainsKey(key) ? m_TmpProperyList[key] + 1 : 1;
                        }
                    }
                }
            }

            foreach (var property in m_TmpProperyList)
            {
                var key = property.Key;
                var displayName = property.Value == itemCount ? key : key + $" - [on {property.Value} of {itemCount} items]";
                m_TmpList.Add(new Tuple<string, string>(displayName, key));
            }

            return Populate(m_TmpList, selectedKey, noneAsFirstItem);
        }
    }
}
