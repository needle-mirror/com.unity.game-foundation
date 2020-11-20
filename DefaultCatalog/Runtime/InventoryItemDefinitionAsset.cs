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
        /// <inheritdoc cref="initialAllocation"/>
        [Min(0)]
        [SerializeField]
        int m_InitialAllocation;

        /// <summary>
        ///     Wrapper for <see cref="m_InitialAllocation"/>.
        /// </summary>
        ExternalizableValue<int> m_InitialAllocationWrapper;

        /// <summary>
        ///     Determines how many of this <see cref="InventoryItemDefinition"/>
        ///     to automatically add to player's inventory.
        /// </summary>
        public ExternalizableValue<int> initialAllocation
        {
            get
            {
                if (m_InitialAllocationWrapper is null)
                {
                    m_InitialAllocationWrapper = new ExternalizableValue<int>(m_InitialAllocation);
                }

                return m_InitialAllocationWrapper;
            }
        }

        /// <inheritdoc cref="isStackableFlag"/>
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

        /// <inheritdoc cref="initialQuantityPerStack"/>
        [Min(0)]
        [SerializeField]
        long m_InitialQuantityPerStack = 1;

        /// <summary>
        ///     Wrapper for <see cref="m_InitialQuantityPerStack"/>.
        /// </summary>
        ExternalizableValue<long> m_InitialQuantityPerStackWrapper;

        /// <summary>
        ///     If this <see cref="InventoryItemDefinition"/> is stackable, how many
        ///     items (quantity) should be initialized in each initial stack?
        /// </summary>
        public ExternalizableValue<long> initialQuantityPerStack
        {
            get
            {
                if (m_InitialQuantityPerStackWrapper is null)
                {
                    m_InitialQuantityPerStackWrapper = new ExternalizableValue<long>(m_InitialQuantityPerStack);
                }

                return m_InitialQuantityPerStackWrapper;
            }
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
        internal Dictionary<string, ExternalizableValue<Property>> mutableProperties { get; }
            = new Dictionary<string, ExternalizableValue<Property>>();

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

            var hasValueProvider = !(valueProvider is null);
            var mutablePropertiesKeys = new List<string>(mutableProperties.Keys);

            //We can't do a foreach loop on the dictionary since we modify the entry.
            foreach (var propertyKey in mutablePropertiesKeys)
            {
                var propertyName = ExternalValueProviderNames.GetMutablePropertyName(propertyKey);
                if (hasValueProvider
                    && valueProvider.TryGetValue(propertyName, key, out var externalPropertyValue))
                {
                    mutableProperties[propertyKey].SetExternalValue(externalPropertyValue);
                }
                else
                {
                    mutableProperties[propertyKey].ResetExternalValue();
                }

                item.properties.Add(propertyKey, mutableProperties[propertyKey]);
            }

            return item;
        }

        /// <inheritdoc/>
        internal override void OverridePreConfigurationData(IExternalValueProvider valueProvider)
        {
            if (valueProvider is null)
            {
                initialAllocation.ResetExternalValue();
                initialQuantityPerStack.ResetExternalValue();

                return;
            }

            if (valueProvider.TryGetValue(
                ExternalValueProviderNames.initialAllocation, key, out var externalInitialBalance))
            {
                initialAllocation.SetExternalValue(externalInitialBalance);
            }
            else
            {
                initialAllocation.ResetExternalValue();
            }

            if (isStackableFlag
                && valueProvider.TryGetValue(
                    ExternalValueProviderNames.initialQuantityPerStack, key, out var externalInitialStack))
            {
                initialQuantityPerStack.SetExternalValue(externalInitialStack);
            }
            else
            {
                initialQuantityPerStack.ResetExternalValue();
            }
        }

        /// <inheritdoc/>
        protected override void OnBeforeItemSerialize()
        {
            base.OnBeforeItemSerialize();

            m_MutablePropertyKeys = new List<string>(mutableProperties.Keys);
            m_MutablePropertyDefaultValues = new List<Property>(mutableProperties.Values.Count);
            foreach (var mutablePropertiesValue in mutableProperties.Values)
            {
                m_MutablePropertyDefaultValues.Add(mutablePropertiesValue);
            }
        }

        /// <inheritdoc/>
        protected override void OnAfterItemDeserialize()
        {
            base.OnAfterItemDeserialize();

            m_InitialAllocationWrapper = new ExternalizableValue<int>(m_InitialAllocation);
            m_InitialQuantityPerStackWrapper = new ExternalizableValue<long>(m_InitialQuantityPerStack);

            DeserializeListsToDictionary(m_MutablePropertyKeys, m_MutablePropertyDefaultValues, mutableProperties);
        }
    }
}
