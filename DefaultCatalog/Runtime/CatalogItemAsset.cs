using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Data;
using UnityEngine.Serialization;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Base class for most of the static data in Game Foundation.
    /// </summary>
    public abstract partial class CatalogItemAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <inheritdoc cref="displayName"/>
        [SerializeField]
        internal string m_DisplayName;

        /// <summary>
        ///     The readable name of this <see cref="CatalogItemAsset"/> instance.
        ///     It is used to make the Editor more comfortable, but it can also be
        ///     used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName => m_DisplayName;

        /// <inheritdoc cref="key"/>
        [SerializeField]
        [FormerlySerializedAs("m_Id")]
        internal string m_Key;

        /// <summary>
        ///     The identifier of the <see cref="CatalogItem"/> constructed from this asset.
        /// </summary>
        public string key => m_Key;

        /// <summary>
        ///     The <see cref="TagAsset"/> instances this item is linked to.
        ///     Those tags are stored in the same catalog than the one storing this item.
        /// </summary>
        [SerializeField]
        internal List<TagAsset> m_Tags;

        [SerializeField]
        List<string> m_StaticPropertyKeys;

        [SerializeField]
        List<Property> m_StaticPropertyValues;

        /// <summary>
        ///     Stores all properties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> staticProperties { get; }
            = new Dictionary<string, Property>();

        /// <summary>
        ///     Fills the given <paramref name="target"/> collection with all the <see cref="TagAsset"/>
        ///     instances linked to this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="TagAsset"/> instances.
        /// </param>
        /// <param name="clearTarget">
        ///     If <c>true</c>, it clears the <paramref name="target"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of <see cref="TagAsset"/> instances linked to this <see cref="CatalogItemAsset"/> instance.
        /// </returns>
        public int GetTags(ICollection<TagAsset> target = null, bool clearTarget = true)
            => GFTools.Copy(m_Tags, target, clearTarget);

        /// <summary>
        ///     Tells whether or not the given <paramref name="tag"/> is within
        ///     this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="TagAsset"/> instance to search for.
        /// </param>
        /// <returns>
        ///     Whether or not this <see cref="CatalogItemAsset"/> instance
        ///     has the specified <see cref="TagAsset"/> included.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag"/> is <c>null</c>.
        /// </exception>
        public bool HasTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return m_Tags.Contains(tag);
        }

        /// <summary>
        ///     Get all static properties stored in this item.
        /// </summary>
        /// <returns>
        ///     Return a list of <see cref="PropertyData"/>
        ///     for each properties stored in this item.
        ///     The returned list is never null.
        /// </returns>
        public List<PropertyData> GetStaticProperties()
        {
            var propertiesData = new List<PropertyData>(staticProperties.Count);
            foreach (var propertyEntry in staticProperties)
            {
                var data = new PropertyData
                {
                    key = propertyEntry.Key,
                    value = propertyEntry.Value
                };
                propertiesData.Add(data);
            }

            return propertiesData;
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

        /// <summary>
        ///     Initializes the internal collections.
        /// </summary>
        protected void Awake()
        {
            if (m_Tags is null)
            {
                m_Tags = new List<TagAsset>();
            }

            AwakeDefinition();
        }

        /// <summary>
        ///     Overriden by inherited classes to initialize specific members.
        /// </summary>
        protected virtual void AwakeDefinition() { }

        /// <summary>
        ///     Called before serialization, this will copy over all keys and values
        ///     from the <see cref="staticProperties"/> dictionary into their serializable
        ///     lists <see cref="m_StaticPropertyKeys"/> and <see cref="m_StaticPropertyValues"/>.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_StaticPropertyKeys = new List<string>(staticProperties.Keys);
            m_StaticPropertyValues = new List<Property>(staticProperties.Values);

            OnBeforeItemSerialize();
        }

        /// <summary>
        ///     Called at the end of <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
        ///     Enable inheritance to add specific serialization process.
        /// </summary>
        protected virtual void OnBeforeItemSerialize() { }

        /// <summary>
        ///     Called after serialization, this will deserialize the <see cref="m_StaticPropertyKeys"/> and
        ///     <see cref="m_StaticPropertyValues"/> lists and convert them into a dictionary <see cref="staticProperties"/>.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            DeserializeListsToDictionary(m_StaticPropertyKeys, m_StaticPropertyValues, staticProperties);

            OnAfterItemDeserialize();
        }

        /// <summary>
        ///     Called at the end of <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>.
        ///     Enable inheritance to add specific deserialization process.
        /// </summary>
        protected virtual void OnAfterItemDeserialize() { }

        /// <summary>
        ///     Configures a specified <paramref name="builder"/> with this item.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data
        ///     with an external source when configuring them.
        /// </param>
        internal void Configure(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            var item = ConfigureItem(builder, valueProvider);

            if (valueProvider == null
                || !valueProvider.TryGetValue(
                    ExternalValueProviderNames.displayName, key, out var externalDisplayName))
            {
                item.displayName = displayName;
            }
            else
            {
                item.displayName = externalDisplayName;
            }

            foreach (var property in staticProperties)
            {
                var propertyName = ExternalValueProviderNames.GetStaticPropertyName(property.Key);
                if (valueProvider == null
                    || !valueProvider.TryGetValue(propertyName, key, out var propertyValue))
                {
                    propertyValue = property.Value;
                }

                item.staticProperties.Add(property.Key, propertyValue);
            }

            foreach (var tagAsset in m_Tags)
            {
                item.tags.Add(tagAsset.key);
            }
        }

        /// <summary>
        ///     Override all data that isn't set through <see cref="ICatalogConfigurator.Configure"/>
        ///     but are still used by Game Foundation systems.
        /// </summary>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data with an external source.
        /// </param>
        internal virtual void OverrideNonConfiguredData(IExternalValueProvider valueProvider) { }

        /// <summary>
        ///     Configures a specified <paramref name="builder"/> with the specifics
        ///     of this item.
        /// </summary>
        /// <param name="builder">
        ///     The target builder.
        /// </param>
        /// <param name="valueProvider">
        ///     A value provider to override some catalog item's data with an external source.
        /// </param>
        /// <returns>
        ///     The item config.
        /// </returns>
        protected abstract CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider);

        /// <summary>
        ///     Deserialize the given lists into the given dictionary.
        /// </summary>
        /// <param name="keys">
        ///     A list of keys.
        ///     It is cleared after the dictionary has been filled.
        /// </param>
        /// <param name="values">
        ///     A list of values.
        ///     It is cleared after the dictionary has been filled.
        /// </param>
        /// <param name="container">
        ///     The dictionary that will be filled with serialized data.
        ///     It is cleared even if the given lists are empty.
        /// </param>
        /// <typeparam name="TKey">
        ///     The type of the <paramref name="keys"/>.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///     The type of the <paramref name="values"/>.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the argument is null.
        /// </exception>
        /// <exception cref="SerializationException">
        ///     Thrown if the given lists have a different item count.
        /// </exception>
        protected static void DeserializeListsToDictionary<TKey, TValue>(
            List<TKey> keys, List<TValue> values, Dictionary<TKey, TValue> container)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Clear();

            var itemCount = keys.Count;
            if (itemCount != values.Count)
                throw new SerializationException(
                    $"{nameof(CatalogItemAsset)}: An error occured during the deserialization of this object. It contains corrupted data.");

            if (itemCount <= 0)
                return;

            for (var i = 0; i < itemCount; i++)
            {
                var key = keys[i];
                var value = values[i];

                container[key] = value;
            }

            // Clear lists to avoid storing duplicated data.
            keys.Clear();
            values.Clear();
        }
    }
}
