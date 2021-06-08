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

        internal RewardItemObject Clone()
        {
            return new RewardItemObject
            {
                m_Payout = m_Payout.Clone(),
                key = key,
                reward = reward
            };
        }

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal void RefreshReferences(CatalogAsset owner)
        {
            reward = (RewardAsset)owner.FindItem(reward.key);

            m_Payout.RefreshReferences(owner);
        }
    }
}
#endif
