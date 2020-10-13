using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    static class Tools
    {
        public static class Pools
        {
            //public static readonly Pool<List<CatalogItemAsset>> itemList = new Pool<List<CatalogItemAsset>>(
            //    () => new List<CatalogItemAsset>(),
            //    list => list.Clear());

            //public static readonly Pool<List<InventoryItemDefinitionAsset>> inventoryItemDefinitionList = new Pool<List<InventoryItemDefinitionAsset>>(
            //    () => new List<InventoryItemDefinitionAsset>(),
            //    list => list.Clear());

            public static readonly Pool<List<TagAsset>> tagList = new Pool<List<TagAsset>>(
                () => new List<TagAsset>(),
                list => list.Clear());

            //public static Pool<List<RewardAsset>> rewardList = new Pool<List<RewardAsset>>(
            //    () => new List<RewardAsset>(),
            //    list => list.Clear());

            public static Pool<List<RewardItemObject>> rewardItemList = new Pool<List<RewardItemObject>>(
                () => new List<RewardItemObject>(),
                list => list.Clear());

            public static Pool<List<StoreItemObject>> storeItemList = new Pool<List<StoreItemObject>>(
                () => new List<StoreItemObject>(),
                list => list.Clear());
        }

        /// <summary>
        ///     Checks to see if the argument is a valid Identifier.
        ///     Valid Ids are alphanumeric with optional dashes or underscores.
        ///     No whitespace is permitted.
        /// </summary>
        /// <param name="key">
        ///     Identifier to check.
        /// </param>
        /// <returns>
        ///     Whether Identifier is valid or not.
        /// </returns>
        public static bool IsValidKey(string key)
        {
            return key != null && Regex.IsMatch(key, @"^[a-zA-Z][a-zA-Z0-9\-_]*$");
        }
    }
}
