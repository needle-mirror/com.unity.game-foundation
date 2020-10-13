using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the Virtual transaction info, and outcome.
    /// </summary>
    public sealed class VirtualTransaction : BaseTransaction
    {
        /// <summary>
        ///     The description of the cost of the transaction.
        ///     That is what the player pays to get the <see cref="BaseTransaction.payout"/>.
        /// </summary>
        public TransactionExchangeDefinition costs { get; internal set; }

        /// <summary>
        ///     Test whether the costs can be met by the currencies in the wallet and/or the items in the inventory.
        /// </summary>
        /// <param name="exceptions">
        ///     A collection to populate with all exceptions generated.
        /// </param>
        public void VerifyCost(ICollection<Exception> exceptions)
        {
            exceptions?.Clear();

            foreach (var exchange in costs.m_Exchanges)
            {
                if (!exchange.tradableDefinition.VerifyCost(exchange.amount, out var failReason))
                {
                    exceptions?.Add(failReason);
                }
            }
        }

        /// <summary>
        ///     Test whether the payouts are valid.
        /// </summary>
        /// <param name="exceptions">
        ///     A collection to populate with all exceptions generated.
        /// </param>
        public void VerifyPayout(ICollection<Exception> exceptions)
        {
            // prevent a payout from overflowing a currency slot
            // TODO: option of clamping instead of throwing
            foreach (var exchange in payout.m_Exchanges)
            {
                if (exchange.tradableDefinition is Currency payoutCurrency)
                {
                    var currentBalance = GameFoundationSdk.wallet.Get(payoutCurrency);

                    if (payoutCurrency.maximumBalance > 0
                        && currentBalance + exchange.amount > payoutCurrency.maximumBalance)
                    {
                        exceptions?.Add(new OverflowException(
                            $"{nameof(VirtualTransaction)}: " +
                            $"payout amount {exchange.amount} for currency {payoutCurrency.key} " +
                            $"added to current balance of {currentBalance} " +
                            $"would exceed maximum of {payoutCurrency.maximumBalance}."));
                    }
                }
            }
        }

        /// <summary>
        ///     Gets a list of the first InventoryItem ids from the inventory that satisfy the cost of a transaction.
        /// </summary>
        /// <param name="costItemIds">
        ///     An existing collection of strings to populate (must be non-null and must be empty).
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if the collection passed in is not empty.
        /// </exception>
        public void AutoFillCostItemIds(ICollection<string> costItemIds)
        {
            if (costItemIds == null || costItemIds.Count > 0)
            {
                throw new ArgumentException("Cannot pass a null or non-empty collection into FindFirstValidItemsForCost.");
            }

            foreach (var exchange in costs.m_Exchanges)
            {
                if (!(exchange.tradableDefinition is InventoryItemDefinition definition))
                    continue;

                using (Tools.Pools.inventoryItemList.Get(out var items))
                {
                    var itemCount = GameFoundationSdk.inventory.FindItems(definition, items);

                    // copy enough items to ensure fulfillment of the cost requirement
                    // note: because items may be stacks (which we've already validated quantity for), 
                    //  ...  we can just copy what we have to avoid copying past end of list
                    var copyCount = itemCount <= exchange.amount ? itemCount : exchange.amount;
                    for (var i = 0; i < copyCount; i++)
                    {
                        costItemIds.Add(items[i].id);
                    }
                }
            }
        }
    }
}
