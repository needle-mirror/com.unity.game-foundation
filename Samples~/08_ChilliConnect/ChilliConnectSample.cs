using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if CHILLICONNECT_ENABLED
using ChilliConnect;
using UnityEngine.GameFoundation.ChilliConnect;
#else
using UnityEngine.GameFoundation.DefaultLayers;
#endif
using UnityEngine.Promise;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    public class ChilliConnectSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// The VirtualTransactions supply all the rules for their given transaction.
        /// Because they are Virtual (vs IAPTransactions) their input costs will be solely virtual items
        /// (either from the WalletManager or the InventoryManager).
        /// </summary>
        private VirtualTransaction m_AppleIngredientTransaction;
        private VirtualTransaction m_OrangeIngredientTransaction;
        private VirtualTransaction m_BananaIngredientTransaction;
        private VirtualTransaction m_BroccoliIngredientTransaction;
        private VirtualTransaction m_CarrotIngredientTransaction;
        private VirtualTransaction m_FruitSmoothieTransaction;
        private VirtualTransaction m_VeggieSmoothieTransaction;

        /// <summary>
        /// A list of exceptions used when checking whether the user has all the costs necessary for a transaction.
        /// </summary>
        private readonly List<Exception> m_CostExceptions = new List<Exception>();

        /// <summary>
        /// Reference to a list of InventoryItems in the GameFoundationSdk.inventory.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// The App token ChilliConnect provides in its dashboard
        /// </summary>
        public string chilliConnectAppToken;

        /// <summary>
        /// References to the specific buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button appleIngredientButton;
        public Button orangeIngredientButton;
        public Button bananaIngredientButton;
        public Button broccoliIngredientButton;
        public Button carrotIngredientButton;
        public Button blendFruitButton;
        public Button blendVeggieButton;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        IEnumerator Start()
        {
            IDataAccessLayer dataLayer = null;

#if CHILLICONNECT_ENABLED
            var sdk = new ChilliConnectSdk(chilliConnectAppToken, false);

            string
                userId = default,
                userSecret = default;

            // Creating the player

            var createPlayerDesc = new CreatePlayerRequestDesc();

            Debug.Log("Creating a player");

            var requestFailed = false;
            sdk.PlayerAccounts.CreatePlayer(
                createPlayerDesc,
                (request, response) =>
                {
                    userId = response.ChilliConnectId;
                    userSecret = response.ChilliConnectSecret;

                    Debug.Log($"Player {userId} created (secret: {userSecret})");
                },
                (request, error) =>
                {
                    Debug.LogError($"Unable to create a player: {error.ErrorDescription}");
                    requestFailed = true;
                });

            while (string.IsNullOrEmpty(userId))
            {
                if (requestFailed)
                {
                    yield break;
                }

                yield return null;
            }

            // Connecting the player

            Debug.Log($"Connecting the player {userId} (secret: {userSecret})");

            string catalogVersion = default;

            var logInDesc = new LogInUsingChilliConnectRequestDesc(userId, userSecret);

            sdk.PlayerAccounts.LogInUsingChilliConnect(
                logInDesc,
                (request, response) =>
                {
                    catalogVersion = response.CatalogVersion;
                    Debug.Log($"Player {userId} connected (secret: {userSecret})");
                },
                (request, error) =>
                {
                    Debug.LogError($"Unable to log in: {error.ErrorDescription}");
                    requestFailed = true;
                });

            while (string.IsNullOrEmpty(catalogVersion))
            {
                if (requestFailed)
                {
                    yield break;
                }

                yield return null;
            }

            dataLayer = new ChilliConnectCloudSync(sdk);
#else
            // The database has been properly setup.
            m_WrongDatabase = !SamplesHelper.VerifyDatabase();
            if (m_WrongDatabase)
            {
                wrongDatabasePanel.SetActive(true);
                yield break;
            }

            dataLayer = new MemoryDataLayer();
#endif

            // Initialize must always be called before working with any game foundation code.
            var deferred = GameFoundationSdk.Initialize(dataLayer);
            try
            {
                yield return deferred.Wait();
                if (!deferred.isFulfilled)
                    yield break;
            }
            finally
            {
                deferred.Release();
            }

            var catalog = GameFoundationSdk.catalog;

            // Grab references to the transactions.
            m_AppleIngredientTransaction = catalog.Find<VirtualTransaction>("appleIngredient");
            m_OrangeIngredientTransaction = catalog.Find<VirtualTransaction>("orangeIngredient");
            m_BananaIngredientTransaction = catalog.Find<VirtualTransaction>("bananaIngredient");
            m_BroccoliIngredientTransaction = catalog.Find<VirtualTransaction>("broccoliIngredient");
            m_CarrotIngredientTransaction = catalog.Find<VirtualTransaction>("carrotIngredient");
            m_FruitSmoothieTransaction = catalog.Find<VirtualTransaction>("fruitSmoothie");
            m_VeggieSmoothieTransaction = catalog.Find<VirtualTransaction>("veggieSmoothie");

            // Here we bind listeners that will set an inventoryChanged flag to callbacks on the Inventory Manager.
            // These callbacks will automatically be invoked anytime an inventory item is added or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            GameFoundationSdk.inventory.itemAdded += OnInventoryItemChanged;
            GameFoundationSdk.inventory.itemDeleted += OnInventoryItemChanged;

            // Here we bind listeners to callbacks on the Transaction Manager.
            // These callbacks will automatically be invoked during the processing of a transaction.
            GameFoundationSdk.transactions.transactionInitiated += OnTransactionInitiated;
            GameFoundationSdk.transactions.transactionProgressed += OnTransactionProgress;
            GameFoundationSdk.transactions.transactionSucceeded += OnTransactionSucceeded;
            GameFoundationSdk.transactions.transactionFailed += OnTransactionFailed;

            RefreshUI();
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUI();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// This will begin a transaction using the appleIngredient Transaction.
        /// This transaction uses coins from the wallet as the purchase price, and adds an apple Inventory Item to the Inventory Manager.
        /// </summary>
        public void PurchaseAppleIngredient()
        {
            Purchase(m_AppleIngredientTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the orangeIngredient Transaction.
        /// This transaction uses coins from the wallet as the purchase price, and adds an orange Inventory Item to the Inventory Manager.
        /// </summary>
        public void PurchaseOrangeIngredient()
        {
            Purchase(m_OrangeIngredientTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the bananaIngredient Transaction.
        /// This transaction uses coins from the wallet as the purchase price, and adds a banana Inventory Item to the Inventory Manager.
        /// </summary>
        public void PurchaseBananaIngredient()
        {
            Purchase(m_BananaIngredientTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the broccoliIngredient Transaction.
        /// This transaction uses coins from the wallet as the purchase price, and adds a broccoli Inventory Item to the Inventory Manager.
        /// </summary>
        public void PurchaseBroccoliIngredient()
        {
            Purchase(m_BroccoliIngredientTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the carrotIngredient Transaction.
        /// This transaction uses coins from the wallet as the purchase price, and adds a carrot Inventory Item to the Inventory Manager.
        /// </summary>
        public void PurchaseCarrotIngredient()
        {
            Purchase(m_CarrotIngredientTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the fruit smoothie Transaction.
        /// This transaction exchanges InventoryItems (fruits) for a different InventoryItem (smoothie).
        /// TransactionManager will find instances of each InventoryItem for the fruits that are consumed during the transaction.
        /// </summary>
        public void BlendFruitSmoothie()
        {
            Purchase(m_FruitSmoothieTransaction);
        }

        /// <summary>
        /// This will begin a transaction using the veggies smoothie Transaction.
        /// This transaction exchanges InventoryItems (veggies) for a different InventoryItem (smoothie).
        /// TransactionManager will find instances of each InventoryItem for the veggies that are consumed during the transaction.
        /// </summary>
        public void BlendVeggieSmoothie()
        {
            Purchase(m_VeggieSmoothieTransaction);
        }

        /// <summary>
        /// Calls <see cref="O:GameFoundationSdk.transactions.BeginTransaction"/> with the purchase detail of the Transaction Item displayed in the button.
        /// Is automatically attached to the onClick event of the PurchaseButton.
        /// </summary>
        private void Purchase(VirtualTransaction transaction)
        {
            if (transaction == null)
            {
                Debug.LogError($"Transaction shouldn't be null.");
                return;
            }

            StartCoroutine(ExecuteTransaction(transaction));
        }

        /// <summary>
        ///  Execute the transaction with Coroutine since transaction uses deferred objects. 
        /// </summary>
        /// <param name="transaction">The Transaction being purchased.</param>
        /// <returns></returns>
        private IEnumerator ExecuteTransaction(BaseTransaction transaction)
        {
            Debug.Log($"Now processing purchase: {transaction.displayName}");

            Deferred<TransactionResult> deferred = default;

            var counterparts = new List<string>();

            if (transaction is VirtualTransaction virtualTransaction)
            {
                virtualTransaction.AutoFillCostItemIds(counterparts);
            }

            deferred = GameFoundationSdk.transactions.BeginTransaction(transaction, counterparts);

            // wait for the transaction to be processed
            int currentStep = 0;

            while (!deferred.isDone)
            {
                // keep track of the current step and possibly show a progress UI
                if (deferred.currentStep != currentStep)
                {
                    currentStep = deferred.currentStep;

                    Debug.Log($"Transaction is now on step {currentStep} of {deferred.totalSteps}");
                }

                yield return null;
            }

            // now that the transaction has been processed, check for an error
            if (!deferred.isFulfilled)
            {
                Debug.LogError($"Transaction Id:  {transaction.key} - Error Message: {deferred.error}");

                deferred.Release();
                yield break;
            }

            // here we can assume success
            Debug.Log("The purchase was successful in both the platform store and the data layer!");

            foreach (var exchange in deferred.result.payout.products)
            {
                // the log will depend on the type of the received tradable.
                if (exchange is CurrencyExchange currencyExchange)
                    Debug.Log($"Player was awarded {currencyExchange.amount} of currency '{currencyExchange.currency.displayName}'");
                else if (exchange is InventoryItem item)
                    Debug.Log($"Player was awarded an item made from '{item.definition.displayName}'");
            }

            // all done
            deferred.Release();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            var coin = GameFoundationSdk.catalog.Find<Currency>("coin");

            m_DisplayText.Clear();
            m_DisplayText.Append("<b><i>Wallet:</i></b>");
            m_DisplayText.AppendLine();
            m_DisplayText.Append("Coins: " + GameFoundationSdk.wallet.Get(coin));
            m_DisplayText.AppendLine();
            m_DisplayText.AppendLine();
            m_DisplayText.Append("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();

            GameFoundationSdk.inventory.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                // All game items have an associated display name, this includes game items.
                string itemName = inventoryItem.definition.key;

                m_DisplayText.Append(itemName);
                m_DisplayText.AppendLine();
            }

#if CHILLICONNECT_ENABLED
            m_DisplayText.AppendLine();
            m_DisplayText.Append("<i>(using ChilliConnect cloud sync successfully)</i>");
#else
            m_DisplayText.AppendLine();
            m_DisplayText.Append("<b><i>*** Important: Using memory data layer, not ChilliConnect! ***</i></b>");
#endif

            mainText.text = m_DisplayText.ToString();

            RefreshBlendButtons();
        }

        /// <summary>
        /// Enables/Disables the blend buttons
        /// Each button should verify there are enough ingredients for what it needs.
        /// </summary>
        private void RefreshBlendButtons()
        {
            // We'll check to see whether we can afford each transaction using the VirtualTransaction's VerifyCost method.
            // It will return an exception for each cost item or currency that the user doesn't have enough of in their inventory or wallet.
            // If there are no exceptions, then they can afford the transaction and we can enable the purchase button.
            m_FruitSmoothieTransaction.VerifyCost(m_CostExceptions);
            blendFruitButton.interactable = m_CostExceptions.Count <= 0;

            m_VeggieSmoothieTransaction.VerifyCost(m_CostExceptions);
            blendVeggieButton.interactable = m_CostExceptions.Count <= 0;

            var coin = GameFoundationSdk.catalog.Find<Currency>("coin");

            var appleCost = m_AppleIngredientTransaction?.costs?.GetExchange(0);
            appleIngredientButton.interactable = GameFoundationSdk.wallet.Get(coin) > appleCost?.amount;

            var orangeCost = m_OrangeIngredientTransaction?.costs?.GetExchange(0);
            orangeIngredientButton.interactable = GameFoundationSdk.wallet.Get(coin) > orangeCost?.amount;

            var bananaCost = m_BananaIngredientTransaction?.costs?.GetExchange(0);
            bananaIngredientButton.interactable = GameFoundationSdk.wallet.Get(coin) > bananaCost?.amount;

            var broccoliCost = m_BroccoliIngredientTransaction?.costs?.GetExchange(0);
            broccoliIngredientButton.interactable = GameFoundationSdk.wallet.Get(coin) > broccoliCost?.amount;

            var carrotCost = m_CarrotIngredientTransaction?.costs?.GetExchange(0);
            carrotIngredientButton.interactable = GameFoundationSdk.wallet.Get(coin) > carrotCost?.amount;
        }

        /// <summary>
        /// Listener for changes in GameFoundationSdk.inventory. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_InventoryChanged = true;
        }

        /// <summary>
        /// Listens to OnTransactionInitiated callback and logs the name of the transaction initiated.
        /// </summary>
        /// <param name="transaction">The transaction initiated.</param>
        private void OnTransactionInitiated(BaseTransaction transaction)
        {
            Debug.Log("Transaction " + transaction.displayName + " initiated");
        }

        /// <summary>
        /// Listens to OnTransactionProgress callback and logs the name and step count of the transaction.
        /// Could be used to display a progress bar to waiting users.
        /// </summary>
        /// <param name="transaction">The transaction initiated.</param>
        /// <param name="currentStep">The current step of the transaction progress.</param>
        /// <param name="totalSteps">The total number of steps to complete the transaction.</param>
        private void OnTransactionProgress(BaseTransaction transaction, int currentStep, int totalSteps)
        {
            Debug.Log("Transaction " + transaction.displayName + " is on step " + currentStep + " out of " + totalSteps);
        }

        /// <summary>
        /// Listens to OnTransactionSucceeded callback and logs the name of the successful transaction.
        /// </summary>
        /// <param name="transaction">The transaction that succeeded</param>
        /// <param name="result">The result of the transaction</param>
        private void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            Debug.Log("Transaction " + transaction.displayName + " has succeeded.");
        }

        /// <summary>
        /// Listens to OnTransactionFailed callback and logs the name and exception of the failed transaction.
        /// </summary>
        /// <param name="transaction">The transaction that failed.</param>
        /// <param name="exception">The failure reason.</param>
        private void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            Debug.Log("Transaction " + transaction.displayName + " has failed. " + exception);
        }
    }
}
