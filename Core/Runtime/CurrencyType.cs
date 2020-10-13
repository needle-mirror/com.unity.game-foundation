namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     This better enables identifying and tracking different types of currency in your game.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>
        ///     Also called "regular currency" or "free currency", is a resource designed to be adequately
        ///     accessible through normal gameplay, without having to make micro-transactions.
        /// </summary>
        Soft = 0,

        /// <summary>
        ///     Also called "premium currency", is a resource that is exclusively, or near exclusively,
        ///     acquired by paying for it (real-money transactions).  Premium currencies are much harder to
        ///     acquire without making purchases, and thus are considered to be premium game content.
        /// </summary>
        Hard = 1,
    }
}
