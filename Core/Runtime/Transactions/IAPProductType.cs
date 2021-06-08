namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     This indicates the type of IAP product an IAPTransaction refers to.
    /// </summary>
    public enum IAPProductType
    {
        /// <summary>
        ///     Defaulting to this type can help diagnose catalog problems.
        /// </summary>
        Undetermined,

        /// <summary>
        ///     Consumables may be purchased more than once.
        ///     Purchase history for consumables is not typically retained by store systems.
        /// </summary>
        Consumable,

        /// <summary>
        ///     Non consumables cannot be re-purchased and are owned indefinitely.
        /// </summary>
        NonConsumable,

        /// <summary>
        ///     Subscriptions have a finite window of validity.
        ///     <note type="caution">
        ///         Game Foundation does not yet officially support subscription type products.
        ///     </note>
        /// </summary>
        Subscription,
    }
}
