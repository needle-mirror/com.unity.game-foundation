﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.Internal;
using UnityEngine.Promise;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
using PurchaseEventArgs = UnityEngine.Purchasing.PurchaseEventArgs;
using CrossPlatformValidator = UnityEngine.Purchasing.Security.CrossPlatformValidator;

#else
using PurchaseEventArgs = System.Object;
using CrossPlatformValidator = System.Object;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contains portable information about a successful IAP purchase.
    /// </summary>
    struct PurchaseConfirmation
    {
        public string productId;
        public string[] receiptParts;
    }

    /// <summary>
    ///     Contains portable localized information about an IAP product.
    /// </summary>
    public struct LocalizedProductMetadata
    {
        /// <summary>
        ///     The name of the IAP product.
        /// </summary>
        public string name;

        /// <summary>
        ///     The price of the IAP product.
        /// </summary>
        public string price;
    }

    /// <inheritdoc cref="ITransactionManager"/>
    [ExcludeFromDocs]
    partial class TransactionManagerImpl
    {
        /// <summary>
        ///     Simple wrapper for list of IAP Product Id strings.
        ///     Needed to serialize to json format.
        /// </summary>
        class IapProductsJsonWrapper
        {
            /// <summary>
            ///     Create a Json wrapper for specified list of IAP Product Ids.
            /// </summary>
            /// <param name="iapProducts">
            ///     List of IAP Product Id strings.
            /// </param>
            public IapProductsJsonWrapper(List<string> iapProducts)
            {
                this.iapProducts = iapProducts;
            }

            /// <summary>
            ///     Construct Json wrapper from a json string.
            /// </summary>
            /// <param name="iapProducts">
            /// </param>
            public IapProductsJsonWrapper(string iapProducts)
            {
                this.iapProducts =
                    JsonUtility.FromJson<IapProductsJsonWrapper>(iapProducts).iapProducts;
            }

            /// <summary>
            ///     List of IAP Product Id strings in this json wrapper.
            ///     Note: it is not possible to use property here as JsonUtility needs to access the list as a memeber.
            /// </summary>
            public List<string> iapProducts;
        }

        /// <summary>
        ///     Filename to save iap products.
        ///     File will be written to App's persistent data path.
        /// </summary>
        const string k_purchasedIapProductsFilename = "iapProducts";

        /// <summary>
        ///     Extension for json files 'json'.
        /// </summary>
        const string k_jsonExtension = "json";

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(TransactionManagerImpl));

        /// <summary>
        ///     Fully qualified path for writing iap products to applications persistent data path.
        /// </summary>
        static string purchasedIapProductsFullPath =>
            $"{Application.persistentDataPath}/{k_purchasedIapProductsFilename}.{k_jsonExtension}";

        /// <summary>
        ///     Fully qualified path for backing up iap products to applications persistent data path.
        /// </summary>
        static string purchasedIapProductsBackupPath =>
            $"{Application.persistentDataPath}/{k_purchasedIapProductsFilename}_backup.{k_jsonExtension}";

        /// <summary>
        ///     Accessor to GameFoundation's current DAL.
        /// </summary>
        static ITransactionDataLayer dataLayer => GameFoundationSdk.dataLayer;

        VirtualTransaction m_CurrentVirtualTransaction;

        /// <summary>
        ///     List of all successfully-purchased IAP product ids.
        /// </summary>
        internal List<string> m_PurchasedIapProducts;

        /// <summary>
        ///     The In-App purchase process data.
        /// </summary>
        (IAPTransaction transaction, bool isSuccessful, string failureMessage) m_CurrentIap;

        /// <inheritdoc cref="ITransactionManager.transactionInitiated"/>
        public event Action<BaseTransaction> transactionInitiated;

        /// <inheritdoc cref="ITransactionManager.transactionProgressed"/>
        public event Action<BaseTransaction, int, int> transactionProgressed;

        /// <inheritdoc cref="ITransactionManager.transactionSucceeded"/>
        public event Action<BaseTransaction, TransactionResult> transactionSucceeded;

        /// <inheritdoc cref="ITransactionManager.transactionFailed"/>
        public event Action<BaseTransaction, Exception> transactionFailed;

        /// <inheritdoc cref="ITransactionManager.purchasingAdapterInitializeSucceeded"/>
        public event Action purchasingAdapterInitializeSucceeded;

        /// <inheritdoc cref="ITransactionManager.purchasingAdapterInitializeFailed"/>
        public event Action<Exception> purchasingAdapterInitializeFailed;

