using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public abstract partial class BaseTransactionConfig<TTransaction>
        where TTransaction : BaseTransaction
    {
        /// <inheritdoc/>
        protected override TTransaction CompileItem(Rejectable rejectable)
        {
            var transaction = CompileTransaction(rejectable);
            if (transaction is null)
            {
                return null;
            }

            using (var payoutCompilation = payout.Compile())
            {
                if (payoutCompilation.isFulfilled)
                {
                    transaction.payout = payoutCompilation.result;
                }
                else
                {
                    //The sub-compiler doesn't have enough context to fill all required
                    //information so it is our responsibility to add the missing pieces.
                    if (payoutCompilation.error is InvalidConfigDataException invalidConfigDataException)
                    {
                        invalidConfigDataException.invalidConfig = this;
                        invalidConfigDataException.fieldName = nameof(payout);
                    }

                    rejectable.Reject(payoutCompilation.error);

                    return null;
                }
            }

            return transaction;
        }

        /// <inheritdoc/>
        protected override void LinkItem(
            TTransaction runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable)
        {
            LinkTransaction(runtimeObject, compiledItems, rejectable);

            //Assert previous link operation didn't fail.
            if (!rejectable.isActive)
            {
                return;
            }

            using (var payoutLinking = payout.Link(runtimeObject.payout, compiledItems))
            {
                if (payoutLinking.isFulfilled)
                {
                    return;
                }

                //The sub-compiler doesn't have enough context to fill all required
                //information so it is our responsibility to add the missing pieces.
                if (payoutLinking.error is InvalidConfigDataException invalidConfigDataException)
                {
                    invalidConfigDataException.invalidConfig = this;
                    invalidConfigDataException.fieldName = nameof(payout);
                }

                rejectable.Reject(payoutLinking.error);
            }
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        protected abstract TTransaction CompileTransaction(Rejectable rejectable);

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        protected virtual void LinkTransaction(
            TTransaction runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable) { }
    }
}
