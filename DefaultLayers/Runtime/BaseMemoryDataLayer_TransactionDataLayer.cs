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
        ///     Provides <see cref="List{T}"/> instances;
        /// </summary>
        static readonly Pool<List<ExchangeDefinitionObject>> s_ExchangesListPool =
            new Pool<List<ExchangeDefinitionObject>>(
                () => new List<ExchangeDefinitionObject>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{Exception}"/> instances;
        /// </summary>
        static readonly Pool<List<Exception>> s_ExceptionListPool =
            new Pool<List<Exception>>(
                () => new List<Exception>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{string}"/> instances;
        /// </summary>
        static readonly Pool<List<string>> s_StringListPool =
            new Pool<List<string>>(
                () => new List<string>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{CurrencyExchangeData}"/> instances;
        /// </summary>
        static readonly Pool<List<CurrencyExchangeData>> s_CurrencyDataListPool =
            new Pool<List<CurrencyExchangeData>>(
                () => new List<CurrencyExchangeData>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="List{InventoryItemSerializableData}"/> instances;
        /// </summary>
        static readonly Pool<List<InventoryItemData>> s_ItemDataListPool =
            new Pool<List<InventoryItemData>>(
                () => new List<InventoryItemData>(),
                list => list.Clear());

        /// <summary>
        ///     Provides <see cref="Dictionary{TKey, TValue}"/> instances;
        /// </summary>
        static readonly Pool<Dictionary<string, long>> s_DictionaryStringLongPool =
            new Pool<Dictionary<string, long>>(
                () => new Dictionary<string, long>(),
                dic => dic.Clear());

        /// <summary>
        ///     We cannot guarantee that the <paramref name="transactionExchange"/>
        ///     contains a unique entry for each currency or item.
        ///     This method consolidates the list to regroup the amounts per
        ///     currency/item.
        /// </summary>
        /// <param name="transactionExchange">The transaction exchange object</param>
        /// <param name="currencies">The target currencies exchange map.</param>
        /// <param name="items">The target items exchange map.</param>
        static void ConsolidateTransactionExchange(
            TransactionExchangeDefinitionObject transactionExchange,
            Dictionary<string, long> currencies,
            Dictionary<string, long> items)
        {
            using (s_ExchangesListPool.Get(out var exchanges))
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
        /// <param name="currencies">The currencies to pay</param>
        /// <param name="items">The items to consume</param>
        /// <param name="exceptions">
        ///     The target collection where the errors are
        ///     added.
        /// </param>
        void VerifyCost(
            Dictionary<string, long> currencies,
            Dictionary<string, long> items,
            ICollection<Exception> exceptions)
        {
            // Checking balances

            foreach (var exchange in currencies)
            {
                var balance = m_WalletDataLayer.GetBalance(exchange.Key);
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
                foreach (var item in m_InventoryDataLayer.m_Items.Values)
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
        ///     Verifies if the item counterparts provided buy the player matches
        ///     the item cost requirements.
        /// </summary>
        /// <param name="counterparts">The items provided by te player.</param>
        /// <param name="requirements">The item cost requirements.</param>
        /// <param name="consumed">
        ///     The collection where consumed items are
        ///     added.
        /// </param>
        /// <param name="exceptions">
        ///     The target collection where the errors are
        ///     added.
        /// </param>
        void VerifyItemPayload(
            ICollection<string> counterparts,
            Dictionary<string, long> requirements,
            ICollection<InventoryItemData> consumed,
            ICollection<Exception> exceptions)
        {
            var inventory = m_InventoryDataLayer;

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
                    var itemFound = inventory.TryGetItem(counterpartId, out var itemData);

                    if (!itemFound)
                    {
                        var exception =
                            new InventoryItemNotFoundException(counterpartId);

                        exceptions.Add(exception);
                        continue;
                    }

                    // Get the definition and decrement/delete the requirements.
                    var definitionKey = itemData.definitionKey;

                    var requirementFound =
                        requirements.TryGetValue(definitionKey, out var count);

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
                    //    var exception =
                    //        new Exception($"Wrong item for the transaction");

                    //    exceptions.Add(exception);
                    //}
                }
            }

            // At the moment, if they is still an entry in the requirements,
            // that means the item payload didn't cover the requirements.

            foreach (var requirement in requirements)
            {
                var exception = new Exception
                    ($"{nameof(BaseMemoryDataLayer)}_TransactionDataLayer: Missing requirement {requirement.Key} ({requirement.Value})");

                exceptions.Add(exception);
            }
        }

        /// <summary>
        ///     Applies the payouts of a transaction.
        /// </summary>
        /// <param name="transaction">
        ///     The transaction whose payouts are
        ///     applied.
        /// </param>
        /// <returns>The description of the payout.</returns>
        TransactionExchangeData ApplyTransactionPayout(BaseTransactionAsset transaction)
        {
            var result = new TransactionExchangeData();

            var itemDataListHandle = s_ItemDataListPool.Get(out var itemDataList);
            var currencyDataListHandle = s_CurrencyDataListPool.Get(out var currencyDataList);
            var exchangeListHandle = s_ExchangesListPool.Get(out var exchangeList);

            try
            {
                transaction.payout.GetItems(exchangeList);

                foreach (var exchange in exchangeList)
                {
                    switch (exchange.catalogItem)
                    {
                        // [3a] Increment the currencies
                        case CurrencyAsset _:
                        {
                            var currencyKey = exchange.catalogItem.key;
                            var balance = exchange.amount;
                            m_WalletDataLayer.AdjustBalance(currencyKey, balance);

                            currencyDataList.Add(new CurrencyExchangeData
                            {
                                currencyKey = currencyKey,
                                amount = balance
                            });

                            break;
                        }

                        // [3b] Create the new items
                        case InventoryItemDefinitionAsset _:
                        {
                            var key = exchange.catalogItem.key;

                            // check if exchange item is stackable
                            var inventoryItemDefinition = exchange.catalogItem as
                                InventoryItemDefinitionAsset;
                            if (inventoryItemDefinition.isStackableFlag)
                            {
                                // create 1 stackable item with desired quantity
                                var item = m_InventoryDataLayer.CreateItem(key, exchange.amount);
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
                                    var item = m_InventoryDataLayer.CreateItem(key);

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

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                result.items = itemDataList.ToArray();
                result.currencies = currencyDataList.ToArray();
            }
            finally
            {
                itemDataListHandle.Release();
                currencyDataListHandle.Release();
                exchangeListHandle.Release();
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
                // TODO: this should use a runtime definition instead of an asset
                var transaction =
                    CatalogSettings.catalogAsset.FindItem(key) as IAPTransactionAsset;

                if (transaction is null)
                {
                    throw new KeyNotFoundException
                    ($"{nameof(BaseMemoryDataLayer)}_TransactionDataLayer: Cannot redeem IAP because" +
                        $" no {nameof(IAPTransactionAsset)} with key {key} was found.");
                }

                var result = ApplyTransactionPayout(transaction);
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

                // TODO: this should use a runtime definition instead of an asset
                var transaction =
                    CatalogSettings.catalogAsset.FindItem(key) as VirtualTransactionAsset;

                if (transaction is null)
                {
                    throw new KeyNotFoundException
                    ($"{nameof(BaseMemoryDataLayer)}_TransactionDataLayer: Cannot complete virtual " +
                        $"transaction because {nameof(VirtualTransactionAsset)} {key} was not found.");
                }

                // [2] I check the cost of this transaction to be sure that the
                //     player fulfills its requirements.

                var currencyExchangesHandle = s_DictionaryStringLongPool.Get(out var currencyExchanges);
                var itemExchangesHandle = s_DictionaryStringLongPool.Get(out var itemExchanges);
                var consumedHandle = s_ItemDataListPool.Get(out var consumed);

                try
                {
                    // [2a] I'm consolidating the costs in a dictionary so I'm
                    //      sure that I have only one amount for each currency
                    //      and each inventory item definition.

                    ConsolidateTransactionExchange
                        (transaction.costs, currencyExchanges, itemExchanges);

                    // [2b] Now I need to validate that the player fulfills the
                    //      requirements of this transaction.
                    //      That means:
                    //      - Check the wallet to validate that he has enough of
                    //        the required currencies
                    //      - Check the specified item ids to confirm their
                    //        existence in the player inventory, and their match
                    //        with the requirement of the transaction.

                    using (s_ExceptionListPool.Get(out var exceptions))
                    {
                        VerifyCost
                            (currencyExchanges, itemExchanges, exceptions);

                        VerifyItemPayload
                            (counterparts, itemExchanges, consumed, exceptions);

                        // If I found some errors while checking the transaction
                        // requirements, I reject the completer with this list
                        // of errors.

                        if (exceptions.Count > 0)
                        {
                            throw new AggregateException(exceptions);
                        }
                    }

                    // [3] Perform the transaction:
                    //     a. Consuming the currencies
                    //     b. Consuming the items
                    //     c. Apply rexards
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
                            m_WalletDataLayer.AdjustBalance(currencyKey, balance);

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
                                m_InventoryDataLayer.DeleteItem(itemData.id);
                            }
                            else
                            {
                                m_InventoryDataLayer.SetQuantity(itemData.id, itemData.quantity);
                            }
                        }

                        result.cost.items = consumed.ToArray();
                    }

                    // [3c] Applying the payouts
                    result.payout = ApplyTransactionPayout(transaction);

                    // [3d] Resolving the promise
                    completer.Resolve(result);
                }
                finally
                {
                    itemExchangesHandle.Release();
                    currencyExchangesHandle.Release();
                    consumedHandle.Release();
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
