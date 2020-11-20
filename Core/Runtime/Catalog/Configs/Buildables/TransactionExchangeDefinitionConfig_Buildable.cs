using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class TransactionExchangeDefinitionConfig : IBuildable<TransactionExchangeDefinition>
    {
        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public Deferred<TransactionExchangeDefinition> Compile()
        {
            Promises.GetHandles<TransactionExchangeDefinition>(out var deferred, out var completer);

            var exchangeCount = exchangeConfigs.Count;
            var exchanges = new ExchangeDefinition[exchangeCount];

            for (var i = 0; i < exchangeCount; i++)
            {
                var exchangeConfig = exchangeConfigs[i];
                if (exchangeConfig is null)
                {
                    //Since we don't have enough context here, it is the caller's
                    //responsibility to complete missing information for this exception.
                    completer.Reject(new InvalidConfigDataException("The {0} has an invalid entry in {1}."));

                    return deferred;
                }

                using (var exchangeCompilation = exchangeConfig.Compile())
                {
                    if (!exchangeCompilation.isFulfilled)
                    {
                        completer.Reject(exchangeCompilation.error);

                        return deferred;
                    }

                    exchanges[i] = exchangeCompilation.result;
                }
            }

            var transactionExchange = new TransactionExchangeDefinition
            {
                m_Exchanges = exchanges
            };
            completer.Resolve(transactionExchange);

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public Deferred Link(TransactionExchangeDefinition runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);

            for (var i = 0; i < exchangeConfigs.Count; i++)
            {
                using (var exchangeLinking = exchangeConfigs[i].Link(runtimeObject.m_Exchanges[i], compiledItems))
                {
                    if (exchangeLinking.isFulfilled)
                    {
                        continue;
                    }

                    completer.Reject(exchangeLinking.error);

                    return deferred;
                }
            }

            completer.Resolve();

            return deferred;
        }
    }
}
