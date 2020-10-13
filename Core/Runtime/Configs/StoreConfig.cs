using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configurator for a <see cref="Store"/> instance.
    /// </summary>
    public sealed class StoreConfig : CatalogItemConfig<Store>
    {
        /// <summary>
        ///     The identifiers of the <see cref="BaseTransaction"/> this store will
        ///     expose.
        /// </summary>
        public readonly List<string> transactions = new List<string>();

        /// <inheritdoc/>
        protected internal override Store CompileItem()
        {
            for (var i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];
                Tools.ThrowIfArgNullOrEmpty(transaction, nameof(transactions), i);
            }

            var store = new Store
            {
                m_Items = new BaseTransaction[transactions.Count]
            };

            return store;
        }

        /// <inheritdoc/>
        protected internal override void LinkItem(CatalogBuilder builder)
        {
            for (var i = 0; i < transactions.Count; i++)
            {
                var transactionKey = transactions[i];

                var catalogItemConfig = builder.GetConfigOrDie(transactionKey);

                if (!(catalogItemConfig is BaseTransactionConfig transactionConfig))
                {
                    throw new InvalidCastException
                        ($"{nameof(CatalogItemConfig)} {transactionKey} is not a {nameof(BaseTransactionConfig)}");
                }

                runtimeItem.m_Items[i] = transactionConfig.runtimeItem;
            }
        }
    }
}
