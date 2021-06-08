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
    /// <summary>
    ///     This class contains methods to process virtual transactions and in-app purchases.
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        ///     Invoked as soon as a valid transaction is initiated.
        /// </summary>
        event Action<BaseTransaction> transactionInitiated;

        /// <summary>
        ///     Invoked after every time the progress number is increased on a transaction's <see cref="Deferred{TResult}"/>.
        /// </summary>
        event Action<BaseTransaction, int, int> transactionProgressed;

        /// <summary>
        ///     Invoked after a transaction has succeeded at all levels.
        /// </summary>
        event Action<BaseTransaction, TransactionResult> transactionSucceeded;

        /// <summary>
        ///     Invoked after a transaction fails.
        /// </summary>
        event Action<BaseTransaction, Exception> transactionFailed;

        /// <summary>
        ///     Invoked after the purchasing adapter reports successfully being initialized.
        /// </summary>
        event Action purchasingAdapterInitializeSucceeded;

        /// <summary>
        ///     Invoked after the purchasing adapter reports failing to initialize.
        /// </summary>
        event Action<Exception> purchasingAdapterInitializeFailed;

        /// <summary>
        ///     Invoked after a purchase has succeeded in the IAP SDK,
        ///     but before the purchase has been processed by the Game Foundation data layer.
        ///     It includes the PurchaseEventArgs object that was returned by the IAP SDK.
        ///     It does not include an IAPTransaction or IAPTransaction key because this event
        ///     happens regardless of whether or not an associated IAPTransaction exists.
        ///     You can use <see cref="Catalog.FindIAPTransactionByProductId"/> to see if
        ///     this PurchaseEventArgs does relate to an IAPTransaction.
        /// </summary>
        event Action<PurchaseEventArgs> purchaseSucceededInIAPSDK;

        /// <summary>
        ///     Invoked after a purchase has succeeded in the purchasing adapter,
        ///     but before the purchase has been processed by the Game Foundation data layer.
        ///     It includes data that was returned by the purchasing adapter.
        ///     It does not include an IAPTransaction or IAPTransaction key because this event
        ///     happens regardless of whether or not an associated IAPTransaction exists.
        ///     You can use <see cref="Catalog.FindIAPTransactionByProductId"/> to see if
        ///     this IapResult does relate to an IAPTransaction.
        /// </summary>
        event Action<IapResult> purchaseSucceededInPurchasingAdapter;

        /// <summary>
        ///     Returns true if the optional purchasing adapter has finished initializing.
        /// </summary>
        bool purchasingAdapterIsInitialized { get; }

        /// <summary>
        ///     This is a Transaction that is currently in progress, recently initiated by the user.
        /// </summary>
        IAPTransaction currentIap { get; }

        /// <summary>
        ///     A list of <see cref="PurchaseEventArgs"/> that were not automatically
        ///     processed because the "Process Background Purchases" option was unchecked.
        ///     It is up to the developer to fully process these.
        /// </summary>
        List<PurchaseEventArgs> unprocessedPurchases { get; }

        /// <summary>
        ///     Process a transaction.
        /// </summary>
        /// <param name="transaction">
        ///     A <see cref="BaseTransaction"/> to process.
        /// </param>
        /// <param name="costItemIds">
        ///     If this is a virtual transaction with item costs, this is the list of items to consume.
        ///     If this argument is null or empty, the first inventory items that satisfy the cost will be consumed.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="Deferred"/> struct which can be used to track the state of the transaction.
        /// </returns>
        Deferred<TransactionResult> BeginTransaction(BaseTransaction transaction, List<string> costItemIds = null);

        /// <summary>
        ///     Determine if specified Product Id is owned by the player.
        /// </summary>
        /// <param name="productId">Product Id for which to search.</param>
        /// <returns>
        ///     <c>true</c> if specified Product Id is owned by the player, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the <see cref="ITransactionManager"/> implementation has not been initialized.
        /// </exception>
        bool IsIapProductOwned(string productId);

        /// <summary>
        ///     This uses the purchasing adapter to get localized product info from the platform store.
        /// </summary>
        /// <param name="productId">
        ///     The product ID for which you want localized info.
        /// </param>
        /// <returns>
        ///     A struct containing localized name and price strings.
        /// </returns>
        /// <exception cref="Exception">
        ///     Throws an exception if no purchasing adapter has been initialized.
        /// </exception>
        LocalizedProductMetadata GetLocalizedIAPProductInfo(string productId);

        /// <summary>
        ///     Set a validator instance for the purchasing adapter to use.
        /// </summary>
        /// <param name="validator">
        ///     The validator reference to set.
        /// </param>
        void SetIAPValidator(CrossPlatformValidator validator);

        /// <summary>
        ///     Use this to manually process purchases that succeeded in the background,
        ///     such as when purchases are restored, or automatically processed from a previous session.
        /// </summary>
        /// <param name="purchaseEventArgs">
        ///     The <see cref="PurchaseEventArgs"/> to process.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="Deferred"/> struct which can be used to track the state of the processing.
        /// </returns>
        Deferred<TransactionResult> ProcessPurchaseEventArgs(PurchaseEventArgs purchaseEventArgs);

        /// <summary>
        ///     Tells the IAP system to try and restore past purchases.
        ///     Currently this only applies to iOS.
        /// </summary>
        void RestoreIAPPurchases();
    }
}
