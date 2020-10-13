using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    public abstract partial class CatalogItem
    {
        /// <summary>
        ///     Stores a collection of static properties for this catalog item.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> staticProperties { get; }
            = new Dictionary<string, Property>();

        /// <summary>
        ///     Get all static properties stored in this item.
        /// </summary>
        /// <returns>
        ///     Return a new dictionary containing all properties stored in this item.
        ///     The returned dictionary is never null.
        /// </returns>
        public Dictionary<string, Property> GetStaticProperties()
        {
            var propertiesData = new Dictionary<string, Property>(staticProperties);

            return propertiesData;
        }

        /// <summary>
        ///     Get all static properties stored in this item.
        /// </summary>
        /// <param name="target">
        ///     The dictionary to fill with the properties stored in this item.
        ///     It is cleared before being filled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="target"/> is null.
        /// </exception>
        public void GetStaticProperties(Dictionary<string, Property> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));

            target.Clear();

            foreach (var propertyEntry in staticProperties)
            {
                target.Add(propertyEntry.Key, propertyEntry.Value);
            }
        }

        /// <summary>
        ///     Check if this item has a static property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     The identifier of the static property to look for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this item has a static property with the given <paramref name="propertyKey"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey"/> is null, empty, or whitespace.
        /// </exception>
        public bool HasStaticProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            return staticProperties.ContainsKey(propertyKey);
        }

        /// <summary>
        ///     Get the value of the static property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the static property to get the value of.
        /// </param>
        /// <returns>
        ///     The value of the static property with the given <paramref name="propertyKey"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     Thrown if there is no static property with the given <paramref name="propertyKey"/> in this item.
        /// </exception>
        public Property GetStaticProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            if (!staticProperties.TryGetValue(propertyKey, out var property))
                throw new PropertyNotFoundException(key, propertyKey);

            return property;
        }

        /// <summary>
        ///     Try to get the value of the static property with the given <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the static property to get the value of.
        /// </param>
        /// <param name="property">
        ///     The value of the searched static property, if found.
        /// </param>
        /// <returns>
        ///     Returns true if a static property with the given <paramref name="propertyKey"/> exists on this item;
        ///     returns false otherwise.
        /// </returns>
        public bool TryGetStaticProperty(string propertyKey, out Property property)
        {
            if (propertyKey == null)
            {
                property = default;

                return false;
            }

            return staticProperties.TryGetValue(propertyKey, out property);
        }

        /// <inheritdoc cref="GetStaticProperty"/>
        public Property this[string propertyKey] => GetStaticProperty(propertyKey);
    }
}
