using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     Stores the balance for each <see cref="Currency"/>
        /// </summary>
        readonly Dictionary<string, long> m_Balances = new Dictionary<string, long>();

        /// <summary>
        ///     Initializes the data layer for <see cref="IWalletManager"/>.
        /// </summary>
        /// <param name="data">
        ///     Wallet's serializable data.
        /// </param>
        protected void InitializeWalletDataLayer(WalletData data)
        {
            //Reset containers.
            m_Balances.Clear();

            var currencyAssets = new List<CurrencyAsset>();
            m_CatalogAsset.GetItems(currencyAssets);

            var balances = data.balances;

            foreach (var currencyAsset in currencyAssets)
            {
                var found = false;

                // On ChilliConnect, existing players wallet are not updated
                // when a new currency is added, so we don't add any missing
                // currency with their initial balance.
                long balance = 0;
                if (balances != null)
                {
                    foreach (var balanceData in balances)
                    {
                        found = balanceData.currencyKey == currencyAsset.key;

                        if (found)
                        {
                            balance = balanceData.balance;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    k_Logger.LogWarning($"Balance for {nameof(Currency)} {currencyAsset.key} not found in {nameof(WalletData)}");
                }

                m_Balances.Add(currencyAsset.key, balance);
            }
        }

        /// <summary>
        ///     Checks if the specified <paramref name="balance"/> fits with the constraints of the currency.
        /// </summary>
        /// <param name="currencyKey">
        ///     The identifier of the currency to check the balance for.
        /// </param>
        /// <param name="balance">
        ///     The candidate balance to the specific currency
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        /// </param>
        void CheckBalance(string currencyKey, long balance, Rejectable rejectable)
        {
            if (balance < 0)
            {
                var reason = new OverflowException($"{nameof(BaseMemoryDataLayer)}: {nameof(balance)} cannot be less than zero");
                rejectable.Reject(reason);
                return;
            }

            if (!(m_CatalogAsset.FindItem(currencyKey) is CurrencyAsset currency))
            {
                var reason = new CatalogItemNotFoundException<Currency>(currencyKey);
                rejectable.Reject(reason);
                return;
            }

            var maximum = currency.maximumBalance;
            if (maximum > 0 && balance > maximum)
            {
                var oldBalance = m_Balances[currencyKey];

                // Only throw an overflow exception when increasing the balance.
                if (balance > oldBalance)
                {
                    var reason = new OverflowException($"{nameof(BaseMemoryDataLayer)}: {balance.ToString()} exceeds the limits ({maximum.currentValue.ToString()})");
                    rejectable.Reject(reason);
                }
            }
        }

        /// <summary>
        ///     Add or remove the <paramref name="amount"/> to/from the existing
        ///     balance of the currency given by its <paramref name="currencyKey"/>.
        /// </summary>
        /// <param name="currencyKey">
        ///     The identifier of currency to adjust.
        /// </param>
        /// <param name="amount">
        ///     The amount to add (if positive) or remove (if negative).
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        /// </param>
        /// <returns>
        ///     Return the up-to-date value of the balance.
        /// </returns>
        long AdjustBalance(string currencyKey, long amount, Rejectable rejectable)
        {
            m_Balances.TryGetValue(currencyKey, out var oldBalance);
            var newBalance = oldBalance + amount;

            CheckBalance(currencyKey, newBalance, rejectable);

            m_Balances[currencyKey] = newBalance;

            return newBalance;
        }

        /// <summary>
        ///     Sets the balance of the currency given by its
        ///     <paramref name="currencyKey"/> to the given <paramref name="balance"/>.
        /// </summary>
        /// <param name="currencyKey">
        ///     The identifier of the currency to set.
        /// </param>
        /// <param name="balance">
        ///     The new balance.
        /// </param>
        /// <param name="rejectable">
        ///     The handle to the rejectable promise in case this operation fails.
        /// </param>
        void SetBalance(string currencyKey, long balance, Rejectable rejectable)
        {
            CheckBalance(currencyKey, balance, rejectable);
            m_Balances[currencyKey] = balance;
        }

        /// <summary>
        ///     Get the amount of a given currency in the wallet.
        /// </summary>
        /// <param name="currencyKey">
        ///     The key of the currency to count.
        /// </param>
        /// <returns>
        ///     The amount of the given currency in the wallet.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the currency could not be found in the wallet.
        /// </exception>
        long GetBalance(string currencyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(currencyKey, nameof(currencyKey));

            var found = m_Balances.TryGetValue(currencyKey, out var balance);
            if (!found)
            {
                throw new ArgumentException(
                    $"{nameof(BaseMemoryDataLayer)}: Cannot get balance because {nameof(Currency)} {currencyKey} " +
                    "cannot be found", nameof(currencyKey));
            }

            return balance;
        }

        /// <inheritdoc/>
        WalletData IWalletDataLayer.GetData()
        {
            var data = new WalletData
            {
                balances = new BalanceData[m_Balances.Count]
            };
            var index = 0;
            foreach (var balanceEntry in m_Balances)
            {
                data.balances[index++] = new BalanceData
                {
                    currencyKey = balanceEntry.Key,
                    balance = balanceEntry.Value
                };
            }

            return data;
        }

        /// <inheritdoc/>
        void IWalletDataLayer.AddBalance(string currencyKey, long balance, Completer<long> completer)
        {
            // Check balance validity
            {
                var isRejected = Tools.RejectIfArgNegative(balance, nameof(balance), completer);
                if (isRejected)
                    return;
            }

            try
            {
                var newBalance = AdjustBalance(currencyKey, balance, completer);
                completer.Resolve(newBalance);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void IWalletDataLayer.RemoveBalance(string currencyKey, long balance, Completer<long> completer)
        {
            // Check balance validity
            {
                var isRejected = Tools.RejectIfArgNegative(balance, nameof(balance), completer);
                if (isRejected)
                    return;
            }

            try
            {
                var newBalance = AdjustBalance(currencyKey, -balance, completer);
                completer.Resolve(newBalance);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void IWalletDataLayer.SetBalance(string currencyKey, long balance, Completer completer)
        {
            try
            {
                SetBalance(currencyKey, balance, completer);
                completer.Resolve();
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }
    }
}
