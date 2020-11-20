using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class RewardItemConfig : IBuildable<RewardItemDefinition>
    {
        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public Deferred<RewardItemDefinition> Compile()
        {
            Promises.GetHandles<RewardItemDefinition>(out var deferred, out var completer);

            using (var payoutCompilation = payout.Compile())
            {
                if (!payoutCompilation.isFulfilled)
                {
                    completer.Reject(payoutCompilation.error);

                    return deferred;
                }

                var rewardItemDefinition = new RewardItemDefinition
                {
                    key = key,
                    payout = payoutCompilation.result
                };

                completer.Resolve(rewardItemDefinition);
            }

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public Deferred Link(RewardItemDefinition runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);

            using (var payoutLinking = payout.Link(runtimeObject.payout, compiledItems))
            {
                if (payoutLinking.isFulfilled)
                {
                    completer.Resolve();
                }
                else
                {
                    completer.Reject(payoutLinking.error);
                }
            }

            return deferred;
        }
    }
}
