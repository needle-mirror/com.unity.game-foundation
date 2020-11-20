using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Preset values and rules for an InventoryItem.
    ///     During runtime, it may be useful to refer back to the InventoryItemDefinition for
    ///     the presets and rules, but the values cannot be changed at runtime.
    ///     InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    public class InventoryItemDefinition : TradableDefinition
    {
        /// <summary>
        ///     Stores all mutable properties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> defaultMutableProperties { get; }

        internal InventoryItemDefinition()
        {
            defaultMutableProperties = new Dictionary<string, Property>();
        }

        internal InventoryItemDefinition(IDictionary<string, Property> properties)
        {
            defaultMutableProperties = new Dictionary<string, Property>(properties);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryItem"/> from this
        ///     <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of this item.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal virtual InventoryItem CreateInventoryItem(string id)
        {
            return new InventoryItem(this, id);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryItem"/> from this
        ///     <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="inventoryItemData">
        ///     Values to use to create new <see cref="InventoryItem"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal virtual InventoryItem CreateInventoryItem(InventoryItemData inventoryItemData)
        {
            return new InventoryItem(this, inventoryItemData.id);
        }

        /// <summary>
        ///     Get all default properties stored in this definition.
        /// </summary>
        /// <returns>
        ///     Return a new dictionary containing all properties stored in this definition.
        ///     The returned dictionary is never null.
        /// </returns>
        public Dictionary<string, Property> GetDefaultValuesOfMutableProperties()
        {
            var propertiesData = new Dictionary<string, Property>(defaultMutableProperties);

            return propertiesData;
        }

        /// <summary>
        ///     Get all default values of mutable properties stored in this definition.
        /// </summary>
        /// <param name="target">
        ///     The dictionary to fill with the mutable properties stored in this definition.
        ///     It is cleared before being filled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="target"/> is null.
        /// </exception>
        public void GetDefaultValuesOfMutableProperties(Dictionary<string, Property> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));

            target.Clear();

            foreach (var propertyEntry in defaultMutableProperties)
            {
                target.Add(propertyEntry.Key, propertyEntry.Value);
            }
        }

        /// <summary>
        ///     Check if this definition has a mutable property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The identifier of the mutable property to look for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this definition has a mutable property with the given <paramref name="propertyKey"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey"/> is null, empty, or whitespace.
        /// </exception>
        public bool HasMutableProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            return defaultMutableProperties.ContainsKey(propertyKey);
        }

        /// <summary>
        ///     Get the default value of the mutable property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <returns>
        ///     The default value of the mutable property with the given <paramref name="propertyKey"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     Thrown if there is no mutable property with the given <paramref name="propertyKey"/> in this item.
        /// </exception>
        public Property GetDefaultValueOfMutableProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            if (!defaultMutableProperties.TryGetValue(propertyKey, out var property))
                throw new PropertyNotFoundException(key, propertyKey);

            return property;
        }

        /// <summary>
        ///     Try to get the default value of the mutable property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <param name="property">
        ///     The default value of the searched mutable property, if found.
        /// </param>
        /// <returns>
        ///     Returns true if a mutable property with the given <paramref name="propertyKey"/> exists on this item;
        ///     returns false otherwise.
        /// </returns>
        public bool TryGetDefaultValueOfMutableProperty(string propertyKey, out Property property)
        {
            if (propertyKey == null)
            {
                property = default;

                return false;
            }

            return defaultMutableProperties.TryGetValue(propertyKey, out property);
        }

        /// <inheritdoc/>
        internal override bool VerifyCost(long cost, out Exception failReason)
        {
            var itemQuantity = GameFoundationSdk.inventory.GetTotalQuantity(this);
            var canPay = itemQuantity >= cost;

            failReason = canPay ? null : new NotEnoughItemOfDefinitionException(key, cost, itemQuantity);

            return canPay;
        }
    }
}
