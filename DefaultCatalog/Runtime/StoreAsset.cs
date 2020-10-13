using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Description of a virtual store.
    ///     A store exposes a list of <see cref="BaseTransactionAsset"/>.
    /// </summary>
    public sealed partial class StoreAsset : CatalogItemAsset
    {
        /// <summary>
        ///     The list of <see cref="BaseTransactionAsset"/> with the possibility to disable the entry.
        /// </summary>
        [SerializeField]
        internal List<StoreItemObject> m_StoreItems;

        /// <summary>
        ///     Adds all the <see cref="StoreItemObject"/> to the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">
        ///     The target collection where the <see cref="StoreItemObject"/> are added.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="StoreItemObject"/> added.
        /// </returns>
        public int GetStoreItems(ICollection<StoreItemObject> target = null, bool clearTarget = true)
            => GFTools.Copy(m_StoreItems, target, clearTarget);

        /// <summary>
        ///     Tells whether or not the store contains the given <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="BaseTransactionAsset"/> to find.
        /// </param>
        /// <returns>
        ///     <c>true</c> if found, <c>false</c> otherwise.
        /// </returns>
        public bool Contains(BaseTransactionAsset transaction)
        {
            GFTools.ThrowIfArgNull(transaction, nameof(transaction));

            foreach (var item in m_StoreItems)
            {
                if (item.m_Transaction == transaction)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Initializes the store items collection.
        /// </summary>
        protected override void AwakeDefinition()
        {
            if (m_StoreItems is null)
            {
                m_StoreItems = new List<StoreItemObject>();
            }

            foreach (var item in m_StoreItems)
            {
                item.store = this;
            }
        }

        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var item = builder.Create<StoreConfig>(key);

            foreach (var storeItem in m_StoreItems)
            {
                if (storeItem.enabled)
                {
                    if (storeItem.m_Transaction is null) continue;
                    item.transactions.Add(storeItem.m_Transaction.key);
                }
            }

            return item;
        }
    }
}
