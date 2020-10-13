namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     The unit of time.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>
        ///     Represents a second.
        /// </summary>
        Seconds = 0,

        /// <summary>
        ///     Represents a minute.
        /// </summary>
        Minutes = 1,

        /// <summary>
        ///     Represents an hour.
        /// </summary>
        Hours = 2,

        /// <summary>
        ///     Represents a day.
        /// </summary>
        Days = 3
    }

    /// <summary>
    ///     Utility functions for <see cref="TimeUnit"/>.
    /// </summary>
    public static class TimeUnitUtility
    {
        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_Logger = GameFoundationDebug.Get<TimeUnit>();

        /// <summary>
        ///     Convert the given <paramref name="value"/> from <paramref name="this"/> unit into seconds.
        /// </summary>
        /// <param name="this">
        ///     The unit of the entry value.
        /// </param>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <returns>
        ///     Return a value in seconds.
        /// </returns>
        public static int ConvertToSeconds(this TimeUnit @this, int value)
        {
            switch (@this)
            {
                case TimeUnit.Seconds:
                    return value;
                case TimeUnit.Minutes:
                    return value * 60;
                case TimeUnit.Hours:
                    return value * 60 * 60;
                case TimeUnit.Days:
                    return value * 60 * 60 * 24;
                default:
                    k_Logger.LogWarning($"{nameof(TimeUnit)} '{@this}' from which to convert " +
                        "seconds is not recognized.");
                    return value;
            }
        }

        /// <summary>
        ///     Convert the given <paramref name="value"/> from seconds into <paramref name="this"/> unit.
        /// </summary>
        /// <param name="this">
        ///     The unit of the result.
        /// </param>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <returns>
        ///     Return a value in <paramref name="@this"/> unit.
        /// </returns>
        public static int ConvertFromSeconds(this TimeUnit @this, int value)
        {
            switch (@this)
            {
                case TimeUnit.Seconds:
                    return value;
                case TimeUnit.Minutes:
                    return value / 60;
                case TimeUnit.Hours:
                    return value / 60 / 60;
                case TimeUnit.Days:
                    return value / 60 / 60 / 24;
                default:
                    k_Logger.LogWarning($"{nameof(TimeUnit)} '{@this}' to convert to from seconds " +
                        "is not recognized.");
                    return value;
            }
        }
    }
}
