using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    class GameFoundationDebug
    {
        const string k_PackageName = "Game Foundation";
        readonly string k_ClassName;
        static Dictionary<string, GameFoundationDebug> m_Loggers = new Dictionary<string, GameFoundationDebug>();

        /// <summary>
        ///     Method to use to get an instance of GameFoundationDebug with non-static classes. Stores the instance
        ///     so it can be returned without allocation in future calls.
        /// </summary>
        /// <typeparam name="TClass">
        ///     The class that is calling the GameFoundationDebug class. Will be part of any logging done by that class.
        /// </typeparam>
        /// <returns>
        ///     An existing or newly created instance of GameFoundationDebug, depending on whether it's the first time
        ///     the class has called this method.
        /// </returns>
        public static GameFoundationDebug Get<TClass>() => Get(typeof(TClass));

        /// <summary>
        ///     Method to use to get an instance of GameFoundationDebug with static classes. Stores the instance
        ///     so it can be returned without allocation in future calls.
        /// </summary>
        /// <param name="classType">
        ///     The class that is calling the GameFoundationDebug class. Will be part of any logging done by that class.
        /// </param>
        /// <returns>
        ///     An existing or newly created instance of GameFoundationDebug, depending on whether it's the first time
        ///     the class has called this method.
        /// </returns>
        public static GameFoundationDebug Get(Type classType)
        {
            var name = classType.Name;

            m_Loggers.TryGetValue(name, out var debugger);

            if (debugger == null)
            {
                debugger = new GameFoundationDebug(name);
                m_Loggers.Add(name, debugger);
            }

            return debugger;
        }

        /// <summary>
        ///     Private constructor used by the Get method to create new instances of GameFoundationDebug when necessary.
        /// </summary>
        /// <param name="className">
        ///     The string name of the class that is calling GameFoundationDebug. Will be part of any logging done by that class.
        /// </param>
        GameFoundationDebug(string className)
        {
            k_ClassName = className;
        }

        /// <summary>
        ///     Calls Debug.Log using the passed in message combined with specific Game Foundation formatting of the log message.
        /// </summary>
        /// <param name="message">
        ///     The message to log to the console.
        /// </param>
        public void Log(string message)
        {
            Debug.Log($"{k_PackageName} : {k_ClassName} - {message}");
        }

        /// <summary>
        ///     Calls Debug.LogWarning using the passed in message combined with specific Game Foundation formatting
        ///     of the log message.
        /// </summary>
        /// <param name="message">
        ///     The message to log to the console.
        /// </param>
        public void LogWarning(string message)
        {
            Debug.LogWarning($"{k_PackageName} : {k_ClassName} - {message}");
        }

        /// <summary>
        ///     Calls Debug.LogError using the passed in message combined with specific Game Foundation formatting of
        ///     the log message.
        /// </summary>
        /// <param name="message">
        ///     The message to log to the console.
        /// </param>
        public void LogError(string message)
        {
            Debug.LogError($"{k_PackageName} : {k_ClassName} - {message}");
        }

        /// <summary>
        ///     Calls Debug.LogError using the passed in message combined with specific Game Foundation formatting of
        ///     the log message. Also calls Debug.LogException with the passed in exception, and prints the stack trace
        ///     of the exception.
        /// </summary>
        /// <param name="message">
        ///     The message to log to the console.
        /// </param>
        /// <param name="exception">
        ///     The exception to log to the console.
        /// </param>
        public void LogException(string message, Exception exception)
        {
            Debug.LogError($"{k_PackageName} : {k_ClassName} - {message}");
            Debug.LogException(exception);
            Debug.LogError(exception.StackTrace);
        }
    }
}
