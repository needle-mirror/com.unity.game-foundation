using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class RewardConfig
    {
        static readonly GameFoundationDebug k_Logger = GameFoundationDebug.Get(typeof(RewardConfig));

        /// <inheritdoc/>
        protected override RewardDefinition CompileItem(Rejectable rejectable)
        {
            var itemCount = rewardItemConfigs.Count;
            var rewardItems = new RewardItemDefinition[itemCount];

            for (var i = 0; i < itemCount; i++)
            {
                using (var rewardItemCompilation = rewardItemConfigs[i].Compile())
                {
                    if (rewardItemCompilation.isFulfilled)
                    {
                        rewardItems[i] = rewardItemCompilation.result;
                    }
                    else
                    {
                        //The sub-compiler doesn't have enough context to fill all required
                        //information so it is our responsibility to add the missing pieces.
                        if (rewardItemCompilation.error is InvalidConfigDataException invalidConfigDataException)
                        {
                            invalidConfigDataException.invalidConfig = this;
                            invalidConfigDataException.fieldName = $"{nameof(rewardItemConfigs)}[{i.ToString()}]";
                        }

                        rejectable.Reject(rewardItemCompilation.error);

                        return null;
                    }
                }
            }

            void AssertFieldIsPositive(ref int field, string fieldName)
            {
                if (field >= 0)
                {
                    return;
                }

                var message = $"The {nameof(RewardDefinition)} \"{key}\" has a negative {fieldName}. " +
                    "It has been set to 0 to avoid failure.";
                k_Logger.LogWarning(message);

                field = 0;
            }

            AssertFieldIsPositive(ref cooldownSeconds, nameof(cooldownSeconds));
            AssertFieldIsPositive(ref expirationSeconds, nameof(expirationSeconds));

            var runtimeItem = new RewardDefinition
            {
                cooldownSeconds = cooldownSeconds,
                expirationSeconds = expirationSeconds,
                resetIfExpired = resetIfExpired,
                m_Items = rewardItems
            };

            return runtimeItem;
        }

        /// <inheritdoc/>
        protected override void LinkItem(
            RewardDefinition runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable)
        {
            for (var i = 0; i < runtimeObject.m_Items.Length; i++)
            {
                var rewardItemDefinition = runtimeObject.m_Items[i];

                // make sure this reward item knows about its parent reward
                rewardItemDefinition.rewardDefinition = runtimeObject;

                //Run a linker to resolve reward item's payouts' references.
                using (var rewardItemLinking = rewardItemConfigs[i].Link(rewardItemDefinition, compiledItems))
                {
                    if (rewardItemLinking.isFulfilled)
                    {
                        continue;
                    }

                    //The sub-compiler doesn't have enough context to fill all required
                    //information so it is our responsibility to add the missing pieces.
                    if (rewardItemLinking.error is InvalidConfigDataException invalidConfigDataException)
                    {
                        invalidConfigDataException.invalidConfig = this;
                        invalidConfigDataException.fieldName = $"{nameof(rewardItemConfigs)}[{i.ToString()}]";
                    }

                    rejectable.Reject(rewardItemLinking.error);

                    return;
                }
            }
        }
    }
}
