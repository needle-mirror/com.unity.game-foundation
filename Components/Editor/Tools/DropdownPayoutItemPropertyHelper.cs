using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Static Properties of Base Transaction Asset's Payout Objects for dropdown menus.
    /// </summary>
    class DropdownPayoutItemPropertyHelper : DropdownStaticPropertyHelper
    {
        List<ExchangeDefinitionObject> m_PayoutExchangeObjects = new List<ExchangeDefinitionObject>();
        List<TradableDefinitionAsset> m_UniquePayoutItems = new List<TradableDefinitionAsset>();
        int m_TotalNumberOfPayoutItems;

        /// <summary>
        ///     Populates arrays for dropdown menu from Static Properties of Base Transaction Asset's Payout Objects
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
        /// <param name="onlyFirstPayout">
        ///     Whether only the first Payout Item's Static Properties will be used to populate arrays or all of
        ///     the Payout Items'.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(BaseTransactionAsset transactionAsset, string selectedKey,
            PropertyType[] desiredPropertyTypes, bool onlyFirstPayout = true, bool noneAsFirstItem = true)
        {
            ClearCollections();

            if (transactionAsset != null)
            {
                transactionAsset.payout.GetItems(m_PayoutExchangeObjects);
                m_TotalNumberOfPayoutItems = m_PayoutExchangeObjects.Count;

                foreach (var exchangeObj in m_PayoutExchangeObjects)
                {
                    AddEligibleStaticPropertiesToDropdown(exchangeObj.catalogItem, desiredPropertyTypes);

                    if (onlyFirstPayout)
                    {
                        break;
                    }
                }
            }

            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }

        /// <summary>
        ///     Populates arrays for dropdown menu from Static Properties of Base Transaction Asset's Payout Objects.
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
        /// <param name="onlyFirstPayout">
        ///     Whether only the first Payout Item's Static Properties will be used to populate arrays or all of
        ///     the Payout Items'.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(List<BaseTransactionAsset> baseTransactionAssets, string selectedKey,
            PropertyType[] desiredPropertyTypes, bool onlyFirstPayout = true, bool noneAsFirstItem = true)
        {
            ClearCollections();
            m_TotalNumberOfPayoutItems = 0;

            if (baseTransactionAssets != null)
            {
                foreach (var baseTransactionAsset in baseTransactionAssets)
                {
                    baseTransactionAsset.payout.GetItems(m_PayoutExchangeObjects);
                    GetUniquePayoutItems(onlyFirstPayout);
                }

                m_TotalNumberOfPayoutItems = m_UniquePayoutItems.Count;

                AddStaticPropertiesOfUniquePayoutItems(desiredPropertyTypes);
            }

            BuildFinalizedDropdownItems(useNestedNameFormat: true);
            return Populate(m_DropdownItems, selectedKey, noneAsFirstItem);
        }
        
        protected override void ClearCollections()
        {
            base.ClearCollections();
            m_PayoutExchangeObjects.Clear();
            m_UniquePayoutItems.Clear();
        }

        private void GetUniquePayoutItems(bool onlyFirstPayout = true)
        {
            foreach (var exchangeObj in m_PayoutExchangeObjects)
            {
                AddIfCatalogItemIsUnique(exchangeObj.catalogItem);

                if (onlyFirstPayout)
                {
                    return;
                }
            }
        }

        private void AddIfCatalogItemIsUnique(TradableDefinitionAsset catalogItem)
        {
            if (!m_UniquePayoutItems.Exists(x => x.key == catalogItem.key))
            {
                m_UniquePayoutItems.Add(catalogItem);
            }
        }

        private void AddStaticPropertiesOfUniquePayoutItems(PropertyType[] desiredPropertyTypes)
        {
            foreach (var payoutItem in m_UniquePayoutItems)
            {
                AddEligibleStaticPropertiesToDropdown(payoutItem, desiredPropertyTypes);
            }
        }

        protected override DetailedDropdownItem CreateDetailedDropdownItem(string propertyKey, PropertyType propertyType)
        {
            return new DetailedDropdownItem(propertyKey, propertyType, m_TotalNumberOfPayoutItems);
        }
    }
}
