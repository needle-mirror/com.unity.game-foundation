using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class VirtualTransactionConfig
    {
        /// <inheritdoc/>
        protected override VirtualTransaction CompileTransaction(Rejectable rejectable)
        {
            using (var costsCompilation = costs.Compile())
            {
                if (!costsCompilation.isFulfilled)
                {
                    //The sub-compiler doesn't have enough context to fill all required
                    //information so it is our responsibility to add the missing pieces.
                    if (costsCompilation.error is InvalidConfigDataException invalidConfigDataException)
                    {
                        invalidConfigDataException.invalidConfig = this;
                        invalidConfigDataException.fieldName = nameof(costs);
                    }

                    rejectable.Reject(costsCompilation.error);

                    return null;
                }

                var transaction = new VirtualTransaction
                {
                    costs = costsCompilation.result
                };

                return transaction;
            }
        }

        /// <inheritdoc/>
        protected override void LinkTransaction(
            VirtualTransaction runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable)
        {
            using (var costsLinking = costs.Link(runtimeObject.costs, compiledItems))
            {
                if (costsLinking.isFulfilled)
                {
                    return;
                }

                //The sub-compiler doesn't have enough context to fill all required
                //information so it is our responsibility to add the missing pieces.
                if (costsLinking.error is InvalidConfigDataException invalidConfigDataException)
                {
                    invalidConfigDataException.invalidConfig = this;
                    invalidConfigDataException.fieldName = nameof(costs);
                }

                rejectable.Reject(costsLinking.error);
            }
        }
    }
}
