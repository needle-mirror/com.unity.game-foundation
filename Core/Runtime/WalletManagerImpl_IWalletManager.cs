using System;

namespace UnityEngine.GameFoundation
{
    partial class WalletManagerImpl : IWalletManager
    {
        event Action<IQuantifiable, long> IWalletManager.balanceChanged
        {
            add => balanceChanged += value;
            remove => balanceChanged -= value;
        }

        bool IWalletManager.Add(Currency currency, long balance)
        {
            Tools.ThrowIfArgNegative(balance, nameof(balance));
            Validate(currency, nameof(currency));

            return Add(currency, balance);
        }

        long IWalletManager.Get(Currency currency)
        {
            Validate(currency, nameof(currency));

            return Get(currency);
        }

        bool IWalletManager.Remove(Currency currency, long balance)
        {
            Tools.ThrowIfArgNegative(balance, nameof(balance));
            Validate(currency, nameof(currency));

            return Remove(currency, balance);
        }

        bool IWalletManager.Set(Currency currency, long balance)
        {
            Tools.ThrowIfArgNegative(balance, nameof(balance));
            Validate(currency, nameof(currency));

            return Set(currency, balance);
        }
    }
}
