#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Exception occuring when <see cref="UnityPurchasingAdapter"/> fails to initialize.
    /// </summary>
    public class PurchasingAdapterInitializationException : GameFoundationException
    {
        /// <inheritdoc cref="PurchasingAdapterInitializationException"/>
        /// <param name="reason">
        ///     The reason of the initialization failure of <see cref="UnityPurchasingAdapter"/>.
        /// </param>
        public PurchasingAdapterInitializationException(InitializationFailureReason reason)
            : base(BuildMessage(reason)) { }

        /// <summary>
        ///     Get the error message for the given <paramref name="reason"/>.
        /// </summary>
        /// <param name="reason">
        ///     The reason of the initialization failure of <see cref="UnityPurchasingAdapter"/>.
        /// </param>
        static string BuildMessage(InitializationFailureReason reason)
        {
            k_MessageBuilder.Clear()
                .Append("Unity Purchasing failed to initialize for the following reason: ");

            switch (reason)
            {
                case InitializationFailureReason.PurchasingUnavailable:
                    k_MessageBuilder.Append("Purchasing is not enabled on this platform.");
                    break;

                case InitializationFailureReason.NoProductsAvailable:
                    k_MessageBuilder.Append("No products are available for purchase.");
                    break;

                case InitializationFailureReason.AppNotKnown:
                    k_MessageBuilder.Append(
                        "Unknown app. Make sure your app was uploaded to the respective platform store.");
                    break;

                default:
                    k_MessageBuilder.Append("An unrecognized problem occurred!");
                    break;
            }

            return k_MessageBuilder.ToString();
        }
    }
}
#endif