#pragma warning disable 0067
        /// <inheritdoc cref="ITransactionManager.purchaseSucceededInIAPSDK"/>
        public event Action<PurchaseEventArgs> purchaseSucceededInIAPSDK;
#pragma warning restore 0067

        /// <inheritdoc cref="ITransactionManager.currentIap"/>
        public IAPTransaction currentIap => m_CurrentIap.transaction;

        /// <inheritdoc cref="ITransactionManager.purchasingAdapterInitializeFailed"/>
        public bool purchasingAdapterIsInitialized { get; private set; }

        /// <summary>
        ///     Initializes the transaction manager.
        /// </summary>
        public void Initialize()
        {
            // read the purchased iap products list
            DeserializePurchasedIapProducts();

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

            // you should only ever construct one UnityPurchasingAdapter per session
            if (m_PurchasingAdapter == null)
            {
                m_PurchasingAdapter = new UnityPurchasingAdapter();
            }

            m_PurchasingAdapter.Initialize();
#endif
        }

        /// <summary>
        ///     Resets the transaction manager.
        /// </summary>
        public void Uninitialize()
        {
            if (m_CurrentIap.transaction != null)
            {
                const string message =
                    "An IAP is still pending while uninitializing TransactionManager. This might cause unexpected behaviour.";
                k_GFLogger.LogWarning(message);
            }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            m_PurchasingAdapter?.Uninitialize();

            // don't nullify this; you should only ever construct one UnityPurchasingAdapter per session
            // s_PurchasingAdapter = null;

            purchasingAdapterIsInitialized = false;
#endif

            m_CurrentVirtualTransaction = null;
            m_CurrentIap = default;

            m_PurchasedIapProducts = null;
        }

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
        /// <exception cref="NotSupportedException">
        ///     The completer is rejected if purchasing is not enabled in Game Foundation settings,
        ///     or if purchasing is not enabled in Unity Services.
        /// </exception>
        public Deferred<TransactionResult> BeginTransaction
            (BaseTransaction transaction, List<string> costItemIds = null)
        {
            Tools.ThrowIfArgNull(transaction, nameof(transaction));

            Promises.GetHandles<TransactionResult>(out var deferred, out var completer);

            switch (transaction)
            {
                case VirtualTransaction virtualTransaction:
                {
                    // The given transaction is already being processed.
                    if (ReferenceEquals(transaction, m_CurrentVirtualTransaction))
                    {
                        completer.Reject(new ArgumentException(
                            $"{nameof(TransactionManagerImpl)}: This transaction key ({transaction.key}) is already being processed."));

                        return deferred;
                    }

                    m_CurrentVirtualTransaction = virtualTransaction;

                    if (costItemIds == null) costItemIds = new List<string>();
                    GameFoundationSdk.updater.StartCoroutine(
                        ProcessVirtualTransactionCoroutine(virtualTransaction, completer, costItemIds));

                    break;
                }

                case IAPTransaction iapTransaction:
                {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

                    // The given transaction is already being processed.
                    if (ReferenceEquals(transaction, m_CurrentIap.transaction))
                    {
                        completer.Reject(new ArgumentException(
                            $"{nameof(TransactionManagerImpl)}: This transaction key ({transaction.key}) is already being processed."));

                        return deferred;
                    }

                    // Another IAP is already being processed.
                    if (m_CurrentIap.transaction != null)
                    {
                        completer.Reject(new ArgumentException(
                            $"{nameof(TransactionManagerImpl)}: Can't start the IAP \"{iapTransaction.productId}\" because"
                            + $" \"{m_CurrentIap.transaction.productId}\" is already being processed."));

                        return deferred;
                    }

                    // Make sure to reset the whole tuple to start on a clean slate.
                    m_CurrentIap = (iapTransaction, false, null);

                    GameFoundationSdk.updater.StartCoroutine(
                        ProcessIAPTransactionCoroutine(completer, iapTransaction));

#else
                    completer.Reject(new NotSupportedException(
                        $"{nameof(TransactionManagerImpl)}: Tried to process an IAP transaction, but IAP support is not enabled."));
#endif
                    break;
                }

                default:
                    completer.Reject(new ArgumentException(
                        $"{nameof(TransactionManagerImpl)}: Unknown or unsupported transaction type: {transaction.GetType()}"));
                    break;
            }

            return deferred;
        }

        /// <summary>
        ///     This uses the purchasing adapter to get localized product info from the platform store.
        /// </summary>
        /// <param name="productId">
        ///     The product ID for which you want localized info.
        /// </param>
        /// <returns>
        ///     A struct containing localized name and price strings.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     Throws an exception if no purchasing adapter has been initialized.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown if purchasing is not enabled in Game Foundation settings,
        ///     or if purchasing is not enabled in Unity Services.
        /// </exception>
        internal LocalizedProductMetadata GetLocalizedIAPProductInfo(string productId)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            return m_PurchasingAdapter.GetLocalizedProductInfo(productId);
