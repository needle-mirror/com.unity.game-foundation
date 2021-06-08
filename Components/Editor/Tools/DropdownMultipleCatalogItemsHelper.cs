using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from a List of Catalog Item Assets' display name and key for dropdown menus.
    /// </summary>
    class DropdownMultipleCatalogItemsHelper : DropdownStaticPropertyHelper
    {
        int m_TotalNumberOfItems;

        /// <summary>
        ///     Populates arrays for dropdown menu from Catalog Item Assets' display name and key.
        /// </summary>
        /// <param name="catalogItemAssets">
        ///     A list of Catalog Item Asset that are used to populate the array.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="desiredPropertyType">
        ///     The type of the Property.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate<T>(List<T> catalogItemAssets, string selectedKey, PropertyType desiredPropertyType,
            bool noneAsFirstItem = true)
            where T : CatalogItemAsset
        {
            ClearCollections();

            m_TotalNumberOfItems = 0;
            if (catalogItemAssets != null)
            {
                m_TotalNumberOfItems = catalogItemAssets.Count;

                foreach (var catalogItemAsset in catalogItemAssets)
                {
                    AddEligibleStaticPropertiesToDropdown(catalogItemAsset, desiredPropertyType);
                }
            }

            BuildFinalizedDropdownItems();
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        /// <summary>
        ///     Populates arrays for dropdown menu from Catalog Item Assets' display name and key.
        /// </summary>
        /// <param name="catalogItemAssets">
        ///     A list of Catalog Item Asset that are used to populate the array.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="desiredPropertyTypes">
        ///     The array of PropertyTypes that are desired to gather keys from.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate<T>(List<T> catalogItemAssets, string selectedKey, PropertyType[] desiredPropertyTypes,
            bool noneAsFirstItem = true)
            where T : CatalogItemAsset
        {
            ClearCollections();

            m_TotalNumberOfItems = 0;
            if (catalogItemAssets != null)
            {
                m_TotalNumberOfItems = catalogItemAssets.Count;

                foreach (var catalogItemAsset in catalogItemAssets)
                {
                    AddEligibleStaticPropertiesToDropdown(catalogItemAsset, desiredPropertyTypes);
                }
            }

            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        protected override DetailedDropdownItem CreateDetailedDropdownItem(string propertyKey, PropertyType propertyType)
        {
            return new DetailedDropdownItem(propertyKey, propertyType, m_TotalNumberOfItems);
        }
    }
}
