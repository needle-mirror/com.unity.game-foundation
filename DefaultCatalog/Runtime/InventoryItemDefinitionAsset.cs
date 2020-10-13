using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Data;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Asset version of a definition for inventory items.
    /// </summary>
    public sealed partial class InventoryItemDefinitionAsset : TradableDefinitionAsset
    {
        /// <summary>
        ///     Determines how many of this <see cref="InventoryItemDefinition"/>
        ///     to automatically add to player's inventory.
        /// </summary>
        [SerializeField]
        int m_InitialAllocation;

        /// <summary>
        ///     External override for <see cref="m_InitialAllocation"/>.
        /// </summary>
        [NonSerialized]
        int? m_ExternalInitialAllocation;

        /// <summary>
        ///     Determines how many of this <see cref="InventoryItemDefinition"/>
        ///     to automatically add to player's inventory.
        /// </summary>
        public int initialAllocation
        {
            get => m_ExternalInitialAllocation ?? m_InitialAllocation;
            internal set => m_InitialAllocation = value;
        }

        /// <summary>
        ///     Is this <see cref="InventoryItemDefinition"/> stackable?
        /// </summary>
        [SerializeField]
        bool m_IsStackableFlag;

        /// <summary>
        ///     Is this <see cref="InventoryItemDefinition"/> stackable?
        /// </summary>
        public bool isStackableFlag
        {
            get => m_IsStackableFlag;
            internal set => m_IsStackableFlag = value;
        }

        /// <summary>
        ///     If this <see cref="InventoryItemDefinition"/> is stackable, how many
        ///     items should be initialized in each initial stack?
        /// </summary>
        [SerializeField]
        long m_InitialQuantityPerStack = 1;

        /// <summary>
        ///     If this <see cref="InventoryItemDefinition"/> is stackable, how many
        ///     items (quantity) should be initialized in each initial stack?
        /// </summary>
        public long initialQuantityPerStack
        {
            get => m_InitialQuantityPerStack;
            internal set => m_InitialQuantityPerStack = value;
        }

        [FormerlySerializedAs("m_PropertyKeys")]
        [SerializeField]
        List<string> m_MutablePropertyKeys;

        [FormerlySerializedAs("m_PropertyDefaultValues")]
        [SerializeField]
        List<Property> m_MutablePropertyDefaultValues;

        /// <summary>
        ///     Stores all mutableProperties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> mutableProperties { get; }
            = new Dictionary<string, Property>();

        /// <summary>
        ///     Get all default properties stored in this definition.
        /// </summary>
        /// <returns>
        ///     Return a list of <see cref="PropertyData"/>
        ///     for each mutableProperties stored in this definition.
        ///     The returned list is never null.
        /// </returns>
        public List<PropertyData> GetDefaultProperties()
        {
            var defaultProperties = new List<PropertyData>(mutableProperties.Count);
            foreach (var propertyEntry in mutableProperties)
            {
                var data = new PropertyData
                {
                    key = propertyEntry.Key,
                    value = propertyEntry.Value
                };
                defaultProperties.Add(data);
            }

            return defaultProperties;
        }

        /// <inheritdoc/>
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder, IExternalValueProvider valueProvider)
        {
            InventoryItemDefinitionConfig item;

            if (isStackableFlag)
            {
                item = builder.Create<StackableInventoryItemDefinitionConfig>(key);
            }
            else
            {
                item = builder.Create<InventoryItemDefinitionConfig>(key);
            }

            foreach (var property in mutableProperties)
            {
                var propertyName = ExternalValueProviderNames.GetMutablePropertyName(property.Key);
                if (valueProvider == null
                    || !valueProvider.TryGetValue(propertyName, key, out var propertyValue))
                {
                    propertyValue = property.Value;
                }

                item.properties.Add(property.Key, propertyValue);
            }

            return item;
        }

        /// <inheritdoc/>
        internal override void OverrideNonConfiguredData(IExternalValueProvider valueProvider)
        {
            if (valueProvider != null
                && valueProvider.TryGetValue(
                    ExternalValueProviderNames.initialAllocation, key, out var externalInitialBalance))
            {
                m_ExternalInitialAllocation = externalInitialBalance;
            }
            else
            {
                m_ExternalInitialAllocation = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnBeforeItemSerialize()
        {
            m_MutablePropertyKeys = new List<string>(mutableProperties.Keys);
            m_MutablePropertyDefaultValues = new List<Property>(mutableProperties.Values);
        }

        /// <inheritdoc/>
        protected override void OnAfterItemDeserialize()
        {
            DeserializeListsToDictionary(m_MutablePropertyKeys, m_MutablePropertyDefaultValues, mutableProperties);
        }
    }
}
