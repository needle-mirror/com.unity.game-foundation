using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Static Properties of a Catalog Item Asset.
    /// </summary>
    class DropdownStaticPropertyHelper : DropdownPopulateHelper
    {
        protected List<DropdownItem> m_DropdownItems = new List<DropdownItem>();
        protected Dictionary<string, DetailedDropdownItem> m_FilteredDropdownItems = 
            new Dictionary<string, DetailedDropdownItem>();

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
        /// <param name="desiredPropertyType">
        ///     The type of the Property.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(CatalogItemAsset catalogItemAsset, string selectedKey, PropertyType desiredPropertyType,
            bool noneAsFirstItem = true)
        {
            ClearCollections();
            AddEligibleStaticPropertiesToDropdown(catalogItemAsset, desiredPropertyType);
            BuildFinalizedDropdownItems();
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

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
        /// <param name="desiredPropertyTypes">
        ///     The array of PropertyTypes that are desired to gather keys from.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(CatalogItemAsset catalogItemAsset, string selectedKey, PropertyType[] desiredPropertyTypes,
            bool noneAsFirstItem = true)
        {
            ClearCollections();
            AddEligibleStaticPropertiesToDropdown(catalogItemAsset, desiredPropertyTypes);
            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        protected virtual void ClearCollections()
        {
            m_DropdownItems.Clear();
            m_FilteredDropdownItems.Clear();
        }

        protected void AddEligibleStaticPropertiesToDropdown(CatalogItemAsset catalogItemAsset,
            PropertyType desiredPropertyType)
        {
            if (catalogItemAsset != null)
            {
                var properties = catalogItemAsset.GetStaticProperties();
                FilterStaticProperties(properties, desiredPropertyType);
            }
        }

        protected void AddEligibleStaticPropertiesToDropdown(CatalogItemAsset catalogItemAsset,
            PropertyType[] desiredPropertyTypes)
        {
            if (!(catalogItemAsset is null))
            {
                var properties = catalogItemAsset.GetStaticProperties();
                foreach (var desiredPropertyType in desiredPropertyTypes)
                {
                    FilterStaticProperties(properties, desiredPropertyType);
                }
            }
        }

        void FilterStaticProperties(List<PropertyData> properties, PropertyType desiredPropertyType)
        {
            foreach (var propertyData in properties)
            {
                if (PropertyTypeMatchesDesired(propertyData, desiredPropertyType) &&
                    PropertyHasExpectedTypeValuePairing(propertyData.value))
                {
                    AddOrIncrementFilteredDropdownItem(propertyData);
                }
            }
        }

        bool PropertyTypeMatchesDesired(PropertyData propertyData, PropertyType desiredPropertyType)
        {
            return propertyData.value.type == desiredPropertyType;
        }

        bool PropertyHasExpectedTypeValuePairing(Property property)
        {
            if (property.type == PropertyType.ResourcesAsset && property.AsAsset<Sprite>() is null)
            {
                return false;
            }

            return true;
        }

        void AddOrIncrementFilteredDropdownItem(PropertyData propertyData)
        {
            var key = propertyData.key;

            var valueFound = m_FilteredDropdownItems.TryGetValue(key, out var detailedDropdownItem);

            if (valueFound)
            {
                detailedDropdownItem.IncrementOccurrences();
            }
            else
            {
                detailedDropdownItem = CreateDetailedDropdownItem(propertyData.key, propertyData.value.type);
            }

            m_FilteredDropdownItems[propertyData.key] = detailedDropdownItem;
        }

        protected virtual DetailedDropdownItem CreateDetailedDropdownItem(string propertyKey, PropertyType propertyType)
        {
            return new DetailedDropdownItem(propertyKey, propertyType);
        }

        protected void BuildFinalizedDropdownItems(bool useNestedNameFormat = false)
        {
            foreach (var detailedDropdownItem in m_FilteredDropdownItems.Values)
            {
                var displayName = GetDropdownItemDisplayName(detailedDropdownItem, useNestedNameFormat);
                m_DropdownItems.Add(new DropdownItem(displayName, detailedDropdownItem.propertyKey));
            }
        }

        string GetDropdownItemDisplayName(DetailedDropdownItem detailedDropdownItem,
            bool useNestedNameFormat = false)
        {
            return detailedDropdownItem.GetDisplayName(useNestedNameFormat);
        }
    }
}
