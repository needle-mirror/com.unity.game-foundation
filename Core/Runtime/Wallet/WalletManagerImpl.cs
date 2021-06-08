using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    [ExcludeFromDocs]
    partial class WalletManagerImpl : ManagerImplementation
    {
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(WalletManagerImpl));

        /// <summary>
        ///     Accessor to GameFoundation's current DAL.
        /// </summary>
        static IWalletDataLayer dataLayer => GameFoundationSdk.dataLayer;

        /// <inheritdoc cref="IWalletManager.balanceChanged"/>
        event Action<IQuantifiable, long> balanceChanged;

        /// <summary>
        ///     The cached balances.
        ///     This dictionary is the one used when getting the balances so there is no need to ask the data layer.
        /// </summary>
        readonly Dictionary<Currency, long> m_Balances = new Dictionary<Currency, long>();

        /// <summary>
        ///     Initializes the player balances.
        ///     For each currency available in the current catalog,
        ///     it gets the persisted player balance, or set it to 0.
        /// </summary>
        /// <inheritdoc/>
        protected override void InitializeData(Completer completer, GameFoundationInitOptions initOptions = null)
        {
            var data = dataLayer.GetData();
            var currencies = new List<Currency>();
            GameFoundationSdk.catalog.GetItems(currencies);

            foreach (var currency in currencies)
            {
                long balance = 0;

                foreach (var balanceEntry in data.balances)
                {
                    if (balanceEntry.currencyKey == currency.key)
                    {
                        balance = balanceEntry.balance;
                        break;
                    }
                }

                m_Balances.Add(currency, balance);
            }

            completer.Resolve();
        }

        /// <inheritdoc/>
        internal override void Uninitialize()
        {
            m_Balances.Clear();
            balanceChanged = default;
        }

        /// <summary>
        ///     Increases the balance of the specified <paramref name="currency"/>, but does not sync with the data
        ///     layer.
        /// </summary>
        /// <param name="currency">
        ///     The currency to add the <paramref name="balance"/> to.
        /// </param>
        /// <param name="balance">
        ///     The balance to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the update is done, <c>false</c> otherwise.
        /// </returns>
        internal bool AddBalanceInternal(Currency currency, long balance)
        {
            if (balance == 0) return true;

            var oldBalance = m_Balances[currency];
            var newBalance = oldBalance + balance;

            if (currency.maximumBalance != 0 && newBalance > currency.maximumBalance)
            {
                return false;
            }

            m_Balances[currency] = newBalance;

            balanceChanged?.Invoke(currency, oldBalance);

            return true;
        }

        /// <summary>
        ///     Decreases the balance of the specified <paramref name="currency"/>, but does not sync with the data
        ///     layer.
        /// </summary>
        /// <param name="currency">
        ///     The currency to remove the <paramref name="balance"/> from.
        /// </param>
        /// <param name="balance">
        ///     The balance to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the update is done, <c>false</c> otherwise.
        /// </returns>
        internal bool RemoveBalanceInternal(Currency currency, long balance)
        {
            if (balance == 0) return true;

            var oldBalance = m_Balances[currency];
            var newBalance = oldBalance - balance;

            if (newBalance < 0) return false;

            m_Balances[currency] = newBalance;

            balanceChanged?.Invoke(currency, oldBalance);

            return true;
        }

        /// <summary>
        ///     Sets the balance of the specified <paramref name="currency"/>, but does not sync with the data layer.
        /// </summary>
        /// <param name="currency">
        ///     The currency to set the balance of.
        /// </param>
        /// <param name="balance">
        ///     The balance to set.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the update is done, <c>false</c> otherwise.
        /// </returns>
        internal bool SetBalanceInternal(Currency currency, long balance)
        {
            if (currency.maximumBalance > 0 && balance > currency.maximumBalance)
            {
                return false;
            }

            var oldBalance = m_Balances[currency];

            if (oldBalance == balance) return true;

            m_Balances[currency] = balance;

            balanceChanged?.Invoke(currency, oldBalance);

            return true;
        }

        /// <inheritdoc cref="IWalletManager.Add(Currency, long)"/>
        public bool Add(Currency currency, long balance)
        {
            if (!AddBalanceInternal(currency, balance))
            {
                return false;
            }

            dataLayer.AddBalance(currency.key, balance, Completer<long>.None);

            return true;
        }

        /// <inheritdoc cref="IWalletManager.Get(Currency)"/>
        public long Get(Currency currency) => m_Balances[currency];

        /// <inheritdoc cref="IWalletManager.Remove(Currency, long)"/>
        public bool Remove(Currency currency, long balance)
        {
            if (!RemoveBalanceInternal(currency, balance))
            {
                return false;
            }

            dataLayer.RemoveBalance(currency.key, balance, Completer<long>.None);

            return true;
        }

        /// <inheritdoc cref="IWalletManager.Set(Currency, long)"/>
        public bool Set(Currency currency, long balance)
        {
            if (!SetBalanceInternal(currency, balance))
            {
                return false;
            }

            dataLayer.SetBalance(currency.key, balance, Completer.None);

            return true;
        }

        /// <summary>
        ///     Validate a currency parameter.
        /// </summary>
        /// <param name="currency">
        ///     The currency to check.
        /// </param>
        /// <param name="paramName">
        ///     The name of the currency parameter in the calling method.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Validate(Currency currency, string paramName)
        {
            Tools.ThrowIfArgNull(currency, paramName);

            if (!m_Balances.ContainsKey(currency))
            {
                throw new ArgumentException($"{nameof(WalletManagerImpl)}: {currency.displayName} not found", nameof(currency));
            }
        }
    }
}
