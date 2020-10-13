using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.Components
{
    /// <summary>
    ///     It's an helper class that populates Names and Key arrays
    ///     from Static Properties of Base Transaction Asset's Payout Objects for dropdown menus.
    /// </summary>
    class DropdownPayoutItemPropertyHelper : DropdownPopulateHelper
    {
        List<Tuple<string, string>> m_TmpList = new List<Tuple<string, string>>();
        ICollection<ExchangeDefinitionObject> m_TmpExhangeObjects = new List<ExchangeDefinitionObject>();

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
        /// <param name="propertyType">
        ///     A Property key.
        /// </param>
        /// <param name="onlyFirstPayout">
        ///     Whether only the first Payout Item's Static Properties will be used to populate arrays or all of Payout Items'.
        /// </param>
        /// <param name="noneAsFirstItem">
        ///     Whether the first item in dropdown menu will be None or not. Its key will be null.
        /// </param>
        /// <returns>
        ///     Return the index of the selected key.
        /// </returns>
        public int Populate(BaseTransactionAsset transactionAsset, string selectedKey, PropertyType propertyType, bool onlyFirstPayout = true, bool noneAsFirstItem = false)
        {
            m_TmpList.Clear();
            m_TmpExhangeObjects.Clear();

            if (transactionAsset != null)
            {
                transactionAsset.payout.GetItems(m_TmpExhangeObjects);

                foreach (var exchangeObj in m_TmpExhangeObjects)
                {
                    var staticProperties = exchangeObj.catalogItem.GetStaticProperties();
                    foreach (var staticProperty in staticProperties)
                    {
                        var key = staticProperty.key;
                        if (staticProperty.value.type == propertyType && !m_TmpList.Exists(x => x.Item1 == key))
                        {
                            m_TmpList.Add(new Tuple<string, string>(key, key));
                        }
                    }

                    if (onlyFirstPayout) break;
                }
            }

            return Populate(m_TmpList, selectedKey, noneAsFirstItem);
        }
    }
}