#else
            return default;
#endif
        }

        /// <summary>
        ///     The in-app purchasing adapter should call this method after it's successfully initialized.
        /// </summary>
        internal void PurchasingAdapterInitializationSucceeded()
        {
            purchasingAdapterIsInitialized = true;
            purchasingAdapterInitializeSucceeded?.Invoke();
        }

        /// <summary>
        ///     The in-app purchasing adapter should call this if it fails to initialize.
        /// </summary>
        /// <param name="exception">
        ///     The reason for the failure.
        /// </param>
        internal void PurchasingAdapterInitializationFailed(Exception exception)
        {
            purchasingAdapterIsInitialized = false;
            purchasingAdapterInitializeFailed?.Invoke(exception);
        }

        /// <summary>
        ///     Coroutine which processes a <see cref="VirtualTransaction"/> and updates a <see cref="Completer{TResult}"/>.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="VirtualTransaction"/> to process.
        /// </param>
        /// <param name="completer">
        ///     The <see cref="Completer{TResult}"/> to update as the <see cref="VirtualTransaction"/> is processed.
        /// </param>
        /// <param name="costItemIds">
        ///     The specific ids for each <see cref="InventoryItem"/> to consume as the cost for this transaction.
        ///     This parameter is required but it can be empty.
        /// </param>
        IEnumerator ProcessVirtualTransactionCoroutine(
            VirtualTransaction transaction,
            Completer<TransactionResult> completer,
            ICollection<string> costItemIds)
        {
            completer.SetProgression(0, 3);
            transactionInitiated?.Invoke(transaction);
            transactionProgressed?.Invoke(transaction, 0, 3);

            // pre-validate the transaction before sending it to the data layer

            var exceptionList = new List<Exception>();

            transaction.VerifyCost(exceptionList);
            transaction.VerifyPayout(exceptionList);

            if (exceptionList.Count > 0)
            {
                var exceptions = new AggregateException(exceptionList);
                completer.Reject(exceptions);
                transactionFailed?.Invoke(transaction, exceptions);
                m_CurrentVirtualTransaction = null;
                yield break;
            }

            completer.SetProgression(1, 3);
            transactionProgressed?.Invoke(transaction, 1, 3);

            // the data layer will re-validate the transaction and fulfill it if it's valid

            Promises.GetHandles<VirtualTransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            // if there are item costs, but no instance ids are supplied,
            // then we infer that we should go find some instances automatically
            var itemCost = transaction.costs.GetDefinitionCount<InventoryItemDefinition>();
            if (itemCost > 0 && costItemIds.Count <= 0)
            {
                transaction.AutoFillCostItemIds(costItemIds);
            }

            dataLayer.MakeVirtualTransaction(transaction.key, costItemIds, dalCompleter);

            while (!dalDeferred.isDone)
            {
                yield return null;
            }

            completer.SetProgression(2, 3);
            transactionProgressed?.Invoke(transaction, 2, 3);

            // handle the response from the DAL
            // even if the pre-validation succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                var costs = ProcessCostsInternally(dalDeferred.result.cost);
                var payout = ApplyPayoutInternally(dalDeferred.result.payout);

                var result = new TransactionResult(costs, payout);

                completer.Resolve(result);
                transactionSucceeded?.Invoke(transaction, result);
            }
            else
            {
                completer.Reject(dalDeferred.error);
                transactionFailed?.Invoke(transaction, dalDeferred.error);
            }

            dalDeferred.Release();
            m_CurrentVirtualTransaction = null;
        }

        /// <summary>
        ///     Removes currency and items from the Game Foundation Wallet and Inventory without also informing the Data Layer.
        /// </summary>
        /// <param name="exchangeCost">
        ///     The costs to deduct from the Wallet and/or Inventory.
        /// </param>
        /// <returns>
        ///     An object representing the costs actually deducted, including ids of any Inventory Items that were removed.
        /// </returns>
        TransactionCosts ProcessCostsInternally(TransactionExchangeData exchangeCost)
        {
            // WALLET COST

            var currencyCostCount = exchangeCost.currencies.Length;
            var currencyCosts = new CurrencyExchange[currencyCostCount];

            for (var i = 0; i < currencyCostCount; i++)
            {
                // the amount in the currency exchange is negative,
                // but RemoveBalance expects only positive numbers
                var amount = Math.Abs(exchangeCost.currencies[i].amount);
                var currency = GameFoundationSdk.catalog.Find<Currency>(exchangeCost.currencies[i].currencyKey);
                (GameFoundationSdk.wallet as WalletManagerImpl).RemoveBalanceInternal(currency, amount);

                currencyCosts[i] = new CurrencyExchange
                {
                    amount = exchangeCost.currencies[i].amount,
                    currency = GameFoundationSdk.catalog.Find<Currency>(exchangeCost.currencies[i].currencyKey)
                };
            }

            // INVENTORY COST

            var itemCostCount = exchangeCost.items.Length;
            var itemCosts = new string[itemCostCount];

            for (var i = 0; i < itemCostCount; i++)
            {
                var exchangeItem = exchangeCost.items[i];
                var item = GameFoundationSdk.inventory.FindItem(exchangeItem.id);

                // if item is used up (or nonstackable) then remove it
                if (exchangeItem.quantity <= 0)
                {
                    (GameFoundationSdk.inventory as InventoryManagerImpl).DeleteInternal(item);
                }

                // if item is stackable and NOT used up then set new quantity
                else
                {
                    (item as StackableInventoryItem).SetQuantityInternal(exchangeItem.quantity);
                }

                itemCosts[i] = exchangeCost.items[i].definitionKey;
            }

            return new TransactionCosts(itemCosts, currencyCosts);
        }

        /// <summary>
        ///     Grants items and currency to the Game Foundation Wallet and Inventory without also informing the Data Layer.
        /// </summary>
        /// <param name="exchange">
        ///     The payout to grant to the wallet and/or inventory.
        /// </param>
        /// <returns>
        ///     An object representing the payout actually granted, including any new Inventory Items created.
        /// </returns>
        internal Payout ApplyPayoutInternally(TransactionExchangeData exchange)
        {
            using (Tools.Pools.tradableList.Get(out var tradables))
            {
                // WALLET

                foreach (var currencyData in exchange.currencies)
                {
                    var currency = GameFoundationSdk.catalog.Find<Currency>(currencyData.currencyKey);
                    (GameFoundationSdk.wallet as WalletManagerImpl).AddBalanceInternal(currency, currencyData.amount);

                    tradables.Add(new CurrencyExchange
                    {
                        currency = currency,
                        amount = currencyData.amount
                    });
                }

                // INVENTORY

                foreach (var itemData in exchange.items)
                {
                    var definition = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(itemData.definitionKey);
                    var item = (GameFoundationSdk.inventory as InventoryManagerImpl).CreateInternal(definition, itemData.id);
                    tradables.Add(item);

                    // update stackable item quantity.
                    if (item is StackableInventoryItem stackable)
                    {
                        stackable.SetQuantityInternal(itemData.quantity);
                    }
                }

                // RESULT

                var collection = new ReadOnlyCollection<ITradable>(tradables.ToArray());
                return new Payout(collection);
            }
        }

        /// <summary>
        ///     Tells the IAP SDK to begin the purchase restoration process (only works on iOS).
        /// </summary>
        void RestoreIAPPurchases()
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (m_PurchasingAdapter.isAppleIOS)
            {
                m_PurchasingAdapter.RestorePurchases();
            }
            else
            {
                throw new NotSupportedException($"{nameof(ITransactionManager.RestoreIAPPurchases)} currently only works on iOS.");
            }
