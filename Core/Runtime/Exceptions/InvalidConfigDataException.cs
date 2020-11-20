using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    ///     Thrown when a <see cref="CatalogItemConfig"/> has invalid data
    ///     when it tries to create a <see cref="CatalogItem"/>.
    /// </summary>
    /// <remarks>
    ///     The error message for this exception can't be built like other <see cref="GameFoundationException"/>
    ///     because the required parameters can be set after the exception creation.
    /// </remarks>
    public class InvalidConfigDataException : GameFoundationException
    {
        /// <summary>
        ///     The config at the origin of the exception.
        /// </summary>
        public CatalogItemConfig invalidConfig { get; internal set; }

        /// <summary>
        ///     The name of the <see cref="invalidConfig"/>'s field at the origin of the exception.
        /// </summary>
        public string fieldName { get; internal set; }

        /// <summary>
        ///     A message format to describe the exception.
        ///     It can expect to have up to 2 arguments:
        ///     {0}: The catalog item type and key at the origin of the exception.
        ///     {1}: The field that caused the exception.
        /// </summary>
        readonly string m_MessageFormat;

        /// <summary>
        ///     Create a new <see cref="InvalidConfigDataException"/>.
        /// </summary>
        /// <param name="messageFormat">
        ///     A message format to describe the exception.
        ///     It can expect to have up to 2 arguments:
        ///     {0}: The catalog item type and key at the origin of the exception.
        ///     {1}: The field that caused the exception.
        /// </param>
        /// <param name="innerException">
        ///     The internal exception.
        /// </param>
        internal InvalidConfigDataException(string messageFormat, Exception innerException = null)
            : base("", innerException)
        {
            m_MessageFormat = messageFormat;
        }

        /// <inheritdoc/>
        public override string Message
            => string.Format(
                m_MessageFormat,
                $"{GetMessageFriendlyNameFor(invalidConfig)} \"{invalidConfig?.key ?? ""}\"",
                fieldName ?? "");

        /// <summary>
        ///     Get a readable name from the type of the given <paramref name="config"/>.
        /// </summary>
        static string GetMessageFriendlyNameFor(CatalogItemConfig config)
        {
            switch (config)
            {
                case CurrencyConfig _:
                    return nameof(Currency);
                case GameParameterConfig _:
                    return nameof(GameParameter);
                case IAPTransactionConfig _:
                    return nameof(IAPTransaction);
                case StackableInventoryItemDefinitionConfig _:
                    return nameof(StackableInventoryItemDefinition);
                case InventoryItemDefinitionConfig _:
                    return nameof(InventoryItemDefinition);
                case RewardConfig _:
                    return nameof(Reward);
                case StoreConfig _:
                    return nameof(Store);
                case VirtualTransactionConfig _:
                    return nameof(VirtualTransaction);

                default:
                    return nameof(CatalogItem);
            }
        }
    }
}
