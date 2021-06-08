using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     An enum for the different purchasing platforms.
    /// </summary>
    enum PurchasingPlatform
    {
        FakeStore,
        AppleIOS,
        GooglePlay
    };

    /// <summary>
    ///     An interface comprised of methods necessary for making and managing real money in-app purchases (IAP).
    /// </summary>
    interface IPurchasingAdapter
    {
        /// <summary>
        ///     Current purchasing platform being used.
        /// </summary>
        PurchasingPlatform currentPurchasingPlatform { get; }
        
        /// <summary>
        ///     True if this purchasing adapter instance has been successfully initialized, otherwise false.
        /// </summary>
        bool isInitialized { get; }

        /// <summary>
        ///     Does whatever is necessary to prepare the purchasing system for use.
        /// </summary>
        /// <param name="OnPurchasingAdapterSuccess">
        ///     Invoked when the interfaced purchasing system is initialized successfully.
        /// </param>
        /// <param name="OnPurchasingAdapterFailure">
        ///     Invoked when a fatal problem initializing the interfaced purchasing system is detected.
        /// </param>
        void Initialize(Action OnPurchasingAdapterSuccess, Action<Exception> OnPurchasingAdapterFailure);

        /// <summary>
        ///     Does anything needed to shut down the interfaced purchasing system.
        /// </summary>
        void Uninitialize();

        /// <summary>
        ///     Starts the asynchronous process of making a purchase using the interfaced purchasing system.
        /// </summary>
        /// <param name="productId">
        ///     The product identifier that the interfaced purchasing system should recognize and use for this purchase.
        /// </param>
        void BeginPurchase(string productId);

        /// <summary>
        ///     After a purchasing system successfully completes a purchase, it will normally repeatedly send a
        ///     message to Game Foundation to please fulfill the purchase, until Game Foundation confirms that it was
        ///     fulfilled. This method tells the purchasing system that Game Foundation has successfully fulfilled
        ///     the purchase, so it can stop sending the "please fulfill" message.
        /// </summary>
        /// <param name="productId">
        ///     The product identifier that the interfaced purchasing system should recognize
        ///     and use to finalize a purchase in progress.
        /// </param>
        void CompletePendingPurchase(string productId);

        /// <summary>
        ///     Get info about an active purchase currently in progress.
        ///     This will not include any purchases being processed in the background, such as restored purchases.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="PurchaseConfirmation"/>.
        /// </returns>
        PurchaseConfirmation GetCurrentPurchaseData();

        /// <summary>
        ///     Get a name and price of a product translated for the user's locale (if available).
        /// </summary>
        /// <param name="productId">
        ///     The product identifier that the interfaced purchasing system should recognize and get localized info for.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="LocalizedProductMetadata"/>.
        /// </returns>
        LocalizedProductMetadata GetLocalizedProductInfo(string productId);

        /// <summary>
        ///     Begins the asynchronous process to restore any non-consumable purchases the user has
        ///     made on the interfaced purchasing platform.
        ///     <remarks>
        ///         Note: This is currently only relevant when the platform is iOS.
        ///     </remarks>
        /// </summary>
        void RestorePurchases();
    }
}
