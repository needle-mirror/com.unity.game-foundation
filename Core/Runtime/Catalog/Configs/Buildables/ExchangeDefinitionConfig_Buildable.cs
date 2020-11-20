using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class ExchangeDefinitionConfig : IBuildable<ExchangeDefinition>
    {
        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public Deferred<ExchangeDefinition> Compile()
        {
            Promises.GetHandles<ExchangeDefinition>(out var deferred, out var completer);

            if (string.IsNullOrWhiteSpace(tradableKey))
            {
                //Since we don't have enough context here, it is the caller's
                //responsibility to complete missing information for this exception.
                const string messageFormat = "The {0} has an invalid catalog item reference in {1}.";
                completer.Reject(new InvalidConfigDataException(messageFormat));

                return deferred;
            }

            if (amount < 0)
            {
                //Since we don't have enough context here, it is the caller's
                //responsibility to complete missing information for this exception.
                const string messageFormat = "The {0} can't have a negative amount for {1}.";
                completer.Reject(new InvalidConfigDataException(messageFormat));

                return deferred;
            }

            var exchange = new ExchangeDefinition
            {
                amount = amount
            };
            completer.Resolve(exchange);

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public Deferred Link(ExchangeDefinition runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);

            if (!compiledItems.TryGetValue(tradableKey, out var rawTradableDefinition))
            {
                //Since we don't have enough context here, it is the caller's
                //responsibility to complete missing information for this exception.
                var messageFormat = "No catalog item with the key \"" + tradableKey + "\" could be found for {1} in the {0}.";
                completer.Reject(new InvalidConfigDataException(messageFormat));

                return deferred;
            }

            if (!(rawTradableDefinition is TradableDefinition definition))
            {
                //Since we don't have enough context here, it is the caller's
                //responsibility to complete missing information for this exception.
                var messageFormat = $"The catalog item \"{tradableKey}\" is not a {nameof(TradableDefinition)}" +
                    " can't be used for {1} in the {0}.";
                completer.Reject(new InvalidConfigDataException(messageFormat));

                return deferred;
            }

            runtimeObject.tradableDefinition = definition;

            completer.Resolve();

            return deferred;
        }
    }
}
