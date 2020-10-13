using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Static Properties of a Catalog Item Asset.
    /// </summary>
    class DropdownStaticPropertyHelper : DropdownPopulateHelper
    {
        List<Tuple<string, string>> m_TmpList = new List<Tuple<string, string>>();

        /// <summary>
        ///     Populates arrays for dropdown menu from Static Properties of a Catalog Item Asset.
        /// </summary>
        /// <param name="catalogItemAsset">
        ///     A reference to a Catalog Item Asset that is used to populate the array
        ///     from its Static Properties.
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
        public int Populate(CatalogItemAsset catalogItemAsset, string selectedKey, PropertyType propertyType, bool noneAsFirstItem = false)
        {
            m_TmpList.Clear();

            var properties = catalogItemAsset?.GetStaticProperties();
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property.value.type == propertyType)
                    {
                        m_TmpList.Add(new Tuple<string, string>(property.key, property.key));
                    }
                }
            }

            return Populate(m_TmpList, selectedKey, noneAsFirstItem);
        }
    }
}
