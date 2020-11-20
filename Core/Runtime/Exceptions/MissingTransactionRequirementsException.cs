using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Exception occuring when requesting a transaction without the requirements.
    /// </summary>
    public class MissingTransactionRequirementsException : GameFoundationException
    {
        /// <inheritdoc cref="MissingTransactionRequirementsException"/>
        /// <param name="transactionKey">
        ///     The key of the transaction requirements are missing for.
        /// </param>
        /// <param name="requirements">
        ///     The collection of requirements that couldn't be met.
        /// </param>
        public MissingTransactionRequirementsException(string transactionKey, Dictionary<string, long> requirements)
            : base(BuildMessage(transactionKey, requirements)) { }

        /// <inheritdoc cref="MissingTransactionRequirementsException(string, Dictionary{string,long})"/>
        /// <param name="innerException">
        ///     The internal exception.
        /// </param>
        public MissingTransactionRequirementsException(string transactionKey, Dictionary<string, long> requirements, Exception innerException)
            : base(BuildMessage(transactionKey, requirements), innerException) { }

        /// <summary>
        ///     Get the error message for the transaction requirements are missing for.
        /// </summary>
        /// <param name="transactionKey">
        ///     The key of the transaction requirements are missing for.
        /// </param>
        /// <param name="requirements">
        ///     The collection of requirements that couldn't be met.
        /// </param>
        static string BuildMessage(string transactionKey, Dictionary<string, long> requirements)
        {
            k_MessageBuilder.Clear()
                .AppendLine($"The following requirements are missing for the transaction \"{transactionKey}\":");

            foreach (var requirement in requirements)
            {
                k_MessageBuilder.AppendLine($"\t- Requirement {requirement.Key}: {requirement.Value.ToString()}");
            }

            return k_MessageBuilder.ToString();
        }
    }
}