#else
            throw new NotSupportedException($"{nameof(ITransactionManager.RestoreIAPPurchases)} cannot be used because IAP support is not enabled.");
#endif
        }

        /// <summary>
        ///     Add IAP Product Id to list of successfully purchased IAP products.
        /// </summary>
        /// <param name="productId">Product Id to add.</param>
        /// <returns>
        ///     true if new IAP product was added, else false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if <see cref="ITransactionManager"/> has not been initialized.
        /// </exception>
        internal bool AddPurchasedIapProduct(string productId)
        {
            if (IsIapProductOwned(productId))
            {
                return false;
            }

            m_PurchasedIapProducts.Add(productId);

            SerializePurchasedIapProducts();

            return true;
        }

        /// <summary>
        ///     Remove IAP Product Id (if it exists).
        /// </summary>
        /// <param name="productId">Product Id to remove.</param>
        /// <returns>
        ///     true if IAP Product Id was found and removed, else false.
        /// </returns>
        internal bool RemovePurchasedIapProduct(string productId)
        {
            if (m_PurchasedIapProducts.Remove(productId))
            {
                SerializePurchasedIapProducts();

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determine if specified Product Id is owned by the player.
        /// </summary>
        /// <param name="productId">Product Id for which to search.</param>
        /// <returns>
        ///     true if specifeid Product Id is owned by the player, otherwise false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if <see cref="GameFoundationSdk.transactions"/> has not been initialized.
        /// </exception>
        internal bool IsIapProductOwned(string productId)
        {
            Tools.ThrowIfArgNullOrEmpty(productId, nameof(productId));

            return m_PurchasedIapProducts.Contains(productId);
        }

        /// <summary>
        ///     Remove all Product Ids.
        /// </summary>
        internal void RemoveAllPurchasedIapProducts()
        {
            if (m_PurchasedIapProducts.Count > 0)
            {
                m_PurchasedIapProducts.Clear();

                SerializePurchasedIapProducts();
            }
        }

        /// <summary>
        ///     Gets full list of all purchased iap products.
        /// </summary>
        internal List<string> GetPurchasedIapProducts() => new List<string>(m_PurchasedIapProducts);

        /// <summary>
        ///     Sets and serializes list of purchased iap products.
        ///     Used mainly to restore list after unit testing.
        /// </summary>
        /// <param name="purchasedIapProducts">List of iap products.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <see cref="purchasedIapProducts"/> is null.
        /// </exception>
        internal void SetPurchasedIapProducts(List<string> purchasedIapProducts)
        {
            m_PurchasedIapProducts = purchasedIapProducts;
            SerializePurchasedIapProducts();
        }

        /// <summary>
        ///     Helper method to write iap products list as json to app's persistent data path.
        /// </summary>
        void SerializePurchasedIapProducts()
        {
            // write iap products to json file in App persistent data path
            using (var sw = new StreamWriter(purchasedIapProductsFullPath, false, Encoding.Default))
            {
                var ispProducts = new IapProductsJsonWrapper(m_PurchasedIapProducts);
                var jsonString = JsonUtility.ToJson(ispProducts);
                sw.Write(jsonString);
            }

            // make a backup
            File.Copy(purchasedIapProductsFullPath, purchasedIapProductsBackupPath, true);
        }

        /// <summary>
        ///     Helper method to read iap products list as json from app's persistent data path.
        /// </summary>
        void DeserializePurchasedIapProducts()
        {
            // read from default path/filename
            string path = purchasedIapProductsFullPath;

            // if the main file doesn't exist, check for backup
            if (!File.Exists(path))
            {
                path = purchasedIapProductsBackupPath;
                if (!File.Exists(path))
                {
                    // if neither main nor backup file exist, clear out the iap products list and exit
                    m_PurchasedIapProducts = new List<string>();

                    return;
                }
            }

            // read list of IAP Product Id strings and store for later use.
            var fileInfo = new FileInfo(path);
            using (var sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                var iapProductsWrapper = JsonUtility.FromJson<IapProductsJsonWrapper>(sr.ReadToEnd());
                m_PurchasedIapProducts = iapProductsWrapper.iapProducts;

                // if json was empty or formatted incorrectly recreate empty list to avoid later null 
                // ref errors and permit future purchasing
                if (m_PurchasedIapProducts == null)
                {
                    m_PurchasedIapProducts = new List<string>();
                }
            }
        }

        /// <summary>
        ///     Set a validator instance for the purchasing adapter to use.
        /// </summary>
        /// <param name="validator">
        ///     The validator reference to set.
        /// </param>
        /// <exception cref="NotSupportedException">
        ///     Thrown if purchasing is not enabled in Game Foundation settings,
        ///     or if purchasing is not enabled in Unity Services.
        /// </exception>
        internal void SetIAPValidator(CrossPlatformValidator validator)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            m_PurchasingAdapter.validator = validator;
#else
            throw new NotSupportedException(
                $"{nameof(TransactionManagerImpl)}: Tried to set an IAP Validator, but IAP support is not enabled.");
#endif
        }

        /// <summary>
        ///     A list of <see cref="PurchaseEventArgs"/> that were not automatically
        ///     processed because the "Process Background Purchases" option was unchecked.
        ///     It is up to the developer to fully process these.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     Thrown if purchasing is not enabled in Game Foundation settings,
        ///     or if purchasing is not enabled in Unity Services.
        /// </exception>
        internal List<PurchaseEventArgs> unprocessedPurchases
        {
            get
            {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                return m_PurchasingAdapter?.unprocessedPurchases;
#else
                throw new NotSupportedException(
                    $"{nameof(TransactionManagerImpl)}: Tried to access the unprocessed purchase collection, but IAP support is not enabled.");
#endif
            }
            set
            {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                m_PurchasingAdapter.unprocessedPurchases = value;
#else
                throw new NotSupportedException(
                    $"{nameof(TransactionManagerImpl)}: Tried to access the unprocessed purchase collection, but IAP support is not enabled.");
#endif
            }
        }

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
        /// <exception cref="NotSupportedException">
        ///     Thrown if purchasing is not enabled in Game Foundation settings,
        ///     or if purchasing is not enabled in Unity Services.
        /// </exception>
        internal Deferred<TransactionResult> ProcessPurchaseEventArgs(PurchaseEventArgs purchaseEventArgs)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

            // send purchaseEventArgs to UnityPurchasingAdapter for immediate processing

            Promises.GetHandles<TransactionResult>(out var deferred, out var completer);

            GameFoundationSdk.updater.StartCoroutine(
                m_PurchasingAdapter.ValidateAndProcessSuccessfulPurchase(
                    purchaseEventArgs, completer));

            return deferred;
#else
            throw new NotSupportedException(
                $"{nameof(TransactionManagerImpl)}: Tried to process a PurchaseEventArgs, but IAP support is not enabled.");
#endif
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

        /// <summary>
        ///     An instance of the optional in-app purchasing adapter.
        /// </summary>
        UnityPurchasingAdapter m_PurchasingAdapter;

        /// <summary>
        ///     Indicates whether we are expecting a response from UnityPurchasingAdapter.
        ///     It is possible to get purchase responses without expecting them.
        ///     Expected and unexpected purchase responses need to be handled in slightly different ways.
        /// </summary>
        bool m_WaitingForPurchaseResponse;

        /// <summary>
        ///     Coroutine which processes an <see cref="IAPTransaction"/> and updates a <see cref="Completer{TResult}"/>.
        /// </summary>
        /// <param name="completer">
        ///     The <see cref="Completer{TResult}"/> to update as the <see cref="IAPTransaction"/> is processed.
        /// </param>
        /// <param name="transaction">
        ///     An optional <see cref="IAPTransaction"/> object to resolve.
        ///     If one is provided, then that means it is a foreground purchase (purchase currently in progress).
        ///     If none is provided, then this is likely a background purchase (restore purchases, delayed success, etc).
        /// </param>
        IEnumerator ProcessIAPTransactionCoroutine(
            Completer<TransactionResult> completer, IAPTransaction transaction = null)
        {
            var hasTransaction = transaction != null;

            completer.SetProgression(0, 4);

            if (hasTransaction)
            {
                transactionInitiated?.Invoke(transaction);
                transactionProgressed?.Invoke(transaction, 0, 4);
            }

            if (m_PurchasingAdapter == null)
            {
                var exception = new Exception(
                    $"{nameof(TransactionManagerImpl)}: Tried to process an in-app purchase transaction " +
                    $"{transaction?.displayName} with no platform purchase handler configured.");

                completer.Reject(exception);

                if (hasTransaction)
                {
                    transactionFailed?.Invoke(transaction, exception);
                }

                m_CurrentIap = default;

                yield break;
            }

            if (hasTransaction && string.IsNullOrEmpty(transaction.productId))
            {
                var exception = new Exception(
                    $"{nameof(TransactionManagerImpl)}: Transaction definition with key {transaction.key} doesn't have a product id.");

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                m_CurrentIap = default;

                yield break;
            }

            if (hasTransaction)
            {
                // if the transaction is null, it means this coroutine is being used to process a background purchase
                // that means the purchase was already successful in the past,
                // so we don't need to wait for a response from the IAP SDK (we won't get one)

                m_WaitingForPurchaseResponse = true;

                m_PurchasingAdapter.BeginPurchase(transaction.productId);
            }

            completer.SetProgression(1, 4);

            if (hasTransaction)
            {
                transactionProgressed?.Invoke(transaction, 1, 4);
            }

            // wait for the purchasing adapter

            while (m_WaitingForPurchaseResponse) yield return null;

            // purchasing manager has now responded with success or failure

            if (!string.IsNullOrEmpty(m_CurrentIap.failureMessage))
            {
                var exception = new Exception
                    ($"{nameof(TransactionManagerImpl)}: Purchasing manager failed to complete purchase: {m_CurrentIap.failureMessage}");

                completer.Reject(exception);

                if (hasTransaction)
                {
                    transactionFailed?.Invoke(transaction, exception);
                }

                m_CurrentIap = default;

                yield break;
            }

            // at this point, we assume the platform purchase was successful

            completer.SetProgression(2, 4);

            if (hasTransaction)
            {
                transactionProgressed?.Invoke(transaction, 2, 4);
            }

            // now send it all to the data layer

            var confirmation = m_PurchasingAdapter.GetCurrentPurchaseData();

            yield return SuccessfulPurchaseToDataLayer(confirmation, completer, transaction);
        }

        /// <summary>
        ///     Send a successful purchase to the data layer for processing there.
        /// </summary>
        /// <param name="confirmation">
        ///     The successful purchase data to send to the data layer for processing.
        /// </param>
        /// <param name="completer">
        ///     A completer for updating progress.
        /// </param>
        /// <param name="transaction">
        ///     If this is null, then the purchase will be treated as a "background purchase".
        ///     This can be the case when restoring multiple purchases, or finishing a
        ///     purchase that was interrupted in a previous session.
        /// </param>
        IEnumerator SuccessfulPurchaseToDataLayer(
            PurchaseConfirmation confirmation,
            Completer<TransactionResult> completer,
            IAPTransaction transaction = null)
        {
            Promises.GetHandles<TransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            if (transaction == null)
            {
                m_CurrentIap.transaction = GameFoundationSdk.catalog
                    .FindIAPTransactionByProductId(confirmation.productId);
                if (m_CurrentIap.transaction == null)
                {
                    completer.Reject(new Exception(
                        $"{nameof(TransactionManagerImpl)}: Could not find a transaction using product id " +
                        $"'{confirmation.productId}'."));
                    dalDeferred.Release();
                    m_CurrentIap.transaction = null;
                    yield break;
                }
            }

            if (m_PurchasingAdapter.isAppleIOS)
            {
                dataLayer.RedeemAppleIap(
                    key: m_CurrentIap.transaction.key,
                    receipt: confirmation.receiptParts[0],
                    completer: dalCompleter);
            }
            else if (m_PurchasingAdapter.isGooglePlay)
            {
                dataLayer.RedeemGoogleIap(
                    key: m_CurrentIap.transaction.key,
                    purchaseData: confirmation.receiptParts[0],
                    purchaseDataSignature: confirmation.receiptParts[1],
                    completer: dalCompleter);
            }
            else if (m_PurchasingAdapter.isFakeStore)
            {
                // TODO: fake a result based on the transaction asset values
                // TODO: something like s_DataLayer.RedeemTestIap() maybe ?
                // for now, just pretend we're trying with apple
                dataLayer.RedeemAppleIap(
                    key: m_CurrentIap.transaction.key,
                    receipt: "",
                    completer: dalCompleter);
            }
            else
            {
                completer.Reject(new Exception(
                    $"{nameof(TransactionManagerImpl)}: Game Foundation currently cannot redeem IAP for platforms other than Apple iOS or Google Play."));
            }

            if (!dalDeferred.isDone) yield return dalDeferred.Wait();

            completer.SetProgression(3, 4);

            if (transaction != null)
            {
                transactionProgressed?.Invoke(transaction, 3, 4);
            }

            // now handle the response from the DAL
            // even if the platform purchase succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                TransactionCosts costs = default; // an IAP transaction should never have virtual costs
                var payout = ApplyPayoutInternally(dalDeferred.result);
                var result = new TransactionResult(costs, payout);

                // tell the purchasing adapter that redemption worked and it
                // can stop asking TransactionManager to fulfill the purchase

                m_PurchasingAdapter.CompletePendingPurchase(confirmation.productId);

                // tell the caller that the purchase and redemption are successfully finished

                completer.Resolve(result);

                // check for product definition in purchased product and in transaction

                ProductDefinition productDefinition = null;

                var purchasedProduct = m_PurchasingAdapter.FindProduct(confirmation.productId);
                if (purchasedProduct?.definition != null)
                {
                    productDefinition = purchasedProduct.definition;
                }
                else if (transaction?.product?.definition != null)
                {
                    productDefinition = transaction.product.definition;
                }
                else
                {
                    // TODO: a way to add non-fatal warnings to a successful promise

                    string message = $"Processed a purchase for a product id ({confirmation.productId} that cannot " +
                        "be found in the currently loaded IAP catalog.";
                    k_GFLogger.LogError(message);
                }

                // if non-consumable, add the purchase to list of successfully purchased IAP products
                if (productDefinition?.type == ProductType.NonConsumable)
                {
                    AddPurchasedIapProduct(confirmation.productId);
                }

                if (transaction != null)
                {
                    transactionProgressed?.Invoke(transaction, 4, 4);
                    transactionSucceeded?.Invoke(transaction, result);
                }

                // TODO: should we invoke a different event for a background purchase?
            }
            else
            {
                completer.Reject(dalDeferred.error);

                if (transaction != null)
                {
                    transactionFailed?.Invoke(transaction, dalDeferred.error);
                }
            }

            m_CurrentIap = (null, false, null);
            dalDeferred.Release();
        }

        /// <summary>
        ///     The purchasing adapter should call this method any time a purchase succeeds.
        /// </summary>
        /// <param name="purchaseEventArgs">
        ///     The PurchaseEventArgs that the IAP SDK successfully processed.
        /// </param>
        /// <returns>
        ///     A Deferred which the caller can use to monitor the progress.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if you try to finalize a PurchaseEventArgs while another purchase is being finalized.
        /// </exception>
        internal Deferred<TransactionResult> FinalizeSuccessfulIAP(PurchaseEventArgs purchaseEventArgs)
        {
            purchaseSucceededInIAPSDK?.Invoke(purchaseEventArgs);

            if (m_CurrentIap.transaction != null)
            {
                if (m_CurrentIap.transaction.productId == purchaseEventArgs.purchasedProduct.definition.storeSpecificId)
                {
                    m_WaitingForPurchaseResponse = false;

                    // this is the transaction we have been currently waiting on
                    m_CurrentIap.isSuccessful = true;

                    // TODO: somehow give the caller something to wait for
                    // for now return a fake and already finished deferred
                    Promises.GetHandles<TransactionResult>(out var deferredTemp, out var completerTemp);
                    completerTemp.Resolve(default);
                    return deferredTemp; // temporary

                    // TODO: this is returning before current transaction is set, so the caller isn't waiting for anything
                }

                // a successful purchase is being processed
                // but this product id wasn't the one we were expecting

                throw new InvalidOperationException(
                    $"{nameof(TransactionManagerImpl)}: Tried to finalize a successful purchase " +
                    "while already in the process of finalizing a different successful purchase.");
            }

            // this is some other success we were not expecting at all
            // and we're not currently processing anything
            // so, we want to honor this transaction silently
            // (such as when Android restores purchases automatically after a reinstall)
            Promises.GetHandles<TransactionResult>(out var deferred, out var completer);

            // by calling this coroutine without a transaction parameter,
            // the purchase will be treated as a "background purchase"
            GameFoundationSdk.updater.StartCoroutine(ProcessIAPTransactionCoroutine(completer));

            return deferred;
        }

        /// <summary>
        ///     The purchasing adapter should call this method when a purchase fails.
        /// </summary>
        /// <param name="productId">
        ///     The IAP product ID for the purchase that failed.
        /// </param>
        /// <param name="message">
        ///     The reason for the failure.
        /// </param>
        internal void PlatformPurchaseFailure(string productId, string message)
        {
            // is this the transaction we're currently waiting on?
            if (m_CurrentIap.transaction != null
                && m_CurrentIap.transaction.productId == productId)
            {
                // by setting the error message, the current coroutine will discover there was a failure
                m_CurrentIap.failureMessage = message;

                // we were waiting for this particular response, but not anymore
                m_WaitingForPurchaseResponse = false;

                return;
            }

            // this is some other failure we were not expecting

            transactionFailed?.Invoke(
                m_CurrentIap.transaction,
                new Exception(
                    $"{nameof(TransactionManagerImpl)}: received an unexpected platform purchase failure " +
                    $"for product id '{productId}' with message: {message}"));
        }

        /// <summary>
        ///     Get the IAP product info from the initialized Unity Purchasing instance.
        /// </summary>
        /// <param name="transaction">
        ///     An <see cref="IAPTransaction"/> with a productId to search for in the catalog.
        /// </param>
        /// <returns>
        ///     A <see cref="Product"/> from the IAP Catalog.
        /// </returns>
        internal Product FindIAPProduct(IAPTransaction transaction)
        {
            return transaction == null ? null : FindIAPProduct(transaction.productId);
        }

        /// <summary>
        ///     Get the IAP product info from the initialized Unity Purchasing instance.
        /// </summary>
        /// <param name="productId">
        ///     The product ID to search for in the catalog.
        /// </param>
        /// <returns>
        ///     A <see cref="Product"/> from the IAP Catalog.
        /// </returns>
        internal Product FindIAPProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            return m_PurchasingAdapter.FindProduct(productId);
        }

#endif
    }
}