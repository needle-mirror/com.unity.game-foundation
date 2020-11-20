#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class InventoryItemDefinitionAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "Item";

        /// <summary>
        ///     Before this inventory item is destroyed,
        ///     remove any references to it from any transactions and rewards.
        /// </summary>
        protected override void OnBeforeDestroy()
        {
            var rewardAssets = new List<RewardAsset>();
            CatalogSettings.catalogAsset.GetItems(rewardAssets);

            foreach (var rewardAsset in rewardAssets)
            {
                rewardAsset.Editor_HandleInventoryItemRemoved(this);
            }

            var transactionAssets = new List<BaseTransactionAsset>();
            CatalogSettings.catalogAsset.GetItems(transactionAssets);

            foreach (var transactionAsset in transactionAssets)
            {
                transactionAsset.Editor_HandleInventoryItemRemoved(this);
            }

            base.OnBeforeDestroy();
        }

        /// <summary>
        ///     Adds a property to the definition of this <see cref="InventoryItemDefinitionAsset"/> instance.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the new property.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value of this new property.
        ///     Also defines its type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if created, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_AddMutableProperty(string propertyName, Property defaultValue)
        {
            if (mutableProperties.ContainsKey(propertyName)
                || !UnityEngine.GameFoundation.Tools.IsValidId(propertyName))
            {
                return false;
            }

            mutableProperties.Add(propertyName, defaultValue);
            EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        ///     Remove the mutable property with the given <paramref name="propertyName"/> from this catalog.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the mutable property to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if removed, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveMutableProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return false;

            var isRemoved = mutableProperties.Remove(propertyName);

            if (isRemoved)
            {
                EditorUtility.SetDirty(this);
            }

            return isRemoved;
        }

        /// <summary>
        ///     Update a mutable property of this instance.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the mutable property to update.
        /// </param>
        /// <param name="value">
        ///     The updated value of the mutable property.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if updated, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_UpdateMutableProperty(string propertyName, Property value)
        {
            if (!mutableProperties.ContainsKey(propertyName))
                return false;

            //Don't forget to update the entry since we are working with structs.
            mutableProperties[propertyName] = value;
            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        ///     Set the <see cref="initialAllocation"/> of this instance.
        /// </summary>
        /// <param name="value">
        ///     The initial allocation to set.
        ///     Negative values are considered as 0.
        /// </param>
        internal void Editor_SetInitialAllocation(int value)
        {
            // negative allocation not permitted.
            m_InitialAllocation = Math.Max(0, value);
            m_InitialAllocationWrapper = new ExternalizableValue<int>(m_InitialAllocation);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the <see cref="isStackableFlag"/> of this instance.
        /// </summary>
        /// <param name="value">
        ///     The is stackable flag to set.
        /// </param>
        internal void Editor_SetIsStackableFlag(bool value)
        {
            m_IsStackableFlag = value;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the <see cref="initialQuantityPerStack"/> of this instance.
        /// </summary>
        /// <param name="value">
        ///     The initial quantity per stack to set.
        ///     Negative values are considered as 0.
        /// </param>
        internal void Editor_SetInitialQuantityPerStack(long value)
        {
            m_InitialQuantityPerStack = Math.Max(0, value);
            m_InitialQuantityPerStackWrapper = new ExternalizableValue<long>(m_InitialQuantityPerStack);

            EditorUtility.SetDirty(this);
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(InventoryItemDefinitionAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            if (!(target is InventoryItemDefinitionAsset inventoryItemTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(InventoryItemDefinitionAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            inventoryItemTarget.Editor_SetInitialAllocation(m_InitialAllocation);
            inventoryItemTarget.Editor_SetIsStackableFlag(m_IsStackableFlag);
            inventoryItemTarget.Editor_SetInitialQuantityPerStack(m_InitialQuantityPerStack);

            foreach (var kvp in mutableProperties)
            {
                inventoryItemTarget.Editor_AddMutableProperty(kvp.Key, kvp.Value);
            }

            base.CopyValues(inventoryItemTarget);
        }
    }
}

#endif
