using System;
using System.Collections.Generic;
using UnityEngine.Promise;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using PurchaseEventArgs = UnityEngine.Purchasing.PurchaseEventArgs;
using CrossPlatformValidator = UnityEngine.Purchasing.Security.CrossPlatformValidator;

#else
using PurchaseEventArgs = System.Object;
using CrossPlatformValidator = System.Object;
#endif

namespace UnityEngine.GameFoundation
{
    partial class TransactionManagerImpl : ITransactionManager
    {
        event Action<BaseTransaction> ITransactionManager.transactionInitiated
        {
            add => transactionInitiated += value;
            remove => transactionInitiated -= value;
        }

        event Action<BaseTransaction, int, int> ITransactionManager.transactionProgressed
        {
            add => transactionProgressed += value;
            remove => transactionProgressed -= value;
        }

        event Action<BaseTransaction, TransactionResult> ITransactionManager.transactionSucceeded
        {
            add => transactionSucceeded += value;
            remove => transactionSucceeded -= value;
        }

        event Action<BaseTransaction, Exception> ITransactionManager.transactionFailed
        {
            add => transactionFailed += value;
            remove => transactionFailed -= value;
        }

        event Action ITransactionManager.purchasingAdapterInitializeSucceeded
        {
            add => purchasingAdapterInitializeSucceeded += value;
            remove => purchasingAdapterInitializeSucceeded -= value;
        }

        event Action<Exception> ITransactionManager.purchasingAdapterInitializeFailed
        {
            add => purchasingAdapterInitializeFailed += value;
            remove => purchasingAdapterInitializeFailed -= value;
        }

        event Action<PurchaseEventArgs> ITransactionManager.purchaseSucceededInIAPSDK
        {
            add => purchaseSucceededInIAPSDK += value;
            remove => purchaseSucceededInIAPSDK -= value;
        }

        bool ITransactionManager.purchasingAdapterIsInitialized => purchasingAdapterIsInitialized;

        IAPTransaction ITransactionManager.currentIap => currentIap;

        List<PurchaseEventArgs> ITransactionManager.unprocessedPurchases => unprocessedPurchases;

        Deferred<TransactionResult> ITransactionManager.BeginTransaction(BaseTransaction transaction, List<string> costItemIds)
            => BeginTransaction(transaction, costItemIds);

        bool ITransactionManager.IsIapProductOwned(string productId)
            => IsIapProductOwned(productId);

        void ITransactionManager.SetIAPValidator(CrossPlatformValidator validator)
            => SetIAPValidator(validator);

        LocalizedProductMetadata ITransactionManager.GetLocalizedIAPProductInfo(string productId)
            => GetLocalizedIAPProductInfo(productId);

        Deferred<TransactionResult> ITransactionManager.ProcessPurchaseEventArgs(PurchaseEventArgs purchaseEventArgs)
            => ProcessPurchaseEventArgs(purchaseEventArgs);

        void ITransactionManager.RestoreIAPPurchases() => RestoreIAPPurchases();
    }
}
