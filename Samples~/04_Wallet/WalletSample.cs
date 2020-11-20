using System;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the wallet and store sample.
    /// </summary>
    public class WalletSample : MonoBehaviour
    {
        /// <summary>
        /// Initial quantity of coins in wallet.
        /// </summary>
        public const int k_InitialQuantity = 50;

        /// <summary>
        /// Quantity of coins to add when 'find' button is pressed.
        /// </summary>
        public const int k_FindQuantity = 50;

        /// <summary>
        /// Quantity of coins to remove when 'drop' button is pressed.
        /// </summary>
        public const int k_DropQuantity = 10;

        /// <summary>
        /// Flag for whether the Wallet has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_WalletChanged;

        /// <summary>
        /// We will want to hold onto reference to currency for easy use later.
        /// </summary>
        private Currency m_CoinDefinition;

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Reference to the find coin button to enable/disable when app is enabled/disabled.
        /// </summary>
        public Button findCoinsButton;

        /// <summary>
        /// Reference to the drop coin button to enable/disable as needed or when the action is not possible.
        /// </summary>
        public Button dropCoinsButton;

        /// <summary>
        /// Flag for wallet changed callback events to ensure they are added exactly once when
        /// Game Foundation finishes initialization or when script is enabled.
        /// </summary>
        private bool m_SubscribedFlag = false;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private void Start()
        {
            // Game Foundation Initialization is being managed by GameFoundationInit Game Object
            if (!GameFoundationSdk.IsInitialized)
            {
                // Disable all buttons while initializing
                findCoinsButton.interactable = false;
                dropCoinsButton.interactable = false;
            }
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we enable buttons, setup callbacks, update GUI, etc.
        /// </summary>
        public void OnGameFoundationInitialized()
        {
            // We'll initialize our WalletManager's coin balance with correct quantity.
            // This will set the correct balance no matter what it's current balance is.
            m_CoinDefinition = GameFoundationSdk.catalog.Find<Currency>("coin");
            GameFoundationSdk.wallet.Set(m_CoinDefinition, k_InitialQuantity);

            // Enable the static 'add' button.
            // Note: the dynamic 'remove' button will be enabled in RefreshUI.
            findCoinsButton.interactable = true;

            // Show list of currencies and update the button interactability.
            RefreshUI();

            // Ensure that the wallet changed callback is connected
            SubscribeToGameFoundationEvents();
        }

        /// <summary>
        /// Called when this object becomes enabled to ensure our callbacks are active, if Game Foundation
        /// has already completed initialization (otherwise they will be enabled at end of initialization).
        /// </summary>
        private void OnEnable()
        {
            SubscribeToGameFoundationEvents();
        }

        /// <summary>
        /// Disable WalletManager callbacks if Game Foundation has been initialized and callbacks were added.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeFromGameFoundationEvents();
        }

        /// <summary>
        /// Bind a listener that will set a walletChanged flag to callbacks on the Wallet Manager.
        /// These callbacks will automatically be invoked anytime a currency balance is changed.
        /// This prevents us from having to manually invoke RefreshUI every time we perform one of 
        /// these actions.
        /// </summary>
        private void SubscribeToGameFoundationEvents()
        {
            // If wallet has not yet been initialized the ignore request.
            if (GameFoundationSdk.wallet is null)
            {
                return;
            }

            // If app has been disabled then ignore the request
            if (!enabled)
            {
                return;
            }

            // If balanceChanged callback has not been added then add it now and remember.
            // Note: this ignores repeated requests to add callback if already properly set up.
            if (!m_SubscribedFlag)
            {
                GameFoundationSdk.wallet.balanceChanged += OnCoinBalanceChanged;
                m_SubscribedFlag = true;
            }
        }

        /// <summary>
        /// Disable WalletManager callbacks.
        /// </summary>
        private void UnsubscribeFromGameFoundationEvents()
        {
            // If wallet has not yet been initialized the ignore request.
            if (GameFoundationSdk.wallet is null)
            {
                return;
            }

            // If callbacks have been added then remove them.
            // Note: this will ignore repeated requests to remove callbacks when they have not been added.
            if (m_SubscribedFlag)
            {
                GameFoundationSdk.wallet.balanceChanged -= OnCoinBalanceChanged;
                m_SubscribedFlag = false;
            }
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when the balance of a currency has changed in the WalletManager
            if (m_WalletChanged)
            {
                RefreshUI();
                m_WalletChanged = false;
            }
        }

        /// <summary>
        /// This method adds coins to the wallet.
        /// Note: Because changes to Wallet trigger callback events, we do NOT need to add additional 
        ///       processing to detect the change nor update UI here.
        /// </summary>
        public void FindCoins()
        {
            GameFoundationSdk.wallet.Add(m_CoinDefinition, k_FindQuantity);
        }

        /// <summary>
        /// This method deducts coins from the wallet.
        /// Note: Because changes to Wallet trigger callback events, we do NOT need to add additional 
        ///       processing to detect the change nor update UI here.
        /// </summary>
        public void DropCoins()
        {
            GameFoundationSdk.wallet.Remove(m_CoinDefinition, k_DropQuantity);
        }

        /// <summary>
        /// This will fill out the main text box with information about the wallet.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.AppendLine("<b><i>Wallet:</i></b>");
            
            var coinBalance = GameFoundationSdk.wallet.Get(m_CoinDefinition);
            m_DisplayText.AppendLine($"Currency - {m_CoinDefinition.displayName}: {coinBalance.ToString()}");

            mainText.text = m_DisplayText.ToString();

            RefreshLoseCoinsButton();
        }

        /// <summary>
        /// Enables/Disables the add/lose coins buttons.
        /// The addButton will always be interactable,
        /// but we can only lose coins if we have enough to lose.
        /// </summary>
        private void RefreshLoseCoinsButton()
        {
            var coinBalance = GameFoundationSdk.wallet.Get(m_CoinDefinition);
            dropCoinsButton.interactable = coinBalance >= k_DropQuantity;
        }

        /// <summary>
        /// This will be called every time a currency balance is changed.
        /// </summary>
        /// <param name="quantifiable">
        /// Quantifiable Game Foundation object that changed quantity (always Currency in this sample).
        /// </param>
        private void OnCoinBalanceChanged(IQuantifiable quantifiable, long _)
        {
            if (quantifiable is Currency currency)
            {
                if (currency.key == m_CoinDefinition.key)
                {
                    m_WalletChanged = true;
                }
            }
        }

        /// <summary>
        /// If GameFoundation throws exception, log the error to console.
        /// </summary>
        /// <param name="exception">
        /// Exception thrown by GameFoundation.
        /// </param>
        public void OnGameFoundationException(Exception exception)
        {
            Debug.LogError($"GameFoundation exception: {exception}");
        }
    }
}
