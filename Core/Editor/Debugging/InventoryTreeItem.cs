using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.Debugging
{
    sealed class InventoryItemDefinitionView : TreeViewItem
    {
        public InventoryItemDefinition definition { get; }

        public InventoryItemDefinitionView(int id, int depth, InventoryItemDefinition definition)
            : base(id, depth, $"{definition.displayName} ({definition.key})")
        {
            this.definition = definition;
        }

        public override string ToString()
        {
            return $"{nameof(InventoryItemDefinitionView)} definition:{definition.key}";
        }
    }

    sealed class InventoryItemView : TreeViewItem
    {
        public readonly InventoryItem inventoryItem;

        public InventoryItemView(int id, int depth, InventoryItem inventoryItem)
            : base(id, depth, $"#{inventoryItem.id}")
        {
            this.inventoryItem = inventoryItem;
        }

        public override string ToString()
        {
            return $"{nameof(InventoryItemView)} definition:{inventoryItem.definition.key} item:{inventoryItem.id}";
        }
    }

    sealed class PropertyView : TreeViewItem
    {
        public readonly InventoryItem inventoryItem;

        public readonly (string key, Property value) property;

        public PropertyView(
            int id, int depth, string displayName,
            InventoryItem inventoryItem,
            (string key, Property value) property)
            : base(id, depth, displayName)
        {
            this.inventoryItem = inventoryItem;
            this.property = property;
        }

        public override string ToString()
        {
            return $"{nameof(PropertyView)} item:{inventoryItem.definition.key}, property:{property.key}";
        }
    }

    sealed class CurrencyView : TreeViewItem
    {
        public readonly Currency currency;

        public CurrencyView(
            int id, int depth, string displayName,
            Currency currency)
            : base(id, depth, displayName)
        {
            this.currency = currency;
        }

        public override string ToString()
        {
            return $"{nameof(CurrencyView)} key:{currency.key}";
        }
    }

    sealed class RewardView : TreeViewItem
    {
        public readonly Reward reward;

        public RewardView(int id, int depth, string displayName, Reward reward) : base(id, depth, displayName)
        {
            this.reward = reward;
        }

        public override string ToString()
        {
            return $"{nameof(RewardView)} key:{reward.rewardDefinition.key}";
        }
    }
    
    sealed class RewardItemView : TreeViewItem
    {
        
        public readonly Reward reward;

        public readonly (string key, RewardItemState value) rewardItem;

        public RewardItemView(int id, int depth, string displayName, Reward reward, (string key, RewardItemState value) rewardItem)
            : base(id, depth, $"{displayName} (key: {rewardItem.key})")
        {
            this.reward = reward;
            this.rewardItem = rewardItem;
        }

        public override string ToString()
        {
            return $"{nameof(RewardItemView)} reward key: {reward.rewardDefinition.key}, reward item key:{rewardItem.key}";
        }
    }
}
