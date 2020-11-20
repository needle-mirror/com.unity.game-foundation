using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class StoreConfig
    {
        /// <inheritdoc/>
        protected override Store CompileItem(Rejectable rejectable)
        {
            for (var i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];
                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    continue;
                }

                const string messageFormat = "The {0} has an empty entry in {1}.";
                rejectable.Reject(new InvalidConfigDataException(messageFormat)
                {
                    invalidConfig = this,
                    fieldName = $"{nameof(transactions)}[{i.ToString()}]"
                });

                return null;
            }

            var store = new Store
            {
                m_Items = new BaseTransaction[transactions.Count]
            };

            return store;
        }

        /// <inheritdoc/>
        protected override void LinkItem(
            Store runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable)
        {
            for (var i = 0; i < transactions.Count; i++)
            {
                var transactionKey = transactions[i];
                if (!compiledItems.TryGetValue(transactionKey, out var rawItem))
                {
                    var messageFormat = "No catalog item with the key \"" + transactionKey + "\" could be found for {1} in the {0}.";
                    rejectable.Reject(new InvalidConfigDataException(messageFormat)
                    {
                        invalidConfig = this,
                        fieldName = $"{nameof(transactions)}[{i.ToString()}]"
                    });

                    return;
                }

                if (!(rawItem is BaseTransaction transaction))
                {
                    var messageFormat = $"The catalog item \"{transactionKey}\" is not a {nameof(BaseTransaction)}" +
                        " and can't be used for {1} in the {0}.";
                    rejectable.Reject(new InvalidConfigDataException(messageFormat)
                    {
                        invalidConfig = this,
                        fieldName = $"{nameof(transactions)}[{i.ToString()}]"
                    });

                    return;
                }

                runtimeObject.m_Items[i] = transaction;
            }
        }
    }
}
