using UnityEditor;

#if UNITY_EDITOR
namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public sealed partial class RewardItemObject
    {
        /// <summary>
        ///     Add a payout with the given data.
        /// </summary>
        /// <param name="catalogItem">
        ///     The catalog item to add.
        /// </param>
        /// <param name="amount">
        ///     The amount of the <paramref name="catalogItem"/> to add.
        /// </param>
        internal void Editor_AddPayout(TradableDefinitionAsset catalogItem, long amount)
        {
            GameFoundation.Tools.ThrowIfArgNull(catalogItem, nameof(catalogItem));

            var exchange = new ExchangeDefinitionObject
            {
                m_CatalogItem = catalogItem,
                m_Amount = amount
            };
            m_Payout.m_Exchanges.Add(exchange);

            EditorUtility.SetDirty(reward);
        }

        /// <summary>
        ///     Remove the given <paramref name="item"/> from the payout.
        /// </summary>
        /// <param name="item">
        ///     The payout entry to remove.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the item has been removed from payout;
        ///     return <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemovePayout(ExchangeDefinitionObject item)
        {
            var isRemoved = m_Payout.m_Exchanges.Remove(item);

            if (isRemoved)
            {
                EditorUtility.SetDirty(reward);
            }

            return isRemoved;
        }
    }
}
#endif
