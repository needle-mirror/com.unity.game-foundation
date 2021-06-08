using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     A helper class that populates Names and Key arrays from Static Properties of Virtual Transaction
    ///     Asset's Cost Objects for dropdown menus. Accepts either a single transaction or a list of transactions.
    /// </summary>
    class DropdownCostItemPropertyHelper : DropdownStaticPropertyHelper
    {
        List<ExchangeDefinitionObject> m_CostExchangeObjects = new List<ExchangeDefinitionObject>();
        List<TradableDefinitionAsset> m_UniqueCostItems = new List<TradableDefinitionAsset>();
        int m_TotalNumberOfCostItems;

        /// <summary>
        ///     Populates arrays for dropdown menu from Static Properties of Virtual Transaction Asset's Cost Objects.
        ///     If the BaseTransactionAsset passed in is of type IAP Transaction Asset, the only list option will be
        ///     None or empty (depending on noneAsFirstItem flag).
        /// </summary>
        /// <param name="transactionAsset">
        ///     A reference to a Base Transaction Asset that is used to populate the array
        ///     from its Payout Objects' Static Properties.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="desiredPropertyTypes">
        ///     The array of PropertyTypes that are desired to gather keys from.
        /// </param>
        /// <param name="onlyFirstCost">
        ///     Whether only the first Cost Item's Static Properties will be used to populate arrays or all of
        ///     the Cost Items'.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(BaseTransactionAsset transactionAsset, string selectedKey, PropertyType[] desiredPropertyTypes,
            bool onlyFirstCost = true, bool noneAsFirstItem = true)
        {
            ClearCollections();

            if (transactionAsset != null && transactionAsset is VirtualTransactionAsset virtualTransactionAsset)
            {
                virtualTransactionAsset.costs.GetItems(m_CostExchangeObjects);
                m_TotalNumberOfCostItems = m_CostExchangeObjects.Count;

                foreach (var exchangeObj in m_CostExchangeObjects)
                {
                    AddEligibleStaticPropertiesToDropdown(exchangeObj.catalogItem, desiredPropertyTypes);

                    if (onlyFirstCost)
                    {
                        break;
                    }
                }
            }

            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        /// <summary>
        ///     Populates arrays for dropdown menu from Static Properties of Virtual Transaction Asset's Cost Objects.
        ///     If the BaseTransactionAsset passed in is of type IAP Transaction Asset, the only list option will be
        ///     None or empty (depending on noneAsFirstItem flag).
        /// </summary>
        /// <param name="baseTransactionAssets">
        ///     A list of Base Transaction Assets that are used to populate the array.
        /// </param>
        /// <param name="selectedKey">
        ///     A key for selected item in the dropdown menu. If nothing is selected, the key should be null.
        /// </param>
        /// <param name="desiredPropertyTypes">
        ///     The array of PropertyTypes that are desired to gather keys from.
        /// </param>
        /// <param name="onlyFirstCost">
        ///     Whether only the first Cost Item's Static Properties will be used to populate arrays or all of
        ///     the Cost Items'.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(List<BaseTransactionAsset> baseTransactionAssets, string selectedKey,
            PropertyType[] desiredPropertyTypes, bool onlyFirstCost = true, bool noneAsFirstItem = true)
        {
            ClearCollections();
            m_TotalNumberOfCostItems = 0;

            if (baseTransactionAssets != null)
            {
                foreach (var baseTransactionAsset in baseTransactionAssets)
                {
                    if (!(baseTransactionAsset is VirtualTransactionAsset virtualTransactionAsset))
                        continue;

                    virtualTransactionAsset.costs.GetItems(m_CostExchangeObjects);
                    GetUniqueCostItems(onlyFirstCost);
                }

                m_TotalNumberOfCostItems = m_UniqueCostItems.Count;

                AddStaticPropertiesOfUniqueCostItems(desiredPropertyTypes);
            }

            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        protected override void ClearCollections()
        {
            base.ClearCollections();
            m_CostExchangeObjects.Clear();
            m_UniqueCostItems.Clear();
        }

        private void GetUniqueCostItems(bool onlyFirstCost = true)
        {
            foreach (var exchangeObj in m_CostExchangeObjects)
            {
                AddIfCatalogItemIsUnique(exchangeObj.catalogItem);

                if (onlyFirstCost)
                {
                    return;
                }
            }
        }

        private void AddIfCatalogItemIsUnique(TradableDefinitionAsset catalogItem)
        {
            if (!m_UniqueCostItems.Exists(x => x.key == catalogItem.key))
            {
                m_UniqueCostItems.Add(catalogItem);
            }
        }

        private void AddStaticPropertiesOfUniqueCostItems(PropertyType[] desiredPropertyTypes)
        {
            foreach (var costItem in m_UniqueCostItems)
            {
                AddEligibleStaticPropertiesToDropdown(costItem, desiredPropertyTypes);
            }
        }

        protected override DetailedDropdownItem CreateDetailedDropdownItem(string propertyKey, PropertyType propertyType)
        {
            return new DetailedDropdownItem(propertyKey, propertyType, m_TotalNumberOfCostItems);
        }
    }
}
