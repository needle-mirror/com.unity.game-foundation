using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     We cannot guarantee that the <paramref name="transactionExchange"/>
        ///     contains a unique entry for each currency or item.
        ///     This method consolidates the list to regroup the amounts per currency/item.
        /// </summary>
        /// <param name="transactionExchange">
        ///     The transaction exchange object.
        /// </param>
        /// <param name="currencies">
        ///     The target currencies exchange map.
        /// </param>
        /// <param name="items">
        ///     The target items exchange map.
        /// </param>
        static void ConsolidateTransactionExchange(
            TransactionExchangeDefinitionObject transactionExchange,
            Dictionary<string, long> currencies,
            Dictionary<string, long> items)
        {
            using (k_ExchangesListPool.Get(out var exchanges))
            {
                // Consolidating the costs.
                transactionExchange.GetItems(exchanges);
                foreach (var exchange in exchanges)
                {
                    var itemKey = exchange.catalogItem.key;

                    switch (exchange.catalogItem)
                    {
                        case CurrencyAsset _:
                        {
                            var found = currencies.TryGetValue(itemKey, out var amount);

                            amount = exchange.amount + (found ? amount : 0);
                            currencies[itemKey] = amount;
                            break;
                        }

                        case InventoryItemDefinitionAsset _:
                        {
                            var found = items.TryGetValue(itemKey, out var amount);
                            amount = exchange.amount + (found ? amount : 0);
                            items[itemKey] = amount;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Checks the costs of a virtual transaction with the player's
        ///     resources.
        /// </summary>
        /// <param name="currencies">
        ///     The currencies to pay.
        /// </param>
        /// <param name="items">
        ///     The items to consume.
        /// </param>
        /// <param name="exceptions">
        ///     The target collection where the errors are added.
        /// </param>
        void VerifyCost(
            Dictionary<string, long> currencies,
            Dictionary<string, long> items,
            ICollection<Exception> exceptions)
        {
            // Checking balances

            foreach (var exchange in currencies)
            {
                long balance;
                try
                {
                    balance = GetBalance(exchange.Key);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);

                    continue;
                }

                if (balance < exchange.Value)
                {
                    var exception = new NotEnoughBalanceException
                        (exchange.Key, exchange.Value, balance);
                    exceptions.Add(exception);
                }
            }

            // TODO: exception if resulting balance would be over the limit

            // Checking items count.

            foreach (var exchange in items)
            {
                // determine total quantity of all items
                long quantity = 0;
                foreach (var item in m_Items.Values)
                {
                    if (item.definitionKey == exchange.Key)
                    {
                        quantity += item.quantity;
                    }
                }

                // throw exception if quantity is insufficient
                if (quantity < exchange.Value)
                {
                    var exception = new NotEnoughItemOfDefinitionException
                        (exchange.Key, exchange.Value, quantity);

                    exceptions.Add(exception);
                }
            }
        }

        /// <summary>
        ///     Verifies if the item counterparts provided by the player matches the item cost requirements.
        /// </summary>
        /// <param name="counterparts">
        ///     The items provided by te player.
        /// </param>
        /// <param name="requirements">
        ///     The item cost requirements.
        /// </param>
        /// <param name="consumed">
        ///     The collection where consumed items are added.
        /// </param>
        /// <param name="transactionKey">
        ///     The key of the verified transaction.
        /// </param>
        /// <param name="exceptions">
        ///     The target collection where the errors are added.
        /// </param>
        void VerifyItemPayload(
            ICollection<string> counterparts,
            Dictionary<string, long> requirements,
            ICollection<InventoryItemData> consumed,
            string transactionKey,
            ICollection<Exception> exceptions)
        {
            using (Tools.Pools.stringList.Get(out var uniqueCounterparts))
            {
                // Get unique item ids
                foreach (var itemId in counterparts)
                {
                    if (!uniqueCounterparts.Contains(itemId))
                    {
                        uniqueCounterparts.Add(itemId);
                    }
                }

                foreach (var counterpartId in uniqueCounterparts)
                {
                    // Check if the item exists
                    var itemFound = TryGetItem(counterpartId, out var itemData);

                    if (!itemFound)
                    {
                        var exception = new InventoryItemNotFoundException(counterpartId);

                        exceptions.Add(exception);
                        continue;
                    }

                    // Get the definition and decrement/delete the requirements.
                    var definitionKey = itemData.definitionKey;

                    var requirementFound = requirements.TryGetValue(definitionKey, out var count);

                    if (requirementFound)
                    {
                        // consume appropriate quantity
                        var consumeQuantity = count <= itemData.quantity
                            ? count
                            : itemData.quantity;

                        var consumedItem = new InventoryItemData
                        {
                            definitionKey = itemData.definitionKey,
                            id = itemData.id,
                            quantity = itemData.quantity - consumeQuantity,
                            mutableProperties = itemData.mutableProperties
                        };

                        // consume correct quantity of the item
                        consumed.Add(consumedItem);

                        // reduce remaining count needed
                        count -= consumeQuantity;

                        // update remaining quantity needed and remove requirement if fulfilled
                        if (count > 0)
                        {
                            requirements[definitionKey] = count;
                        }

                        // if done with this requirement then remove it
                        else
                        {
                            requirements.Remove(definitionKey);

                            // if we're out of requirements, no need to keep searching
                            if (requirements.Count == 0)
                            {
                                break;
                            }
                        }
                    }

                    // I've decided not the throw an error if an item of the
                    // counterparts is not necessary.
                    //else
                    //{
                    //    var exception = new GameFoundationException($"Wrong item for the transaction");
                    //    exceptions.Add(exception);
                    //}
                }
            }

            // At the moment, if there is still an entry in the requirements,
            // that means the item payload didn't cover the requirements.

            if (requirements.Count > 0)
            {
                var exception = new MissingTransactionRequirementsException(transactionKey, requirements);
                exceptions.Add(exception);
            }
        }

        /// <summary>
        ///     Applies the payouts of a transaction.
        /// </summary>
        /// <param name="transaction">
        ///     The transaction whose payouts are applied.
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        /// </param>
        /// <returns>
        ///     The description of the payout.
        /// </returns>
        TransactionExchangeData ApplyTransactionPayout(BaseTransactionAsset transaction, Rejectable rejectable)
        {
            var result = new TransactionExchangeData();

            using (k_ItemDataListPool.Get(out var itemDataList))
            using (k_CurrencyDataListPool.Get(out var currencyDataList))
            using (k_ExchangesListPool.Get(out var exchangeList))
            {
                transaction.payout.GetItems(exchangeList);

                for (var exchangeIndex = 0; exchangeIndex < exchangeList.Count; exchangeIndex++)
                {
                    var exchange = exchangeList[exchangeIndex];
                    switch (exchange.catalogItem)
                    {
                        // [3a] Increment the currencies
                        case CurrencyAsset _:
                        {
                            var currencyKey = exchange.catalogItem.key;
                            var balance = exchange.amount;
                            AdjustBalance(currencyKey, balance, rejectable);
                            if (!rejectable.isActive)
                            {
                                return default;
                            }

                            currencyDataList.Add(new CurrencyExchangeData
                            {
                                currencyKey = currencyKey,
                                amount = balance
                            });

                            break;
                        }

                        // [3b] Create the new items
                        case InventoryItemDefinitionAsset inventoryItemDefinition:
                        {
                            var key = exchange.catalogItem.key;

                            // check if exchange item is stackable
                            if (inventoryItemDefinition.isStackableFlag)
                            {
                                // create 1 stackable item with desired quantity
                                var item = CreateItem(key, rejectable, exchange.amount);
                                if (!rejectable.isActive)
                                {
                                    return default;
                                }

                                var itemData = new InventoryItemData
                                {
                                    id = item.id,
                                    quantity = exchange.amount,
                                    definitionKey = key
                                };

                                itemDataList.Add(itemData);
                            }

                            // if item is NOT stackable
                            else
                            {
                                // iterate to grant desired count of items
                                for (var i = 0; i < exchange.amount; i++)
                                {
                                    var item = CreateItem(key, rejectable);
                                    if (!rejectable.isActive)
                                    {
                                        return default;
                                    }

                                    var itemData = new InventoryItemData
                                    {
                                        id = item.id,
                                        quantity = 1,
                                        definitionKey = key
                                    };

                                    itemDataList.Add(itemData);
                                }
                            }

                            break;
                        }

                        case null:
                        {
                            var message = $"The payout item #{exchangeIndex.ToString()} for the transaction \"{transaction.key}\" is null.";
                            rejectable.Reject(new NullReferenceException(message));

                            return default;
                        }

                        default:
                        {
                            var message = $"The payout item #{exchangeIndex.ToString()} for the transaction \"{transaction.key}\" isn't supported.";
                            rejectable.Reject(new NotSupportedException(message));

                            return default;
                        }
                    }
                }

                result.items = itemDataList.ToArray();
                result.currencies = currencyDataList.ToArray();
            }

            return result;
        }

        /// <summary>
        ///     Redeems an IAP.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the IAP Transaction to redeem.
        /// </param>
        /// <param name="completer">
        ///     The completer of the caller of this method.
        /// </param>
        void RedeemIap(string key, Completer<TransactionExchangeData> completer)
        {
            try
            {
                if (!(m_CatalogAsset.FindItem(key) is IAPTransactionAsset transaction))
                {
                    var message = $"{nameof(BaseMemoryDataLayer)}_TransactionDataLayer: Cannot redeem IAP because" +
                        $" no {nameof(IAPTransactionAsset)} with key {key} was found.";
                    completer.Reject(new KeyNotFoundException(message));

                    return;
                }

                var result = ApplyTransactionPayout(transaction, completer);
                completer.Resolve(result);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void ITransactionDataLayer.MakeVirtualTransaction(
            string key,
            ICollection<string> counterparts,
            Completer<VirtualTransactionExchangeData> completer)
        {
            try
            {
                // [1] I get the transaction description form its key.

                if (!(m_CatalogAsset.FindItem(key) is VirtualTransactionAsset transaction))
                {
                    var message = $"{nameof(BaseMemoryDataLayer)}_TransactionDataLayer: Cannot complete virtual " +
                        $"transaction because {nameof(VirtualTransactionAsset)} {key} was not found.";
                    completer.Reject(new KeyNotFoundException(message));

                    return;
                }

                // [2] I check the cost of this transaction to be sure that the
                //     player fulfills its requirements.

                using (k_DictionaryStringLongPool.Get(out var currencyExchanges))
                using (k_DictionaryStringLongPool.Get(out var itemExchanges))
                using (k_ItemDataListPool.Get(out var consumed))
                {
                    // [2a] I'm consolidating the costs in a dictionary so I'm
                    //      sure that I have only one amount for each currency
                    //      and each inventory item definition.

                    ConsolidateTransactionExchange(transaction.costs, currencyExchanges, itemExchanges);

                    // [2b] Now I need to validate that the player fulfills the
                    //      requirements of this transaction.
                    //      That means:
                    //      - Check the wallet to validate that he has enough of
                    //        the required currencies
                    //      - Check the specified item ids to confirm their
                    //        existence in the player inventory, and their match
                    //        with the requirement of the transaction.

                    using (k_ExceptionListPool.Get(out var exceptions))
                    {
                        VerifyCost(currencyExchanges, itemExchanges, exceptions);

                        VerifyItemPayload(counterparts, itemExchanges, consumed, key, exceptions);

                        // If I found some errors while checking the transaction
                        // requirements, I reject the completer with this list
                        // of errors.

                        if (exceptions.Count > 0)
                        {
                            completer.Reject(new AggregateException(exceptions));

                            return;
                        }
                    }

                    // [3] Perform the transaction:
                    //     a. Consuming the currencies
                    //     b. Consuming the items
                    //     c. Apply payouts
                    //     d. Complete the promise

                    var result = new VirtualTransactionExchangeData();

                    // [3a] Consuming the currencies
                    {
                        var count = currencyExchanges.Count;
                        result.cost.currencies = new CurrencyExchangeData[count];
                        var index = 0;

                        foreach (var exchange in currencyExchanges)
                        {
                            var currencyKey = exchange.Key;
                            var balance = -exchange.Value;
                            AdjustBalance(currencyKey, balance, completer);
                            if (!completer.isActive)
                            {
                                return;
                            }

                            result.cost.currencies[index++] = new CurrencyExchangeData
                            {
                                currencyKey = currencyKey,
                                amount = balance
                            };
                        }
                    }

                    // [3b] Consuming the items
                    {
                        foreach (var itemData in consumed)
                        {
                            if (itemData.quantity <= 0)
                            {
                                DeleteItem(itemData.id);
                            }
                            else
                            {
                                SetQuantity(itemData.id, itemData.quantity, completer);
                            }
                        }

                        result.cost.items = consumed.ToArray();
                    }

                    // [3c] Applying the payouts
                    result.payout = ApplyTransactionPayout(transaction, completer);

                    // [3d] Resolving the promise
                    completer.Resolve(result);
                }
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void ITransactionDataLayer.RedeemAppleIap
            (string key, string receipt, Completer<TransactionExchangeData> completer)
            => RedeemIap(key, completer);

        /// <inheritdoc/>
        void ITransactionDataLayer.RedeemGoogleIap(
            string key,
            string purchaseData,
            string purchaseDataSignature,
            Completer<TransactionExchangeData> completer)
            => RedeemIap(key, completer);
    }
}
