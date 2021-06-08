using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    public partial class InventoryItem
    {
        /// <summary>
        ///     Event triggered when a mutable property is updated.
        /// </summary>
        public event Action<PropertyChangedEventArgs> mutablePropertyChanged;

        /// <summary>
        ///     Get all mutable properties stored in this item.
        /// </summary>
        /// <returns>
        ///     Return a new dictionary containing all mutable properties stored in this item.
        ///     The returned dictionary is never null.
        /// </returns>
        public Dictionary<string, Property> GetMutableProperties()
        {
            AssertActive();

            var defaultProperties = definition.defaultMutableProperties;
            var properties = new Dictionary<string, Property>(defaultProperties.Count);
            foreach (var defaultEntry in defaultProperties)
            {
                var key = defaultEntry.Key;
                var value = GameFoundationSdk.dataLayer.GetMutablePropertyValue(id, key);
                properties.Add(key, value);
            }

            return properties;
        }

        /// <summary>
        ///     Get all mutable properties stored in this item.
        /// </summary>
        /// <param name="target">
        ///     The dictionary to fill with the mutable properties stored in this item.
        ///     It is cleared before being filled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="target"/> is null.
        /// </exception>
        public void GetMutableProperties(Dictionary<string, Property> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            AssertActive();

            target.Clear();

            var defaultProperties = definition.defaultMutableProperties;
            foreach (var defaultEntry in defaultProperties)
            {
                var key = defaultEntry.Key;
                var value = GameFoundationSdk.dataLayer.GetMutablePropertyValue(id, key);
                target.Add(key, value);
            }
        }

        /// <summary>
        ///     Check if this item has a mutable property for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the mutable property to look for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this item has a mutable property with the given <paramref name="key"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public bool HasMutableProperty(string key)
        {
            AssertActive();

            //No need to check the argument, InventoryItemDefinition.HasMutableProperty already does.
            return m_Definition.HasMutableProperty(key);
        }

        /// <summary>
        ///     Get the value of the mutable property with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <returns>
        ///     The value of the mutable property with the given <paramref name="key"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no mutable property with the given <paramref name="key"/> in this item.
        /// </exception>
        public Property GetMutableProperty(string key)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            return GameFoundationSdk.dataLayer.GetMutablePropertyValue(m_Id, key);
        }

        /// <summary>
        ///     Try to get the value of the mutable property with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <param name="property">
        ///     The value of the searched mutable property, if found.
        /// </param>
        /// <returns>
        ///     Returns true if a mutable property with the given <paramref name="key"/> exists on this item;
        ///     returns false otherwise.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public bool TryGetMutableProperty(string key, out Property property)
        {
            AssertActive();

            return GameFoundationSdk.dataLayer.TryGetMutablePropertyValue(m_Id, key, out property);
        }

        /// <summary>
        ///     Set the mutable property with the given <paramref name="key"/>
        ///     with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the mutable property.
        /// </param>
        /// <returns>
        ///     The new value of the mutable property.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no mutable property with the given <paramref name="key"/> in this item.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If the given <paramref name="value"/> type is different from the expected type.
        /// </exception>
        public Property SetMutableProperty(string key, Property value)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            if (!m_Definition.TryGetDefaultValueOfMutableProperty(key, out var storedProperty))
                throw new PropertyNotFoundException(m_Id, key);

            if (storedProperty.type != value.type)
                throw new PropertyInvalidCastException(key, storedProperty.type, value.type);

            SynchronizedSetMutableProperty(key, value);

            return value;
        }

        /// <summary>
        ///     Adjust the value of the mutable property with the given <paramref name="key"/>
        ///     by adding the given <paramref name="change"/> to its current value.
        ///     Work only with numeric properties.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <param name="change">
        ///     Change to apply to the current value of the mutable property.
        /// </param>
        /// <returns>
        ///     The new value of the mutable property.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key"/> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no mutable property with the given <paramref name="key"/> in this item.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the mutable property's type isn't a numeric type.
        /// </exception>
        public Property AdjustMutableProperty(string key, Property change)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            var storedProperty = GameFoundationSdk.dataLayer.GetMutablePropertyValue(m_Id, key);

            if (!storedProperty.type.IsNumber())
                throw new InvalidOperationException(
                    $"The property \"{key}\" can't be adjusted because it is a \"{storedProperty.type}\" type.");

            if (!change.type.IsNumber())
                throw new InvalidOperationException(
                    "The given property change isn't a numeric type.");

            storedProperty += change;

            SynchronizedSetMutableProperty(key, storedProperty);

            return storedProperty;
        }

        /// <summary>
        ///     Reset the mutable property with the given <paramref name="key"/> to its default value.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the mutable property to reset.
        /// </param>
        /// <returns>
        ///     Return the new mutable property's value after its reset.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this has been discarded.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="key"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no mutable property with the given <paramref name="key"/> in this item.
        /// </exception>
        public Property ResetMutableProperty(string key)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            if (!m_Definition.defaultMutableProperties.TryGetValue(key, out var defaultValue))
                throw new PropertyNotFoundException(m_Id, key);

            SynchronizedSetMutableProperty(key, defaultValue);

            return defaultValue;
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultValuesOfMutableProperties()"/>
        public Dictionary<string, Property> GetDefaultValuesOfMutableProperties()
        {
            AssertActive();

            return definition.GetDefaultValuesOfMutableProperties();
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultValuesOfMutableProperties(Dictionary{string, Property})"/>
        public void GetDefaultValuesOfMutableProperties(Dictionary<string, Property> target)
        {
            AssertActive();

            definition.GetDefaultValuesOfMutableProperties(target);
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultValueOfMutableProperty"/>
        public Property GetDefaultValueOfMutableProperty(string propertyKey)
        {
            AssertActive();

            return definition.GetDefaultValueOfMutableProperty(propertyKey);
        }

        /// <inheritdoc cref="InventoryItemDefinition.TryGetDefaultValueOfMutableProperty"/>
        public bool TryGetDefaultValueOfMutableProperty(string propertyKey, out Property property)
        {
            AssertActive();

            return definition.TryGetDefaultValueOfMutableProperty(propertyKey, out property);
        }

        /// <summary>
        ///     Clean properties related event and members.
        /// </summary>
        void CleanMutableProperties()
        {
            mutablePropertyChanged = null;
        }

        /// <summary>
        ///     Set the value of the mutable property with the given <paramref name="key"/>,
        ///     synchronize it with the Data Access Layer,
        ///     and raise the <see cref="mutablePropertyChanged"/> event.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the mutable property to get the value of.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the mutable property.
        /// </param>
        void SynchronizedSetMutableProperty(string key, Property value)
        {
            GameFoundationSdk.dataLayer.SetMutablePropertyValue(m_Id, key, value, Completer.None);

            // fire event for this inventory item that its property has changed
            var args = new PropertyChangedEventArgs(this, key, value);
            mutablePropertyChanged?.Invoke(args);

            // also fire inventory event that any item's property has changed
            var inventory = GameFoundationSdk.inventory as InventoryManagerImpl;
            inventory?.OnMutablePropertyChanged(args);
        }
    }
}
